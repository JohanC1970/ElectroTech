
using ElectroTech.Helpers;
using ElectroTech.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace ElectroTech.DataAccess
{
    /// <summary>
    /// Repositorio para operaciones de base de datos relacionadas con la entidad Devolucion.
    /// </summary>
    public class DevolucionRepository : DatabaseRepository
    {
        /// <summary>
        /// Obtiene todas las devoluciones.
        /// </summary>
        /// <returns>Lista de devoluciones.</returns>
        public List<Devolucion> ObtenerTodas()
        {
            try
            {
                string query = @"
                    SELECT d.idDevolucion, d.idVenta, d.fecha, d.motivo, d.montoDevuelto, d.estado,
                           v.numeroFactura
                    FROM Devolucion d
                    LEFT JOIN Venta v ON d.idVenta = v.idVenta
                    ORDER BY d.fecha DESC";

                DataTable dataTable = ExecuteQuery(query);
                List<Devolucion> devoluciones = new List<Devolucion>();

                foreach (DataRow row in dataTable.Rows)
                {
                    devoluciones.Add(ConvertirDataRowADevolucion(row));
                }

                return devoluciones;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener todas las devoluciones");
                throw new Exception("Error al obtener devoluciones.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene una devolución por su ID.
        /// </summary>
        /// <param name="idDevolucion">ID de la devolución.</param>
        /// <returns>La devolución si se encuentra, null en caso contrario.</returns>
        public Devolucion ObtenerPorId(int idDevolucion)
        {
            try
            {
                string query = @"
                    SELECT d.idDevolucion, d.idVenta, d.fecha, d.motivo, d.montoDevuelto, d.estado,
                           v.numeroFactura
                    FROM Devolucion d
                    LEFT JOIN Venta v ON d.idVenta = v.idVenta
                    WHERE d.idDevolucion = :idDevolucion";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idDevolucion", idDevolucion }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);

                if (dataTable.Rows.Count > 0)
                {
                    return ConvertirDataRowADevolucion(dataTable.Rows[0]);
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener devolución con ID {idDevolucion}");
                throw new Exception("Error al obtener devolución por ID.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene las devoluciones de una venta específica.
        /// </summary>
        /// <param name="idVenta">ID de la venta.</param>
        /// <returns>Lista de devoluciones de la venta.</returns>
        public List<Devolucion> ObtenerPorVenta(int idVenta)
        {
            try
            {
                string query = @"
                    SELECT d.idDevolucion, d.idVenta, d.fecha, d.motivo, d.montoDevuelto, d.estado,
                           v.numeroFactura
                    FROM Devolucion d
                    LEFT JOIN Venta v ON d.idVenta = v.idVenta
                    WHERE d.idVenta = :idVenta
                    ORDER BY d.fecha DESC";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idVenta", idVenta }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);
                List<Devolucion> devoluciones = new List<Devolucion>();

                foreach (DataRow row in dataTable.Rows)
                {
                    devoluciones.Add(ConvertirDataRowADevolucion(row));
                }

                return devoluciones;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener devoluciones de la venta {idVenta}");
                throw new Exception("Error al obtener devoluciones por venta.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene los IDs de ventas que ya tienen devoluciones asociadas.
        /// </summary>
        /// <returns>Lista de IDs de ventas con devoluciones.</returns>
        public List<int> ObtenerVentasConDevolucion()
        {
            try
            {
                string query = "SELECT DISTINCT idVenta FROM Devolucion";

                DataTable dataTable = ExecuteQuery(query);
                List<int> ventasConDevolucion = new List<int>();

                foreach (DataRow row in dataTable.Rows)
                {
                    ventasConDevolucion.Add(Convert.ToInt32(row["idVenta"]));
                }

                return ventasConDevolucion;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener ventas con devolución");
                throw new Exception("Error al obtener ventas con devolución.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Crea una nueva devolución en la base de datos.
        /// </summary>
        /// <param name="devolucion">Devolución a crear.</param>
        /// <returns>ID de la devolución creada.</returns>
        public int Crear(Devolucion devolucion)
        {
            try
            {
                BeginTransaction();

                // Obtener el próximo ID de devolución
                int idDevolucion = GetNextSequenceValue("SEQ_DEVOLUCION");

                // Insertar la devolución
                string query = @"
                    INSERT INTO Devolucion (idDevolucion, idVenta, fecha, motivo, montoDevuelto, estado)
                    VALUES (:idDevolucion, :idVenta, :fecha, :motivo, :montoDevuelto, :estado)";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idDevolucion", idDevolucion },
                    { ":idVenta", devolucion.IdVenta },
                    { ":fecha", devolucion.Fecha },
                    { ":motivo", devolucion.Motivo },
                    { ":montoDevuelto", devolucion.MontoDevuelto },
                    { ":estado", devolucion.Estado.ToString() }
                };

                ExecuteNonQuery(query, parameters);
                CommitTransaction();

                return idDevolucion;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Logger.LogException(ex, $"Error al crear devolución para la venta {devolucion.IdVenta}");
                throw new Exception("Error al crear devolución.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Actualiza una devolución existente.
        /// </summary>
        /// <param name="devolucion">Devolución con los datos actualizados.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool Actualizar(Devolucion devolucion)
        {
            try
            {
                string query = @"
                    UPDATE Devolucion
                    SET fecha = :fecha,
                        motivo = :motivo,
                        montoDevuelto = :montoDevuelto,
                        estado = :estado
                    WHERE idDevolucion = :idDevolucion";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":fecha", devolucion.Fecha },
                    { ":motivo", devolucion.Motivo },
                    { ":montoDevuelto", devolucion.MontoDevuelto },
                    { ":estado", devolucion.Estado.ToString() },
                    { ":idDevolucion", devolucion.IdDevolucion }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al actualizar devolución ID {devolucion.IdDevolucion}");
                throw new Exception("Error al actualizar devolución.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Convierte un DataRow a un objeto Devolucion.
        /// </summary>
        /// <param name="row">DataRow con los datos de la devolución.</param>
        /// <returns>Objeto Devolucion con los datos del DataRow.</returns>
        private Devolucion ConvertirDataRowADevolucion(DataRow row)
        {
            var devolucion = new Devolucion
            {
                IdDevolucion = Convert.ToInt32(row["idDevolucion"]),
                IdVenta = Convert.ToInt32(row["idVenta"]),
                Fecha = Convert.ToDateTime(row["fecha"]),
                Motivo = row["motivo"].ToString(),
                MontoDevuelto = Convert.ToDouble(row["montoDevuelto"]),
                Estado = row["estado"].ToString()[0]
            };

            // Si hay una venta relacionada, guardar el número de factura para referencia
            if (row["numeroFactura"] != DBNull.Value)
            {
                // Como no tenemos una propiedad específica para esto en el modelo,
                // podríamos asignar la Venta si necesitáramos este dato
                // devolucion.Venta = new Venta { NumeroFactura = row["numeroFactura"].ToString() };
            }

            return devolucion;
        }
    }
}
