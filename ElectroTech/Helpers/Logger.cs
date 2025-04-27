using System;
using System.IO;
using System.Text;

namespace ElectroTech.Helpers
{
    /// <summary>
    /// Clase para el registro de logs de la aplicación.
    /// </summary>
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static readonly string _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        private static readonly string _logFileName = "ElectroTech_Log_" + DateTime.Now.ToString("yyyyMMdd") + ".log";
        private static readonly string _logFilePath = Path.Combine(_logPath, _logFileName);

        /// <summary>
        /// Inicializa el directorio de logs si no existe.
        /// </summary>
        static Logger()
        {
            try
            {
                if (!Directory.Exists(_logPath))
                {
                    Directory.CreateDirectory(_logPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al inicializar el sistema de logs: " + ex.Message);
            }
        }

        /// <summary>
        /// Registra un mensaje informativo en el log.
        /// </summary>
        /// <param name="message">Mensaje a registrar.</param>
        public static void LogInfo(string message)
        {
            Log("INFO", message);
        }

        /// <summary>
        /// Registra un mensaje de advertencia en el log.
        /// </summary>
        /// <param name="message">Mensaje a registrar.</param>
        public static void LogWarning(string message)
        {
            Log("WARNING", message);
        }

        /// <summary>
        /// Registra un mensaje de error en el log.
        /// </summary>
        /// <param name="message">Mensaje a registrar.</param>
        public static void LogError(string message)
        {
            Log("ERROR", message);
        }

        /// <summary>
        /// Registra un error con la excepción completa en el log.
        /// </summary>
        /// <param name="ex">Excepción a registrar.</param>
        /// <param name="additionalInfo">Información adicional sobre el contexto del error.</param>
        public static void LogException(Exception ex, string additionalInfo = "")
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine(additionalInfo);
            message.AppendLine("Mensaje: " + ex.Message);
            message.AppendLine("StackTrace: " + ex.StackTrace);

            if (ex.InnerException != null)
            {
                message.AppendLine("InnerException: " + ex.InnerException.Message);
                message.AppendLine("InnerException StackTrace: " + ex.InnerException.StackTrace);
            }

            Log("EXCEPTION", message.ToString());
        }

        /// <summary>
        /// Método general para registrar mensajes en el log.
        /// </summary>
        /// <param name="level">Nivel del mensaje (INFO, WARNING, ERROR, EXCEPTION).</param>
        /// <param name="message">Mensaje a registrar.</param>
        private static void Log(string level, string message)
        {
            try
            {
                lock (_lock)
                {
                    using (StreamWriter writer = new StreamWriter(_logFilePath, true, Encoding.UTF8))
                    {
                        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] - {message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al escribir en el log: " + ex.Message);
            }
        }
    }
}