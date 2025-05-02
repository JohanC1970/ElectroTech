using ElectroTech.DataAccess;
using ElectroTech.Helpers;
using ElectroTech.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ElectroTech.Services
{
    /// <summary>
    /// Servicio para la gestión de clientes.
    /// </summary>
    public class ClienteService
    {
        private readonly ClienteRepository _clienteRepository;

        /// <summary>
        /// Constructor del servicio de clientes.
        /// </summary>
        public ClienteService()
        {
            _clienteRepository = new ClienteRepository();
        }

        /// <summary>
        /// Obtiene todos los clientes activos.
        /// </summary>
        /// <returns>Lista de clientes activos.</returns>
        public List<Cliente> ObtenerTodos()
        {
            try
            {
                return _clienteRepository.ObtenerTodos();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener todos los clientes");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un cliente por su ID.
        /// </summary>
        /// <param name="idCliente">ID del cliente.</param>
        /// <returns>El cliente si se encuentra, null en caso contrario.</returns>
        public Cliente ObtenerPorId(int idCliente)
        {
            try
            {
                return _clienteRepository.ObtenerPorId(idCliente);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener cliente con ID {idCliente}");
                throw;
            }
        }

        /// <summary>
        /// Busca clientes según un término de búsqueda.
        /// </summary>
        /// <param name="termino">Término de búsqueda.</param>
        /// <returns>Lista de clientes que coinciden con el término.</returns>
        public List<Cliente> Buscar(string termino)
        {
            try
            {
                return _clienteRepository.Buscar(termino);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al buscar clientes con término '{termino}'");
                throw;
            }
        }

        /// <summary>
        /// Crea un nuevo cliente.
        /// </summary>
        /// <param name="cliente">Cliente a crear.</param>
        /// <param name="errorMessage">Mensaje de error si la creación falla.</param>
        /// <returns>ID del cliente creado o -1 si falla.</returns>
        public int CrearCliente(Cliente cliente, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Validar datos del cliente
                if (!ValidarCliente(cliente, out errorMessage))
                {
                    return -1;
                }

                // Asignar fecha de registro
                cliente.FechaRegistro = DateTime.Now;

                // Crear el cliente
                int idCliente = _clienteRepository.Crear(cliente);
                Logger.LogInfo($"Cliente creado exitosamente: {cliente.NombreCompleto} (ID: {idCliente})");
                return idCliente;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al crear cliente {cliente.NombreCompleto}");
                return -1;
            }
        }

        /// <summary>
        /// Actualiza un cliente existente.
        /// </summary>
        /// <param name="cliente">Cliente con los datos actualizados.</param>
        /// <param name="errorMessage">Mensaje de error si la actualización falla.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool ActualizarCliente(Cliente cliente, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Validar datos del cliente
                if (!ValidarCliente(cliente, out errorMessage))
                {
                    return false;
                }

                // Actualizar el cliente
                bool resultado = _clienteRepository.Actualizar(cliente);

                if (resultado)
                {
                    Logger.LogInfo($"Cliente actualizado exitosamente: {cliente.NombreCompleto} (ID: {cliente.IdCliente})");
                }
                else
                {
                    errorMessage = "No se pudo actualizar el cliente en la base de datos.";
                    Logger.LogError($"Error al actualizar cliente {cliente.NombreCompleto} (ID: {cliente.IdCliente})");
                }

                return resultado;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al actualizar cliente {cliente.NombreCompleto} (ID: {cliente.IdCliente})");
                return false;
            }
        }

        /// <summary>
        /// Elimina un cliente (marca como inactivo).
        /// </summary>
        /// <param name="idCliente">ID del cliente a eliminar.</param>
        /// <param name="errorMessage">Mensaje de error si la eliminación falla.</param>
        /// <returns>True si la eliminación es exitosa, False en caso contrario.</returns>
        public bool EliminarCliente(int idCliente, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Obtener el cliente para registrar en el log
                Cliente cliente = _clienteRepository.ObtenerPorId(idCliente);

                if (cliente == null)
                {
                    errorMessage = "No se encontró el cliente especificado.";
                    return false;
                }

                // Eliminar el cliente
                bool resultado = _clienteRepository.Eliminar(idCliente);

                if (resultado)
                {
                    Logger.LogInfo($"Cliente eliminado (marcado como inactivo) exitosamente: {cliente.NombreCompleto} (ID: {idCliente})");
                }
                else
                {
                    errorMessage = "No se pudo eliminar el cliente.";
                    Logger.LogError($"Error al eliminar cliente {cliente.NombreCompleto} (ID: {idCliente})");
                }

                return resultado;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al eliminar cliente {idCliente}");
                return false;
            }
        }

        /// <summary>
        /// Valida los datos de un cliente.
        /// </summary>
        /// <param name="cliente">Cliente a validar.</param>
        /// <param name="errorMessage">Mensaje de error si la validación falla.</param>
        /// <returns>True si el cliente es válido, False en caso contrario.</returns>
        private bool ValidarCliente(Cliente cliente, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Validar tipo de documento
            if (string.IsNullOrWhiteSpace(cliente.TipoDocumento))
            {
                errorMessage = "El tipo de documento es obligatorio.";
                return false;
            }

            // Validar número de documento
            if (string.IsNullOrWhiteSpace(cliente.NumeroDocumento))
            {
                errorMessage = "El número de documento es obligatorio.";
                return false;
            }

            // Validar nombre
            if (string.IsNullOrWhiteSpace(cliente.Nombre))
            {
                errorMessage = "El nombre del cliente es obligatorio.";
                return false;
            }

            // Validar apellido
            if (string.IsNullOrWhiteSpace(cliente.Apellido))
            {
                errorMessage = "El apellido del cliente es obligatorio.";
                return false;
            }

            // Validar formato de correo electrónico
            if (!string.IsNullOrEmpty(cliente.Correo) && !EsCorreoElectronicoValido(cliente.Correo))
            {
                errorMessage = "El formato del correo electrónico no es válido.";
                return false;
            }

            // Validar longitud máxima del teléfono
            if (!string.IsNullOrEmpty(cliente.Telefono) && cliente.Telefono.Length > 20)
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