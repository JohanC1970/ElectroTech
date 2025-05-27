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
                    WHERE v.fecha BETWEEN :fechaInicio AND :fechaFin
                    ORDER BY v.fecha DESC";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":fechaInicio", fechaInicio },
                    { ":fechaFin", fechaFin }
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
                Logger.LogException(ex, $"Error al obtener ventas entre {fechaInicio} y {fechaFin}");
                throw new Exception("Error al obtener ventas por fechas.", ex);
            }
            finally
            {
                CloseConnection();
            }
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
                IdVenta = Convert.ToInt32(row["idVenta"]),
                NumeroFactura = row["numeroFactura"].ToString(),
                Fecha = Convert.ToDateTime(row["fecha"]),
                IdCliente = Convert.ToInt32(row["idCliente"]),
                IdEmpleado = Convert.ToInt32(row["idEmpleado"]),
                IdMetodoPago = Convert.ToInt32(row["idMetodoPago"]),
                Subtotal = Convert.ToDouble(row["subtotal"]),
                Descuento = Convert.ToDouble(row["descuento"]),
                Impuestos = Convert.ToDouble(row["impuestos"]),
                Total = Convert.ToDouble(row["total"]),
                Observaciones = row["observaciones"] != DBNull.Value ? row["observaciones"].ToString() : null,
                Estado = row["estado"].ToString()[0]
            };

            try
            {
                // Verificar si las columnas de navegación existen en el DataRow
                if (row.Table.Columns.Contains("nombreCliente") && row["nombreCliente"] != DBNull.Value)
                {
                    venta.Cliente = new Cliente
                    {
                        IdCliente = venta.IdCliente,
                        Nombre = row["nombreCliente"].ToString() // Asignar a Nombre, no a NombreCompleto
                    };
                }

                if (row.Table.Columns.Contains("nombreEmpleado") && row["nombreEmpleado"] != DBNull.Value)
                {
                    venta.Empleado = new Empleado
                    {
                        IdEmpleado = venta.IdEmpleado,
                        Nombre = row["nombreEmpleado"].ToString() // Asignar a Nombre, no a NombreCompleto
                    };
                }

                if (row.Table.Columns.Contains("nombreMetodoPago") && row["nombreMetodoPago"] != DBNull.Value)
                {
                    venta.MetodoPagoNombre = row["nombreMetodoPago"].ToString();
                }
            }
            catch (Exception ex)
            {
                // Log warning pero no fallar por problemas de navegación
                Logger.LogWarning($"Error al establecer propiedades de navegación para venta ID {venta.IdVenta}: {ex.Message}");
            }

            return venta;
        }
    }
}