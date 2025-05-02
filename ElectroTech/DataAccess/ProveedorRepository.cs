using ElectroTech.Helpers;
using ElectroTech.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace ElectroTech.DataAccess
{
    /// <summary>
    /// Repositorio para operaciones de base de datos relacionadas con la entidad Proveedor.
    /// </summary>
    public class ProveedorRepository : DatabaseRepository
    {
        /// <summary>
        /// Obtiene todos los proveedores activos.
        /// </summary>
        /// <returns>Lista de proveedores activos.</returns>
        public List<Proveedor> ObtenerTodos()
        {
            try
            {
                string query = @"
                    SELECT idProveedor, nombre, direccion, telefono, correo, 
                           contacto, condicionesPago, activo
                    FROM Proveedor
                    WHERE activo = 'S'
                    ORDER BY nombre";

                DataTable dataTable = ExecuteQuery(query);
                List<Proveedor> proveedores = new List<Proveedor>();

                foreach (DataRow row in dataTable.Rows)
                {
                    proveedores.Add(ConvertirDataRowAProveedor(row));
                }

                return proveedores;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener todos los proveedores");
                throw new Exception("Error al obtener proveedores.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene un proveedor por su ID.
        /// </summary>
        /// <param name="idProveedor">ID del proveedor.</param>
        /// <returns>El objeto Proveedor si se encuentra, null en caso contrario.</returns>
        public Proveedor ObtenerPorId(int idProveedor)
        {
            try
            {
                string query = @"
                    SELECT idProveedor, nombre, direccion, telefono, correo, 
                           contacto, condicionesPago, activo
                    FROM Proveedor
                    WHERE idProveedor = :idProveedor";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idProveedor", idProveedor }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);

                if (dataTable.Rows.Count > 0)
                {
                    return ConvertirDataRowAProveedor(dataTable.Rows[0]);
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener proveedor con ID {idProveedor}");
                throw new Exception("Error al obtener proveedor por ID.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Busca proveedores según un término de búsqueda.
        /// </summary>
        /// <param name="termino">Término de búsqueda.</param>
        /// <returns>Lista de proveedores que coinciden con el término.</returns>
        public List<Proveedor> Buscar(string termino)
        {
            try
            {
                string terminoBusqueda = $"%{termino}%";

                string query = @"
                    SELECT idProveedor, nombre, direccion, telefono, correo, 
                           contacto, condicionesPago, activo
                    FROM Proveedor
                    WHERE (UPPER(nombre) LIKE UPPER(:termino) OR 
                           UPPER(contacto) LIKE UPPER(:termino) OR
                           UPPER(correo) LIKE UPPER(:termino) OR
                           telefono LIKE :termino)
                    AND activo = 'S'
                    ORDER BY nombre";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":termino", terminoBusqueda }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);
                List<Proveedor> proveedores = new List<Proveedor>();

                foreach (DataRow row in dataTable.Rows)
                {
                    proveedores.Add(ConvertirDataRowAProveedor(row));
                }

                return proveedores;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al buscar proveedores con término '{termino}'");
                throw new Exception("Error al buscar proveedores.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Crea un nuevo proveedor en la base de datos.
        /// </summary>
        /// <param name="proveedor">Proveedor a crear.</param>
        /// <returns>ID del proveedor creado.</returns>
        public int Crear(Proveedor proveedor)
        {
            try
            {
                BeginTransaction();

                // Obtener el próximo ID de proveedor
                int idProveedor = GetNextSequenceValue("SEQ_PROVEEDOR");

                // Insertar el proveedor
                string query = @"
                    INSERT INTO Proveedor (idProveedor, nombre, direccion, telefono, correo, 
                                           contacto, condicionesPago, activo)
                    VALUES (:idProveedor, :nombre, :direccion, :telefono, :correo, 
                            :contacto, :condicionesPago, :activo)";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idProveedor", idProveedor },
                    { ":nombre", proveedor.Nombre },
                    { ":direccion", proveedor.Direccion ?? (object)DBNull.Value },
                    { ":telefono", proveedor.Telefono ?? (object)DBNull.Value },
                    { ":correo", proveedor.Correo ?? (object)DBNull.Value },
                    { ":contacto", proveedor.Contacto ?? (object)DBNull.Value },
                    { ":condicionesPago", proveedor.CondicionesPago ?? (object)DBNull.Value },
                    { ":activo", proveedor.Activo ? "S" : "N" }
                };

                ExecuteNonQuery(query, parameters);
                CommitTransaction();
                return idProveedor;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Logger.LogException(ex, $"Error al crear proveedor {proveedor.Nombre}");
                throw new Exception("Error al crear proveedor.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Actualiza un proveedor existente.
        /// </summary>
        /// <param name="proveedor">Proveedor con los datos actualizados.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool Actualizar(Proveedor proveedor)
        {
            try
            {
                string query = @"
                    UPDATE Proveedor SET 
                        nombre = :nombre,
                        direccion = :direccion,
                        telefono = :telefono,
                        correo = :correo,
                        contacto = :contacto,
                        condicionesPago = :condicionesPago,
                        activo = :activo
                    WHERE idProveedor = :idProveedor";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":nombre", proveedor.Nombre },
                    { ":direccion", proveedor.Direccion ?? (object)DBNull.Value },
                    { ":telefono", proveedor.Telefono ?? (object)DBNull.Value },
                    { ":correo", proveedor.Correo ?? (object)DBNull.Value },
                    { ":contacto", proveedor.Contacto ?? (object)DBNull.Value },
                    { ":condicionesPago", proveedor.CondicionesPago ?? (object)DBNull.Value },
                    { ":activo", proveedor.Activo ? "S" : "N" },
                    { ":idProveedor", proveedor.IdProveedor }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al actualizar proveedor {proveedor.Nombre}");
                throw new Exception("Error al actualizar proveedor.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Elimina un proveedor (marcándolo como inactivo).
        /// </summary>
        /// <param name="idProveedor">ID del proveedor a eliminar.</param>
        /// <returns>True si la eliminación es exitosa, False en caso contrario.</returns>
        public bool Eliminar(int idProveedor)
        {
            try
            {
                // En lugar de eliminar físicamente, marcamos como inactivo
                string query = "UPDATE Proveedor SET activo = 'N' WHERE idProveedor = :idProveedor";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idProveedor", idProveedor }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al eliminar proveedor {idProveedor}");
                throw new Exception("Error al eliminar proveedor.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Convierte un DataRow a un objeto Proveedor.
        /// </summary>
        /// <param name="row">DataRow con los datos del proveedor.</param>
        /// <returns>Objeto Proveedor con los datos del DataRow.</returns>
        private Proveedor ConvertirDataRowAProveedor(DataRow row)
        {
            var proveedor = new Proveedor
            {
                IdProveedor = Convert.ToInt32(row["idProveedor"]),
                Nombre = row["nombre"].ToString(),
                Direccion = row["direccion"] != DBNull.Value ? row["direccion"].ToString() : null,
                Telefono = row["telefono"] != DBNull.Value ? row["telefono"].ToString() : null,
                Correo = row["correo"] != DBNull.Value ? row["correo"].ToString() : null,
                Contacto = row["contacto"] != DBNull.Value ? row["contacto"].ToString() : null,
                CondicionesPago = row["condicionesPago"] != DBNull.Value ? row["condicionesPago"].ToString() : null,
                Activo = row["activo"].ToString() == "S"
            };

            return proveedor;
        }
    }
}