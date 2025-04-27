using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using ElectroTech.Helpers;

namespace ElectroTech.DataAccess
{
    /// <summary>
    /// Clase de ayuda para ejecutar operaciones SQL comunes.
    /// </summary>
    public static class SqlHelper
    {
        /// <summary>
        /// Ejecuta una consulta SQL y devuelve un DataTable con los resultados.
        /// </summary>
        /// <param name="sql">Consulta SQL a ejecutar.</param>
        /// <param name="parameters">Parámetros para la consulta.</param>
        /// <returns>DataTable con los resultados.</returns>
        public static DataTable ExecuteQuery(string sql, Dictionary<string, object> parameters = null)
        {
            string connectionString = DbSettings.GetConnectionString();
            DataTable dataTable = new DataTable();

            try
            {
                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    connection.Open();
                    using (OracleCommand command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        command.CommandType = CommandType.Text;

                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.Add(new OracleParameter(param.Key, param.Value ?? DBNull.Value));
                            }
                        }

                        using (OracleDataAdapter adapter = new OracleDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }

                return dataTable;
            }
            catch (Exception ex)
            {
                Exception dbException = DatabaseException.ConvertException(ex, "Error al ejecutar la consulta SQL");
                Logger.LogException(dbException);
                throw dbException;
            }
        }

        /// <summary>
        /// Ejecuta un comando SQL que no devuelve resultados (INSERT, UPDATE, DELETE).
        /// </summary>
        /// <param name="sql">Comando SQL a ejecutar.</param>
        /// <param name="parameters">Parámetros para el comando.</param>
        /// <returns>Número de filas afectadas.</returns>
        public static int ExecuteNonQuery(string sql, Dictionary<string, object> parameters = null)
        {
            string connectionString = DbSettings.GetConnectionString();

            try
            {
                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    connection.Open();
                    using (OracleCommand command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        command.CommandType = CommandType.Text;

                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.Add(new OracleParameter(param.Key, param.Value ?? DBNull.Value));
                            }
                        }

                        return command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Exception dbException = DatabaseException.ConvertException(ex, "Error al ejecutar el comando SQL");
                Logger.LogException(dbException);
                throw dbException;
            }
        }

        /// <summary>
        /// Ejecuta un comando SQL que devuelve un valor escalar.
        /// </summary>
        /// <param name="sql">Comando SQL a ejecutar.</param>
        /// <param name="parameters">Parámetros para el comando.</param>
        /// <returns>Valor escalar devuelto por el comando.</returns>
        public static object ExecuteScalar(string sql, Dictionary<string, object> parameters = null)
        {
            string connectionString = DbSettings.GetConnectionString();

            try
            {
                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    connection.Open();
                    using (OracleCommand command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        command.CommandType = CommandType.Text;

                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.Add(new OracleParameter(param.Key, param.Value ?? DBNull.Value));
                            }
                        }

                        object result = command.ExecuteScalar();
                        return result == DBNull.Value ? null : result;
                    }
                }
            }
            catch (Exception ex)
            {
                Exception dbException = DatabaseException.ConvertException(ex, "Error al ejecutar el comando SQL");
                Logger.LogException(dbException);
                throw dbException;
            }
        }

        /// <summary>
        /// Ejecuta una serie de comandos SQL dentro de una transacción.
        /// </summary>
        /// <param name="sqlCommands">Lista de comandos SQL a ejecutar.</param>
        /// <param name="parametersList">Lista de parámetros para cada comando.</param>
        /// <returns>True si la transacción fue exitosa, False en caso contrario.</returns>
        public static bool ExecuteTransaction(List<string> sqlCommands, List<Dictionary<string, object>> parametersList = null)
        {
            string connectionString = DbSettings.GetConnectionString();

            try
            {
                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    connection.Open();
                    using (OracleTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (OracleCommand command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;

                                for (int i = 0; i < sqlCommands.Count; i++)
                                {
                                    command.CommandText = sqlCommands[i];
                                    command.Parameters.Clear();

                                    if (parametersList != null && i < parametersList.Count && parametersList[i] != null)
                                    {
                                        foreach (var param in parametersList[i])
                                        {
                                            command.Parameters.Add(new OracleParameter(param.Key, param.Value ?? DBNull.Value));
                                        }
                                    }

                                    command.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Exception dbException = DatabaseException.ConvertException(ex, "Error al ejecutar la transacción SQL");
                            Logger.LogException(dbException);
                            throw dbException;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Exception dbException = DatabaseException.ConvertException(ex, "Error al conectar con la base de datos");
                Logger.LogException(dbException);
                throw dbException;
            }
        }

        /// <summary>
        /// Obtiene el siguiente valor de una secuencia de Oracle.
        /// </summary>
        /// <param name="sequenceName">Nombre de la secuencia.</param>
        /// <returns>Siguiente valor de la secuencia.</returns>
        public static int GetNextSequenceValue(string sequenceName)
        {
            string sql = $"SELECT {sequenceName}.NEXTVAL FROM DUAL";
            object result = ExecuteScalar(sql);
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// Convierte un DataRow a un tipo específico mediante reflexión.
        /// </summary>
        /// <typeparam name="T">Tipo al que se convertirá el DataRow.</typeparam>
        /// <param name="row">DataRow con los datos.</param>
        /// <returns>Objeto del tipo T con los datos del DataRow.</returns>
        public static T ConvertTo<T>(DataRow row) where T : new()
        {
            T obj = new T();
            Type type = typeof(T);

            foreach (DataColumn column in row.Table.Columns)
            {
                // Buscar una propiedad con el mismo nombre que la columna
                var property = type.GetProperty(column.ColumnName);

                // Si encuentra una propiedad con el mismo nombre, asignar el valor
                if (property != null && row[column] != DBNull.Value)
                {
                    // Convertir el valor al tipo de la propiedad
                    property.SetValue(obj, Convert.ChangeType(row[column], property.PropertyType));
                }
            }

            return obj;
        }

        /// <summary>
        /// Convierte un DataTable a una lista de objetos del tipo especificado.
        /// </summary>
        /// <typeparam name="T">Tipo de objetos en la lista.</typeparam>
        /// <param name="table">DataTable con los datos.</param>
        /// <returns>Lista de objetos del tipo T.</returns>
        public static List<T> ConvertToList<T>(DataTable table) where T : new()
        {
            List<T> list = new List<T>();

            foreach (DataRow row in table.Rows)
            {
                list.Add(ConvertTo<T>(row));
            }

            return list;
        }
    }
}