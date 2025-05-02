using ElectroTech.DataAccess;
using ElectroTech.Helpers;
using ElectroTech.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ElectroTech.Services
{
    /// <summary>
    /// Servicio para la gestión de proveedores.
    /// </summary>
    public class ProveedorService
    {
        private readonly ProveedorRepository _proveedorRepository;

        /// <summary>
        /// Constructor del servicio de proveedores.
        /// </summary>
        public ProveedorService()
        {
            _proveedorRepository = new ProveedorRepository();
        }

        /// <summary>
        /// Obtiene todos los proveedores activos.
        /// </summary>
        /// <returns>Lista de proveedores activos.</returns>
        public List<Proveedor> ObtenerTodos()
        {
            try
            {
                return _proveedorRepository.ObtenerTodos();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener todos los proveedores");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un proveedor por su ID.
        /// </summary>
        /// <param name="idProveedor">ID del proveedor.</param>
        /// <returns>El proveedor si se encuentra, null en caso contrario.</returns>
        public Proveedor ObtenerPorId(int idProveedor)
        {
            try
            {
                return _proveedorRepository.ObtenerPorId(idProveedor);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener proveedor con ID {idProveedor}");
                throw;
            }
        }

        /// <summary>
        /// Busca proveedores según un término de búsqueda.
        /// </summary>
        /// <param name="termino">Término de búsqueda.</param>
        /// <returns>Lista de proveedores que coinciden con el término.</returns>
        public List<Proveedor> Buscar(string termino)
        {
            try
            {
                return _proveedorRepository.Buscar(termino);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al buscar proveedores con término '{termino}'");
                throw;
            }
        }

        /// <summary>
        /// Crea un nuevo proveedor.
        /// </summary>
        /// <param name="proveedor">Proveedor a crear.</param>
        /// <param name="errorMessage">Mensaje de error si la creación falla.</param>
        /// <returns>ID del proveedor creado o -1 si falla.</returns>
        public int CrearProveedor(Proveedor proveedor, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Validar datos del proveedor
                if (!ValidarProveedor(proveedor, out errorMessage))
                {
                    return -1;
                }

                // Crear el proveedor
                int idProveedor = _proveedorRepository.Crear(proveedor);
                Logger.LogInfo($"Proveedor creado exitosamente: {proveedor.Nombre} (ID: {idProveedor})");
                return idProveedor;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al crear proveedor {proveedor.Nombre}");
                return -1;
            }
        }

        /// <summary>
        /// Actualiza un proveedor existente.
        /// </summary>
        /// <param name="proveedor">Proveedor con los datos actualizados.</param>
        /// <param name="errorMessage">Mensaje de error si la actualización falla.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool ActualizarProveedor(Proveedor proveedor, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Validar datos del proveedor
                if (!ValidarProveedor(proveedor, out errorMessage))
                {
                    return false;
                }

                // Actualizar el proveedor
                bool resultado = _proveedorRepository.Actualizar(proveedor);

                if (resultado)
                {
                    Logger.LogInfo($"Proveedor actualizado exitosamente: {proveedor.Nombre} (ID: {proveedor.IdProveedor})");
                }
                else
                {
                    errorMessage = "No se pudo actualizar el proveedor en la base de datos.";
                    Logger.LogError($"Error al actualizar proveedor {proveedor.Nombre} (ID: {proveedor.IdProveedor})");
                }

                return resultado;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al actualizar proveedor {proveedor.Nombre} (ID: {proveedor.IdProveedor})");
                return false;
            }
        }

        /// <summary>
        /// Elimina un proveedor (marca como inactivo).
        /// </summary>
        /// <param name="idProveedor">ID del proveedor a eliminar.</param>
        /// <param name="errorMessage">Mensaje de error si la eliminación falla.</param>
        /// <returns>True si la eliminación es exitosa, False en caso contrario.</returns>
        public bool EliminarProveedor(int idProveedor, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Obtener el proveedor para registrar en el log
                Proveedor proveedor = _proveedorRepository.ObtenerPorId(idProveedor);

                if (proveedor == null)
                {
                    errorMessage = "No se encontró el proveedor especificado.";
                    return false;
                }

                // Eliminar el proveedor
                bool resultado = _proveedorRepository.Eliminar(idProveedor);

                if (resultado)
                {
                    Logger.LogInfo($"Proveedor eliminado (marcado como inactivo) exitosamente: {proveedor.Nombre} (ID: {idProveedor})");
                }
                else
                {
                    errorMessage = "No se pudo eliminar el proveedor.";
                    Logger.LogError($"Error al eliminar proveedor {proveedor.Nombre} (ID: {idProveedor})");
                }

                return resultado;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al eliminar proveedor {idProveedor}");
                return false;
            }
        }

        /// <summary>
        /// Valida los datos de un proveedor.
        /// </summary>
        /// <param name="proveedor">Proveedor a validar.</param>
        /// <param name="errorMessage">Mensaje de error si la validación falla.</param>
        /// <returns>True si el proveedor es válido, False en caso contrario.</returns>
        private bool ValidarProveedor(Proveedor proveedor, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Validar nombre
            if (string.IsNullOrWhiteSpace(proveedor.Nombre))
            {
                errorMessage = "El nombre del proveedor es obligatorio.";
                return false;
            }

            // Validar longitud máxima del nombre
            if (proveedor.Nombre.Length > 100)
            {
                errorMessage = "El nombre del proveedor no puede exceder los 100 caracteres.";
                return false;
            }

            // Validar formato de correo electrónico
            if (!string.IsNullOrEmpty(proveedor.Correo) && !EsCorreoElectronicoValido(proveedor.Correo))
            {
                errorMessage = "El formato del correo electrónico no es válido.";
                return false;
            }

            // Validar longitud máxima del teléfono
            if (!string.IsNullOrEmpty(proveedor.Telefono) && proveedor.Telefono.Length > 20)
            {
                errorMessage = "El teléfono no puede exceder los 20 caracteres.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Verifica si un correo electrónico tiene un formato válido.
        /// </summary>
        /// <param name="correo">Correo electrónico a verificar.</param>
        /// <returns>True si el formato es válido, False en caso contrario.</returns>
        private bool EsCorreoElectronicoValido(string correo)
        {
            try
            {
                // Expresión regular para validar correo electrónico
                string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                return Regex.IsMatch(correo, pattern);
            }
            catch
            {
                return false;
            }
        }
    }
}