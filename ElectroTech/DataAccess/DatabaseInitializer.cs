using System;
using System.IO;
using System.Threading.Tasks;
using ElectroTech.Helpers;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;

namespace ElectroTech.DataAccess
{
    /// <summary>
    /// Clase para inicializar la conexión a la base de datos y su estructura.
    /// </summary>
    public static class DatabaseInitializer
    {
        /// <summary>
        /// Inicializa la base de datos al iniciar la aplicación.
        /// </summary>
        /// <returns>True si la inicialización es exitosa, False en caso contrario.</returns>
        public static bool Initialize()
        {
            try
            {
                Logger.LogInfo("Iniciando conexión a la base de datos...");

                // Verificar si se puede conectar a la base de datos
                try
                {
                    bool canConnect = TestConnection();
                    if (!canConnect)
                    {
                        Logger.LogError("No se pudo conectar a la base de datos con la configuración actual.");
                        return false;
                    }
                }
                catch (Exception connEx)
                {
                    Logger.LogError($"Error específico de conexión: {connEx.Message}");
                    Logger.LogException(connEx, "Detalles del error de conexión");
                    return false;
                }

                Logger.LogInfo("Conexión a la base de datos establecida exitosamente.");

                // Aquí iría la inicialización del resto del sistema...

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al inicializar la base de datos");
                // Opcional: mensaje en pantalla si tienes interfaz gráfica
                // MessageBox.Show($"Error detallado: {ex.Message}", "Error de inicialización", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

        }

        /// <summary>
        /// Verifica la conexión a la base de datos con la configuración actual.
        /// </summary>
        /// <returns>True si la conexión es exitosa, False en caso contrario.</returns>
        public static bool TestConnection()
        {
            try
            {
                string connectionString = DbSettings.GetConnectionString();
                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Capture el mensaje de error específico
                string errorMessage = ex.Message;
                Logger.LogError($"Error específico de conexión: {errorMessage}");

                // Si hay una excepción interna, captúrela también
                if (ex.InnerException != null)
                {
                    Logger.LogError($"Inner Exception: {ex.InnerException.Message}");
                }

                return false;
            }
        }

        /// <summary>
        /// Ejecuta el script de creación de base de datos de manera asíncrona.
        /// </summary>
        /// <returns>Task que representa la operación asincrónica.</returns>
        public static async Task<bool> ExecuteDatabaseScriptAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", "CreacionBaseDatos.sql");
                    if (!File.Exists(scriptPath))
                    {
                        Logger.LogError("El script de creación de base de datos no existe.");
                        return false;
                    }

                    Logger.LogInfo("Ejecutando script de creación de base de datos...");
                    bool scriptExecuted = DbSettings.ExecuteScript(scriptPath);
                    if (!scriptExecuted)
                    {
                        Logger.LogError("Error al ejecutar el script de creación de base de datos.");
                        return false;
                    }

                    Logger.LogInfo("Script de creación de base de datos ejecutado exitosamente.");
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "Error al ejecutar el script de creación de base de datos");
                    return false;
                }
            });
        }

        /// <summary>
        /// Verifica si la tabla Usuario existe en la base de datos.
        /// </summary>
        /// <returns>True si la tabla existe, False en caso contrario.</returns>
        public static bool TableUsuarioExists()
        {
            try
            {
                string sql = @"
                    SELECT COUNT(*) 
                    FROM ALL_TABLES 
                    WHERE TABLE_NAME = 'USUARIO'";

                object result = SqlHelper.ExecuteScalar(sql);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al verificar existencia de tabla Usuario");
                return false;
            }
        }

        /// <summary>
        /// Verifica si existe un usuario administrador en la base de datos.
        /// </summary>
        /// <returns>True si existe un administrador, False en caso contrario.</returns>
        public static bool AdminUserExists()
        {
            try
            {
                if (!TableUsuarioExists())
                    return false;

                string sql = "SELECT COUNT(*) FROM Usuario WHERE nivel = 1";
                object result = SqlHelper.ExecuteScalar(sql);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al verificar existencia de usuario administrador");
                return false;
            }
        }

        /// <summary>
        /// Crea un usuario administrador en la base de datos.
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario.</param>
        /// <param name="clave">Contraseña del usuario.</param>
        /// <param name="nombreCompleto">Nombre completo del usuario.</param>
        /// <param name="correo">Correo electrónico del usuario.</param>
        /// <returns>True si la creación es exitosa, False en caso contrario.</returns>
        public static bool CreateAdminUser(string nombreUsuario, string clave, string nombreCompleto, string correo)
        {
            try
            {
                if (!TableUsuarioExists())
                {
                    Logger.LogError("La tabla Usuario no existe en la base de datos.");
                    return false;
                }

                if (AdminUserExists())
                {
                    Logger.LogWarning("Ya existe un usuario administrador en la base de datos.");
                    return true;
                }

                // Hashear la contraseña
                string hashedPassword = PasswordValidator.HashPassword(clave);

                // Crear el usuario administrador
                string sql = @"
                    INSERT INTO Usuario (idUsuario, nombreUsuario, clave, nivel, nombreCompleto, correo, estado, fechaCreacion) 
                    VALUES (SEQ_USUARIO.NEXTVAL, :nombreUsuario, :clave, 1, :nombreCompleto, :correo, 'A', SYSDATE)";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":nombreUsuario", nombreUsuario },
                    { ":clave", hashedPassword },
                    { ":nombreCompleto", nombreCompleto },
                    { ":correo", correo }
                };

                int rowsAffected = SqlHelper.ExecuteNonQuery(sql, parameters);

                if (rowsAffected > 0)
                {
                    Logger.LogInfo($"Usuario administrador {nombreUsuario} creado exitosamente.");
                    return true;
                }
                else
                {
                    Logger.LogError("No se pudo crear el usuario administrador.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al crear usuario administrador");
                return false;
            }
        }

        /// <summary>
        /// Verifica el estado de la base de datos y realiza las inicializaciones necesarias.
        /// </summary>
        /// <returns>Resultado de la verificación de la base de datos.</returns>
        public static DatabaseCheckResult CheckDatabase()
        {
            try
            {
                // Verificar conexión
                if (!TestConnection())
                {
                    return DatabaseCheckResult.ConnectionError;
                }

                // Verificar si la tabla Usuario existe
                if (!TableUsuarioExists())
                {
                    return DatabaseCheckResult.TablesNotExist;
                }

                // Verificar si existe un usuario administrador
                if (!AdminUserExists())
                {
                    return DatabaseCheckResult.AdminUserNotExist;
                }

                return DatabaseCheckResult.OK;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al verificar el estado de la base de datos");
                return DatabaseCheckResult.UnknownError;
            }
        }
    }

    /// <summary>
    /// Enumeración que representa el resultado de la verificación de la base de datos.
    /// </summary>
    public enum DatabaseCheckResult
    {
        /// <summary>
        /// Todo está correcto.
        /// </summary>
        OK,

        /// <summary>
        /// Error de conexión a la base de datos.
        /// </summary>
        ConnectionError,

        /// <summary>
        /// Las tablas no existen en la base de datos.
        /// </summary>
        TablesNotExist,

        /// <summary>
        /// No existe un usuario administrador.
        /// </summary>
        AdminUserNotExist,

        /// <summary>
        /// Error desconocido.
        /// </summary>
        UnknownError
    }
}