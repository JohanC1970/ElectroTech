using ElectroTech.DataAccess;
using ElectroTech.Helpers;
using ElectroTech.Models;
using System;
using System.Collections.Generic;

namespace ElectroTech.Services
{
    /// <summary>
    /// Servicio para la gestión de empleados.
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
                // Validar datos del empleado
                if (!ValidarEmpleado(empleado, out errorMessage))
                {
                    return false;
                }

                // Crear primero el empleado sin usuario asociado
                int idEmpleado = _empleadoRepository.Crear(empleado);

                if (idEmpleado <= 0)
                {
                    errorMessage = "No se pudo crear el empleado en la base de datos.";
                    Logger.LogError($"Error al crear empleado {empleado.Nombre} {empleado.Apellido}");
                    return false;
                }

                // Actualizar el ID del empleado
                empleado.IdEmpleado = idEmpleado;

                // Si se debe crear un usuario asociado
                if (!string.IsNullOrEmpty(nombreUsuario) && nivelUsuario > 0)
                {
                    try
                    {
                        // Generar contraseña inicial (combinación de nombre y documento)
                        string claveInicial = $"{nombreUsuario.Substring(0, Math.Min(3, nombreUsuario.Length))}{empleado.NumeroDocumento.Substring(0, Math.Min(3, empleado.NumeroDocumento.Length))}123";

                        // Crear usuario
                        var usuario = new Usuario
                        {
                            NombreUsuario = nombreUsuario,
                            Nivel = nivelUsuario,
                            NombreCompleto = empleado.NombreCompleto,
                            Correo = "correo@ejemplo.com", // Idealmente se pediría el correo en el formulario
                            Estado = 'A' // Activo
                        };

                        if (_authService.CrearUsuario(usuario, claveInicial, out string errorUsuario))
                        {
                            // Asociar usuario al empleado
                            if (!_empleadoRepository.ActualizarIdUsuario(idEmpleado, usuario.IdUsuario))
                            {
                                errorMessage = "Se creó el empleado pero no se pudo asociar el usuario.";
                                return false;
                            }

                            // Actualizar el ID de usuario en el objeto empleado
                            empleado.IdUsuario = usuario.IdUsuario;
                        }
                        else
                        {
                            errorMessage = $"Se creó el empleado pero no se pudo crear el usuario: {errorUsuario}";
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMessage = "Se creó el empleado pero ocurrió un error al crear el usuario asociado.";
                        Logger.LogException(ex, $"Error al crear usuario para el empleado {idEmpleado}");
                        return false;
                    }
                }

                Logger.LogInfo($"Empleado creado exitosamente: {empleado.Nombre} {empleado.Apellido} (ID: {idEmpleado})");
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al crear empleado {empleado.Nombre} {empleado.Apellido}");
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
                // Validar datos del empleado
                if (!ValidarEmpleado(empleado, out errorMessage))
                {
                    return false;
                }

                // Actualizar el empleado
                bool resultado = _empleadoRepository.Actualizar(empleado);

                if (!resultado)
                {
                    errorMessage = "No se pudo actualizar el empleado en la base de datos.";
                    Logger.LogError($"Error al actualizar empleado {empleado.Nombre} {empleado.Apellido} (ID: {empleado.IdEmpleado})");
                    return false;
                }

                // Si se debe actualizar o crear el usuario asociado
                if (!string.IsNullOrEmpty(nombreUsuario) && nivelUsuario > 0)
                {
                    try
                    {
                        // Verificar si ya tiene un usuario asociado
                        if (empleado.IdUsuario > 0)
                        {
                            // Obtener el usuario actual
                            var usuario = _authService.ObtenerPorId(empleado.IdUsuario);

                            if (usuario != null)
                            {
                                // Actualizar datos del usuario
                                usuario.NombreUsuario = nombreUsuario;
                                usuario.Nivel = nivelUsuario;
                                usuario.NombreCompleto = empleado.NombreCompleto;

                                string errorUsuario;
                                if (!_authService.ActualizarUsuario(usuario, out errorUsuario))
                                {
                                    errorMessage = $"Se actualizó el empleado pero no se pudo actualizar el usuario: {errorUsuario}";
                                    return false;
                                }
                            }
                            else
                            {
                                errorMessage = "Se actualizó el empleado pero no se encontró el usuario asociado.";
                                return false;
                            }
                        }
                        else
                        {
                            // Crear un nuevo usuario y asociarlo
                            // Generar contraseña inicial
                            string claveInicial = $"{nombreUsuario.Substring(0, Math.Min(3, nombreUsuario.Length))}{empleado.NumeroDocumento.Substring(0, Math.Min(3, empleado.NumeroDocumento.Length))}123";

                            // Crear usuario
                            var usuario = new Usuario
                            {
                                NombreUsuario = nombreUsuario,
                                Nivel = nivelUsuario,
                                NombreCompleto = empleado.NombreCompleto,
                                Correo = "correo@ejemplo.com", // Idealmente se pediría el correo en el formulario
                                Estado = 'A' // Activo
                            };

                            string errorUsuario;
                            if (_authService.CrearUsuario(usuario, claveInicial, out errorUsuario))
                            {
                                // Asociar usuario al empleado
                                if (!_empleadoRepository.ActualizarIdUsuario(empleado.IdEmpleado, usuario.IdUsuario))
                                {
                                    errorMessage = "Se actualizó el empleado pero no se pudo asociar el nuevo usuario.";
                                    return false;
                                }

                                // Actualizar el ID de usuario en el objeto empleado
                                empleado.IdUsuario = usuario.IdUsuario;
                            }
                            else
                            {
                                errorMessage = $"Se actualizó el empleado pero no se pudo crear el usuario: {errorUsuario}";
                                return false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMessage = "Se actualizó el empleado pero ocurrió un error al actualizar el usuario asociado.";
                        Logger.LogException(ex, $"Error al actualizar usuario del empleado {empleado.IdEmpleado}");
                        return false;
                    }
                }

                Logger.LogInfo($"Empleado actualizado exitosamente: {empleado.Nombre} {empleado.Apellido} (ID: {empleado.IdEmpleado})");
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al actualizar empleado {empleado.Nombre} {empleado.Apellido} (ID: {empleado.IdEmpleado})");
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

                // Verificar si tiene ventas asociadas (simulado por ahora)
                // TODO: Implementar verificación real
                bool tieneVentasAsociadas = false;

                if (tieneVentasAsociadas)
                {
                    errorMessage = "No se puede eliminar el empleado porque tiene ventas asociadas.";
                    return false;
                }

                // Eliminar el empleado
                bool resultado = _empleadoRepository.Eliminar(idEmpleado);

                if (resultado)
                {
                    // Si tiene usuario asociado, también inactivarlo
                    if (empleado.IdUsuario > 0)
                    {
                        try
                        {
                            var usuario = _authService.ObtenerPorId(empleado.IdUsuario);
                            if (usuario != null)
                            {
                                usuario.Estado = 'I'; // Inactivo
                                string errorUsuario;
                                _authService.ActualizarUsuario(usuario, out errorUsuario);
                                // No fallamos la operación si no se puede actualizar el usuario
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogException(ex, $"Error al inactivar usuario del empleado {idEmpleado}");
                            // No fallamos la operación si no se puede actualizar el usuario
                        }
                    }

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

            // Validar apellido
            if (string.IsNullOrWhiteSpace(empleado.Apellido))
            {
                errorMessage = "El apellido del empleado es obligatorio.";
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

            // Validar teléfono
            if (string.IsNullOrWhiteSpace(empleado.Telefono))
            {
                errorMessage = "El teléfono del empleado es obligatorio.";
                return false;
            }

            // Validar fecha de contratación
            if (empleado.FechaContratacion == DateTime.MinValue)
            {
                errorMessage = "La fecha de contratación es obligatoria.";
                return false;
            }

            // Validar salario base
            if (empleado.SalarioBase <= 0)
            {
                errorMessage = "El salario base debe ser mayor que cero.";
                return false;
            }

            return true;
        }
    }
}
