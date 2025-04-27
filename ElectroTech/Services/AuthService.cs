using System;
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
                // Hashear la contraseña para comparar con la almacenada
                string hashedPassword = PasswordValidator.HashPassword(clave);

                // Intentar autenticar al usuario
                Usuario usuario = _usuarioRepository.Autenticar(nombreUsuario, hashedPassword);

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
        /// <param name="clave">Contraseña del nuevo usuario.</param>
        /// <param name="errorMessage">Mensaje de error si la creación falla.</param>
        /// <returns>True si la creación es exitosa, False en caso contrario.</returns>
        public bool CrearUsuario(Usuario usuario, string clave, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
               

                // Hashear la contraseña
                usuario.Clave = PasswordValidator.HashPassword(clave);

                // Crear el usuario en la base de datos
                int idUsuario = _usuarioRepository.Crear(usuario);

                if (idUsuario > 0)
                {
                    usuario.IdUsuario = idUsuario;
                    Logger.LogInfo($"Usuario {usuario.NombreUsuario} creado exitosamente.");
                    return true;
                }
                else
                {
                    errorMessage = "No se pudo crear el usuario en la base de datos.";
                    Logger.LogError($"Error al crear el usuario {usuario.NombreUsuario}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al crear el usuario {usuario.NombreUsuario}");
                return false;
            }
        }

        /// <summary>
        /// Actualiza los datos de un usuario existente.
        /// </summary>
        /// <param name="usuario">Usuario con los datos actualizados.</param>
        /// <param name="errorMessage">Mensaje de error si la actualización falla.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool ActualizarUsuario(Usuario usuario, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Actualizar el usuario en la base de datos
                bool result = _usuarioRepository.Actualizar(usuario);

                if (result)
                {
                    Logger.LogInfo($"Usuario {usuario.NombreUsuario} actualizado exitosamente.");
                }
                else
                {
                    errorMessage = "No se pudo actualizar el usuario en la base de datos.";
                    Logger.LogError($"Error al actualizar el usuario {usuario.NombreUsuario}");
                }

                return result;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al actualizar el usuario {usuario.NombreUsuario}");
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
    }
}