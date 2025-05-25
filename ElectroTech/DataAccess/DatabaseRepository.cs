using ElectroTech.Helpers;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroTech.DataAccess
{
    /// <summary>
    /// Clase base que proporciona métodos genéricos para operaciones de base de datos.
    /// </summary>
    public abstract class DatabaseRepository
    {
        protected OracleConnection _connection;
        protected OracleTransaction _transaction;

        /// <summary>
        /// Constructor que inicializa la conexión a la base de datos.
        /// </summary>
        protected DatabaseRepository()
        {
            _connection = null;
            _transaction = null;
        }

        /// <summary>
        /// Abre la conexión a la base de datos.
        /// </summary>
        protected void OpenConnection()
        {
            _connection = ConnectionManager.Instance.GetConnection();
        }

        /// <summary>
        /// Cierra la conexión a la base de datos.
        /// </summary>
        protected void CloseConnection()
        {
            ConnectionManager.Instance.CloseConnection(_connection);
            _connection = null;
        }

        /// <summary>
        /// Inicia una transacción en la base de datos.
        /// </summary>
        protected void BeginTransaction()
        {
            if (_connection == null)
            {
                OpenConnection();
            }
            _transaction = _connection.BeginTransaction();
        }

        /// <summary>
        /// Confirma una transacción abierta.
        /// </summary>
        protected void CommitTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                _transaction = null;
            }
        }

        /// <summary>
        /// Cancela una transacción abierta.
        /// </summary>
        protected void RollbackTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction = null;
            }
        }

        /// <summary>
        /// Ejecuta un comando SQL que no devuelve resultados (INSERT, UPDATE, DELETE).
        /// </summary>
        /// <param name="commandText">El comando SQL a ejecutar.</param>
        /// <param name="parameters">Los parámetros del comando.</param>
        /// <returns>El número de filas afectadas.</returns>
        protected int ExecuteNonQuery(string commandText, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (OracleCommand command = PrepareCommand(commandText, parameters))
                {
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar el comando SQL.", ex);
            }
        }

        /// <summary>
        /// Ejecuta un comando SQL que devuelve un valor escalar.
        /// </summary>
        /// <param name="commandText">El comando SQL a ejecutar.</param>
        /// <param name="parameters">Los parámetros del comando.</param>
        /// <returns>El valor escalar devuelto por el comando.</returns>
        protected object ExecuteScalar(string commandText, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (OracleCommand command = PrepareCommand(commandText, parameters))
                {
                    return command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar el comando SQL.", ex);
            }
        }

        /// <summary>
        /// Ejecuta un comando SQL que devuelve un conjunto de resultados.
        /// </summary>
        /// <param name="commandText">El comando SQL a ejecutar.</param>
        /// <param name="parameters">Los parámetros del comando.</param>
        /// <returns>Un DataTable con los resultados.</returns>
        protected DataTable ExecuteQuery(string commandText, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (OracleCommand command = PrepareCommand(commandText, parameters))
                {
                    using (OracleDataAdapter adapter = new OracleDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar la consulta SQL.", ex);
            }
        }

        /// <summary>
        /// Prepara un comando SQL con sus parámetros.
        /// </summary>
        /// <param name="commandText">El texto del comando SQL.</param>
        /// <param name="parameters">Los parámetros del comando.</param>
        /// <returns>Un OracleCommand preparado para su ejecución.</returns>
        private OracleCommand PrepareCommand(string commandText, Dictionary<string, object> parameters)
        {
            if (_connection == null)
            {
                OpenConnection();
            }

            OracleCommand command = new OracleCommand(commandText, _connection)
            {
                CommandType = CommandType.Text,
                BindByName = true
            };

            if (_transaction != null)
            {
                command.Transaction = _transaction;
            }

            if (parameters != null && parameters.Count > 0)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(new OracleParameter(parameter.Key, parameter.Value ?? DBNull.Value));
                }
            }

            return command;
        }

        /// <summary>
        /// Obtiene el siguiente valor de una secuencia.
        /// </summary>
        /// <param name="sequenceName">Nombre de la secuencia.</param>
        /// <returns>Siguiente valor de la secuencia.</returns>
        protected int GetNextSequenceValue(string sequenceName)
        {
            try
            {
                string query = $"SELECT {sequenceName}.NEXTVAL FROM DUAL";
                object result = ExecuteScalar(query);
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener el siguiente valor de la secuencia {sequenceName}");
                throw new Exception($"Error al obtener el siguiente valor de la secuencia {sequenceName}.", ex);
            }
        }
    }

}
