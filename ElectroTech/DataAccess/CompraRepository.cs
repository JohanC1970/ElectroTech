using ElectroTech.Helpers;
using ElectroTech.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ElectroTech.DataAccess
{
    /// <summary>
    /// Repositorio para operaciones de base de datos relacionadas con la entidad Compra.
    /// </summary>
    public class CompraRepository : DatabaseRepository
    {
        /// <summary>
        /// Obtiene todas las compras.
        /// </summary>
        /// <returns>Lista de compras.</returns>
        public List<Compra> ObtenerTodas()
        {
            try
            {
                string query = @"
                    SELECT c.idCompra, c.numeroOrden, c.fecha, c.idProveedor, 
                           p.nombre as nombreProveedor, c.subtotal, c.impuestos, c.total, 
                           c.estado, c.observaciones
                    FROM Compra c
                    LEFT JOIN Proveedor p ON c.idProveedor = p.idProveedor
                    ORDER BY c.fecha DESC";

                DataTable dataTable = ExecuteQuery(query);
                List<Compra> compras = new List<Compra>();

                foreach (DataRow row in dataTable.Rows)
                {
                    Compra compra = ConvertirDataRowACompra(row);
                    compra.Detalles = ObtenerDetallesPorCompra(compra.IdCompra);
                    compras.Add(compra);
                }

                return compras;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener todas las compras");
                throw new Exception("Error al obtener las compras.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene una compra por su ID.
        /// </summary>
        /// <param name="idCompra">ID de la compra.</param>
        /// <returns>El objeto Compra si se encuentra, null en caso contrario.</returns>
        public Compra ObtenerPorId(int idCompra)
        {
            try
            {
                string query = @"
                    SELECT c.idCompra, c.numeroOrden, c.fecha, c.idProveedor, 
                           p.nombre as nombreProveedor, c.subtotal, c.impuestos, c.total, 
                           c.estado, c.observaciones
                    FROM Compra c
                    LEFT JOIN Proveedor p ON c.idProveedor = p.idProveedor
                    WHERE c.idCompra = :idCompra";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idCompra", idCompra }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);

                if (dataTable.Rows.Count > 0)
                {
                    Compra compra = ConvertirDataRowACompra(dataTable.Rows[0]);
                    compra.Detalles = ObtenerDetallesPorCompra(compra.IdCompra);
                    return compra;
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener compra con ID {idCompra}");
                throw new Exception("Error al obtener compra por ID.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene una compra por su número de orden.
        /// </summary>
        /// <param name="numeroOrden">Número de orden de la compra.</param>
        /// <returns>El objeto Compra si se encuentra, null en caso contrario.</returns>
        public Compra ObtenerPorNumeroOrden(string numeroOrden)
        {
            try
            {
                string query = @"
                    SELECT c.idCompra, c.numeroOrden, c.fecha, c.idProveedor, 
                           p.nombre as nombreProveedor, c.subtotal, c.impuestos, c.total, 
                           c.estado, c.observaciones
                    FROM Compra c
                    LEFT JOIN Proveedor p ON c.idProveedor = p.idProveedor
                    WHERE c.numeroOrden = :numeroOrden";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":numeroOrden", numeroOrden }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);

                if (dataTable.Rows.Count > 0)
                {
                    Compra compra = ConvertirDataRowACompra(dataTable.Rows[0]);
                    compra.Detalles = ObtenerDetallesPorCompra(compra.IdCompra);
                    return compra;
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener compra con número de orden {numeroOrden}");
                throw new Exception("Error al obtener compra por número de orden.", ex);
            }
            finally
            {
                CloseConnection();
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
                string query = @"
                    SELECT c.idCompra, c.numeroOrden, c.fecha, c.idProveedor, 
                           p.nombre as nombreProveedor, c.subtotal, c.impuestos, c.total, 
                           c.estado, c.observaciones
                    FROM Compra c
                    LEFT JOIN Proveedor p ON c.idProveedor = p.idProveedor
                    WHERE c.estado = :estado
                    ORDER BY c.fecha DESC";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":estado", estado.ToString() }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);
                List<Compra> compras = new List<Compra>();

                foreach (DataRow row in dataTable.Rows)
                {
                    Compra compra = ConvertirDataRowACompra(row);
                    compra.Detalles = ObtenerDetallesPorCompra(compra.IdCompra);
                    compras.Add(compra);
                }

                return compras;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener compras con estado {estado}");
                throw new Exception("Error al obtener compras por estado.", ex);
            }
            finally
            {
                CloseConnection();
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
                string terminoBusqueda = $"%{termino}%";

                string query = @"
                    SELECT c.idCompra, c.numeroOrden, c.fecha, c.idProveedor, 
                           p.nombre as nombreProveedor, c.subtotal, c.impuestos, c.total, 
                           c.estado, c.observaciones
                    FROM Compra c
                    LEFT JOIN Proveedor p ON c.idProveedor = p.idProveedor
                    WHERE UPPER(c.numeroOrden) LIKE UPPER(:termino) OR
                          UPPER(p.nombre) LIKE UPPER(:termino) OR
                          UPPER(c.observaciones) LIKE UPPER(:termino)
                    ORDER BY c.fecha DESC";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":termino", terminoBusqueda }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);
                List<Compra> compras = new List<Compra>();

                foreach (DataRow row in dataTable.Rows)
                {
                    Compra compra = ConvertirDataRowACompra(row);
                    compra.Detalles = ObtenerDetallesPorCompra(compra.IdCompra);
                    compras.Add(compra);
                }

                return compras;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al buscar compras con término '{termino}'");
                throw new Exception("Error al buscar compras.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene los detalles de una compra.
        /// </summary>
        /// <param name="idCompra">ID de la compra.</param>
        /// <returns>Lista de detalles de la compra.</returns>
        public List<DetalleCompra> ObtenerDetallesPorCompra(int idCompra)
        {
            try
            {
                string query = @"
                    SELECT dc.idDetalleCompra, dc.idCompra, dc.idProducto, dc.cantidad, 
                           dc.precioUnitario, dc.subtotal, p.codigo, p.nombre, p.descripcion
                    FROM DetalleCompra dc
                    LEFT JOIN Producto p ON dc.idProducto = p.idProducto
                    WHERE dc.idCompra = :idCompra";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idCompra", idCompra }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);
                List<DetalleCompra> detalles = new List<DetalleCompra>();

                foreach (DataRow row in dataTable.Rows)
                {
                    DetalleCompra detalle = new DetalleCompra
                    {
                        IdDetalleCompra = Convert.ToInt32(row["idDetalleCompra"]),
                        IdCompra = Convert.ToInt32(row["idCompra"]),
                        IdProducto = Convert.ToInt32(row["idProducto"]),
                        Cantidad = Convert.ToInt32(row["cantidad"]),
                        PrecioUnitario = Convert.ToDouble(row["precioUnitario"]),
                        Subtotal = Convert.ToDouble(row["subtotal"]),
                        Producto = new Producto
                        {
                            IdProducto = Convert.ToInt32(row["idProducto"]),
                            Codigo = row["codigo"].ToString(),
                            Nombre = row["nombre"].ToString(),
                            Descripcion = row["descripcion"] != DBNull.Value ? row["descripcion"].ToString() : null
                        }
                    };

                    detalles.Add(detalle);
                }

                return detalles;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener detalles de compra {idCompra}");
                throw new Exception("Error al obtener detalles de compra.", ex);
            }
        }

        /// <summary>
        /// Crea una nueva compra en la base de datos.
        /// </summary>
        /// <param name="compra">Compra a crear.</param>
        /// <returns>ID de la compra creada.</returns>
        public int Crear(Compra compra)
        {
            try
            {
                BeginTransaction();

                // Verificar si ya existe una compra con el mismo número de orden
                if (ExisteNumeroOrden(compra.NumeroOrden))
                {
                    throw new Exception("Ya existe una compra con el número de orden especificado.");
                }

                // Obtener el próximo ID de compra
                int idCompra = GetNextSequenceValue("SEQ_COMPRA");

                // Insertar la compra
                string query = @"
                    INSERT INTO Compra (idCompra, numeroOrden, fecha, idProveedor, 
                                        subtotal, impuestos, total, estado, observaciones)
                    VALUES (:idCompra, :numeroOrden, :fecha, :idProveedor, 
                            :subtotal, :impuestos, :total, :estado, :observaciones)";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idCompra", idCompra },
                    { ":numeroOrden", compra.NumeroOrden },
                    { ":fecha", compra.Fecha },
                    { ":idProveedor", compra.IdProveedor },
                    { ":subtotal", compra.Subtotal },
                    { ":impuestos", compra.Impuestos },
                    { ":total", compra.Total },
                    { ":estado", compra.Estado.ToString() },
                    { ":observaciones", compra.Observaciones ?? (object)DBNull.Value }
                };

                ExecuteNonQuery(query, parameters);

                // Insertar los detalles de la compra
                if (compra.Detalles != null && compra.Detalles.Count > 0)
                {
                    foreach (var detalle in compra.Detalles)
                    {
                        int idDetalle = GetNextSequenceValue("SEQ_DETALLE_COMPRA");

                        string queryDetalle = @"
                            INSERT INTO DetalleCompra (idDetalleCompra, idCompra, idProducto, 
                                                      cantidad, precioUnitario, subtotal)
                            VALUES (:idDetalleCompra, :idCompra, :idProducto, 
                                    :cantidad, :precioUnitario, :subtotal)";

                        Dictionary<string, object> paramsDetalle = new Dictionary<string, object>
                        {
                            { ":idDetalleCompra", idDetalle },
                            { ":idCompra", idCompra },
                            { ":idProducto", detalle.IdProducto },
                            { ":cantidad", detalle.Cantidad },
                            { ":precioUnitario", detalle.PrecioUnitario },
                            { ":subtotal", detalle.Subtotal }
                        };

                        ExecuteNonQuery(queryDetalle, paramsDetalle);
                    }
                }

                // Si el estado es Recibido (R), actualizar el inventario
                if (compra.Estado == 'R')
                {
                    ActualizarInventario(idCompra);
                }

                CommitTransaction();
                return idCompra;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Logger.LogException(ex, "Error al crear compra");
                throw new Exception("Error al crear compra.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Actualiza una compra existente.
        /// </summary>
        /// <param name="compra">Compra con los datos actualizados.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool Actualizar(Compra compra)
        {
            try
            {
                BeginTransaction();

                // Verificar si ya existe otra compra con el mismo número de orden
                if (ExisteOtraCompraConNumeroOrden(compra.NumeroOrden, compra.IdCompra))
                {
                    throw new Exception("Ya existe otra compra con el número de orden especificado.");
                }

                // Obtener estado actual de la compra
                char estadoActual = ObtenerEstadoCompra(compra.IdCompra);

                // Verificar si la compra puede ser modificada
                // Solo se pueden modificar compras en estado Pendiente (P)
                if (estadoActual != 'P')
                {
                    throw new Exception("Solo se pueden modificar compras en estado Pendiente.");
                }

                // Actualizar la compra
                string query = @"
                    UPDATE Compra SET 
                        numeroOrden = :numeroOrden,
                        fecha = :fecha,
                        idProveedor = :idProveedor,
                        subtotal = :subtotal,
                        impuestos = :impuestos,
                        total = :total,
                        estado = :estado,
                        observaciones = :observaciones
                    WHERE idCompra = :idCompra";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":numeroOrden", compra.NumeroOrden },
                    { ":fecha", compra.Fecha },
                    { ":idProveedor", compra.IdProveedor },
                    { ":subtotal", compra.Subtotal },
                    { ":impuestos", compra.Impuestos },
                    { ":total", compra.Total },
                    { ":estado", compra.Estado.ToString() },
                    { ":observaciones", compra.Observaciones ?? (object)DBNull.Value },
                    { ":idCompra", compra.IdCompra }
                };

                ExecuteNonQuery(query, parameters);

                // Eliminar los detalles actuales
                string queryEliminarDetalles = "DELETE FROM DetalleCompra WHERE idCompra = :idCompra";
                Dictionary<string, object> paramsEliminar = new Dictionary<string, object>
                {
                    { ":idCompra", compra.IdCompra }
                };

                ExecuteNonQuery(queryEliminarDetalles, paramsEliminar);

                // Insertar los nuevos detalles
                if (compra.Detalles != null && compra.Detalles.Count > 0)
                {
                    foreach (var detalle in compra.Detalles)
                    {
                        int idDetalle = GetNextSequenceValue("SEQ_DETALLE_COMPRA");

                        string queryDetalle = @"
                            INSERT INTO DetalleCompra (idDetalleCompra, idCompra, idProducto, 
                                                      cantidad, precioUnitario, subtotal)
                            VALUES (:idDetalleCompra, :idCompra, :idProducto, 
                                    :cantidad, :precioUnitario, :subtotal)";

                        Dictionary<string, object> paramsDetalle = new Dictionary<string, object>
                        {
                            { ":idDetalleCompra", idDetalle },
                            { ":idCompra", compra.IdCompra },
                            { ":idProducto", detalle.IdProducto },
                            { ":cantidad", detalle.Cantidad },
                            { ":precioUnitario", detalle.PrecioUnitario },
                            { ":subtotal", detalle.Subtotal }
                        };

                        ExecuteNonQuery(queryDetalle, paramsDetalle);
                    }
                }

                // Si el estado cambia a Recibido (R), actualizar el inventario
                if (estadoActual != 'R' && compra.Estado == 'R')
                {
                    ActualizarInventario(compra.IdCompra);
                }

                CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Logger.LogException(ex, $"Error al actualizar compra {compra.IdCompra}");
                throw new Exception("Error al actualizar compra.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Recibe una compra (cambia el estado a Recibido y actualiza el inventario).
        /// </summary>
        /// <param name="idCompra">ID de la compra a recibir.</param>
        /// <returns>True si la recepción es exitosa, False en caso contrario.</returns>
        public bool RecibirCompra(int idCompra)
        {
            try
            {
                BeginTransaction();

                // Verificar el estado actual de la compra
                char estadoActual = ObtenerEstadoCompra(idCompra);

                // Solo se pueden recibir compras en estado Pendiente (P)
                if (estadoActual != 'P')
                {
                    throw new Exception("Solo se pueden recibir compras en estado Pendiente.");
                }

                // Actualizar el estado de la compra a Recibido (R)
                string query = "UPDATE Compra SET estado = 'R' WHERE idCompra = :idCompra";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idCompra", idCompra }
                };

                ExecuteNonQuery(query, parameters);

                // Actualizar el inventario
                ActualizarInventario(idCompra);

                CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Logger.LogException(ex, $"Error al recibir compra {idCompra}");
                throw new Exception("Error al recibir compra.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Cancela una compra (cambia el estado a Cancelado).
        /// </summary>
        /// <param name="idCompra">ID de la compra a cancelar.</param>
        /// <returns>True si la cancelación es exitosa, False en caso contrario.</returns>
        public bool CancelarCompra(int idCompra)
        {
            try
            {
                BeginTransaction();

                // Verificar el estado actual de la compra
                char estadoActual = ObtenerEstadoCompra(idCompra);

                // Solo se pueden cancelar compras en estado Pendiente (P)
                if (estadoActual != 'P')
                {
                    throw new Exception("Solo se pueden cancelar compras en estado Pendiente.");
                }

                // Actualizar el estado de la compra a Cancelado (C)
                string query = "UPDATE Compra SET estado = 'C' WHERE idCompra = :idCompra";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idCompra", idCompra }
                };

                ExecuteNonQuery(query, parameters);

                CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Logger.LogException(ex, $"Error al cancelar compra {idCompra}");
                throw new Exception("Error al cancelar compra.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Actualiza el inventario con los productos de una compra.
        /// </summary>
        /// <param name="idCompra">ID de la compra.</param>
        private void ActualizarInventario(int idCompra)
        {
            try
            {
                // Obtener los detalles de la compra
                List<DetalleCompra> detalles = ObtenerDetallesPorCompra(idCompra);

                foreach (var detalle in detalles)
                {
                    // Verificar si el producto ya existe en el inventario
                    string queryVerificar = "SELECT COUNT(*) FROM Inventario WHERE idProducto = :idProducto";
                    Dictionary<string, object> paramsVerificar = new Dictionary<string, object>
                    {
                        { ":idProducto", detalle.IdProducto }
                    };

                    int existeInventario = Convert.ToInt32(ExecuteScalar(queryVerificar, paramsVerificar));

                    if (existeInventario > 0)
                    {
                        // Actualizar inventario existente
                        string queryActualizar = @"
                            UPDATE Inventario 
                            SET cantidadDisponible = cantidadDisponible + :cantidad,
                                ultimaActualizacion = SYSDATE
                            WHERE idProducto = :idProducto";

                        Dictionary<string, object> paramsActualizar = new Dictionary<string, object>
                        {
                            { ":cantidad", detalle.Cantidad },
                            { ":idProducto", detalle.IdProducto }
                        };

                        ExecuteNonQuery(queryActualizar, paramsActualizar);
                    }
                    else
                    {
                        // Crear nuevo registro en inventario
                        int idInventario = GetNextSequenceValue("SEQ_INVENTARIO");

                        string queryInsertar = @"
                            INSERT INTO Inventario (idInventario, idProducto, cantidadDisponible, ultimaActualizacion)
                            VALUES (:idInventario, :idProducto, :cantidad, SYSDATE)";

                        Dictionary<string, object> paramsInsertar = new Dictionary<string, object>
                        {
                            { ":idInventario", idInventario },
                            { ":idProducto", detalle.IdProducto },
                            { ":cantidad", detalle.Cantidad }
                        };

                        ExecuteNonQuery(queryInsertar, paramsInsertar);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al actualizar inventario para compra {idCompra}");
                throw new Exception("Error al actualizar inventario.", ex);
            }
        }

        /// <summary>
        /// Verifica si ya existe una compra con el número de orden especificado.
        /// </summary>
        /// <param name="numeroOrden">Número de orden a verificar.</param>
        /// <returns>True si ya existe, False en caso contrario.</returns>
        private bool ExisteNumeroOrden(string numeroOrden)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Compra WHERE numeroOrden = :numeroOrden";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":numeroOrden", numeroOrden }
                };

                object result = ExecuteScalar(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al verificar existencia de número de orden {numeroOrden}");
                throw new Exception("Error al verificar existencia de número de orden.", ex);
            }
        }

        /// <summary>
        /// Verifica si ya existe otra compra con el mismo número de orden.
        /// </summary>
        /// <param name="numeroOrden">Número de orden a verificar.</param>
        /// <param name="idCompra">ID de la compra actual (para excluirla).</param>
        /// <returns>True si ya existe otra compra con el mismo número, False en caso contrario.</returns>
        private bool ExisteOtraCompraConNumeroOrden(string numeroOrden, int idCompra)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Compra WHERE numeroOrden = :numeroOrden AND idCompra != :idCompra";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":numeroOrden", numeroOrden },
                    { ":idCompra", idCompra }
                };

                object result = ExecuteScalar(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al verificar existencia de número de orden en otra compra {numeroOrden}");
                throw new Exception("Error al verificar existencia de número de orden en otra compra.", ex);
            }
        }

        /// <summary>
        /// Obtiene el estado actual de una compra.
        /// </summary>
        /// <param name="idCompra">ID de la compra.</param>
        /// <returns>El estado de la compra.</returns>
        private char ObtenerEstadoCompra(int idCompra)
        {
            try
            {
                string query = "SELECT estado FROM Compra WHERE idCompra = :idCompra";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idCompra", idCompra }
                };

                object result = ExecuteScalar(query, parameters);
                return result.ToString()[0];
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener estado de compra {idCompra}");
                throw new Exception("Error al obtener estado de compra.", ex);
            }
        }

        /// <summary>
        /// Obtiene el consecutivo para un nuevo número de orden.
        /// </summary>
        /// <returns>Número consecutivo.</returns>
        /// <summary>
        /// Obtiene el consecutivo para un nuevo número de orden.
        /// </summary>
        /// <returns>Número consecutivo.</returns>
        public int ObtenerConsecutivoOrden()
        {
            try
            {
                string query = @"
            SELECT COUNT(*) + 1 FROM Compra 
            WHERE numeroOrden LIKE 'CO' || TO_CHAR(SYSDATE, 'YYYYMMDD') || '%'";

                // Usar ExecuteScalar en lugar de ExecuteQuery
                object result = ExecuteScalar(query);

                // Validar que el resultado no sea null
                if (result == null || result == DBNull.Value)
                {
                    return 1; // Si no hay registros, empezar en 1
                }

                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener consecutivo de orden");
                // En caso de error, devolver 1 como valor por defecto
                return 1;
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Convierte un DataRow a un objeto Compra.
        /// </summary>
        /// <param name="row">DataRow con los datos de la compra.</param>
        /// <returns>Objeto Compra con los datos del DataRow.</returns>
        private Compra ConvertirDataRowACompra(DataRow row)
        {
            return new Compra
            {
                IdCompra = Convert.ToInt32(row["idCompra"]),
                NumeroOrden = row["numeroOrden"].ToString(),
                Fecha = Convert.ToDateTime(row["fecha"]),
                IdProveedor = Convert.ToInt32(row["idProveedor"]),
                Proveedor = new Proveedor
                {
                    IdProveedor = Convert.ToInt32(row["idProveedor"]),
                    Nombre = row["nombreProveedor"].ToString()
                },
                Subtotal = Convert.ToDouble(row["subtotal"]),
                Impuestos = Convert.ToDouble(row["impuestos"]),
                Total = Convert.ToDouble(row["total"]),
                Estado = row["estado"].ToString()[0],
                Observaciones = row["observaciones"] != DBNull.Value ? row["observaciones"].ToString() : null
            };
        }
    }
}