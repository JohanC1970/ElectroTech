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
    public abstract class DatabaseRepository
    {
        protected OracleConnection _connection;
        protected OracleTransaction _transaction;

        protected DatabaseRepository()
        {
            // No se abre la conexión aquí por defecto. Se abrirá cuando sea necesario.
        }

        protected void OpenConnection()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection = ConnectionManager.Instance.GetConnection(); //
            }
        }

        protected void CloseConnection()
        {
            // Solo cierra si no estamos en medio de una transacción explícita
            // y si la conexión fue abierta por este repositorio.
            if (_transaction == null && _connection != null)
            {
                ConnectionManager.Instance.CloseConnection(_connection); //
                _connection = null;
            }
            // Si hay una transacción activa, la conexión se cerrará cuando la transacción termine (commit/rollback)
            // y luego se llame a CloseConnection en el finally del método que inició la transacción.
        }

        protected void EnsureConnectionIsClosed()
        {
            // Este método es para ser llamado en el finally más externo.
            if (_connection != null)
            {
                ConnectionManager.Instance.CloseConnection(_connection);
                _connection = null;
            }
            _transaction = null; // Asegurarse de que la transacción también se limpie
        }


        protected void BeginTransaction()
        {
            OpenConnection(); // Asegura que la conexión esté abierta
            if (_transaction == null) // Solo inicia una nueva transacción si no hay una activa
            {
                _transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
            }
        }

        protected void CommitTransaction()
        {
            try
            {
                if (_transaction != null)
                {
                    if (_transaction.Connection != null && _transaction.Connection.State == ConnectionState.Open)
                    {
                        _transaction.Commit();
                    }
                    else
                    {
                        Logger.LogWarning("Intento de Commit en una transacción con conexión cerrada o nula."); //
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error durante CommitTransaction."); //
                // Considerar relanzar o manejar específicamente
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                }
            }
        }

        protected void RollbackTransaction()
        {
            try
            {
                if (_transaction != null)
                {
                    // Solo intentar rollback si la conexión de la transacción está abierta
                    if (_transaction.Connection != null && _transaction.Connection.State == ConnectionState.Open)
                    {
                        _transaction.Rollback();
                    }
                    else
                    {
                        // Si la conexión ya está cerrada, el rollback fallaría o no haría nada.
                        // Registrar esto puede ser útil.
                        Logger.LogWarning("Intento de Rollback en una transacción con conexión cerrada o nula."); //
                    }
                }
            }
            catch (Exception ex)
            {
                // Capturar la excepción de Oracle aquí si es ORA-50045 u otra relacionada
                Logger.LogException(ex, "Error durante RollbackTransaction."); //
                // No relanzar para evitar enmascarar la excepción original si ya estamos en un catch.
            }
            finally
            {
                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                }
            }
        }

        protected int ExecuteNonQuery(string commandText, Dictionary<string, object> parameters = null)
        {
            bool connectionOpenedHere = false;
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                OpenConnection();
                connectionOpenedHere = true;
            }

            try
            {
                using (OracleCommand command = PrepareCommand(commandText, parameters))
                {
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error en ExecuteNonQuery: {commandText}"); //
                throw; // Relanzar para que el manejo de transacciones pueda actuar
            }
            finally
            {
                if (connectionOpenedHere && _transaction == null) // Cierra solo si se abrió aquí y no hay transacción activa
                {
                    CloseConnection();
                }
            }
        }

        protected object ExecuteScalar(string commandText, Dictionary<string, object> parameters = null)
        {
            bool connectionOpenedHere = false;
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                OpenConnection();
                connectionOpenedHere = true;
            }
            try
            {
                using (OracleCommand command = PrepareCommand(commandText, parameters))
                {
                    return command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error en ExecuteScalar: {commandText}"); //
                throw;
            }
            finally
            {
                if (connectionOpenedHere && _transaction == null)
                {
                    CloseConnection();
                }
            }
        }

        protected DataTable ExecuteQuery(string commandText, Dictionary<string, object> parameters = null)
        {
            bool connectionOpenedHere = false;
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                OpenConnection();
                connectionOpenedHere = true;
            }
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
                Logger.LogException(ex, $"Error en ExecuteQuery: {commandText}"); //
                throw;
            }
            finally
            {
                if (connectionOpenedHere && _transaction == null)
                {
                    CloseConnection();
                }
            }
        }


        private OracleCommand PrepareCommand(string commandText, Dictionary<string, object> parameters)
        {
            // OpenConnection(); // La conexión se maneja antes de llamar a este método
            OracleCommand command = new OracleCommand(commandText, _connection)
            {
                CommandType = CommandType.Text,
                BindByName = true // Importante para Oracle al usar parámetros nombrados
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

        protected int GetNextSequenceValue(string sequenceName)
        {
            // Esta función ya no necesita abrir/cerrar conexión si las funciones ExecuteScalar lo hacen
            string query = $"SELECT {sequenceName}.NEXTVAL FROM DUAL";
            object result = ExecuteScalar(query);
            return Convert.ToInt32(result);
        }
    }
}