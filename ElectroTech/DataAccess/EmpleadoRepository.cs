using ElectroTech.Helpers;
using ElectroTech.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace ElectroTech.DataAccess
{
    /// <summary>
    /// Repositorio para operaciones de base de datos relacionadas con la entidad Empleado.
    /// </summary>
    public class EmpleadoRepository : DatabaseRepository
    {
        /// <summary>
        /// Obtiene todos los empleados activos.
        /// </summary>
        /// <returns>Lista de empleados activos.</returns>
        public List<Empleado> ObtenerTodos()
        {
            try
            {
                string query = @"
                    SELECT e.idEmpleado, e.tipoDocumento, e.numeroDocumento, e.nombre, e.apellido, 
                           e.direccion, e.telefono, e.fechaContratacion, e.salarioBase, e.idUsuario, e.activo
                    FROM Empleado e
                    WHERE e.activo = 'S'
                    ORDER BY e.apellido, e.nombre";

                DataTable dataTable = ExecuteQuery(query);
                List<Empleado> empleados = new List<Empleado>();

                foreach (DataRow row in dataTable.Rows)
                {
                    empleados.Add(ConvertirDataRowAEmpleado(row));
                }

                return empleados;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener todos los empleados");
                throw new Exception("Error al obtener empleados.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene un empleado por su ID.
        /// </summary>
        /// <param name="idEmpleado">ID del empleado.</param>
        /// <returns>El objeto Empleado si se encuentra, null en caso contrario.</returns>
        public Empleado ObtenerPorId(int idEmpleado)
        {
            try
            {
                string query = @"
                    SELECT e.idEmpleado, e.tipoDocumento, e.numeroDocumento, e.nombre, e.apellido, 
                           e.direccion, e.telefono, e.fechaContratacion, e.salarioBase, e.idUsuario, e.activo
                    FROM Empleado e
                    WHERE e.idEmpleado = :idEmpleado";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idEmpleado", idEmpleado }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);

                if (dataTable.Rows.Count > 0)
                {
                    return ConvertirDataRowAEmpleado(dataTable.Rows[0]);
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener empleado con ID {idEmpleado}");
                throw new Exception("Error al obtener empleado por ID.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene un empleado por su número de documento.
        /// </summary>
        /// <param name="tipoDocumento">Tipo de documento.</param>
        /// <param name="numeroDocumento">Número de documento.</param>
        /// <returns>El objeto Empleado si se encuentra, null en caso contrario.</returns>
        public Empleado ObtenerPorDocumento(string tipoDocumento, string numeroDocumento)
        {
            try
            {
                string query = @"
                    SELECT e.idEmpleado, e.tipoDocumento, e.numeroDocumento, e.nombre, e.apellido, 
                           e.direccion, e.telefono, e.fechaContratacion, e.salarioBase, e.idUsuario, e.activo
                    FROM Empleado e
                    WHERE e.tipoDocumento = :tipoDocumento AND e.numeroDocumento = :numeroDocumento";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":tipoDocumento", tipoDocumento },
                    { ":numeroDocumento", numeroDocumento }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);

                if (dataTable.Rows.Count > 0)
                {
                    return ConvertirDataRowAEmpleado(dataTable.Rows[0]);
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener empleado con documento {tipoDocumento}-{numeroDocumento}");
                throw new Exception("Error al obtener empleado por documento.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Busca empleados según un término de búsqueda.
        /// </summary>
        /// <param name="termino">Término de búsqueda.</param>
        /// <returns>Lista de empleados que coinciden con el término.</returns>
        public List<Empleado> Buscar(string termino)
        {
            try
            {
                string terminoBusqueda = $"%{termino}%";

                string query = @"
                    SELECT e.idEmpleado, e.tipoDocumento, e.numeroDocumento, e.nombre, e.apellido, 
                           e.direccion, e.telefono, e.fechaContratacion, e.salarioBase, e.idUsuario, e.activo
                    FROM Empleado e
                    WHERE e.activo = 'S' AND 
                          (UPPER(e.nombre) LIKE UPPER(:termino) OR 
                           UPPER(e.apellido) LIKE UPPER(:termino) OR 
                           UPPER(e.numeroDocumento) LIKE UPPER(:termino) OR
                           UPPER(e.telefono) LIKE UPPER(:termino))
                    ORDER BY e.apellido, e.nombre";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":termino", terminoBusqueda }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);
                List<Empleado> empleados = new List<Empleado>();

                foreach (DataRow row in dataTable.Rows)
                {
                    empleados.Add(ConvertirDataRowAEmpleado(row));
                }

                return empleados;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al buscar empleados con término '{termino}'");
                throw new Exception("Error al buscar empleados.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Crea un nuevo empleado en la base de datos.
        /// </summary>
        /// <param name="empleado">Empleado a crear.</param>
        /// <returns>ID del empleado creado.</returns>
        public int Crear(Empleado empleado)
        {
            try
            {
                BeginTransaction();

                // Verificar si ya existe un empleado con el mismo documento
                if (ExisteDocumento(empleado.TipoDocumento, empleado.NumeroDocumento))
                {
                    throw new Exception("Ya existe un empleado con el documento especificado.");
                }

                // Obtener el próximo ID de empleado
                int idEmpleado = GetNextSequenceValue("SEQ_EMPLEADO");

                // Insertar el empleado
                string query = @"
                    INSERT INTO Empleado (idEmpleado, tipoDocumento, numeroDocumento, nombre, apellido, 
                                         direccion, telefono, fechaContratacion, salarioBase, 
                                         idUsuario, activo)
                    VALUES (:idEmpleado, :tipoDocumento, :numeroDocumento, :nombre, :apellido, 
                            :direccion, :telefono, :fechaContratacion, :salarioBase, 
                            :idUsuario, :activo)";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idEmpleado", idEmpleado },
                    { ":tipoDocumento", empleado.TipoDocumento },
                    { ":numeroDocumento", empleado.NumeroDocumento },
                    { ":nombre", empleado.Nombre },
                    { ":apellido", empleado.Apellido },
                    { ":direccion", empleado.Direccion ?? (object)DBNull.Value },
                    { ":telefono", empleado.Telefono },
                    { ":fechaContratacion", empleado.FechaContratacion },
                    { ":salarioBase", empleado.SalarioBase },
                    { ":idUsuario", empleado.IdUsuario > 0 ? (object)empleado.IdUsuario : DBNull.Value },
                    { ":activo", empleado.Activo ? "S" : "N" }
                };

                ExecuteNonQuery(query, parameters);

                CommitTransaction();
                return idEmpleado;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Logger.LogException(ex, $"Error al crear empleado {empleado.Nombre} {empleado.Apellido}");
                throw new Exception("Error al crear empleado.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Actualiza un empleado existente.
        /// </summary>
        /// <param name="empleado">Empleado con los datos actualizados.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool Actualizar(Empleado empleado)
        {
            try
            {
                BeginTransaction();

                // Verificar si ya existe otro empleado con el mismo documento
                if (ExisteOtroEmpleadoConDocumento(empleado.TipoDocumento, empleado.NumeroDocumento, empleado.IdEmpleado))
                {
                    throw new Exception("Ya existe otro empleado con el documento especificado.");
                }

                string query = @"
                    UPDATE Empleado SET 
                        tipoDocumento = :tipoDocumento,
                        numeroDocumento = :numeroDocumento,
                        nombre = :nombre,
                        apellido = :apellido,
                        direccion = :direccion,
                        telefono = :telefono,
                        fechaContratacion = :fechaContratacion,
                        salarioBase = :salarioBase,
                        idUsuario = :idUsuario,
                        activo = :activo
                    WHERE idEmpleado = :idEmpleado";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":tipoDocumento", empleado.TipoDocumento },
                    { ":numeroDocumento", empleado.NumeroDocumento },
                    { ":nombre", empleado.Nombre },
                    { ":apellido", empleado.Apellido },
                    { ":direccion", empleado.Direccion ?? (object)DBNull.Value },
                    { ":telefono", empleado.Telefono },
                    { ":fechaContratacion", empleado.FechaContratacion },
                    { ":salarioBase", empleado.SalarioBase },
                    { ":idUsuario", empleado.IdUsuario > 0 ? (object)empleado.IdUsuario : DBNull.Value },
                    { ":activo", empleado.Activo ? "S" : "N" },
                    { ":idEmpleado", empleado.IdEmpleado }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                CommitTransaction();

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Logger.LogException(ex, $"Error al actualizar empleado {empleado.Nombre} {empleado.Apellido}");
                throw new Exception("Error al actualizar empleado.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Actualiza el ID de usuario asociado al empleado.
        /// </summary>
        /// <param name="idEmpleado">ID del empleado.</param>
        /// <param name="idUsuario">ID del usuario asociado.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool ActualizarIdUsuario(int idEmpleado, int idUsuario)
        {
            try
            {
                string query = "UPDATE Empleado SET idUsuario = :idUsuario WHERE idEmpleado = :idEmpleado";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idUsuario", idUsuario },
                    { ":idEmpleado", idEmpleado }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al actualizar idUsuario del empleado {idEmpleado}");
                throw new Exception("Error al actualizar usuario asociado al empleado.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Elimina un empleado (marcándolo como inactivo).
        /// </summary>
        /// <param name="idEmpleado">ID del empleado a eliminar.</param>
        /// <returns>True si la eliminación es exitosa, False en caso contrario.</returns>
        public bool Eliminar(int idEmpleado)
        {
            try
            {
                // En lugar de eliminar físicamente, marcamos como inactivo
                string query = "UPDATE Empleado SET activo = 'N' WHERE idEmpleado = :idEmpleado";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idEmpleado", idEmpleado }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al eliminar empleado {idEmpleado}");
                throw new Exception("Error al eliminar empleado.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Verifica si ya existe un empleado con el documento especificado.
        /// </summary>
        /// <param name="tipoDocumento">Tipo de documento.</param>
        /// <param name="numeroDocumento">Número de documento.</param>
        /// <returns>True si ya existe, False en caso contrario.</returns>
        private bool ExisteDocumento(string tipoDocumento, string numeroDocumento)
        {
            try
            {
                string query = @"
                    SELECT COUNT(*) 
                    FROM Empleado 
                    WHERE tipoDocumento = :tipoDocumento AND numeroDocumento = :numeroDocumento";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":tipoDocumento", tipoDocumento },
                    { ":numeroDocumento", numeroDocumento }
                };

                object result = ExecuteScalar(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al verificar existencia de documento {tipoDocumento}-{numeroDocumento}");
                throw new Exception("Error al verificar existencia de documento.", ex);
            }
        }

        /// <summary>
        /// Verifica si ya existe otro empleado con el mismo documento.
        /// </summary>
        /// <param name="tipoDocumento">Tipo de documento.</param>
        /// <param name="numeroDocumento">Número de documento.</param>
        /// <param name="idEmpleado">ID del empleado actual.</param>
        /// <returns>True si ya existe otro empleado con el mismo documento, False en caso contrario.</returns>
        private bool ExisteOtroEmpleadoConDocumento(string tipoDocumento, string numeroDocumento, int idEmpleado)
        {
            try
            {
                string query = @"
                    SELECT COUNT(*) 
                    FROM Empleado 
                    WHERE tipoDocumento = :tipoDocumento 
                      AND numeroDocumento = :numeroDocumento 
                      AND idEmpleado != :idEmpleado";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":tipoDocumento", tipoDocumento },
                    { ":numeroDocumento", numeroDocumento },
                    { ":idEmpleado", idEmpleado }
                };

                object result = ExecuteScalar(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al verificar existencia de documento en otro empleado {tipoDocumento}-{numeroDocumento}");
                throw new Exception("Error al verificar existencia de documento en otro empleado.", ex);
            }
        }

        /// <summary>
        /// Convierte un DataRow a un objeto Empleado.
        /// </summary>
        /// <param name="row">DataRow con los datos del empleado.</param>
        /// <returns>Objeto Empleado con los datos del DataRow.</returns>
        private Empleado ConvertirDataRowAEmpleado(DataRow row)
        {
            var empleado = new Empleado
            {
                IdEmpleado = Convert.ToInt32(row["idEmpleado"]),
                TipoDocumento = row["tipoDocumento"].ToString(),
                NumeroDocumento = row["numeroDocumento"].ToString(),
                Nombre = row["nombre"].ToString(),
                Apellido = row["apellido"].ToString(),
                Direccion = row["direccion"] != DBNull.Value ? row["direccion"].ToString() : null,
                Telefono = row["telefono"].ToString(),
                FechaContratacion = Convert.ToDateTime(row["fechaContratacion"]),
                SalarioBase = Convert.ToDouble(row["salarioBase"]),
                IdUsuario = row["idUsuario"] != DBNull.Value ? Convert.ToInt32(row["idUsuario"]) : 0,
                Activo = row["activo"].ToString() == "S"
            };

            return empleado;
        }
    }
}