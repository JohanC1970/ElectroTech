using ElectroTech.Helpers;
using ElectroTech.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace ElectroTech.DataAccess
{
    /// <summary>
    /// Repositorio para operaciones de base de datos relacionadas con la entidad Producto.
    /// </summary>
    public class ProductoRepository : DatabaseRepository
    {
        /// <summary>
        /// Obtiene todos los productos activos.
        /// </summary>
        /// <returns>Lista de productos activos.</returns>
        public List<Producto> ObtenerTodos()
        {
            try
            {
                string query = @"
                    SELECT p.idProducto, p.codigo, p.nombre, p.descripcion, p.idCategoria, 
                           c.nombre as nombreCategoria, p.marca, p.modelo, 
                           p.precioCompra, p.precioVenta, p.stockMinimo, p.ubicacionAlmacen, 
                           p.activo, i.cantidadDisponible, i.ultimaActualizacion
                    FROM Producto p
                    LEFT JOIN Categoria c ON p.idCategoria = c.idCategoria
                    LEFT JOIN Inventario i ON p.idProducto = i.idProducto
                    WHERE p.activo = 'S'
                    ORDER BY p.nombre";

                DataTable dataTable = ExecuteQuery(query);
                List<Producto> productos = new List<Producto>();

                foreach (DataRow row in dataTable.Rows)
                {
                    productos.Add(ConvertirDataRowAProducto(row));
                }

                return productos;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener todos los productos");
                throw new Exception("Error al obtener productos.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene un producto por su ID.
        /// </summary>
        /// <param name="idProducto">ID del producto.</param>
        /// <returns>El objeto Producto si se encuentra, null en caso contrario.</returns>
        public Producto ObtenerPorId(int idProducto)
        {
            try
            {
                string query = @"
                    SELECT p.idProducto, p.codigo, p.nombre, p.descripcion, p.idCategoria, 
                           c.nombre as nombreCategoria, p.marca, p.modelo, 
                           p.precioCompra, p.precioVenta, p.stockMinimo, p.ubicacionAlmacen, 
                           p.activo, i.cantidadDisponible, i.ultimaActualizacion
                    FROM Producto p
                    LEFT JOIN Categoria c ON p.idCategoria = c.idCategoria
                    LEFT JOIN Inventario i ON p.idProducto = i.idProducto
                    WHERE p.idProducto = :idProducto";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idProducto", idProducto }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);

                if (dataTable.Rows.Count > 0)
                {
                    return ConvertirDataRowAProducto(dataTable.Rows[0]);
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener producto con ID {idProducto}");
                throw new Exception("Error al obtener producto por ID.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene un producto por su código.
        /// </summary>
        /// <param name="codigo">Código del producto.</param>
        /// <returns>El objeto Producto si se encuentra, null en caso contrario.</returns>
        public Producto ObtenerPorCodigo(string codigo)
        {
            try
            {
                string query = @"
                    SELECT p.idProducto, p.codigo, p.nombre, p.descripcion, p.idCategoria, 
                           c.nombre as nombreCategoria, p.marca, p.modelo, 
                           p.precioCompra, p.precioVenta, p.stockMinimo, p.ubicacionAlmacen, 
                           p.activo, i.cantidadDisponible, i.ultimaActualizacion
                    FROM Producto p
                    LEFT JOIN Categoria c ON p.idCategoria = c.idCategoria
                    LEFT JOIN Inventario i ON p.idProducto = i.idProducto
                    WHERE p.codigo = :codigo";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":codigo", codigo }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);

                if (dataTable.Rows.Count > 0)
                {
                    return ConvertirDataRowAProducto(dataTable.Rows[0]);
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener producto con código {codigo}");
                throw new Exception("Error al obtener producto por código.", ex);
            }
            finally
            {
                CloseConnection();
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
                string query = @"
                    SELECT p.idProducto, p.codigo, p.nombre, p.descripcion, p.idCategoria, 
                           c.nombre as nombreCategoria, p.marca, p.modelo, 
                           p.precioCompra, p.precioVenta, p.stockMinimo, p.ubicacionAlmacen, 
                           p.activo, i.cantidadDisponible, i.ultimaActualizacion
                    FROM Producto p
                    LEFT JOIN Categoria c ON p.idCategoria = c.idCategoria
                    LEFT JOIN Inventario i ON p.idProducto = i.idProducto
                    WHERE p.idCategoria = :idCategoria AND p.activo = 'S'
                    ORDER BY p.nombre";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idCategoria", idCategoria }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);
                List<Producto> productos = new List<Producto>();

                foreach (DataRow row in dataTable.Rows)
                {
                    productos.Add(ConvertirDataRowAProducto(row));
                }

                return productos;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener productos de la categoría {idCategoria}");
                throw new Exception("Error al obtener productos por categoría.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene productos con stock bajo (menor a su stock mínimo).
        /// </summary>
        /// <returns>Lista de productos con stock bajo.</returns>
        public List<Producto> ObtenerProductosBajoStock()
        {
            try
            {
                string query = @"
                    SELECT p.idProducto, p.codigo, p.nombre, p.descripcion, p.idCategoria, 
                           c.nombre as nombreCategoria, p.marca, p.modelo, 
                           p.precioCompra, p.precioVenta, p.stockMinimo, p.ubicacionAlmacen, 
                           p.activo, i.cantidadDisponible, i.ultimaActualizacion
                    FROM Producto p
                    LEFT JOIN Categoria c ON p.idCategoria = c.idCategoria
                    JOIN Inventario i ON p.idProducto = i.idProducto
                    WHERE p.activo = 'S' AND i.cantidadDisponible < p.stockMinimo
                    ORDER BY (p.stockMinimo - i.cantidadDisponible) DESC";

                DataTable dataTable = ExecuteQuery(query);
                List<Producto> productos = new List<Producto>();

                foreach (DataRow row in dataTable.Rows)
                {
                    productos.Add(ConvertirDataRowAProducto(row));
                }

                return productos;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener productos con stock bajo");
                throw new Exception("Error al obtener productos con stock bajo.", ex);
            }
            finally
            {
                CloseConnection();
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
                string query = "SELECT COUNT(*) FROM Producto WHERE activo = 'S'";
                object result = ExecuteScalar(query);
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener total de productos");
                throw new Exception("Error al obtener total de productos.", ex);
            }
            finally
            {
                CloseConnection();
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
                string query = @"
                    SELECT COUNT(*) 
                    FROM Producto p
                    JOIN Inventario i ON p.idProducto = i.idProducto
                    WHERE p.activo = 'S' AND i.cantidadDisponible < p.stockMinimo";

                object result = ExecuteScalar(query);
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener total de productos con stock bajo");
                throw new Exception("Error al obtener total de productos con stock bajo.", ex);
            }
            finally
            {
                CloseConnection();
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
                string terminoBusqueda = $"%{termino}%";

                string query = @"
                    SELECT p.idProducto, p.codigo, p.nombre, p.descripcion, p.idCategoria, 
                           c.nombre as nombreCategoria, p.marca, p.modelo, 
                           p.precioCompra, p.precioVenta, p.stockMinimo, p.ubicacionAlmacen, 
                           p.activo, i.cantidadDisponible, i.ultimaActualizacion
                    FROM Producto p
                    LEFT JOIN Categoria c ON p.idCategoria = c.idCategoria
                    LEFT JOIN Inventario i ON p.idProducto = i.idProducto
                    WHERE p.activo = 'S' AND 
                          (UPPER(p.codigo) LIKE UPPER(:termino) OR 
                           UPPER(p.nombre) LIKE UPPER(:termino) OR 
                           UPPER(p.descripcion) LIKE UPPER(:termino) OR
                           UPPER(p.modelo) LIKE UPPER(:termino) OR
                           UPPER(c.nombre) LIKE UPPER(:termino))
                    ORDER BY p.nombre";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":termino", terminoBusqueda }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);
                List<Producto> productos = new List<Producto>();

                foreach (DataRow row in dataTable.Rows)
                {
                    productos.Add(ConvertirDataRowAProducto(row));
                }

                return productos;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al buscar productos con término '{termino}'");
                throw new Exception("Error al buscar productos.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Crea un nuevo producto en la base de datos.
        /// </summary>
        /// <param name="producto">Producto a crear.</param>
        /// <returns>ID del producto creado.</returns>
        public int Crear(Producto producto)
        {
            try
            {
                BeginTransaction();

                // Verificar si ya existe un producto con el mismo código
                if (ExisteCodigo(producto.Codigo))
                {
                    throw new Exception("Ya existe un producto con el código especificado.");
                }

                // Obtener el próximo ID de producto
                int idProducto = GetNextSequenceValue("SEQ_PRODUCTO");

                // Insertar el producto
                string query = @"
                    INSERT INTO Producto (idProducto, codigo, nombre, descripcion, idCategoria, 
                                          marca, modelo, precioCompra, precioVenta, 
                                          stockMinimo, ubicacionAlmacen, activo)
                    VALUES (:idProducto, :codigo, :nombre, :descripcion, :idCategoria, 
                            :marca, :modelo, :precioCompra, :precioVenta, 
                            :stockMinimo, :ubicacionAlmacen, :activo)";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idProducto", idProducto },
                    { ":codigo", producto.Codigo },
                    { ":nombre", producto.Nombre },
                    { ":descripcion", producto.Descripcion ?? (object)DBNull.Value },
                    { ":idCategoria", producto.IdCategoria },
                    { ":marca", producto.Marca ?? (object)DBNull.Value },
                    { ":modelo", producto.Modelo ?? (object)DBNull.Value },
                    { ":precioCompra", producto.PrecioCompra },
                    { ":precioVenta", producto.PrecioVenta },
                    { ":stockMinimo", producto.StockMinimo },
                    { ":ubicacionAlmacen", producto.UbicacionAlmacen ?? (object)DBNull.Value },
                    { ":activo", producto.Activo ? "S" : "N" }
                };

                ExecuteNonQuery(query, parameters);

                // Crear el registro de inventario con cantidad inicial 0
                string inventarioQuery = @"
                    INSERT INTO Inventario (idInventario, idProducto, cantidadDisponible, ultimaActualizacion)
                    VALUES (SEQ_INVENTARIO.NEXTVAL, :idProducto, 0, SYSDATE)";

                Dictionary<string, object> inventarioParams = new Dictionary<string, object>
                {
                    { ":idProducto", idProducto }
                };

                ExecuteNonQuery(inventarioQuery, inventarioParams);

                CommitTransaction();
                return idProducto;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Logger.LogException(ex, $"Error al crear producto {producto.Nombre}");
                throw new Exception("Error al crear producto.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Actualiza un producto existente.
        /// </summary>
        /// <param name="producto">Producto con los datos actualizados.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool Actualizar(Producto producto)
        {
            try
            {
                BeginTransaction();

                // Verificar si ya existe otro producto con el mismo código
                if (ExisteOtroProductoConCodigo(producto.Codigo, producto.IdProducto))
                {
                    throw new Exception("Ya existe otro producto con el código especificado.");
                }

                string query = @"
                    UPDATE Producto SET 
                        codigo = :codigo,
                        nombre = :nombre,
                        descripcion = :descripcion,
                        idCategoria = :idCategoria,
                        marca = :marca,
                        modelo = :modelo,
                        precioCompra = :precioCompra,
                        precioVenta = :precioVenta,
                        stockMinimo = :stockMinimo,
                        ubicacionAlmacen = :ubicacionAlmacen,
                        activo = :activo
                    WHERE idProducto = :idProducto";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":codigo", producto.Codigo },
                    { ":nombre", producto.Nombre },
                    { ":descripcion", producto.Descripcion ?? (object)DBNull.Value },
                    { ":idCategoria", producto.IdCategoria },
                    { ":marca", producto.Marca ?? (object)DBNull.Value },
                    { ":modelo", producto.Modelo ?? (object)DBNull.Value },
                    { ":precioCompra", producto.PrecioCompra },
                    { ":precioVenta", producto.PrecioVenta },
                    { ":stockMinimo", producto.StockMinimo },
                    { ":ubicacionAlmacen", producto.UbicacionAlmacen ?? (object)DBNull.Value },
                    { ":activo", producto.Activo ? "S" : "N" },
                    { ":idProducto", producto.IdProducto }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                CommitTransaction();

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Logger.LogException(ex, $"Error al actualizar producto {producto.Nombre}");
                throw new Exception("Error al actualizar producto.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Actualiza el stock de un producto.
        /// </summary>
        /// <param name="idProducto">ID del producto.</param>
        /// <param name="cantidad">Cantidad a agregar (positiva) o restar (negativa).</param>
        /// <param name="tipoMovimiento">Tipo de movimiento (E: Entrada, S: Salida).</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool ActualizarStock(int idProducto, int cantidad, char tipoMovimiento)
        {
            try
            {
                BeginTransaction();

                // Verificar stock actual si es una salida
                if (tipoMovimiento == 'S')
                {
                    string queryVerificar = @"
                        SELECT cantidadDisponible 
                        FROM Inventario 
                        WHERE idProducto = :idProducto";

                    Dictionary<string, object> paramsVerificar = new Dictionary<string, object>
                    {
                        { ":idProducto", idProducto }
                    };

                    object stockActual = ExecuteScalar(queryVerificar, paramsVerificar);

                    if (stockActual == null || Convert.ToInt32(stockActual) < cantidad)
                    {
                        RollbackTransaction();
                        return false; // No hay suficiente stock
                    }
                }

                // Actualizar el stock
                string query = @"
                    UPDATE Inventario SET 
                        cantidadDisponible = CASE 
                                              WHEN :tipoMovimiento = 'E' THEN cantidadDisponible + :cantidad
                                              WHEN :tipoMovimiento = 'S' THEN cantidadDisponible - :cantidad
                                              ELSE cantidadDisponible
                                            END,
                        ultimaActualizacion = SYSDATE
                    WHERE idProducto = :idProducto";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idProducto", idProducto },
                    { ":cantidad", cantidad },
                    { ":tipoMovimiento", tipoMovimiento.ToString() }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                CommitTransaction();

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Logger.LogException(ex, $"Error al actualizar stock del producto {idProducto}");
                throw new Exception("Error al actualizar stock del producto.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Elimina un producto (marcándolo como inactivo).
        /// </summary>
        /// <param name="idProducto">ID del producto a eliminar.</param>
        /// <returns>True si la eliminación es exitosa, False en caso contrario.</returns>
        public bool Eliminar(int idProducto)
        {
            try
            {
                // En lugar de eliminar físicamente, marcamos como inactivo
                string query = "UPDATE Producto SET activo = 'N' WHERE idProducto = :idProducto";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idProducto", idProducto }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al eliminar producto {idProducto}");
                throw new Exception("Error al eliminar producto.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Verifica si ya existe un producto con el código especificado.
        /// </summary>
        /// <param name="codigo">Código a verificar.</param>
        /// <returns>True si ya existe, False en caso contrario.</returns>
        private bool ExisteCodigo(string codigo)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Producto WHERE codigo = :codigo";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":codigo", codigo }
                };

                object result = ExecuteScalar(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al verificar existencia de código {codigo}");
                throw new Exception("Error al verificar existencia de código.", ex);
            }
        }

        /// <summary>
        /// Verifica si ya existe otro producto con el mismo código.
        /// </summary>
        /// <param name="codigo">Código a verificar.</param>
        /// <param name="idProducto">ID del producto actual (para excluirlo de la verificación).</param>
        /// <returns>True si ya existe otro producto con el mismo código, False en caso contrario.</returns>
        private bool ExisteOtroProductoConCodigo(string codigo, int idProducto)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Producto WHERE codigo = :codigo AND idProducto != :idProducto";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":codigo", codigo },
                    { ":idProducto", idProducto }
                };

                object result = ExecuteScalar(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al verificar existencia de código en otro producto {codigo}");
                throw new Exception("Error al verificar existencia de código en otro producto.", ex);
            }
        }

        /// <summary>
        /// Convierte un DataRow a un objeto Producto.
        /// </summary>
        /// <param name="row">DataRow con los datos del producto.</param>
        /// <returns>Objeto Producto con los datos del DataRow.</returns>
        private Producto ConvertirDataRowAProducto(DataRow row)
        {
            var producto = new Producto
            {
                IdProducto = Convert.ToInt32(row["idProducto"]),
                Codigo = row["codigo"].ToString(),
                Nombre = row["nombre"].ToString(),
                Descripcion = row["descripcion"] != DBNull.Value ? row["descripcion"].ToString() : null,
                IdCategoria = Convert.ToInt32(row["idCategoria"]),
                NombreCategoria = row["nombreCategoria"] != DBNull.Value ? row["nombreCategoria"].ToString() : null,
                Marca = row["marca"] != DBNull.Value ? row["marca"].ToString() : null,
                Modelo = row["modelo"] != DBNull.Value ? row["modelo"].ToString() : null,
                PrecioCompra = Convert.ToDecimal(row["precioCompra"]),
                PrecioVenta = Convert.ToDecimal(row["precioVenta"]),
                StockMinimo = Convert.ToInt32(row["stockMinimo"]),
                UbicacionAlmacen = row["ubicacionAlmacen"] != DBNull.Value ? row["ubicacionAlmacen"].ToString() : null,
                Activo = row["activo"].ToString() == "S"
            };

            // Datos de inventario (pueden ser nulos si no existe inventario)
            if (row["cantidadDisponible"] != DBNull.Value)
            {
                producto.CantidadDisponible = Convert.ToInt32(row["cantidadDisponible"]);
            }

            if (row["ultimaActualizacion"] != DBNull.Value)
            {
                producto.UltimaActualizacion = Convert.ToDateTime(row["ultimaActualizacion"]);
            }

            return producto;
        }
    }
}