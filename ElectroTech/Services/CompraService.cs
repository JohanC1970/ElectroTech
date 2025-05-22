using ElectroTech.DataAccess;
using ElectroTech.Helpers;
using ElectroTech.Models;
using System;
using System.Collections.Generic;

namespace ElectroTech.Services
{
    /// <summary>
    /// Servicio para la gestión de compras.
    /// </summary>
    public class CompraService
    {
        private readonly CompraRepository _compraRepository;
        private readonly ProductoRepository _productoRepository;

        /// <summary>
        /// Constructor del servicio de compras.
        /// </summary>
        public CompraService()
        {
            _compraRepository = new CompraRepository();
            _productoRepository = new ProductoRepository();
        }

        /// <summary>
        /// Obtiene todas las compras.
        /// </summary>
        /// <returns>Lista de compras.</returns>
        public List<Compra> ObtenerTodas()
        {
            try
            {
                return _compraRepository.ObtenerTodas();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener todas las compras");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una compra por su ID.
        /// </summary>
        /// <param name="idCompra">ID de la compra.</param>
        /// <returns>La compra si se encuentra, null en caso contrario.</returns>
        public Compra ObtenerPorId(int idCompra)
        {
            try
            {
                return _compraRepository.ObtenerPorId(idCompra);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener compra con ID {idCompra}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una compra por su número de orden.
        /// </summary>
        /// <param name="numeroOrden">Número de orden de la compra.</param>
        /// <returns>La compra si se encuentra, null en caso contrario.</returns>
        public Compra ObtenerPorNumeroOrden(string numeroOrden)
        {
            try
            {
                return _compraRepository.ObtenerPorNumeroOrden(numeroOrden);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener compra con número de orden {numeroOrden}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene compras por estado.
        /// </summary>
        /// <param name="estado">Estado de la compra (P: Pendiente, R: Recibida, C: Cancelada).</param>
        /// <returns>Lista de compras con el estado especificado.</returns>
        public List<Compra> ObtenerPorEstado(char estado)
        {
            try
            {
                return _compraRepository.ObtenerPorEstado(estado);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener compras con estado {estado}");
                throw;
            }
        }

        /// <summary>
        /// Busca compras según un término de búsqueda.
        /// </summary>
        /// <param name="termino">Término de búsqueda.</param>
        /// <returns>Lista de compras que coinciden con el término.</returns>
        public List<Compra> Buscar(string termino)
        {
            try
            {
                return _compraRepository.Buscar(termino);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al buscar compras con término '{termino}'");
                throw;
            }
        }

        /// <summary>
        /// Crea una nueva compra.
        /// </summary>
        /// <param name="compra">Compra a crear.</param>
        /// <param name="errorMessage">Mensaje de error si la creación falla.</param>
        /// <returns>ID de la compra creada o -1 si falla.</returns>
        public int CrearCompra(Compra compra, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Validar datos de la compra
                if (!ValidarCompra(compra, out errorMessage))
                {
                    return -1;
                }

                // Crear la compra
                int idCompra = _compraRepository.Crear(compra);
                Logger.LogInfo($"Compra creada exitosamente: {compra.NumeroOrden} (ID: {idCompra})");
                return idCompra;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al crear compra {compra.NumeroOrden}");
                return -1;
            }
        }

        /// <summary>
        /// Actualiza una compra existente.
        /// </summary>
        /// <param name="compra">Compra con los datos actualizados.</param>
        /// <param name="errorMessage">Mensaje de error si la actualización falla.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool ActualizarCompra(Compra compra, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Validar datos de la compra
                if (!ValidarCompra(compra, out errorMessage))
                {
                    return false;
                }

                // Actualizar la compra
                bool resultado = _compraRepository.Actualizar(compra);

                if (resultado)
                {
                    Logger.LogInfo($"Compra actualizada exitosamente: {compra.NumeroOrden} (ID: {compra.IdCompra})");
                }
                else
                {
                    errorMessage = "No se pudo actualizar la compra en la base de datos.";
                    Logger.LogError($"Error al actualizar compra {compra.NumeroOrden} (ID: {compra.IdCompra})");
                }

                return resultado;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al actualizar compra {compra.NumeroOrden} (ID: {compra.IdCompra})");
                return false;
            }
        }

        /// <summary>
        /// Recibe una compra (cambia el estado a Recibido y actualiza el inventario).
        /// </summary>
        /// <param name="idCompra">ID de la compra a recibir.</param>
        /// <param name="errorMessage">Mensaje de error si la recepción falla.</param>
        /// <returns>True si la recepción es exitosa, False en caso contrario.</returns>
        public bool RecibirCompra(int idCompra, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Recibir la compra
                bool resultado = _compraRepository.RecibirCompra(idCompra);

                if (resultado)
                {
                    Compra compra = _compraRepository.ObtenerPorId(idCompra);
                    Logger.LogInfo($"Compra recibida exitosamente: {compra.NumeroOrden} (ID: {idCompra})");
                }
                else
                {
                    errorMessage = "No se pudo recibir la compra en la base de datos.";
                    Logger.LogError($"Error al recibir compra (ID: {idCompra})");
                }

                return resultado;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al recibir compra (ID: {idCompra})");
                return false;
            }
        }

        /// <summary>
        /// Cancela una compra (cambia el estado a Cancelado).
        /// </summary>
        /// <param name="idCompra">ID de la compra a cancelar.</param>
        /// <param name="errorMessage">Mensaje de error si la cancelación falla.</param>
        /// <returns>True si la cancelación es exitosa, False en caso contrario.</returns>
        public bool CancelarCompra(int idCompra, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Cancelar la compra
                bool resultado = _compraRepository.CancelarCompra(idCompra);

                if (resultado)
                {
                    Compra compra = _compraRepository.ObtenerPorId(idCompra);
                    Logger.LogInfo($"Compra cancelada exitosamente: {compra.NumeroOrden} (ID: {idCompra})");
                }
                else
                {
                    errorMessage = "No se pudo cancelar la compra en la base de datos.";
                    Logger.LogError($"Error al cancelar compra (ID: {idCompra})");
                }

                return resultado;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al cancelar compra (ID: {idCompra})");
                return false;
            }
        }

        /// <summary>
        /// Obtiene el consecutivo para un nuevo número de orden.
        /// </summary>
        /// <returns>Número consecutivo.</returns>
        public int ObtenerConsecutivoOrden()
        {
            try
            {
                return _compraRepository.ObtenerConsecutivoOrden();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener consecutivo de orden");
                throw;
            }
        }

        /// <summary>
        /// Valida los datos de una compra.
        /// </summary>
        /// <param name="compra">Compra a validar.</param>
        /// <param name="errorMessage">Mensaje de error si la validación falla.</param>
        /// <returns>True si la compra es válida, False en caso contrario.</returns>
        private bool ValidarCompra(Compra compra, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Validar número de orden
            if (string.IsNullOrWhiteSpace(compra.NumeroOrden))
            {
                errorMessage = "El número de orden es obligatorio.";
                return false;
            }

            // Validar fecha
            if (compra.Fecha == DateTime.MinValue)
            {
                errorMessage = "La fecha de la compra es obligatoria.";
                return false;
            }

            // Validar proveedor
            if (compra.IdProveedor <= 0)
            {
                errorMessage = "Debe seleccionar un proveedor para la compra.";
                return false;
            }

            // Validar estado
            if (compra.Estado != 'P' && compra.Estado != 'R' && compra.Estado != 'C')
            {
                errorMessage = "El estado de la compra debe ser Pendiente (P), Recibida (R) o Cancelada (C).";
                return false;
            }

            // Validar detalles
            if (compra.Detalles == null || compra.Detalles.Count == 0)
            {
                errorMessage = "La compra debe tener al menos un producto.";
                return false;
            }

            foreach (var detalle in compra.Detalles)
            {
                // Validar producto
                if (detalle.IdProducto <= 0)
                {
                    errorMessage = "Todos los detalles deben tener un producto válido.";
                    return false;
                }

                // Validar cantidad
                if (detalle.Cantidad <= 0)
                {
                    errorMessage = "La cantidad de cada producto debe ser mayor que cero.";
                    return false;
                }

                // Validar precio unitario
                if (detalle.PrecioUnitario <= 0)
                {
                    errorMessage = "El precio unitario de cada producto debe ser mayor que cero.";
                    return false;
                }

                // Validar subtotal
                double subtotalCalculado = detalle.Cantidad * detalle.PrecioUnitario;
                if (Math.Abs(detalle.Subtotal - subtotalCalculado) > 0.01)
                {
                    errorMessage = $"El subtotal del producto {detalle.Producto?.Nombre ?? detalle.IdProducto.ToString()} no coincide con la cantidad por el precio unitario.";
                    return false;
                }
            }

            // Validar totales
            double subtotalTotal = 0;
            foreach (var detalle in compra.Detalles)
            {
                subtotalTotal += detalle.Subtotal;
            }

            if (Math.Abs(compra.Subtotal - subtotalTotal) > 0.01)
            {
                errorMessage = "El subtotal de la compra no coincide con la suma de los subtotales de los productos.";
                return false;
            }

            double totalCalculado = compra.Subtotal + compra.Impuestos;
            if (Math.Abs(compra.Total - totalCalculado) > 0.01)
            {
                errorMessage = "El total de la compra no coincide con el subtotal más los impuestos.";
                return false;
            }

            return true;
        }
    }
}