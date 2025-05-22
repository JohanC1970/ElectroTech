using ElectroTech.Helpers;
using ElectroTech.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace ElectroTech.DataAccess
{
    /// <summary>
    /// Repositorio para operaciones de base de datos relacionadas con la entidad MetodoPago.
    /// </summary>
    public class MetodoPagoRepository : DatabaseRepository
    {
        /// <summary>
        /// Obtiene todos los métodos de pago.
        /// </summary>
        /// <returns>Lista de métodos de pago.</returns>
        public List<MetodoPago> ObtenerTodos()
        {
            try
            {
                string query = @"
                    SELECT idMetodoPago, nombre, descripcion, activo
                    FROM MetodoPago
                    ORDER BY nombre";

                DataTable dataTable = ExecuteQuery(query);
                List<MetodoPago> metodosPago = new List<MetodoPago>();

                foreach (DataRow row in dataTable.Rows)
                {
                    metodosPago.Add(ConvertirDataRowAMetodoPago(row));
                }

                return metodosPago;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener todos los métodos de pago");
                throw new Exception("Error al obtener métodos de pago.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene todos los métodos de pago activos.
        /// </summary>
        /// <returns>Lista de métodos de pago activos.</returns>
        public List<MetodoPago> ObtenerActivos()
        {
            try
            {
                string query = @"
                    SELECT idMetodoPago, nombre, descripcion, activo
                    FROM MetodoPago
                    WHERE activo = 'S'
                    ORDER BY nombre";

                DataTable dataTable = ExecuteQuery(query);
                List<MetodoPago> metodosPago = new List<MetodoPago>();

                foreach (DataRow row in dataTable.Rows)
                {
                    metodosPago.Add(ConvertirDataRowAMetodoPago(row));
                }

                return metodosPago;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener métodos de pago activos");
                throw new Exception("Error al obtener métodos de pago activos.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene un método de pago por su ID.
        /// </summary>
        /// <param name="idMetodoPago">ID del método de pago.</param>
        /// <returns>El objeto MetodoPago si se encuentra, null en caso contrario.</returns>
        public MetodoPago ObtenerPorId(int idMetodoPago)
        {
            try
            {
                string query = @"
                    SELECT idMetodoPago, nombre, descripcion, activo
                    FROM MetodoPago
                    WHERE idMetodoPago = :idMetodoPago";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idMetodoPago", idMetodoPago }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);

                if (dataTable.Rows.Count > 0)
                {
                    return ConvertirDataRowAMetodoPago(dataTable.Rows[0]);
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener método de pago con ID {idMetodoPago}");
                throw new Exception("Error al obtener método de pago por ID.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Crea un nuevo método de pago en la base de datos.
        /// </summary>
        /// <param name="metodoPago">Método de pago a crear.</param>
        /// <returns>ID del método de pago creado.</returns>
        public int Crear(MetodoPago metodoPago)
        {
            try
            {
                BeginTransaction();

                // Verificar si ya existe un método de pago con el mismo nombre
                if (ExisteNombre(metodoPago.Nombre))
                {
                    throw new Exception("Ya existe un método de pago con el nombre especificado.");
                }

                // Obtener el próximo ID de método de pago
                int idMetodoPago = GetNextSequenceValue("SEQ_METODO_PAGO");

                // Insertar el método de pago
                string query = @"
                    INSERT INTO MetodoPago (idMetodoPago, nombre, descripcion, activo)
                    VALUES (:idMetodoPago, :nombre, :descripcion, :activo)";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idMetodoPago", idMetodoPago },
                    { ":nombre", metodoPago.Nombre },
                    { ":descripcion", metodoPago.Descripcion ?? (object)DBNull.Value },
                    { ":activo", metodoPago.Activo ? "S" : "N" }
                };

                ExecuteNonQuery(query, parameters);

                CommitTransaction();
                return idMetodoPago;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Logger.LogException(ex, $"Error al crear método de pago {metodoPago.Nombre}");
                throw new Exception("Error al crear método de pago.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Actualiza un método de pago existente.
        /// </summary>
        /// <param name="metodoPago">Método de pago con los datos actualizados.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool Actualizar(MetodoPago metodoPago)
        {
            try
            {
                BeginTransaction();

                // Verificar si ya existe otro método de pago con el mismo nombre
                if (ExisteOtroMetodoPagoConNombre(metodoPago.Nombre, metodoPago.IdMetodoPago))
                {
                    throw new Exception("Ya existe otro método de pago con el nombre especificado.");
                }

                string query = @"
                    UPDATE MetodoPago SET 
                        nombre = :nombre,
                        descripcion = :descripcion,
                        activo = :activo
                    WHERE idMetodoPago = :idMetodoPago";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":nombre", metodoPago.Nombre },
                    { ":descripcion", metodoPago.Descripcion ?? (object)DBNull.Value },
                    { ":activo", metodoPago.Activo ? "S" : "N" },
                    { ":idMetodoPago", metodoPago.IdMetodoPago }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                CommitTransaction();

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Logger.LogException(ex, $"Error al actualizar método de pago {metodoPago.Nombre}");
                throw new Exception("Error al actualizar método de pago.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Elimina un método de pago (marcándolo como inactivo).
        /// </summary>
        /// <param name="idMetodoPago">ID del método de pago a eliminar.</param>
        /// <returns>True si la eliminación es exitosa, False en caso contrario.</returns>
        public bool Eliminar(int idMetodoPago)
        {
            try
            {
                // En lugar de eliminar físicamente, marcamos como inactivo
                string query = "UPDATE MetodoPago SET activo = 'N' WHERE idMetodoPago = :idMetodoPago";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idMetodoPago", idMetodoPago }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al eliminar método de pago {idMetodoPago}");
                throw new Exception("Error al eliminar método de pago.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Verifica si ya existe un método de pago con el nombre especificado.
        /// </summary>
        /// <param name="nombre">Nombre a verificar.</param>
        /// <returns>True si ya existe, False en caso contrario.</returns>
        private bool ExisteNombre(string nombre)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM MetodoPago WHERE UPPER(nombre) = UPPER(:nombre)";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":nombre", nombre }
                };

                object result = ExecuteScalar(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al verificar existencia de nombre {nombre}");
                throw new Exception("Error al verificar existencia de nombre.", ex);
            }
        }

        /// <summary>
        /// Verifica si ya existe otro método de pago con el mismo nombre.
        /// </summary>
        /// <param name="nombre">Nombre a verificar.</param>
        /// <param name="idMetodoPago">ID del método de pago actual (para excluirlo de la verificación).</param>
        /// <returns>True si ya existe otro método de pago con el mismo nombre, False en caso contrario.</returns>
        private bool ExisteOtroMetodoPagoConNombre(string nombre, int idMetodoPago)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM MetodoPago WHERE UPPER(nombre) = UPPER(:nombre) AND idMetodoPago != :idMetodoPago";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":nombre", nombre },
                    { ":idMetodoPago", idMetodoPago }
                };

                object result = ExecuteScalar(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al verificar existencia de nombre en otro método de pago {nombre}");
                throw new Exception("Error al verificar existencia de nombre en otro método de pago.", ex);
            }
        }

        /// <summary>
        /// Convierte un DataRow a un objeto MetodoPago.
        /// </summary>
        /// <param name="row">DataRow con los datos del método de pago.</param>
        /// <returns>Objeto MetodoPago con los datos del DataRow.</returns>
        private MetodoPago ConvertirDataRowAMetodoPago(DataRow row)
        {
            return new MetodoPago
            {
                IdMetodoPago = Convert.ToInt32(row["idMetodoPago"]),
                Nombre = row["nombre"].ToString(),
                Descripcion = row["descripcion"] != DBNull.Value ? row["descripcion"].ToString() : null,
                Activo = row["activo"].ToString() == "S"
            };
        }
    }
}