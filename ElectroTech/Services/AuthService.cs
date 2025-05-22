using System;
using System.Collections.Generic;
using System.Linq;
using ElectroTech.DataAccess;
using ElectroTech.Helpers;
using ElectroTech.Models;

namespace ElectroTech.Services
{
    /// <summary>
    /// Servicio para manejar la autenticación y operaciones relacionadas con usuarios.
    /// </summary>
    public class AuthService
    {
        private readonly UsuarioRepository _usuarioRepository;

        /// <summary>
        /// Constructor del servicio de autenticación.
        /// </summary>
        public AuthService()
        {
            _usuarioRepository = new UsuarioRepository();
        }

        /// <summary>
        /// Autentica a un usuario en el sistema.
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario.</param>
        /// <param name="clave">Contraseña del usuario.</param>
        /// <returns>El usuario autenticado o null si la autenticación falla.</returns>
        public Usuario Autenticar(string nombreUsuario, string clave)
        {
            try
            {

                // Intentar autenticar al usuario
                Usuario usuario = _usuarioRepository.Autenticar(nombreUsuario, clave);

                // Verificar si el usuario existe y está activo
                if (usuario != null && usuario.Estado == 'A')
                {
                    Logger.LogInfo($"Usuario {nombreUsuario} autenticado exitosamente.");
                    return usuario;
                }
                else if (usuario != null && usuario.Estado != 'A')
                {
                    Logger.LogWarning($"Intento de acceso con usuario inactivo: {nombreUsuario}");
                    return null;
                }
                else
                {
                    Logger.LogWarning($"Intento de acceso fallido para el usuario: {nombreUsuario}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error en la autenticación del usuario {nombreUsuario}");
                throw;
            }
        }

        /// <summary>
        /// Registra la salida de un usuario en la bitácora.
        /// </summary>
        /// <param name="idUsuario">ID del usuario que cierra sesión.</param>
        /// <returns>True si el registro es exitoso, False en caso contrario.</returns>
        public bool RegistrarSalida(int idUsuario)
        {
            try
            {
                return _usuarioRepository.RegistrarAcceso(idUsuario, 'S');
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al registrar salida del usuario ID: {idUsuario}");
                throw;
            }
        }

        /// <summary>
        /// Verifica si un usuario tiene permiso para acceder a una funcionalidad específica.
        /// </summary>
        /// <param name="usuario">Usuario a verificar.</param>
        /// <param name="nivelRequerido">Nivel mínimo requerido para acceder.</param>
        /// <returns>True si el usuario tiene permiso, False en caso contrario.</returns>
        public bool TienePermiso(Usuario usuario, int nivelRequerido)
        {
            // Los niveles son: 1-Administrador, 2-Paramétrico, 3-Esporádico
            // Menor número significa mayor nivel de acceso
            return usuario != null && usuario.Nivel <= nivelRequerido;
        }

        /// <summary>
        /// Cambia la contraseña de un usuario.
        /// </summary>
        /// <param name="usuario">Usuario que cambia la contraseña.</param>
        /// <param name="claveActual">Contraseña actual.</param>
        /// <param name="nuevaClave">Nueva contraseña.</param>
        /// <param name="errorMessage">Mensaje de error si el cambio falla.</param>
        /// <returns>True si el cambio es exitoso, False en caso contrario.</returns>
        public bool CambiarClave(Usuario usuario, string claveActual, string nuevaClave, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Verificar que la contraseña actual sea correcta
                string hashedCurrentPassword = PasswordValidator.HashPassword(claveActual);
                Usuario userCheck = _usuarioRepository.Autenticar(usuario.NombreUsuario, hashedCurrentPassword);

                if (userCheck == null)
                {
                    errorMessage = "La contraseña actual es incorrecta.";
                    return false;
                }


                // Hashear la nueva contraseña
                string hashedNewPassword = PasswordValidator.HashPassword(nuevaClave);

                // Actualizar la contraseña en la base de datos
                bool result = _usuarioRepository.CambiarClave(usuario.IdUsuario, hashedNewPassword);

                if (result)
                {
                    Logger.LogInfo($"Contraseña cambiada exitosamente para el usuario {usuario.NombreUsuario}");
                }
                else
                {
                    errorMessage = "No se pudo actualizar la contraseña en la base de datos.";
                    Logger.LogError($"Error al cambiar la contraseña para el usuario {usuario.NombreUsuario}");
                }

                return result;
            }
            catch (Exception ex)
            {
                errorMessage = "Error al procesar el cambio de contraseña.";
                Logger.LogException(ex, $"Error al cambiar la contraseña para el usuario {usuario.NombreUsuario}");
                return false;
            }
        }

        /// <summary>
        /// Crea un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="usuario">Usuario a crear.</param>
        /// <param name="contrasena">Contraseña en texto plano.</param>
        /// <param name="errorMessage">Mensaje de error si la creación falla.</param>
        /// <returns>True si la creación es exitosa, False en caso contrario.</returns>
        public bool CrearUsuario(Usuario usuario, string contrasena, out string errorMessage)
        {
            try
            {
                // Validar usuario
                if (!ValidarUsuario(usuario, out errorMessage))
                {
                    return false;
                }

                // Validar contraseña
                if (string.IsNullOrWhiteSpace(contrasena) || contrasena.Length < 8)
                {
                    errorMessage = "La contraseña debe tener al menos 8 caracteres.";
                    return false;
                }

                // Verificar que la contraseña tenga al menos una mayúscula, una minúscula y un número
                if (!ValidarFormatoContrasena(contrasena))
                {
                    errorMessage = "La contraseña debe incluir al menos una letra mayúscula, una minúscula y un número.";
                    return false;
                }

                // Generar hash de la contraseña
                string hashContrasena = PasswordValidator.HashPassword(contrasena);

                // Crear el usuario
                return _usuarioRepository.Crear(usuario, hashContrasena, out errorMessage);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al crear usuario {usuario.NombreUsuario}");
                return false;
            }
        }

        /// <summary>
        /// Actualiza un usuario existente sin cambiar su contraseña.
        /// </summary>
        /// <param name="usuario">Usuario con los datos actualizados.</param>
        /// <param name="errorMessage">Mensaje de error si la actualización falla.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool ActualizarUsuario(Usuario usuario, out string errorMessage)
        {
            try
            {
                // Validar usuario
                if (!ValidarUsuario(usuario, out errorMessage))
                {
                    return false;
                }

                // Actualizar usuario sin cambiar contraseña
                return _usuarioRepository.Actualizar(usuario, out errorMessage);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al actualizar usuario con ID {usuario.IdUsuario}");
                return false;
            }
        }


        /// <summary>
        /// Actualiza un usuario incluyendo su contraseña.
        /// </summary>
        /// <param name="usuario">Usuario con los datos actualizados.</param>
        /// <param name="nuevaContrasena">Nueva contraseña del usuario.</param>
        /// <param name="errorMessage">Mensaje de error si la actualización falla.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool ActualizarUsuarioConContrasena(Usuario usuario, string nuevaContrasena, out string errorMessage)
        {
            try
            {
                

                // Validar contraseña
                if (string.IsNullOrWhiteSpace(nuevaContrasena) || nuevaContrasena.Length < 8)
                {
                    errorMessage = "La contraseña debe tener al menos 8 caracteres.";
                    return false;
                }

                // Verificar que la contraseña tenga al menos una mayúscula, una minúscula y un número
                if (!ValidarFormatoContrasena(nuevaContrasena))
                {
                    errorMessage = "La contraseña debe incluir al menos una letra mayúscula, una minúscula y un número.";
                    return false;
                }

                // Generar hash de la contraseña
                string hashContrasena = PasswordValidator.HashPassword(nuevaContrasena);

                // Actualizar usuario con la nueva contraseña
                return _usuarioRepository.ActualizarConContrasena(usuario, hashContrasena, out errorMessage);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al actualizar usuario con ID {usuario.IdUsuario}");
                return false;
            }
        }

        // Método para validar un usuario
        private bool ValidarUsuario(Usuario usuario, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (usuario == null)
            {
                errorMessage = "El usuario no puede ser nulo.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(usuario.NombreUsuario))
            {
                errorMessage = "El nombre de usuario es obligatorio.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(usuario.NombreCompleto))
            {
                errorMessage = "El nombre completo es obligatorio.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(usuario.Correo))
            {
                errorMessage = "El correo electrónico es obligatorio.";
                return false;
            }

            if (!ValidarFormatoCorreo(usuario.Correo))
            {
                errorMessage = "El formato del correo electrónico no es válido.";
                return false;
            }

            if (usuario.Nivel <= 0 || usuario.Nivel > 3)
            {
                errorMessage = "El nivel de usuario debe ser 1, 2 o 3.";
                return false;
            }

            if (usuario.Estado != 'A' && usuario.Estado != 'I')
            {
                errorMessage = "El estado del usuario debe ser 'A' (Activo) o 'I' (Inactivo).";
                return false;
            }

            return true;
        }

        // Método para validar el formato de la contraseña
        private bool ValidarFormatoContrasena(string contrasena)
        {
            // Comprobar si tiene al menos una mayúscula
            bool tieneMayuscula = contrasena.Any(c => char.IsUpper(c));

            // Comprobar si tiene al menos una minúscula
            bool tieneMinuscula = contrasena.Any(c => char.IsLower(c));

            // Comprobar si tiene al menos un número
            bool tieneNumero = contrasena.Any(c => char.IsDigit(c));

            return tieneMayuscula && tieneMinuscula && tieneNumero;
        }

        // Método para validar el formato del correo electrónico
        private bool ValidarFormatoCorreo(string correo)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(correo);
                return addr.Address == correo;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Verifica si existe un usuario administrador en el sistema.
        /// </summary>
        /// <returns>True si existe un administrador, False en caso contrario.</returns>
        public bool ExisteAdministrador()
        {
            try
            {
                return _usuarioRepository.ExisteAdministrador();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al verificar existencia de administrador");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un usuario por su ID.
        /// </summary>
        /// <param name="idUsuario">ID del usuario.</param>
        /// <returns>El objeto Usuario si se encuentra, null en caso contrario.</returns>
        public Usuario ObtenerPorId(int idUsuario)
        {
            try
            {
                return _usuarioRepository.ObtenerPorId(idUsuario);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener usuario con ID {idUsuario}");
                throw;
            }
        }

        /// <summary>
        /// Verifica si ya existe un nombre de usuario en el sistema.
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario a verificar.</param>
        /// <returns>True si el nombre de usuario ya existe, False en caso contrario.</returns>
        public bool ExisteNombreUsuario(string nombreUsuario)
        {
            try
            {
                return _usuarioRepository.ExisteNombreUsuario(nombreUsuario);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al verificar existencia de nombre de usuario '{nombreUsuario}'");
                throw;
            }
        }


    }
}