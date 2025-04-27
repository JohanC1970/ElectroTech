using System;
using Oracle.ManagedDataAccess.Client;

namespace ElectroTech.Helpers
{
    /// <summary>
    /// Clase para manejar excepciones específicas de la base de datos.
    /// </summary>
    public class DatabaseException : Exception
    {
        /// <summary>
        /// Código de error de Oracle.
        /// </summary>
        public int ErrorCode { get; }

        /// <summary>
        /// Indica si el error es causado por una restricción de integridad.
        /// </summary>
        public bool IsIntegrityConstraintViolation => ErrorCode == 2292 || ErrorCode == 2291;

        /// <summary>
        /// Indica si el error es causado por una clave única duplicada.
        /// </summary>
        public bool IsUniqueConstraintViolation => ErrorCode == 1 || ErrorCode == 1400;

        /// <summary>
        /// Indica si el error es causado por una verificación de restricción (check).
        /// </summary>
        public bool IsCheckConstraintViolation => ErrorCode == 2290;

        /// <summary>
        /// Indica si el error es causado por una validación definida por el usuario en un trigger.
        /// </summary>
        public bool IsUserDefinedError => ErrorCode >= 20000 && ErrorCode <= 20999;

        /// <summary>
        /// Constructor para crear una excepción de base de datos a partir de otra excepción.
        /// </summary>
        /// <param name="message">Mensaje de error.</param>
        /// <param name="innerException">Excepción interna.</param>
        public DatabaseException(string message, Exception innerException)
            : base(message, innerException)
        {
            if (innerException is OracleException oracleEx)
            {
                ErrorCode = oracleEx.Number;
            }
            else
            {
                ErrorCode = 0;
            }

            // Registrar la excepción en el log
            Logger.LogException(this, "Excepción de base de datos");
        }

        /// <summary>
        /// Constructor para crear una excepción de base de datos con un mensaje personalizado.
        /// </summary>
        /// <param name="message">Mensaje de error.</param>
        /// <param name="errorCode">Código de error.</param>
        public DatabaseException(string message, int errorCode)
            : base(message)
        {
            ErrorCode = errorCode;

            // Registrar la excepción en el log
            Logger.LogException(this, "Excepción de base de datos");
        }

        /// <summary>
        /// Obtiene un mensaje amigable para el usuario según el tipo de error.
        /// </summary>
        /// <returns>Mensaje amigable.</returns>
        public string GetUserFriendlyMessage()
        {
            // Errores de integridad
            if (IsIntegrityConstraintViolation)
            {
                return "No se puede eliminar este registro porque está siendo utilizado en otras partes del sistema.";
            }

            // Errores de unicidad
            if (IsUniqueConstraintViolation)
            {
                if (Message.Contains("PK_") || Message.Contains("PRIMARY"))
                {
                    return "Ya existe un registro con el mismo identificador.";
                }
                if (Message.Contains("UK_") || Message.Contains("UNIQUE"))
                {
                    return "Ya existe un registro con la misma información única.";
                }
                return "Ya existe un registro con la misma información.";
            }

            // Errores de restricciones de check
            if (IsCheckConstraintViolation)
            {
                return "El valor ingresado no cumple con las restricciones establecidas.";
            }

            // Errores definidos por el usuario (triggers)
            if (IsUserDefinedError)
            {
                // Los errores definidos por usuario en Oracle suelen tener mensajes específicos
                // que ya son amigables para el usuario
                string userMessage = GetUserDefinedErrorMessage();
                if (!string.IsNullOrEmpty(userMessage))
                {
                    return userMessage;
                }

                return "Se ha producido un error de validación en la base de datos.";
            }

            // Otros errores
            return "Se ha producido un error al acceder a la base de datos. Por favor, inténtelo de nuevo más tarde.";
        }

        /// <summary>
        /// Extrae el mensaje de error definido por el usuario de un trigger.
        /// </summary>
        /// <returns>Mensaje de error o cadena vacía si no se puede extraer.</returns>
        private string GetUserDefinedErrorMessage()
        {
            try
            {
                if (InnerException is OracleException oracleEx)
                {
                    // En Oracle, los mensajes de error definidos por el usuario 
                    // suelen estar en el formato: ORA-20001: Mensaje personalizado
                    string fullMessage = oracleEx.Message;

                    // Buscar el patrón: ORA-NNNNN: mensaje
                    int startIndex = fullMessage.IndexOf(": ");

                    if (startIndex >= 0 && startIndex + 2 < fullMessage.Length)
                    {
                        return fullMessage.Substring(startIndex + 2);
                    }
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Método estático para verificar si una excepción es de Oracle y convertirla
        /// a DatabaseException para un mejor manejo.
        /// </summary>
        /// <param name="ex">Excepción a verificar.</param>
        /// <param name="customMessage">Mensaje personalizado opcional.</param>
        /// <returns>Una DatabaseException si es de Oracle, la excepción original en caso contrario.</returns>
        public static Exception ConvertException(Exception ex, string customMessage = null)
        {
            if (ex is OracleException oracleEx)
            {
                string message = string.IsNullOrEmpty(customMessage)
                    ? "Error de base de datos Oracle"
                    : customMessage;

                return new DatabaseException(message, ex);
            }

            return ex;
        }
    }
}