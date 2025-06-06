﻿using ElectroTech.DataAccess;
using ElectroTech.Helpers;
using ElectroTech.Models;
using System;
using System.Collections.Generic;

namespace ElectroTech.Services
{
    /// <summary>
    /// Servicio para la gestión de productos.
    /// </summary>
    public class ProductoService
    {
        private readonly ProductoRepository _productoRepository;

        /// <summary>
        /// Constructor del servicio de productos.
        /// </summary>
        public ProductoService()
        {
            _productoRepository = new ProductoRepository();
        }

        /// <summary>
        /// Obtiene todos los productos activos.
        /// </summary>
        /// <returns>Lista de productos activos.</returns>
        public List<Producto> ObtenerTodos()
        {
            try
            {
                return _productoRepository.ObtenerTodos();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener todos los productos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un producto por su ID.
        /// </summary>
        /// <param name="idProducto">ID del producto.</param>
        /// <returns>El producto si se encuentra, null en caso contrario.</returns>
        public Producto ObtenerPorId(int idProducto)
        {
            try
            {
                return _productoRepository.ObtenerPorId(idProducto);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener producto con ID {idProducto}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un producto por su código.
        /// </summary>
        /// <param name="codigo">Código del producto.</param>
        /// <returns>El producto si se encuentra, null en caso contrario.</returns>
        public Producto ObtenerPorCodigo(string codigo)
        {
            try
            {
                return _productoRepository.ObtenerPorCodigo(codigo);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener producto con código {codigo}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos por categoría.
        /// </summary>
        /// <param name="idCategoria">ID de la categoría.</param>
        /// <returns>Lista de productos de la categoría.</returns>
        public List<Producto> ObtenerPorCategoria(int idCategoria)
        {
            try
            {
                return _productoRepository.ObtenerPorCategoria(idCategoria);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener productos de la categoría {idCategoria}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene productos con stock bajo (menor al mínimo).
        /// </summary>
        /// <returns>Lista de productos con stock bajo.</returns>
        public List<Producto> ObtenerProductosBajoStock()
        {
            try
            {
                return _productoRepository.ObtenerProductosBajoStock();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener productos con stock bajo");
                throw;
            }
        }

        /// <summary>
        /// Obtiene la cantidad total de productos activos.
        /// </summary>
        /// <returns>Cantidad total de productos activos.</returns>
        public int ObtenerTotalProductos()
        {
            try
            {
                return _productoRepository.ObtenerTotalProductos();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener total de productos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene la cantidad de productos con stock bajo.
        /// </summary>
        /// <returns>Cantidad de productos con stock bajo.</returns>
        public int ObtenerTotalProductosBajoStock()
        {
            try
            {
                return _productoRepository.ObtenerTotalProductosBajoStock();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener total de productos con stock bajo");
                throw;
            }
        }

        /// <summary>
        /// Busca productos según un término de búsqueda.
        /// </summary>
        /// <param name="termino">Término de búsqueda.</param>
        /// <returns>Lista de productos que coinciden con el término.</returns>
        public List<Producto> Buscar(string termino)
        {
            try
            {
                return _productoRepository.Buscar(termino);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al buscar productos con término '{termino}'");
                throw;
            }
        }

        /// <summary>
        /// Crea un nuevo producto.
        /// </summary>
        /// <param name="producto">Producto a crear.</param>
        /// <param name="errorMessage">Mensaje de error si la creación falla.</param>
        /// <returns>ID del producto creado o -1 si falla.</returns>
        public int CrearProducto(Producto producto, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Validar datos del producto
                if (!ValidarProducto(producto, out errorMessage))
                {
                    return -1;
                }

                // Crear el producto
                int idProducto = _productoRepository.Crear(producto);
                Logger.LogInfo($"Producto creado exitosamente: {producto.Nombre} (ID: {idProducto})");
                return idProducto;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al crear producto {producto.Nombre}");
                return -1;
            }
        }

        /// <summary>
        /// Actualiza un producto existente.
        /// </summary>
        /// <param name="producto">Producto con los datos actualizados.</param>
        /// <param name="errorMessage">Mensaje de error si la actualización falla.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool ActualizarProducto(Producto producto, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Validar datos del producto
                if (!ValidarProducto(producto, out errorMessage))
                {
                    return false;
                }

                // Actualizar el producto
                bool resultado = _productoRepository.Actualizar(producto);

                if (resultado)
                {
                    Logger.LogInfo($"Producto actualizado exitosamente: {producto.Nombre} (ID: {producto.IdProducto})");
                }
                else
                {
                    errorMessage = "No se pudo actualizar el producto en la base de datos.";
                    Logger.LogError($"Error al actualizar producto {producto.Nombre} (ID: {producto.IdProducto})");
                }

                return resultado;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al actualizar producto {producto.Nombre} (ID: {producto.IdProducto})");
                return false;
            }
        }

        /// <summary>
        /// Actualiza el stock de un producto.
        /// </summary>
        /// <param name="idProducto">ID del producto.</param>
        /// <param name="cantidad">Cantidad a agregar (positiva) o restar (negativa).</param>
        /// <param name="tipoMovimiento">Tipo de movimiento (E: Entrada, S: Salida).</param>
        /// <param name="errorMessage">Mensaje de error si la actualización falla.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool ActualizarStock(int idProducto, int cantidad, char tipoMovimiento, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Validaciones de negocio
                if (!ValidarParametrosStock(idProducto, cantidad, tipoMovimiento, out errorMessage))
                {
                    return false;
                }

                // Verificar que el producto existe y está activo
                Producto producto = _productoRepository.ObtenerPorId(idProducto);
                if (producto == null)
                {
                    errorMessage = "El producto especificado no existe.";
                    Logger.LogWarning($"Intento de actualizar stock de producto inexistente: {idProducto}");
                    return false;
                }

                if (!producto.Activo)
                {
                    errorMessage = "No se puede actualizar el stock de un producto inactivo.";
                    Logger.LogWarning($"Intento de actualizar stock de producto inactivo: {idProducto}");
                    return false;
                }

                // Validaciones específicas para salidas
                if (tipoMovimiento == 'S')
                {
                    if (!ValidarSalidaStock(idProducto, cantidad, out errorMessage))
                    {
                        return false;
                    }
                }

                // Ejecutar la actualización en el repositorio
                bool resultado = _productoRepository.ActualizarStock(idProducto, cantidad, tipoMovimiento);

                if (resultado)
                {
                    Logger.LogInfo($"Stock actualizado exitosamente para el producto {producto.Nombre} (ID: {idProducto}). " +
                        $"Movimiento: {(tipoMovimiento == 'E' ? "Entrada" : "Salida")}, Cantidad: {cantidad}");
                }
                else
                {
                    errorMessage = "No se pudo actualizar el stock en la base de datos.";
                    Logger.LogError($"Fallo en actualización de stock para producto {idProducto}");
                }

                return resultado;
            }
            catch (DatabaseException dbEx)
            {
                errorMessage = dbEx.GetUserFriendlyMessage();
                Logger.LogException(dbEx, $"Error de base de datos al actualizar stock del producto {idProducto}");
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = "Error interno del sistema al actualizar el inventario.";
                Logger.LogException(ex, $"Error al actualizar stock del producto {idProducto}");
                return false;
            }
        }

        /// <summary>
        /// Valida los parámetros para la actualización de stock.
        /// </summary>
        /// <param name="idProducto">ID del producto.</param>
        /// <param name="cantidad">Cantidad a modificar.</param>
        /// <param name="tipoMovimiento">Tipo de movimiento.</param>
        /// <param name="errorMessage">Mensaje de error si la validación falla.</param>
        /// <returns>True si los parámetros son válidos, False en caso contrario.</returns>
        private bool ValidarParametrosStock(int idProducto, int cantidad, char tipoMovimiento, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (idProducto <= 0)
            {
                errorMessage = "El ID del producto debe ser válido.";
                return false;
            }

            if (cantidad <= 0)
            {
                errorMessage = "La cantidad debe ser mayor que cero.";
                return false;
            }

            if (tipoMovimiento != 'E' && tipoMovimiento != 'S')
            {
                errorMessage = "El tipo de movimiento debe ser 'E' (Entrada) o 'S' (Salida).";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida que se pueda realizar una salida de stock.
        /// </summary>
        /// <param name="idProducto">ID del producto.</param>
        /// <param name="cantidad">Cantidad a retirar.</param>
        /// <param name="errorMessage">Mensaje de error si la validación falla.</param>
        /// <returns>True si se puede realizar la salida, False en caso contrario.</returns>
        private bool ValidarSalidaStock(int idProducto, int cantidad, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Obtener stock actual
                int stockActual = _productoRepository.ObtenerStock(idProducto);

                if (stockActual == -1)
                {
                    errorMessage = "El producto no tiene un registro de inventario asociado.";
                    Logger.LogWarning($"Producto {idProducto} no tiene inventario");
                    return false;
                }

                if (stockActual < cantidad)
                {
                    errorMessage = $"No hay suficiente stock disponible. Stock actual: {stockActual}, Cantidad solicitada: {cantidad}";
                    Logger.LogWarning($"Stock insuficiente para producto {idProducto}. Disponible: {stockActual}, Requerido: {cantidad}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Error al validar el stock disponible.";
                Logger.LogException(ex, $"Error al validar stock para producto {idProducto}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene el stock actual de un producto.
        /// </summary>
        /// <param name="idProducto">ID del producto.</param>
        /// <returns>Cantidad actual en stock, o 0 si no hay inventario.</returns>
        public int ObtenerStockActual(int idProducto)
        {
            try
            {
                int stock = _productoRepository.ObtenerStock(idProducto);
                return stock == -1 ? 0 : stock;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener stock actual del producto {idProducto}");
                return 0;
            }
        }

        /// <summary>
        /// Verifica si un producto tiene suficiente stock para una cantidad específica.
        /// </summary>
        /// <param name="idProducto">ID del producto.</param>
        /// <param name="cantidadRequerida">Cantidad requerida.</param>
        /// <returns>True si hay suficiente stock, False en caso contrario.</returns>
        public bool TieneSuficienteStock(int idProducto, int cantidadRequerida)
        {
            try
            {
                int stockActual = ObtenerStockActual(idProducto);
                return stockActual >= cantidadRequerida;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al verificar stock suficiente para producto {idProducto}");
                return false;
            }
        }

        /// <summary>
        /// Elimina un producto (marca como inactivo).
        /// </summary>
        /// <param name="idProducto">ID del producto a eliminar.</param>
        /// <param name="errorMessage">Mensaje de error si la eliminación falla.</param>
        /// <returns>True si la eliminación es exitosa, False en caso contrario.</returns>
        public bool EliminarProducto(int idProducto, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Obtener el producto para registrar en el log
                Producto producto = _productoRepository.ObtenerPorId(idProducto);

                if (producto == null)
                {
                    errorMessage = "No se encontró el producto especificado.";
                    return false;
                }

                // Eliminar el producto
                bool resultado = _productoRepository.Eliminar(idProducto);

                if (resultado)
                {
                    Logger.LogInfo($"Producto eliminado (marcado como inactivo) exitosamente: {producto.Nombre} (ID: {idProducto})");
                }
                else
                {
                    errorMessage = "No se pudo eliminar el producto.";
                    Logger.LogError($"Error al eliminar producto {producto.Nombre} (ID: {idProducto})");
                }

                return resultado;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al eliminar producto {idProducto}");
                return false;
            }
        }

        /// <summary>
        /// Valida los datos de un producto.
        /// </summary>
        /// <param name="producto">Producto a validar.</param>
        /// <param name="errorMessage">Mensaje de error si la validación falla.</param>
        /// <returns>True si el producto es válido, False en caso contrario.</returns>
        private bool ValidarProducto(Producto producto, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Validar código
            if (string.IsNullOrWhiteSpace(producto.Codigo))
            {
                errorMessage = "El código del producto es obligatorio.";
                return false;
            }

            // Validar nombre
            if (string.IsNullOrWhiteSpace(producto.Nombre))
            {
                errorMessage = "El nombre del producto es obligatorio.";
                return false;
            }

            // Validar categoría
            if (producto.IdCategoria <= 0)
            {
                errorMessage = "Debe seleccionar una categoría para el producto.";
                return false;
            }

            // Validar precios
            if (producto.PrecioCompra <= 0)
            {
                errorMessage = "El precio de compra debe ser mayor que cero.";
                return false;
            }

            if (producto.PrecioVenta <= 0)
            {
                errorMessage = "El precio de venta debe ser mayor que cero.";
                return false;
            }

            // Validar regla de negocio: precio de venta > precio de compra
            if (producto.PrecioVenta <= producto.PrecioCompra)
            {
                errorMessage = "El precio de venta debe ser mayor al precio de compra.";
                return false;
            }

            // Validar stock mínimo
            if (producto.StockMinimo < 0)
            {
                errorMessage = "El stock mínimo no puede ser negativo.";
                return false;
            }

            return true;
        }
    }
}