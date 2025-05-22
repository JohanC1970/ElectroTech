
using ElectroTech.Helpers;
using ElectroTech.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace ElectroTech.DataAccess
{
    /// <summary>
    /// Repositorio para operaciones de base de datos relacionadas con la entidad Inventario.
    /// Esta es una implementación simplificada para dar soporte al módulo de devoluciones.
    /// </summary>
    public class InventarioRepository : DatabaseRepository
    {
        /// <summary>
        /// Obtiene el inventario de un producto.
        /// </summary>
        /// <param name="idProducto">ID del producto.</param>
        /// <returns>El objeto Inventario si se encuentra, null en caso contrario.</returns>
        public Inventario ObtenerPorProducto(int idProducto)
        {
            try
            {
                string query = @"
                    SELECT i.idInventario, i.idProducto, i.cantidadDisponible, i.ultimaActualizacion,
                           p.codigo, p.nombre, p.stockMinimo, p.precioCompra, p.precioVenta
                    FROM Inventario i
                    LEFT JOIN Producto p ON i.idProducto = p.idProducto
                    WHERE i.idProducto = :idProducto";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idProducto", idProducto }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);

                if (dataTable.Rows.Count > 0)
                {
                    DataRow row = dataTable.Rows[0];

                    Inventario inventario = new Inventario
                    {
                        IdInventario = Convert.ToInt32(row["idInventario"]),
                        IdProducto = Convert.ToInt32(row["idProducto"]),
                        CantidadDisponible = Convert.ToInt32(row["cantidadDisponible"]),
                        UltimaActualizacion = Convert.ToDateTime(row["ultimaActualizacion"]),
                        Producto = new Producto
                        {
                            IdProducto = Convert.ToInt32(row["idProducto"]),
                            Codigo = row["codigo"].ToString(),
                            Nombre = row["nombre"].ToString(),
                            StockMinimo = Convert.ToInt32(row["stockMinimo"]),
                            PrecioCompra = Convert.ToDecimal(row["precioCompra"]),
                            PrecioVenta = Convert.ToDecimal(row["precioVenta"])
                        }
                    };

                    return inventario;
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener inventario del producto {idProducto}");
                throw new Exception("Error al obtener inventario por producto.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene los productos con stock bajo el mínimo.
        /// </summary>
        /// <returns>Lista de inventarios con stock bajo el mínimo.</returns>
        public List<Inventario> ObtenerProductosBajoStock()
        {
            try
            {
                string query = @"
                    SELECT i.idInventario, i.idProducto, i.cantidadDisponible, i.ultimaActualizacion,
                           p.codigo, p.nombre, p.stockMinimo, p.precioCompra, p.precioVenta
                    FROM Inventario i
                    JOIN Producto p ON i.idProducto = p.idProducto
                    WHERE i.cantidadDisponible < p.stockMinimo AND p.activo = 'S'
                    ORDER BY (p.stockMinimo - i.cantidadDisponible) DESC";

                DataTable dataTable = ExecuteQuery(query);
                List<Inventario> inventarios = new List<Inventario>();

                foreach (DataRow row in dataTable.Rows)
                {
                    Inventario inventario = new Inventario
                    {
                        IdInventario = Convert.ToInt32(row["idInventario"]),
                        IdProducto = Convert.ToInt32(row["idProducto"]),
                        CantidadDisponible = Convert.ToInt32(row["cantidadDisponible"]),
                        UltimaActualizacion = Convert.ToDateTime(row["ultimaActualizacion"]),
                        Producto = new Producto
                        {
                            IdProducto = Convert.ToInt32(row["idProducto"]),
                            Codigo = row["codigo"].ToString(),
                            Nombre = row["nombre"].ToString(),
                            StockMinimo = Convert.ToInt32(row["stockMinimo"]),
                            PrecioCompra = Convert.ToDecimal(row["precioCompra"]),
                            PrecioVenta = Convert.ToDecimal(row["precioVenta"])
                        }
                    };

                    inventarios.Add(inventario);
                }

                return inventarios;
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

                // Si no hay registros para actualizar (no existe el inventario), crear uno nuevo
                if (rowsAffected == 0 && tipoMovimiento == 'E')
                {
                    // Obtener el próximo ID de inventario
                    int idInventario = GetNextSequenceValue("SEQ_INVENTARIO");

                    // Insertar nuevo registro
                    string queryInsert = @"
                        INSERT INTO Inventario (idInventario, idProducto, cantidadDisponible, ultimaActualizacion)
                        VALUES (:idInventario, :idProducto, :cantidad, SYSDATE)";

                    Dictionary<string, object> paramsInsert = new Dictionary<string, object>
                    {
                        { ":idInventario", idInventario },
                        { ":idProducto", idProducto },
                        { ":cantidad", cantidad }
                    };

                    rowsAffected = ExecuteNonQuery(queryInsert, paramsInsert);
                }

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
        /// Verifica si hay suficiente stock para una cantidad requerida.
        /// </summary>
        /// <param name="idProducto">ID del producto.</param>
        /// <param name="cantidad">Cantidad requerida.</param>
        /// <returns>True si hay suficiente disponibilidad, False en caso contrario.</returns>
        public bool VerificarDisponibilidad(int idProducto, int cantidad)
        {
            try
            {
                string query = @"
                    SELECT CASE 
                             WHEN cantidadDisponible >= :cantidad THEN 1 
                             ELSE 0 
                           END as disponible
                    FROM Inventario 
                    WHERE idProducto = :idProducto";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idProducto", idProducto },
                    { ":cantidad", cantidad }
                };

                object result = ExecuteScalar(query, parameters);

                // Si no hay resultados, no hay inventario para el producto
                if (result == null)
                {
                    return false;
                }

                return Convert.ToInt32(result) == 1;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al verificar disponibilidad del producto {idProducto}");
                throw new Exception("Error al verificar disponibilidad de producto.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }
    }
}
