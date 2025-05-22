using ElectroTech.Helpers;
using ElectroTech.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace ElectroTech.DataAccess
{
    /// <summary>
    /// Repositorio para operaciones de base de datos relacionadas con la entidad Cliente.
    /// </summary>
    public class ClienteRepository : DatabaseRepository
    {
        /// <summary>
        /// Obtiene todos los clientes activos.
        /// </summary>
        /// <returns>Lista de clientes activos.</returns>
        public List<Cliente> ObtenerTodos()
        {
            try
            {
                string query = @"
                    SELECT idCliente, tipoDocumento, numeroDocumento, nombre, apellido, 
                           direccion, telefono, correo, fechaRegistro, activo
                    FROM Cliente
                    WHERE activo = 'S'
                    ORDER BY nombre, apellido";

                DataTable dataTable = ExecuteQuery(query);
                List<Cliente> clientes = new List<Cliente>();

                foreach (DataRow row in dataTable.Rows)
                {
                    clientes.Add(ConvertirDataRowACliente(row));
                }

                return clientes;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener todos los clientes");
                throw new Exception("Error al obtener clientes.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene un cliente por su ID.
        /// </summary>
        /// <param name="idCliente">ID del cliente.</param>
        /// <returns>El objeto Cliente si se encuentra, null en caso contrario.</returns>
        public Cliente ObtenerPorId(int idCliente)
        {
            try
            {
                string query = @"
                    SELECT idCliente, tipoDocumento, numeroDocumento, nombre, apellido, 
                           direccion, telefono, correo, fechaRegistro, activo
                    FROM Cliente
                    WHERE idCliente = :idCliente";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idCliente", idCliente }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);

                if (dataTable.Rows.Count > 0)
                {
                    return ConvertirDataRowACliente(dataTable.Rows[0]);
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener cliente con ID {idCliente}");
                throw new Exception("Error al obtener cliente por ID.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene todos los clientes activos.
        /// </summary>
        /// <returns>Lista de clientes activos.</returns>
        public List<Cliente> ObtenerActivos()
        {
            try
            {
                string query = @"
                    SELECT idCliente, tipoDocumento, numeroDocumento, nombre, apellido, 
                           direccion, telefono, correo, fechaRegistro, activo
                    FROM Cliente
                    WHERE activo = 'S'
                    ORDER BY nombre, apellido";

                DataTable dataTable = ExecuteQuery(query);
                List<Cliente> clientes = new List<Cliente>();

                foreach (DataRow row in dataTable.Rows)
                {
                    clientes.Add(ConvertirDataRowACliente(row));
                }

                return clientes;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener clientes activos");
                throw new Exception("Error al obtener clientes activos.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Busca clientes según un término de búsqueda.
        /// </summary>
        /// <param name="termino">Término de búsqueda.</param>
        /// <returns>Lista de clientes que coinciden con el término.</returns>
        public List<Cliente> Buscar(string termino)
        {
            try
            {
                string terminoBusqueda = $"%{termino}%";

                string query = @"
                    SELECT idCliente, tipoDocumento, numeroDocumento, nombre, apellido, 
                           direccion, telefono, correo, fechaRegistro, activo
                    FROM Cliente
                    WHERE (UPPER(nombre) LIKE UPPER(:termino) OR 
                           UPPER(apellido) LIKE UPPER(:termino) OR
                           UPPER(correo) LIKE UPPER(:termino) OR
                           telefono LIKE :termino OR
                           numeroDocumento LIKE :termino)
                    AND activo = 'S'
                    ORDER BY nombre, apellido";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":termino", terminoBusqueda }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);
                List<Cliente> clientes = new List<Cliente>();

                foreach (DataRow row in dataTable.Rows)
                {
                    clientes.Add(ConvertirDataRowACliente(row));
                }

                return clientes;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al buscar clientes con término '{termino}'");
                throw new Exception("Error al buscar clientes.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Crea un nuevo cliente en la base de datos.
        /// </summary>
        /// <param name="cliente">Cliente a crear.</param>
        /// <returns>ID del cliente creado.</returns>
        public int Crear(Cliente cliente)
        {
            try
            {
                BeginTransaction();

                // Verificar si ya existe otro cliente con el mismo documento
                if (ExisteDocumento(cliente.TipoDocumento, cliente.NumeroDocumento))
                {
                    throw new Exception($"Ya existe un cliente con el documento {cliente.TipoDocumento} {cliente.NumeroDocumento}.");
                }

                // Obtener el próximo ID de cliente
                int idCliente = GetNextSequenceValue("SEQ_CLIENTE");

                // Insertar el cliente
                string query = @"
                    INSERT INTO Cliente (idCliente, tipoDocumento, numeroDocumento, nombre, apellido, 
                                        direccion, telefono, correo, fechaRegistro, activo)
                    VALUES (:idCliente, :tipoDocumento, :numeroDocumento, :nombre, :apellido, 
                            :direccion, :telefono, :correo, :fechaRegistro, :activo)";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idCliente", idCliente },
                    { ":tipoDocumento", cliente.TipoDocumento },
                    { ":numeroDocumento", cliente.NumeroDocumento },
                    { ":nombre", cliente.Nombre },
                    { ":apellido", cliente.Apellido },
                    { ":direccion", cliente.Direccion ?? (object)DBNull.Value },
                    { ":telefono", cliente.Telefono ?? (object)DBNull.Value },
                    { ":correo", cliente.Correo ?? (object)DBNull.Value },
                    { ":fechaRegistro", cliente.FechaRegistro },
                    { ":activo", cliente.Activo ? "S" : "N" }
                };

                ExecuteNonQuery(query, parameters);
                CommitTransaction();
                return idCliente;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Logger.LogException(ex, $"Error al crear cliente {cliente.Nombre} {cliente.Apellido}");
                throw new Exception("Error al crear cliente.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Actualiza un cliente existente.
        /// </summary>
        /// <param name="cliente">Cliente con los datos actualizados.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool Actualizar(Cliente cliente)
        {
            try
            {
                BeginTransaction();

                // Verificar si ya existe otro cliente con el mismo documento
                if (ExisteOtroClienteConDocumento(cliente.TipoDocumento, cliente.NumeroDocumento, cliente.IdCliente))
                {
                    throw new Exception($"Ya existe otro cliente con el documento {cliente.TipoDocumento} {cliente.NumeroDocumento}.");
                }

                string query = @"
                    UPDATE Cliente SET 
                        tipoDocumento = :tipoDocumento,
                        numeroDocumento = :numeroDocumento,
                        nombre = :nombre,
                        apellido = :apellido,
                        direccion = :direccion,
                        telefono = :telefono,
                        correo = :correo,
                        activo = :activo
                    WHERE idCliente = :idCliente";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":tipoDocumento", cliente.TipoDocumento },
                    { ":numeroDocumento", cliente.NumeroDocumento },
                    { ":nombre", cliente.Nombre },
                    { ":apellido", cliente.Apellido },
                    { ":direccion", cliente.Direccion ?? (object)DBNull.Value },
                    { ":telefono", cliente.Telefono ?? (object)DBNull.Value },
                    { ":correo", cliente.Correo ?? (object)DBNull.Value },
                    { ":activo", cliente.Activo ? "S" : "N" },
                    { ":idCliente", cliente.IdCliente }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                CommitTransaction();

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Logger.LogException(ex, $"Error al actualizar cliente {cliente.Nombre} {cliente.Apellido}");
                throw new Exception("Error al actualizar cliente.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Elimina un cliente (marcándolo como inactivo).
        /// </summary>
        /// <param name="idCliente">ID del cliente a eliminar.</param>
        /// <returns>True si la eliminación es exitosa, False en caso contrario.</returns>
        public bool Eliminar(int idCliente)
        {
            try
            {
                // En lugar de eliminar físicamente, marcamos como inactivo
                string query = "UPDATE Cliente SET activo = 'N' WHERE idCliente = :idCliente";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idCliente", idCliente }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al eliminar cliente {idCliente}");
                throw new Exception("Error al eliminar cliente.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Verifica si ya existe un cliente con el documento especificado.
        /// </summary>
        /// <param name="tipoDocumento">Tipo de documento a verificar.</param>
        /// <param name="numeroDocumento">Número de documento a verificar.</param>
        /// <returns>True si ya existe, False en caso contrario.</returns>
        private bool ExisteDocumento(string tipoDocumento, string numeroDocumento)
        {
            try
            {
                string query = @"
                    SELECT COUNT(*) 
                    FROM Cliente 
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
                Logger.LogException(ex, $"Error al verificar existencia de documento {tipoDocumento} {numeroDocumento}");
                throw new Exception("Error al verificar existencia de documento.", ex);
            }
        }

        /// <summary>
        /// Verifica si ya existe otro cliente con el mismo documento.
        /// </summary>
        /// <param name="tipoDocumento">Tipo de documento a verificar.</param>
        /// <param name="numeroDocumento">Número de documento a verificar.</param>
        /// <param name="idCliente">ID del cliente actual (para excluirlo de la verificación).</param>
        /// <returns>True si ya existe otro cliente con el mismo documento, False en caso contrario.</returns>
        private bool ExisteOtroClienteConDocumento(string tipoDocumento, string numeroDocumento, int idCliente)
        {
            try
            {
                string query = @"
                    SELECT COUNT(*) 
                    FROM Cliente 
                    WHERE tipoDocumento = :tipoDocumento AND numeroDocumento = :numeroDocumento AND idCliente != :idCliente";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":tipoDocumento", tipoDocumento },
                    { ":numeroDocumento", numeroDocumento },
                    { ":idCliente", idCliente }
                };

                object result = ExecuteScalar(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al verificar existencia de documento en otro cliente {tipoDocumento} {numeroDocumento}");
                throw new Exception("Error al verificar existencia de documento en otro cliente.", ex);
            }
        }

        /// <summary>
        /// Convierte un DataRow a un objeto Cliente.
        /// </summary>
        /// <param name="row">DataRow con los datos del cliente.</param>
        /// <returns>Objeto Cliente con los datos del DataRow.</returns>
        private Cliente ConvertirDataRowACliente(DataRow row)
        {
            var cliente = new Cliente
            {
                IdCliente = Convert.ToInt32(row["idCliente"]),
                TipoDocumento = row["tipoDocumento"].ToString(),
                NumeroDocumento = row["numeroDocumento"].ToString(),
                Nombre = row["nombre"].ToString(),
                Apellido = row["apellido"].ToString(),
                Direccion = row["direccion"] != DBNull.Value ? row["direccion"].ToString() : null,
                Telefono = row["telefono"] != DBNull.Value ? row["telefono"].ToString() : null,
                Correo = row["correo"] != DBNull.Value ? row["correo"].ToString() : null,
                FechaRegistro = Convert.ToDateTime(row["fechaRegistro"]),
                Activo = row["activo"].ToString() == "S"
            };

            return cliente;
        }
    }
}