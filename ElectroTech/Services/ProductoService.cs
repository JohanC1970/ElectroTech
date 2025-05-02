using ElectroTech.DataAccess;
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
                // Validar datos
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

                // Actualizar el stock
                bool resultado = _productoRepository.ActualizarStock(idProducto, cantidad, tipoMovimiento);

                if (!resultado && tipoMovimiento == 'S')
                {
                    errorMessage = "No hay suficiente stock disponible para realizar la salida.";
                    Logger.LogWarning($"Intento de reducir stock insuficiente para el producto {idProducto}");
                }
                else if (!resultado)
                {
                    errorMessage = "No se pudo actualizar el stock del producto.";
                    Logger.LogError($"Error al actualizar stock del producto {idProducto}");
                }
                else
                {
                    Logger.LogInfo($"Stock actualizado exitosamente para el producto {idProducto}. " +
                        $"Movimiento: {(tipoMovimiento == 'E' ? "Entrada" : "Salida")}, Cantidad: {cantidad}");
                }

                return resultado;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al actualizar stock del producto {idProducto}");
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