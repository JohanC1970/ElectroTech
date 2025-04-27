using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration; 
using Oracle.ManagedDataAccess.Client;

namespace ElectroTech.DataAccess
{
    public class ConnectionManager
    {

        private static ConnectionManager _instance;
        private readonly string _connectionString;

        /// <summary>
        /// Constructor privado para implementar el patrón Singleton.
        /// Lee la cadena de conexión desde el archivo de configuración.
        /// </summary>
        private ConnectionManager()
        {
            try
            {
                _connectionString = ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener la cadena de conexión desde el archivo de configuración.", ex);
            }
        }

        /// <summary>
        /// Propiedad estática para obtener la única instancia de ConnectionManager.
        /// </summary>
        public static ConnectionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ConnectionManager();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Obtiene una conexión abierta a la base de datos Oracle.
        /// </summary>
        /// <returns>Un objeto OracleConnection abierto.</returns>
        public OracleConnection GetConnection()
        {
            OracleConnection connection = new OracleConnection(_connectionString);
            try
            {
                connection.Open();
                return connection;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al conectar con la base de datos Oracle.", ex);
            }
        }

        /// <summary>
        /// Cierra una conexión abierta y libera los recursos.
        /// </summary>
        /// <param name="connection">La conexión que se desea cerrar.</param>
        public void CloseConnection(OracleConnection connection)
        {
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
                connection.Dispose();
            }
        }

        /// <summary>
        /// Verifica si la conexión a la base de datos está disponible.
        /// </summary>
        /// <returns>True si la conexión está disponible, False en caso contrario.</returns>
        public bool TestConnection()
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

    }
}
