using ElectroTech.DataAccess;
using ElectroTech.Helpers;
using ElectroTech.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ElectroTech.Services
{
    /// <summary>
    /// Servicio para la gestión de empleados - Versión Debug.
    /// </summary>
    public class EmpleadoService
    {
        private readonly EmpleadoRepository _empleadoRepository;
        private readonly AuthService _authService;

        /// <summary>
        /// Constructor del servicio de empleados.
        /// </summary>
        public EmpleadoService()
        {
            _empleadoRepository = new EmpleadoRepository();
            _authService = new AuthService();
        }

        /// <summary>
        /// Obtiene todos los empleados activos.
        /// </summary>
        /// <returns>Lista de empleados activos.</returns>
        public List<Empleado> ObtenerTodos()
        {
            try
            {
                return _empleadoRepository.ObtenerTodos();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener todos los empleados");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un empleado por su ID.
        /// </summary>
        /// <param name="idEmpleado">ID del empleado.</param>
        /// <returns>El empleado si se encuentra, null en caso contrario.</returns>
        public Empleado ObtenerPorId(int idEmpleado)
        {
            try
            {
                return _empleadoRepository.ObtenerPorId(idEmpleado);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener empleado con ID {idEmpleado}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un empleado por su número de documento.
        /// </summary>
        /// <param name="tipoDocumento">Tipo de documento.</param>
        /// <param name="numeroDocumento">Número de documento.</param>
        /// <returns>El empleado si se encuentra, null en caso contrario.</returns>
        public Empleado ObtenerPorDocumento(string tipoDocumento, string numeroDocumento)
        {
            try
            {
                return _empleadoRepository.ObtenerPorDocumento(tipoDocumento, numeroDocumento);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener empleado con documento {tipoDocumento}-{numeroDocumento}");
                throw;
            }
        }

        /// <summary>
        /// Busca empleados según un término de búsqueda.
        /// </summary>
        /// <param name="termino">Término de búsqueda.</param>
        /// <returns>Lista de empleados que coinciden con el término.</returns>
        public List<Empleado> Buscar(string termino)
        {
            try
            {
                return _empleadoRepository.Buscar(termino);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al buscar empleados con término '{termino}'");
                throw;
            }
        }

        /// <summary>
        /// Crea un nuevo empleado, opcionalmente con un usuario asociado.
        /// </summary>
        /// <param name="empleado">Empleado a crear.</param>
        /// <param name="nombreUsuario">Nombre de usuario (opcional).</param>
        /// <param name="nivelUsuario">Nivel de usuario (opcional).</param>
        /// <param name="errorMessage">Mensaje de error si la creación falla.</param>
        /// <returns>True si la creación es exitosa, False en caso contrario.</returns>
        public bool CrearEmpleado(Empleado empleado, string nombreUsuario, int nivelUsuario, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                Logger.LogInfo($"=== INICIANDO CREACIÓN DE EMPLEADO ===");
                Logger.LogInfo($"Empleado: {empleado.Nombre} {empleado.Apellido}");
                Logger.LogInfo($"Documento: {empleado.TipoDocumento} {empleado.NumeroDocumento}");
                Logger.LogInfo($"Usuario a crear: {(!string.IsNullOrEmpty(nombreUsuario) ? nombreUsuario : "NO")}");
                Logger.LogInfo($"Nivel usuario: {nivelUsuario}");

                // Validar datos del empleado
                if (!ValidarEmpleado(empleado, out errorMessage))
                {
                    Logger.LogError($"VALIDACIÓN EMPLEADO FALLIDA: {errorMessage}");
                    return false;
                }
                Logger.LogInfo("✓ Validación de empleado exitosa");

                // Validar que no exista otro empleado con el mismo documento
                var empleadoExistente = _empleadoRepository.ObtenerPorDocumento(empleado.TipoDocumento, empleado.NumeroDocumento);
                if (empleadoExistente != null)
                {
                    errorMessage = $"Ya existe un empleado con el documento {empleado.TipoDocumento} {empleado.NumeroDocumento}.";
                    Logger.LogError($"DOCUMENTO DUPLICADO: {errorMessage}");
                    return false;
                }
                Logger.LogInfo("✓ Verificación de documento único exitosa");

                // Si se va a crear usuario, validar datos adicionales
                bool crearUsuario = !string.IsNullOrEmpty(nombreUsuario) && nivelUsuario > 0;
                Logger.LogInfo($"¿Crear usuario? {crearUsuario}");

                if (crearUsuario)
                {
                    if (!ValidarDatosUsuario(nombreUsuario, nivelUsuario, out errorMessage))
                    {
                        Logger.LogError($"VALIDACIÓN USUARIO FALLIDA: {errorMessage}");
                        return false;
                    }
                    Logger.LogInfo("✓ Validación de datos de usuario exitosa");
                }

                // Inicializar ID de usuario en 0 (sin usuario asociado)
                empleado.IdUsuario = 0;

                // Crear el empleado primero
                Logger.LogInfo("Creando empleado en la base de datos...");
                int idEmpleado = _empleadoRepository.Crear(empleado);
                Logger.LogInfo($"ID empleado retornado: {idEmpleado}");

                if (idEmpleado <= 0)
                {
                    errorMessage = "No se pudo crear el empleado en la base de datos.";
                    Logger.LogError($"ERROR CREAR EMPLEADO: ID retornado = {idEmpleado}");
                    return false;
                }

                // Actualizar el ID del empleado
                empleado.IdEmpleado = idEmpleado;
                Logger.LogInfo($"✓ Empleado creado exitosamente con ID: {idEmpleado}");

                // Si se debe crear un usuario asociado
                if (crearUsuario)
                {
                    try
                    {
                        Logger.LogInfo("=== INICIANDO CREACIÓN DE USUARIO ===");

                        // Generar contraseña inicial
                        string claveInicial = GenerarClaveInicial(nombreUsuario, empleado.NumeroDocumento);
                        Logger.LogInfo($"Contraseña temporal generada: {claveInicial}");

                        // Crear el objeto usuario
                        var usuario = new Usuario
                        {
                            NombreUsuario = nombreUsuario,
                            Nivel = nivelUsuario,
                            NombreCompleto = empleado.NombreCompleto,
                            Correo = $"{nombreUsuario}@electrotech.com", // Correo por defecto
                            Estado = 'A' // Activo
                        };

                        Logger.LogInfo($"Usuario a crear:");
                        Logger.LogInfo($"  - NombreUsuario: {usuario.NombreUsuario}");
                        Logger.LogInfo($"  - Nivel: {usuario.Nivel}");
                        Logger.LogInfo($"  - NombreCompleto: {usuario.NombreCompleto}");
                        Logger.LogInfo($"  - Correo: {usuario.Correo}");
                        Logger.LogInfo($"  - Estado: {usuario.Estado}");

                        // Crear el usuario
                        string errorUsuario;
                        bool usuarioCreado = _authService.CrearUsuario(usuario, claveInicial, out errorUsuario);

                        Logger.LogInfo($"Resultado creación usuario: {usuarioCreado}");
                        if (!usuarioCreado)
                        {
                            Logger.LogError($"ERROR CREAR USUARIO: {errorUsuario}");
                        }
                        else
                        {
                            Logger.LogInfo($"✓ Usuario creado con ID: {usuario.IdUsuario}");
                        }

                        if (usuarioCreado)
                        {
                            // Asociar usuario al empleado
                            Logger.LogInfo($"Asociando usuario {usuario.IdUsuario} al empleado {idEmpleado}...");
                            bool asociacionExitosa = _empleadoRepository.ActualizarIdUsuario(idEmpleado, usuario.IdUsuario);
                            Logger.LogInfo($"Resultado asociación: {asociacionExitosa}");

                            if (asociacionExitosa)
                            {
                                // Actualizar el ID de usuario en el objeto empleado
                                empleado.IdUsuario = usuario.IdUsuario;

                                Logger.LogInfo($"✓ EMPLEADO Y USUARIO CREADOS EXITOSAMENTE");
                                Logger.LogInfo($"  - EmpleadoID: {idEmpleado}");
                                Logger.LogInfo($"  - UsuarioID: {usuario.IdUsuario}");
                                Logger.LogInfo($"  - Contraseña temporal: {claveInicial}");
                            }
                            else
                            {
                                errorMessage = "Se creó el empleado y el usuario, pero no se pudo establecer la asociación.";
                                Logger.LogWarning($"WARNING: {errorMessage}");
                                // No retornamos false porque tanto empleado como usuario se crearon
                            }
                        }
                        else
                        {
                            errorMessage = $"Se creó el empleado pero no se pudo crear el usuario: {errorUsuario}";
                            Logger.LogError($"ERROR CREACIÓN USUARIO: {errorUsuario}");
                            // No retornamos false porque el empleado se creó correctamente
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMessage = "Se creó el empleado pero ocurrió un error al crear el usuario asociado.";
                        Logger.LogException(ex, $"EXCEPCIÓN AL CREAR USUARIO para empleado {idEmpleado}");
                        // No retornamos false porque el empleado se creó correctamente
                    }
                }
                else
                {
                    Logger.LogInfo($"✓ Empleado creado exitosamente sin usuario asociado: {empleado.NombreCompleto} (ID: {idEmpleado})");
                }

                Logger.LogInfo("=== CREACIÓN DE EMPLEADO COMPLETADA ===");
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"EXCEPCIÓN GENERAL al crear empleado {empleado.Nombre} {empleado.Apellido}");
                return false;
            }
        }

        /// <summary>
        /// Actualiza un empleado existente, opcionalmente actualizando su usuario asociado.
        /// </summary>
        /// <param name="empleado">Empleado con los datos actualizados.</param>
        /// <param name="nombreUsuario">Nombre de usuario (opcional).</param>
        /// <param name="nivelUsuario">Nivel de usuario (opcional).</param>
        /// <param name="errorMessage">Mensaje de error si la actualización falla.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool ActualizarEmpleado(Empleado empleado, string nombreUsuario, int nivelUsuario, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                Logger.LogInfo($"=== INICIANDO ACTUALIZACIÓN DE EMPLEADO {empleado.IdEmpleado} ===");

                // Validar datos del empleado
                if (!ValidarEmpleado(empleado, out errorMessage))
                {
                    Logger.LogError($"VALIDACIÓN EMPLEADO FALLIDA: {errorMessage}");
                    return false;
                }

                // Validar que no exista otro empleado con el mismo documento
                var empleadoExistente = _empleadoRepository.ObtenerPorDocumento(empleado.TipoDocumento, empleado.NumeroDocumento);
                if (empleadoExistente != null && empleadoExistente.IdEmpleado != empleado.IdEmpleado)
                {
                    errorMessage = $"Ya existe otro empleado con el documento {empleado.TipoDocumento} {empleado.NumeroDocumento}.";
                    Logger.LogError($"DOCUMENTO DUPLICADO EN ACTUALIZACIÓN: {errorMessage}");
                    return false;
                }

                // Determinar si se debe gestionar usuario
                bool gestionarUsuario = !string.IsNullOrEmpty(nombreUsuario) && nivelUsuario > 0;

                if (gestionarUsuario)
                {
                    if (!ValidarDatosUsuario(nombreUsuario, nivelUsuario, out errorMessage, empleado.IdUsuario))
                    {
                        Logger.LogError($"VALIDACIÓN USUARIO FALLIDA EN ACTUALIZACIÓN: {errorMessage}");
                        return false;
                    }
                }

                // Actualizar el empleado
                bool resultado = _empleadoRepository.Actualizar(empleado);

                if (!resultado)
                {
                    errorMessage = "No se pudo actualizar el empleado en la base de datos.";
                    Logger.LogError($"ERROR AL ACTUALIZAR EMPLEADO {empleado.IdEmpleado}");
                    return false;
                }

                Logger.LogInfo($"✓ Empleado actualizado exitosamente: {empleado.Nombre} {empleado.Apellido} (ID: {empleado.IdEmpleado})");
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"EXCEPCIÓN al actualizar empleado {empleado.IdEmpleado}");
                return false;
            }
        }

        /// <summary>
        /// Elimina un empleado (marca como inactivo).
        /// </summary>
        /// <param name="idEmpleado">ID del empleado a eliminar.</param>
        /// <param name="errorMessage">Mensaje de error si la eliminación falla.</param>
        /// <returns>True si la eliminación es exitosa, False en caso contrario.</returns>
        public bool EliminarEmpleado(int idEmpleado, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Obtener el empleado para registrar en el log
                Empleado empleado = _empleadoRepository.ObtenerPorId(idEmpleado);

                if (empleado == null)
                {
                    errorMessage = "No se encontró el empleado especificado.";
                    return false;
                }

                // Eliminar el empleado (marcar como inactivo)
                bool resultado = _empleadoRepository.Eliminar(idEmpleado);

                if (resultado)
                {
                    Logger.LogInfo($"Empleado eliminado (marcado como inactivo) exitosamente: {empleado.Nombre} {empleado.Apellido} (ID: {idEmpleado})");
                }
                else
                {
                    errorMessage = "No se pudo eliminar el empleado.";
                    Logger.LogError($"Error al eliminar empleado {empleado.Nombre} {empleado.Apellido} (ID: {idEmpleado})");
                }

                return resultado;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al eliminar empleado {idEmpleado}");
                return false;
            }
        }

        /// <summary>
        /// Valida los datos de un empleado.
        /// </summary>
        /// <param name="empleado">Empleado a validar.</param>
        /// <param name="errorMessage">Mensaje de error si la validación falla.</param>
        /// <returns>True si el empleado es válido, False en caso contrario.</returns>
        private bool ValidarEmpleado(Empleado empleado, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Validar nombre
            if (string.IsNullOrWhiteSpace(empleado.Nombre))
            {
                errorMessage = "El nombre del empleado es obligatorio.";
                return false;
            }

            if (empleado.Nombre.Length > 100)
            {
                errorMessage = "El nombre del empleado no puede exceder los 100 caracteres.";
                return false;
            }

            // Validar apellido
            if (string.IsNullOrWhiteSpace(empleado.Apellido))
            {
                errorMessage = "El apellido del empleado es obligatorio.";
                return false;
            }

            if (empleado.Apellido.Length > 100)
            {
                errorMessage = "El apellido del empleado no puede exceder los 100 caracteres.";
                return false;
            }

            // Validar tipo de documento
            if (string.IsNullOrWhiteSpace(empleado.TipoDocumento))
            {
                errorMessage = "El tipo de documento es obligatorio.";
                return false;
            }

            // Validar número de documento
            if (string.IsNullOrWhiteSpace(empleado.NumeroDocumento))
            {
                errorMessage = "El número de documento es obligatorio.";
                return false;
            }

            if (empleado.NumeroDocumento.Length > 20)
            {
                errorMessage = "El número de documento no puede exceder los 20 caracteres.";
                return false;
            }

            // Validar teléfono
            if (string.IsNullOrWhiteSpace(empleado.Telefono))
            {
                errorMessage = "El teléfono del empleado es obligatorio.";
                return false;
            }

            if (empleado.Telefono.Length > 20)
            {
                errorMessage = "El teléfono no puede exceder los 20 caracteres.";
                return false;
            }

            // Validar fecha de contratación
            if (empleado.FechaContratacion == DateTime.MinValue)
            {
                errorMessage = "La fecha de contratación es obligatoria.";
                return false;
            }

            if (empleado.FechaContratacion > DateTime.Now)
            {
                errorMessage = "La fecha de contratación no puede ser futura.";
                return false;
            }

            // Validar salario base
            if (empleado.SalarioBase <= 0)
            {
                errorMessage = "El salario base debe ser mayor que cero.";
                return false;
            }

            if (empleado.SalarioBase > 1000000)
            {
                errorMessage = "El salario base parece excesivamente alto. Por favor, verifique el valor.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida los datos del usuario a crear/actualizar.
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario.</param>
        /// <param name="nivelUsuario">Nivel de usuario.</param>
        /// <param name="errorMessage">Mensaje de error si la validación falla.</param>
        /// <param name="idUsuarioActual">ID del usuario actual (para validaciones en modo edición).</param>
        /// <returns>True si los datos son válidos, False en caso contrario.</returns>
        private bool ValidarDatosUsuario(string nombreUsuario, int nivelUsuario, out string errorMessage, int idUsuarioActual = 0)
        {
            errorMessage = string.Empty;

            Logger.LogInfo($"Validando datos de usuario: {nombreUsuario}, nivel: {nivelUsuario}");

            // Validar nombre de usuario
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                errorMessage = "El nombre de usuario es obligatorio.";
                return false;
            }

            if (nombreUsuario.Length < 3 || nombreUsuario.Length > 50)
            {
                errorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres.";
                return false;
            }

            // Validar formato del nombre de usuario
            if (!Regex.IsMatch(nombreUsuario, @"^[a-zA-Z0-9_]+$"))
            {
                errorMessage = "El nombre de usuario solo puede contener letras, números y guiones bajos.";
                return false;
            }

            // Validar que no inicie con número
            if (char.IsDigit(nombreUsuario[0]))
            {
                errorMessage = "El nombre de usuario no puede iniciar con un número.";
                return false;
            }

            // Validar nivel de usuario
            if (nivelUsuario < 1 || nivelUsuario > 3)
            {
                errorMessage = "El nivel de usuario debe ser 1 (Administrador), 2 (Paramétrico) o 3 (Esporádico).";
                return false;
            }

            // Validar que no se cree más de un administrador
            if (nivelUsuario == 1)
            {
                Logger.LogInfo("Validando restricción de administrador único...");
                bool existeAdmin = _authService.ExisteAdministrador();
                Logger.LogInfo($"¿Existe administrador? {existeAdmin}");

                if (existeAdmin)
                {
                    // Si estamos editando, verificar que el usuario actual sea el administrador
                    if (idUsuarioActual > 0)
                    {
                        var usuarioActual = _authService.ObtenerPorId(idUsuarioActual);
                        if (usuarioActual == null || usuarioActual.Nivel != 1)
                        {
                            errorMessage = "Solo puede existir un usuario administrador en el sistema.";
                            Logger.LogError($"Intento de crear segundo admin: usuario actual {idUsuarioActual} no es admin");
                            return false;
                        }
                    }
                    else
                    {
                        errorMessage = "Solo puede existir un usuario administrador en el sistema.";
                        Logger.LogError("Intento de crear segundo administrador");
                        return false;
                    }
                }
            }

            // Validar que el nombre de usuario no exista
            Logger.LogInfo($"Verificando unicidad del nombre de usuario: {nombreUsuario}");
            bool existeNombre = _authService.ExisteNombreUsuario(nombreUsuario);
            Logger.LogInfo($"¿Existe nombre de usuario? {existeNombre}");

            if (idUsuarioActual == 0) // Modo creación
            {
                if (existeNombre)
                {
                    errorMessage = "El nombre de usuario ya existe en el sistema.";
                    Logger.LogError($"Nombre de usuario duplicado: {nombreUsuario}");
                    return false;
                }
            }
            else // Modo edición
            {
                var usuarioActual = _authService.ObtenerPorId(idUsuarioActual);
                if (usuarioActual != null && usuarioActual.NombreUsuario != nombreUsuario)
                {
                    if (existeNombre)
                    {
                        errorMessage = "El nombre de usuario ya existe en el sistema.";
                        Logger.LogError($"Nombre de usuario duplicado en edición: {nombreUsuario}");
                        return false;
                    }
                }
            }

            Logger.LogInfo("✓ Validación de datos de usuario exitosa");
            return true;
        }

        /// <summary>
        /// Genera una clave inicial para el usuario basada en su información.
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario.</param>
        /// <param name="numeroDocumento">Número de documento del empleado.</param>
        /// <returns>Clave inicial generada.</returns>
        private string GenerarClaveInicial(string nombreUsuario, string numeroDocumento)
        {
            try
            {
                // Tomar las primeras 3 letras del nombre de usuario
                string inicialUsuario = nombreUsuario.Length >= 3
                    ? nombreUsuario.Substring(0, 3)
                    : nombreUsuario.PadRight(3, '0');

                // Tomar los últimos 3 dígitos del documento
                string inicialDocumento = numeroDocumento.Length >= 3
                    ? numeroDocumento.Substring(numeroDocumento.Length - 3)
                    : numeroDocumento.PadLeft(3, '0');

                // Combinar con números adicionales para asegurar complejidad
                string claveGenerada = $"{inicialUsuario}{inicialDocumento}123";
                Logger.LogInfo($"Clave generada: {claveGenerada} (de usuario: {nombreUsuario}, doc: {numeroDocumento})");
                return claveGenerada;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al generar clave inicial");
                // Clave por defecto en caso de error
                return "Temp123456";
            }
        }
    }
}