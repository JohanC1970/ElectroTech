using ElectroTech.Helpers;
using ElectroTech.Models;
using Oracle.ManagedDataAccess.Client;
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
                    SELECT v.idVenta, v.numeroFactura, v.fecha, v.idCliente, 
                           c.nombre as clienteNombre, c.apellido as clienteApellido,
                           v.idEmpleado, e.nombre as empleadoNombre, e.apellido as empleadoApellido,
                           v.idMetodoPago, mp.nombre as metodoPagoNombre,
                           v.subtotal, v.descuento, v.impuestos, v.total, v.observaciones, v.estado
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

                // Cargar detalles para cada venta
                foreach (var venta in ventas)
                {
                    venta.Detalles = ObtenerDetallesPorVenta(venta.IdVenta);
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
        /// Obtiene las ventas entre dos fechas.
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

                // Cargar detalles para cada venta
                foreach (var venta in ventas)
                {
                    venta.Detalles = ObtenerDetallesPorVenta(venta.IdVenta);
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
        /// Obtiene las ventas de un cliente específico.
        /// </summary>
        /// <param name="idCliente">ID del cliente.</param>
        /// <returns>Lista de ventas del cliente.</returns>
        public List<Venta> ObtenerPorCliente(int idCliente)
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

                // Cargar detalles para cada venta
                foreach (var venta in ventas)
                {
                    venta.Detalles = ObtenerDetallesPorVenta(venta.IdVenta);
                }

                return ventas;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener ventas para el cliente {idCliente}");
                throw new Exception("Error al obtener ventas por cliente.", ex);
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
                    SELECT v.idVenta, v.numeroFactura, v.fecha, v.idCliente, v.idEmpleado, v.idMetodoPago,
                           v.subtotal, v.descuento, v.impuestos, v.total, v.observaciones, v.estado,
                           c.nombre as nombreCliente, c.apellido as apellidoCliente, c.tipoDocumento, c.numeroDocumento,
                           e.nombre as nombreEmpleado, e.apellido as apellidoEmpleado,
                           mp.nombre as nombreMetodoPago
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
                    Venta venta = ConvertirDataRowAVenta(dataTable.Rows[0]);

                    // Obtener los detalles de la venta
                    venta.Detalles = ObtenerDetallesVenta(venta.IdVenta);

                    return venta;
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
        /// Obtiene ventas por estado.
        /// </summary>
        /// <param name="estado">Estado de las ventas a obtener ('C': Completada, 'A': Anulada, 'P': Pendiente).</param>
        /// <returns>Lista de ventas con el estado especificado.</returns>
        public List<Venta> ObtenerVentasPorEstado(char estado)
        {
            try
            {
                string query = @"
                    SELECT v.idVenta, v.numeroFactura, v.fecha, v.idCliente, v.idEmpleado, v.idMetodoPago,
                           v.subtotal, v.descuento, v.impuestos, v.total, v.observaciones, v.estado,
                           c.nombre as nombreCliente, c.apellido as apellidoCliente, c.tipoDocumento, c.numeroDocumento,
                           e.nombre as nombreEmpleado, e.apellido as apellidoEmpleado,
                           mp.nombre as nombreMetodoPago
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
                    Venta venta = ConvertirDataRowAVenta(row);
                    ventas.Add(venta);
                }

                return ventas;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener ventas con estado {estado}");
                throw new Exception("Error al obtener ventas por estado.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene una venta por su ID.
        /// </summary>
        /// <param name="idVenta">ID de la venta.</param>
        /// <returns>El objeto Venta si se encuentra, null en caso contrario.</returns>
        public Venta ObtenerPorId(int idVenta)
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
                    WHERE v.idVenta = :idVenta";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idVenta", idVenta }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);

                if (dataTable.Rows.Count > 0)
                {
                    Venta venta = ConvertirDataRowAVenta(dataTable.Rows[0]);
                    venta.Detalles = ObtenerDetallesPorVenta(idVenta);
                    return venta;
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener venta {idVenta}");
                throw new Exception("Error al obtener venta por ID.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene los detalles de una venta específica.
        /// </summary>
        /// <param name="idVenta">ID de la venta.</param>
        /// <returns>Lista de detalles de la venta.</returns>
        public List<DetalleVenta> ObtenerDetallesPorVenta(int idVenta)
        {
            try
            {
                string query = @"
                    SELECT dv.idDetalleVenta, dv.idVenta, dv.idProducto, 
                           p.codigo as productocodigo, p.nombre as productoNombre,
                           dv.cantidad, dv.precioUnitario, dv.descuento, dv.subtotal
                    FROM DetalleVenta dv
                    LEFT JOIN Producto p ON dv.idProducto = p.idProducto
                    WHERE dv.idVenta = :idVenta
                    ORDER BY dv.idDetalleVenta";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idVenta", idVenta }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);
                List<DetalleVenta> detalles = new List<DetalleVenta>();

                foreach (DataRow row in dataTable.Rows)
                {
                    detalles.Add(new DetalleVenta
                    {
                        IdDetalleVenta = Convert.ToInt32(row["idDetalleVenta"]),
                        IdVenta = Convert.ToInt32(row["idVenta"]),
                        IdProducto = Convert.ToInt32(row["idProducto"]),
                        Producto = new Producto
                        {
                            IdProducto = Convert.ToInt32(row["idProducto"]),
                            Codigo = row["productocodigo"].ToString(),
                            Nombre = row["productoNombre"].ToString()
                        },
                        Cantidad = Convert.ToInt32(row["cantidad"]),
                        PrecioUnitario = Convert.ToDouble(row["precioUnitario"]),
                        Descuento = Convert.ToDouble(row["descuento"]),
                        Subtotal = Convert.ToDouble(row["subtotal"])
                    });
                }

                return detalles;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener detalles de la venta {idVenta}");
                throw new Exception("Error al obtener detalles de venta.", ex);
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
                string query = @"
                    INSERT INTO Venta (idVenta, numeroFactura, fecha, idCliente, idEmpleado, 
                                      idMetodoPago, subtotal, descuento, impuestos, total, 
                                      observaciones, estado)
                    VALUES (:idVenta, :numeroFactura, :fecha, :idCliente, :idEmpleado, 
                           :idMetodoPago, :subtotal, :descuento, :impuestos, :total, 
                           :observaciones, :estado)";

                Dictionary<string, object> parameters = new Dictionary<string, object>
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
                    { ":observaciones", venta.Observaciones ?? (object)DBNull.Value },
                    { ":estado", venta.Estado.ToString() }
                };

                ExecuteNonQuery(query, parameters);

                // Insertar los detalles de la venta
                foreach (var detalle in venta.Detalles)
                {
                    string detalleQuery = @"
                        INSERT INTO DetalleVenta (idDetalleVenta, idVenta, idProducto, cantidad, 
                                               precioUnitario, descuento, subtotal)
                        VALUES (SEQ_DETALLE_VENTA.NEXTVAL, :idVenta, :idProducto, :cantidad, 
                               :precioUnitario, :descuento, :subtotal)";

                    Dictionary<string, object> detalleParams = new Dictionary<string, object>
                    {
                        { ":idVenta", idVenta },
                        { ":idProducto", detalle.IdProducto },
                        { ":cantidad", detalle.Cantidad },
                        { ":precioUnitario", detalle.PrecioUnitario },
                        { ":descuento", detalle.Descuento },
                        { ":subtotal", detalle.Subtotal }
                    };

                    ExecuteNonQuery(detalleQuery, detalleParams);
                }

                CommitTransaction();
                return idVenta;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Logger.LogException(ex, "Error al crear venta");
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
                BeginTransaction();

                // Verificar si la venta existe
                string queryVerificar = "SELECT COUNT(*) FROM Venta WHERE idVenta = :idVenta";
                Dictionary<string, object> paramsVerificar = new Dictionary<string, object>
                {
                    { ":idVenta", venta.IdVenta }
                };

                int count = Convert.ToInt32(ExecuteScalar(queryVerificar, paramsVerificar));
                if (count == 0)
                {
                    throw new Exception($"La venta con ID {venta.IdVenta} no existe.");
                }

                // Actualizar la venta
                string query = @"
                    UPDATE Venta SET 
                        numeroFactura = :numeroFactura,
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
                    { ":observaciones", venta.Observaciones ?? (object)DBNull.Value },
                    { ":estado", venta.Estado.ToString() },
                    { ":idVenta", venta.IdVenta }
                };

                ExecuteNonQuery(query, parameters);

                // Eliminar detalles existentes
                string queryEliminar = "DELETE FROM DetalleVenta WHERE idVenta = :idVenta";
                Dictionary<string, object> paramsEliminar = new Dictionary<string, object>
                {
                    { ":idVenta", venta.IdVenta }
                };

                ExecuteNonQuery(queryEliminar, paramsEliminar);

                // Insertar nuevos detalles
                foreach (var detalle in venta.Detalles)
                {
                    string detalleQuery = @"
                        INSERT INTO DetalleVenta (idDetalleVenta, idVenta, idProducto, cantidad, 
                                               precioUnitario, descuento, subtotal)
                        VALUES (SEQ_DETALLE_VENTA.NEXTVAL, :idVenta, :idProducto, :cantidad, 
                               :precioUnitario, :descuento, :subtotal)";

                    Dictionary<string, object> detalleParams = new Dictionary<string, object>
                    {
                        { ":idVenta", venta.IdVenta },
                        { ":idProducto", detalle.IdProducto },
                        { ":cantidad", detalle.Cantidad },
                        { ":precioUnitario", detalle.PrecioUnitario },
                        { ":descuento", detalle.Descuento },
                        { ":subtotal", detalle.Subtotal }
                    };

                    ExecuteNonQuery(detalleQuery, detalleParams);
                }

                CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Logger.LogException(ex, $"Error al actualizar venta {venta.IdVenta}");
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
        /// <param name="estado">Nuevo estado ('C' = Completada, 'A' = Anulada, 'P' = Pendiente).</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool ActualizarEstado(int idVenta, char estado)
        {
            try
            {
                string query = "UPDATE Venta SET estado = :estado WHERE idVenta = :idVenta";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":estado", estado.ToString() },
                    { ":idVenta", idVenta }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al actualizar estado de la venta {idVenta}");
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
        /// <returns>Suma total de ventas en el período.</returns>
        public double ObtenerTotalVentas(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                string query = @"
                    SELECT SUM(total) 
                    FROM Venta 
                    WHERE fecha BETWEEN :fechaInicio AND :fechaFin
                    AND estado = 'C'";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":fechaInicio", fechaInicio },
                    { ":fechaFin", fechaFin }
                };

                object result = ExecuteScalar(query, parameters);
                return result != DBNull.Value ? Convert.ToDouble(result) : 0;
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
        /// Obtiene la última secuencia de número de factura.
        /// </summary>
        /// <returns>Último número de secuencia.</returns>
        public int ObtenerUltimaSecuencia()
        {
            try
            {
                string query = @"
                    SELECT MAX(TO_NUMBER(SUBSTR(numeroFactura, INSTR(numeroFactura, '-', 1, 2) + 1)))
                    FROM Venta
                    WHERE numeroFactura LIKE 'F-%'";

                object result = ExecuteScalar(query);
                return result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener última secuencia de factura");
                return 0;
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Registra una comisión para un empleado por una venta.
        /// </summary>
        /// <param name="idEmpleado">ID del empleado.</param>
        /// <param name="idVenta">ID de la venta.</param>
        /// <param name="comision">Monto de la comisión.</param>
        /// <returns>True si el registro es exitoso, False en caso contrario.</returns>
        public bool RegistrarComision(int idEmpleado, int idVenta, double comision)
        {
            try
            {
                // Esta tabla de comisiones debería crearse en la base de datos si no existe
                string query = @"
                    INSERT INTO Comision (idComision, idEmpleado, idVenta, monto, fecha)
                    VALUES (SEQ_COMISION.NEXTVAL, :idEmpleado, :idVenta, :monto, SYSDATE)";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idEmpleado", idEmpleado },
                    { ":idVenta", idVenta },
                    { ":monto", comision }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al registrar comisión para empleado {idEmpleado} por venta {idVenta}");
                // Si falla, no debe interrumpir el flujo principal
                return false; // Registramos el error pero retornamos false sin lanzar excepción
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Actualiza la comisión de un empleado por una venta.
        /// </summary>
        /// <param name="idEmpleado">ID del empleado.</param>
        /// <param name="idVenta">ID de la venta.</param>
        /// <param name="comision">Nuevo monto de la comisión.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool ActualizarComision(int idEmpleado, int idVenta, double comision)
        {
            try
            {
                string query = @"
                    UPDATE Comision 
                    SET monto = :monto 
                    WHERE idEmpleado = :idEmpleado AND idVenta = :idVenta";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":monto", comision },
                    { ":idEmpleado", idEmpleado },
                    { ":idVenta", idVenta }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);

                // Si no encontró registros, lo insertamos
                if (rowsAffected == 0)
                {
                    return RegistrarComision(idEmpleado, idVenta, comision);
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al actualizar comisión para empleado {idEmpleado} por venta {idVenta}");
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Anula la comisión de un empleado por una venta.
        /// </summary>
        /// <param name="idEmpleado">ID del empleado.</param>
        /// <param name="idVenta">ID de la venta.</param>
        /// <returns>True si la anulación es exitosa, False en caso contrario.</returns>
        public bool AnularComision(int idEmpleado, int idVenta)
        {
            try
            {
                string query = @"
                    DELETE FROM Comision 
                    WHERE idEmpleado = :idEmpleado AND idVenta = :idVenta";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idEmpleado", idEmpleado },
                    { ":idVenta", idVenta }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al anular comisión para empleado {idEmpleado} por venta {idVenta}");
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene los detalles de una venta.
        /// </summary>
        /// <param name="idVenta">ID de la venta.</param>
        /// <returns>Lista de detalles de la venta.</returns>
        private List<DetalleVenta> ObtenerDetallesVenta(int idVenta)
        {
            try
            {
                string query = @"
                    SELECT dv.idDetalleVenta, dv.idVenta, dv.idProducto, dv.cantidad, 
                           dv.precioUnitario, dv.descuento, dv.subtotal,
                           p.codigo, p.nombre, p.descripcion
                    FROM DetalleVenta dv
                    LEFT JOIN Producto p ON dv.idProducto = p.idProducto
                    WHERE dv.idVenta = :idVenta";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idVenta", idVenta }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);
                List<DetalleVenta> detalles = new List<DetalleVenta>();

                foreach (DataRow row in dataTable.Rows)
                {
                    DetalleVenta detalle = new DetalleVenta
                    {
                        IdDetalleVenta = Convert.ToInt32(row["idDetalleVenta"]),
                        IdVenta = Convert.ToInt32(row["idVenta"]),
                        IdProducto = Convert.ToInt32(row["idProducto"]),
                        Cantidad = Convert.ToInt32(row["cantidad"]),
                        PrecioUnitario = Convert.ToDouble(row["precioUnitario"]),
                        Descuento = Convert.ToDouble(row["descuento"]),
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
                Logger.LogException(ex, $"Error al obtener detalles de la venta {idVenta}");
                throw new Exception("Error al obtener detalles de venta.", ex);
            }
        }


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

            // Configurar propiedades de navegación para Cliente si hay datos
            if (row["clienteNombre"] != DBNull.Value && row["clienteApellido"] != DBNull.Value)
            {
                venta.Cliente = new Cliente
                {
                    IdCliente = venta.IdCliente,
                    Nombre = row["clienteNombre"].ToString(),
                    Apellido = row["clienteApellido"].ToString()
                };
            }

            // Configurar propiedades de navegación para Empleado si hay datos
            if (row["empleadoNombre"] != DBNull.Value && row["empleadoApellido"] != DBNull.Value)
            {
                venta.Empleado = new Empleado
                {
                    IdEmpleado = venta.IdEmpleado,
                    Nombre = row["empleadoNombre"].ToString(),
                    Apellido = row["empleadoApellido"].ToString()
                };
            }

            // Configurar nombre del método de pago
            if (row["metodoPagoNombre"] != DBNull.Value)
            {
                venta.MetodoPagoNombre = row["metodoPagoNombre"].ToString();
            }

            return venta;
        }
    }
}