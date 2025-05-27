using ElectroTech.Helpers;
using ElectroTech.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace ElectroTech.DataAccess
{
    /// <summary>
    /// Repositorio para operaciones de base de datos relacionadas con la entidad Venta.
    /// </summary>
    public class VentaRepository : DatabaseRepository
    {
        /// <summary>
        /// Obtiene todas las ventas.
        /// </summary>
        /// <returns>Lista de ventas.</returns>
        public List<Venta> ObtenerTodas()
        {
            try
            {
                string query = @"
                    SELECT v.idVenta, v.numeroFactura, v.fecha, v.idCliente, v.idEmpleado, 
                           v.idMetodoPago, v.subtotal, v.descuento, v.impuestos, v.total, 
                           v.observaciones, v.estado,
                           NVL(c.nombre || ' ' || c.apellido, 'Cliente no disponible') as nombreCliente,
                           NVL(e.nombre || ' ' || e.apellido, 'Empleado no disponible') as nombreEmpleado,
                           NVL(mp.nombre, 'Método no disponible') as nombreMetodoPago
                    FROM Venta v
                    LEFT JOIN Cliente c ON v.idCliente = c.idCliente
                    LEFT JOIN Empleado e ON v.idEmpleado = e.idEmpleado
                    LEFT JOIN MetodoPago mp ON v.idMetodoPago = mp.idMetodoPago
                    ORDER BY v.fecha DESC";

                DataTable dataTable = ExecuteQuery(query);
                List<Venta> ventas = new List<Venta>();

                foreach (DataRow row in dataTable.Rows)
                {
                    ventas.Add(ConvertirDataRowAVenta(row));
                }

                return ventas;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener todas las ventas");
                throw new Exception("Error al obtener ventas.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene ventas por estado.
        /// </summary>
        /// <param name="estado">Estado de las ventas (C: Completada, P: Pendiente, A: Anulada).</param>
        /// <returns>Lista de ventas con el estado especificado.</returns>
        public List<Venta> ObtenerVentasPorEstado(char estado)
        {
            try
            {
                string query = @"
                    SELECT v.idVenta, v.numeroFactura, v.fecha, v.idCliente, v.idEmpleado, 
                           v.idMetodoPago, v.subtotal, v.descuento, v.impuestos, v.total, 
                           v.observaciones, v.estado,
                           NVL(c.nombre || ' ' || c.apellido, 'Cliente no disponible') as nombreCliente,
                           NVL(e.nombre || ' ' || e.apellido, 'Empleado no disponible') as nombreEmpleado,
                           NVL(mp.nombre, 'Método no disponible') as nombreMetodoPago
                    FROM Venta v
                    LEFT JOIN Cliente c ON v.idCliente = c.idCliente
                    LEFT JOIN Empleado e ON v.idEmpleado = e.idEmpleado
                    LEFT JOIN MetodoPago mp ON v.idMetodoPago = mp.idMetodoPago
                    WHERE v.estado = :estado
                    ORDER BY v.fecha DESC";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":estado", estado.ToString() }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);
                List<Venta> ventas = new List<Venta>();

                foreach (DataRow row in dataTable.Rows)
                {
                    ventas.Add(ConvertirDataRowAVenta(row));
                }

                return ventas;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener ventas por estado {estado}");
                throw new Exception("Error al obtener ventas por estado.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene una venta por su ID con sus detalles.
        /// </summary>
        /// <param name="idVenta">ID de la venta.</param>
        /// <returns>La venta si se encuentra, null en caso contrario.</returns>
        public Venta ObtenerPorId(int idVenta)
        {
            try
            {
                // Obtener datos básicos de la venta
                string queryVenta = @"
                    SELECT v.idVenta, v.numeroFactura, v.fecha, v.idCliente, v.idEmpleado, 
                           v.idMetodoPago, v.subtotal, v.descuento, v.impuestos, v.total, 
                           v.observaciones, v.estado,
                           NVL(c.nombre || ' ' || c.apellido, 'Cliente no disponible') as nombreCliente,
                           NVL(e.nombre || ' ' || e.apellido, 'Empleado no disponible') as nombreEmpleado,
                           NVL(mp.nombre, 'Método no disponible') as nombreMetodoPago
                    FROM Venta v
                    LEFT JOIN Cliente c ON v.idCliente = c.idCliente
                    LEFT JOIN Empleado e ON v.idEmpleado = e.idEmpleado
                    LEFT JOIN MetodoPago mp ON v.idMetodoPago = mp.idMetodoPago
                    WHERE v.idVenta = :idVenta";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idVenta", idVenta }
                };

                DataTable dataTableVenta = ExecuteQuery(queryVenta, parameters);

                if (dataTableVenta.Rows.Count == 0)
                {
                    return null;
                }

                Venta venta = ConvertirDataRowAVenta(dataTableVenta.Rows[0]);

                // Obtener detalles de la venta
                string queryDetalles = @"
                    SELECT dv.idDetalleVenta, dv.idVenta, dv.idProducto, dv.cantidad, 
                           dv.precioUnitario, dv.descuento, dv.subtotal,
                           p.codigo, p.nombre as nombreProducto
                    FROM DetalleVenta dv
                    LEFT JOIN Producto p ON dv.idProducto = p.idProducto
                    WHERE dv.idVenta = :idVenta";

                DataTable dataTableDetalles = ExecuteQuery(queryDetalles, parameters);
                List<DetalleVenta> detalles = new List<DetalleVenta>();

                foreach (DataRow row in dataTableDetalles.Rows)
                {
                    var detalle = new DetalleVenta
                    {
                        IdDetalleVenta = Convert.ToInt32(row["idDetalleVenta"]),
                        IdVenta = Convert.ToInt32(row["idVenta"]),
                        IdProducto = Convert.ToInt32(row["idProducto"]),
                        Cantidad = Convert.ToInt32(row["cantidad"]),
                        PrecioUnitario = Convert.ToDouble(row["precioUnitario"]),
                        Descuento = Convert.ToDouble(row["descuento"]),
                        Subtotal = Convert.ToDouble(row["subtotal"])
                    };

                    // Crear objeto producto básico para el detalle
                    if (row["codigo"] != DBNull.Value)
                    {
                        detalle.Producto = new Producto
                        {
                            IdProducto = Convert.ToInt32(row["idProducto"]),
                            Codigo = row["codigo"].ToString(),
                            Nombre = row["nombreProducto"].ToString()
                        };
                    }

                    detalles.Add(detalle);
                }

                venta.Detalles = detalles;
                return venta;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener venta con ID {idVenta}");
                throw new Exception("Error al obtener venta por ID.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene una venta por su número de factura.
        /// </summary>
        /// <param name="numeroFactura">Número de factura.</param>
        /// <returns>La venta si se encuentra, null en caso contrario.</returns>
        public Venta ObtenerPorNumeroFactura(string numeroFactura)
        {
            try
            {
                string query = @"
                    SELECT v.idVenta, v.numeroFactura, v.fecha, v.idCliente, v.idEmpleado, 
                           v.idMetodoPago, v.subtotal, v.descuento, v.impuestos, v.total, 
                           v.observaciones, v.estado,
                           NVL(c.nombre || ' ' || c.apellido, 'Cliente no disponible') as nombreCliente,
                           NVL(e.nombre || ' ' || e.apellido, 'Empleado no disponible') as nombreEmpleado,
                           NVL(mp.nombre, 'Método no disponible') as nombreMetodoPago
                    FROM Venta v
                    LEFT JOIN Cliente c ON v.idCliente = c.idCliente
                    LEFT JOIN Empleado e ON v.idEmpleado = e.idEmpleado
                    LEFT JOIN MetodoPago mp ON v.idMetodoPago = mp.idMetodoPago
                    WHERE v.numeroFactura = :numeroFactura";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":numeroFactura", numeroFactura }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);

                if (dataTable.Rows.Count > 0)
                {
                    return ConvertirDataRowAVenta(dataTable.Rows[0]);
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener venta con número de factura {numeroFactura}");
                throw new Exception("Error al obtener venta por número de factura.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene ventas entre dos fechas.
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio.</param>
        /// <param name="fechaFin">Fecha de fin.</param>
        /// <returns>Lista de ventas en el rango de fechas.</returns>
        public List<Venta> ObtenerPorFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                string query = @"
                    SELECT v.idVenta, v.numeroFactura, v.fecha, v.idCliente, 
                           c.nombre as clienteNombre, c.apellido as clienteApellido,
                           v.idEmpleado, e.nombre as empleadoNombre, e.apellido as empleadoApellido,
                           v.idMetodoPago, mp.nombre as metodoPagoNombre,
                           v.subtotal, v.descuento, v.impuestos, v.total, v.observaciones, v.estado
                    FROM Venta v
                    LEFT JOIN Cliente c ON v.idCliente = c.idCliente
                    LEFT JOIN Empleado e ON v.idEmpleado = e.idEmpleado
                    LEFT JOIN MetodoPago mp ON v.idMetodoPago = mp.idMetodoPago
                    WHERE TRUNC(v.fecha) BETWEEN TRUNC(:fechaInicio) AND TRUNC(:fechaFin) -- Usar TRUNC para comparar solo fechas
                    ORDER BY v.fecha DESC";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":fechaInicio", fechaInicio.Date }, // Enviar solo la parte de la fecha
                    { ":fechaFin", fechaFin.Date }      // Enviar solo la parte de la fecha
                };

                DataTable dataTable = ExecuteQuery(query, parameters);
                List<Venta> ventas = new List<Venta>();

                foreach (DataRow row in dataTable.Rows)
                {
                    // ConvertirDataRowAVenta NO debería cargar detalles por sí mismo
                    // para evitar cargas N+1 si no se necesitan siempre.
                    ventas.Add(ConvertirDataRowAVenta(row));
                }

                // **** AÑADIR ESTE BUCLE PARA CARGAR LOS DETALLES DE CADA VENTA ****
                foreach (var venta in ventas)
                {
                    venta.Detalles = ObtenerDetallesPorVenta(venta.IdVenta);
                }
                // *******************************************************************

                return ventas;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener ventas entre {fechaInicio} y {fechaFin}");
                throw new Exception("Error al obtener ventas por fechas.", ex);
            }
            finally
            {
                CloseConnection(); // Asegúrate que CloseConnection() se llame solo si la conexión fue abierta por este método
            }
        }

        /// <summary>
        /// Obtiene los detalles (líneas de producto) para una venta específica.
        /// </summary>
        /// <param name="idVenta">El ID de la venta.</param>
        /// <returns>Una lista de objetos DetalleVenta.</returns>
        public List<DetalleVenta> ObtenerDetallesPorVenta(int idVenta)
        {
            List<DetalleVenta> detalles = new List<DetalleVenta>();
            try
            {
                string query = @"
                    SELECT dv.idDetalleVenta, dv.idVenta, dv.idProducto, 
                           p.codigo AS productoCodigo, p.nombre AS productoNombre, p.idCategoria, cat.nombre AS productoNombreCategoria,
                           dv.cantidad, dv.precioUnitario, dv.descuento, dv.subtotal
                    FROM DetalleVenta dv
                    INNER JOIN Producto p ON dv.idProducto = p.idProducto
                    LEFT JOIN Categoria cat ON p.idCategoria = cat.idCategoria
                    WHERE dv.idVenta = :idVenta
                    ORDER BY dv.idDetalleVenta";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idVenta", idVenta }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);

                foreach (DataRow row in dataTable.Rows)
                {
                    DetalleVenta detalle = new DetalleVenta
                    {
                        IdDetalleVenta = Convert.ToInt32(row["IDDETALLEVENTA"]),
                        IdVenta = Convert.ToInt32(row["IDVENTA"]),
                        IdProducto = Convert.ToInt32(row["IDPRODUCTO"]),
                        Cantidad = Convert.ToInt32(row["CANTIDAD"]),
                        PrecioUnitario = Convert.ToDouble(row["PRECIOUNITARIO"]),
                        Descuento = Convert.ToDouble(row["DESCUENTO"]),
                        Subtotal = Convert.ToDouble(row["SUBTOTAL"]),
                        Producto = new Producto // Poblar el objeto Producto anidado
                        {
                            IdProducto = Convert.ToInt32(row["IDPRODUCTO"]),
                            Codigo = row["PRODUCTOCODIGO"].ToString(),
                            Nombre = row["PRODUCTONOMBRE"].ToString(),
                            IdCategoria = Convert.ToInt32(row["IDCATEGORIA"]),
                            NombreCategoria = row.Table.Columns.Contains("PRODUCTONOMBRECATEGORIA") && row["PRODUCTONOMBRECATEGORIA"] != DBNull.Value
                                              ? row["PRODUCTONOMBRECATEGORIA"].ToString()
                                              : "Sin Categoría" // Valor por defecto si es null
                        }
                    };
                    detalles.Add(detalle);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener detalles de la venta {idVenta}");
                // No relanzar la excepción aquí podría ser una opción si quieres que la venta principal se cargue sin detalles en caso de error
                // pero para depuración, es mejor relanzarla o al menos loguear extensivamente.
                // throw new Exception($"Error al obtener detalles para la venta {idVenta}.", ex);
            }
            // finally // No cerrar la conexión aquí si es parte de una operación mayor como en ObtenerPorFechas
            // {
            //     CloseConnection(); 
            // }
            return detalles;
        }

        /// <summary>
        /// Obtiene ventas de un cliente específico.
        /// </summary>
        /// <param name="idCliente">ID del cliente.</param>
        /// <returns>Lista de ventas del cliente.</returns>
        public List<Venta> ObtenerPorCliente(int idCliente)
        {
            try
            {
                string query = @"
                    SELECT v.idVenta, v.numeroFactura, v.fecha, v.idCliente, v.idEmpleado, 
                           v.idMetodoPago, v.subtotal, v.descuento, v.impuestos, v.total, 
                           v.observaciones, v.estado,
                           NVL(c.nombre || ' ' || c.apellido, 'Cliente no disponible') as nombreCliente,
                           NVL(e.nombre || ' ' || e.apellido, 'Empleado no disponible') as nombreEmpleado,
                           NVL(mp.nombre, 'Método no disponible') as nombreMetodoPago
                    FROM Venta v
                    LEFT JOIN Cliente c ON v.idCliente = c.idCliente
                    LEFT JOIN Empleado e ON v.idEmpleado = e.idEmpleado
                    LEFT JOIN MetodoPago mp ON v.idMetodoPago = mp.idMetodoPago
                    WHERE v.idCliente = :idCliente
                    ORDER BY v.fecha DESC";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idCliente", idCliente }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);
                List<Venta> ventas = new List<Venta>();

                foreach (DataRow row in dataTable.Rows)
                {
                    ventas.Add(ConvertirDataRowAVenta(row));
                }

                return ventas;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener ventas del cliente {idCliente}");
                throw new Exception("Error al obtener ventas por cliente.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Crea una nueva venta en la base de datos.
        /// </summary>
        /// <param name="venta">Venta a crear.</param>
        /// <returns>ID de la venta creada.</returns>
        public int Crear(Venta venta)
        {
            try
            {
                BeginTransaction();

                // Obtener el próximo ID de venta
                int idVenta = GetNextSequenceValue("SEQ_VENTA");

                // Insertar la venta
                string queryVenta = @"
                    INSERT INTO Venta (idVenta, numeroFactura, fecha, idCliente, idEmpleado, 
                                     idMetodoPago, subtotal, descuento, impuestos, total, 
                                     observaciones, estado)
                    VALUES (:idVenta, :numeroFactura, :fecha, :idCliente, :idEmpleado, 
                           :idMetodoPago, :subtotal, :descuento, :impuestos, :total, 
                           :observaciones, :estado)";

                Dictionary<string, object> parametersVenta = new Dictionary<string, object>
                {
                    { ":idVenta", idVenta },
                    { ":numeroFactura", venta.NumeroFactura },
                    { ":fecha", venta.Fecha },
                    { ":idCliente", venta.IdCliente },
                    { ":idEmpleado", venta.IdEmpleado },
                    { ":idMetodoPago", venta.IdMetodoPago },
                    { ":subtotal", venta.Subtotal },
                    { ":descuento", venta.Descuento },
                    { ":impuestos", venta.Impuestos },
                    { ":total", venta.Total },
                    { ":observaciones", venta.Observaciones },
                    { ":estado", venta.Estado.ToString() }
                };

                ExecuteNonQuery(queryVenta, parametersVenta);

                // Insertar detalles de la venta
                if (venta.Detalles != null && venta.Detalles.Count > 0)
                {
                    string queryDetalle = @"
                        INSERT INTO DetalleVenta (idDetalleVenta, idVenta, idProducto, cantidad, 
                                                precioUnitario, descuento, subtotal)
                        VALUES (:idDetalleVenta, :idVenta, :idProducto, :cantidad, 
                               :precioUnitario, :descuento, :subtotal)";

                    foreach (var detalle in venta.Detalles)
                    {
                        int idDetalleVenta = GetNextSequenceValue("SEQ_DETALLE_VENTA");

                        Dictionary<string, object> parametersDetalle = new Dictionary<string, object>
                        {
                            { ":idDetalleVenta", idDetalleVenta },
                            { ":idVenta", idVenta },
                            { ":idProducto", detalle.IdProducto },
                            { ":cantidad", detalle.Cantidad },
                            { ":precioUnitario", detalle.PrecioUnitario },
                            { ":descuento", detalle.Descuento },
                            { ":subtotal", detalle.Subtotal }
                        };

                        ExecuteNonQuery(queryDetalle, parametersDetalle);
                    }
                }

                CommitTransaction();
                return idVenta;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Logger.LogException(ex, $"Error al crear venta {venta.NumeroFactura}");
                throw new Exception("Error al crear venta.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Actualiza una venta existente.
        /// </summary>
        /// <param name="venta">Venta con los datos actualizados.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool Actualizar(Venta venta)
        {
            try
            {
                string query = @"
                    UPDATE Venta
                    SET numeroFactura = :numeroFactura,
                        fecha = :fecha,
                        idCliente = :idCliente,
                        idEmpleado = :idEmpleado,
                        idMetodoPago = :idMetodoPago,
                        subtotal = :subtotal,
                        descuento = :descuento,
                        impuestos = :impuestos,
                        total = :total,
                        observaciones = :observaciones,
                        estado = :estado
                    WHERE idVenta = :idVenta";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":numeroFactura", venta.NumeroFactura },
                    { ":fecha", venta.Fecha },
                    { ":idCliente", venta.IdCliente },
                    { ":idEmpleado", venta.IdEmpleado },
                    { ":idMetodoPago", venta.IdMetodoPago },
                    { ":subtotal", venta.Subtotal },
                    { ":descuento", venta.Descuento },
                    { ":impuestos", venta.Impuestos },
                    { ":total", venta.Total },
                    { ":observaciones", venta.Observaciones },
                    { ":estado", venta.Estado.ToString() },
                    { ":idVenta", venta.IdVenta }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al actualizar venta ID {venta.IdVenta}");
                throw new Exception("Error al actualizar venta.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Actualiza el estado de una venta.
        /// </summary>
        /// <param name="idVenta">ID de la venta.</param>
        /// <param name="nuevoEstado">Nuevo estado de la venta.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool ActualizarEstado(int idVenta, char nuevoEstado)
        {
            try
            {
                string query = @"
                    UPDATE Venta
                    SET estado = :estado
                    WHERE idVenta = :idVenta";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":estado", nuevoEstado.ToString() },
                    { ":idVenta", idVenta }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al actualizar estado de venta ID {idVenta}");
                throw new Exception("Error al actualizar estado de venta.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene el total de ventas en un período.
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio.</param>
        /// <param name="fechaFin">Fecha de fin.</param>
        /// <returns>Total de ventas en el período.</returns>
        public double ObtenerTotalVentas(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                string query = @"
                    SELECT NVL(SUM(total), 0) as totalVentas
                    FROM Venta
                    WHERE fecha BETWEEN :fechaInicio AND :fechaFin
                    AND estado = 'C'";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":fechaInicio", fechaInicio },
                    { ":fechaFin", fechaFin }
                };

                object result = ExecuteScalar(query, parameters);
                return Convert.ToDouble(result);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener total de ventas entre {fechaInicio} y {fechaFin}");
                throw new Exception("Error al obtener total de ventas.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene la última secuencia utilizada para generar números de factura.
        /// </summary>
        /// <returns>Última secuencia utilizada.</returns>
        public int ObtenerUltimaSecuencia()
        {
            try
            {
                string query = @"
                    SELECT NVL(MAX(TO_NUMBER(SUBSTR(numeroFactura, -4))), 0) as ultimaSecuencia
                    FROM Venta
                    WHERE numeroFactura LIKE 'F-' || TO_CHAR(SYSDATE, 'YYYYMMDD') || '-%'";

                object result = ExecuteScalar(query);
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener última secuencia de factura");
                throw new Exception("Error al obtener última secuencia.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Registra comisión para un empleado por una venta.
        /// </summary>
        /// <param name="idEmpleado">ID del empleado.</param>
        /// <param name="idVenta">ID de la venta.</param>
        /// <param name="comision">Monto de la comisión.</param>
        public void RegistrarComision(int idEmpleado, int idVenta, double comision)
        {
            try
            {
                // Implementar lógica para registrar comisión
                // Por ahora solo log informativo
                Logger.LogInfo($"Comisión registrada: Empleado {idEmpleado}, Venta {idVenta}, Monto {comision:C}");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al registrar comisión para empleado {idEmpleado}");
            }
        }

        /// <summary>
        /// Actualiza comisión para un empleado por una venta.
        /// </summary>
        /// <param name="idEmpleado">ID del empleado.</param>
        /// <param name="idVenta">ID de la venta.</param>
        /// <param name="nuevaComision">Nuevo monto de la comisión.</param>
        public void ActualizarComision(int idEmpleado, int idVenta, double nuevaComision)
        {
            try
            {
                // Implementar lógica para actualizar comisión
                // Por ahora solo log informativo
                Logger.LogInfo($"Comisión actualizada: Empleado {idEmpleado}, Venta {idVenta}, Nuevo monto {nuevaComision:C}");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al actualizar comisión para empleado {idEmpleado}");
            }
        }

        /// <summary>
        /// Anula comisión para un empleado por una venta.
        /// </summary>
        /// <param name="idEmpleado">ID del empleado.</param>
        /// <param name="idVenta">ID de la venta.</param>
        public void AnularComision(int idEmpleado, int idVenta)
        {
            try
            {
                // Implementar lógica para anular comisión
                // Por ahora solo log informativo
                Logger.LogInfo($"Comisión anulada: Empleado {idEmpleado}, Venta {idVenta}");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al anular comisión para empleado {idEmpleado}");
            }
        }

        /// <summary>
        /// Convierte un DataRow a un objeto Venta.
        /// </summary>
        /// <param name="row">DataRow con los datos de la venta.</param>
        /// <returns>Objeto Venta con los datos del DataRow.</returns>
        // ElectroTech/DataAccess/VentaRepository.cs
        /// <summary>
        /// Convierte un DataRow a un objeto Venta.
        /// </summary>
        /// <param name="row">DataRow con los datos de la venta.</param>
        /// <returns>Objeto Venta con los datos del DataRow.</returns>
        private Venta ConvertirDataRowAVenta(DataRow row)
        {
            var venta = new Venta
            {
                IdVenta = Convert.ToInt32(row["IDVENTA"]),
                NumeroFactura = row["NUMEROFACTURA"].ToString(),
                Fecha = Convert.ToDateTime(row["FECHA"]),
                IdCliente = Convert.ToInt32(row["IDCLIENTE"]),
                IdEmpleado = Convert.ToInt32(row["IDEMPLEADO"]),
                IdMetodoPago = Convert.ToInt32(row["IDMETODOPAGO"]),
                Subtotal = Convert.ToDouble(row["SUBTOTAL"]),
                Descuento = Convert.ToDouble(row["DESCUENTO"]),
                Impuestos = Convert.ToDouble(row["IMPUESTOS"]),
                Total = Convert.ToDouble(row["TOTAL"]),
                Observaciones = row.Table.Columns.Contains("OBSERVACIONES") && row["OBSERVACIONES"] != DBNull.Value ? row["OBSERVACIONES"].ToString() : null,
                Estado = row["ESTADO"].ToString()[0]
                // Detalles se cargará por separado
            };

            if (row.Table.Columns.Contains("CLIENTENOMBRE") && row["CLIENTENOMBRE"] != DBNull.Value &&
                row.Table.Columns.Contains("CLIENTEAPELLIDO") && row["CLIENTEAPELLIDO"] != DBNull.Value)
            {
                venta.Cliente = new Cliente
                {
                    IdCliente = venta.IdCliente,
                    Nombre = row["CLIENTENOMBRE"].ToString(),
                    Apellido = row["CLIENTEAPELLIDO"].ToString()
                };
            }

            if (row.Table.Columns.Contains("EMPLEADONOMBRE") && row["EMPLEADONOMBRE"] != DBNull.Value &&
                row.Table.Columns.Contains("EMPLEADOAPELLIDO") && row["EMPLEADOAPELLIDO"] != DBNull.Value)
            {
                venta.Empleado = new Empleado
                {
                    IdEmpleado = venta.IdEmpleado,
                    Nombre = row["EMPLEADONOMBRE"].ToString(),
                    Apellido = row["EMPLEADOAPELLIDO"].ToString()
                };
            }

            if (row.Table.Columns.Contains("METODOPAGONOMBRE") && row["METODOPAGONOMBRE"] != DBNull.Value)
            {
                venta.MetodoPagoNombre = row["METODOPAGONOMBRE"].ToString();
            }

            return venta;
        }
    }
}