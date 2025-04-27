using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectroTech.Helpers
{
    public static class PasswordValidator
    {
        /// <summary>
        /// Genera un hash simple para la contraseña (en una aplicación real, usar un algoritmo más seguro).
        /// </summary>
        /// <param name="password">Contraseña a codificar.</param>
        /// <returns>Hash de la contraseña.</returns>
        public static string HashPassword(string password)
        {
            // En una aplicación real, usar un algoritmo de hash seguro como bcrypt o PBKDF2
            // Esta es una implementación simplificada para el propósito del proyecto
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Verifica si una contraseña coincide con su hash.
        /// </summary>
        /// <param name="password">Contraseña en texto plano.</param>
        /// <param name="hashedPassword">Hash de la contraseña.</param>
        /// <returns>True si la contraseña coincide con el hash, False en caso contrario.</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            string hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }

    }
}
