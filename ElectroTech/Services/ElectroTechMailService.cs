using System;
using System.Collections.Generic;
using ElectroTech.Helpers;

namespace ElectroTech.Services
{
    public class ElectroTechMailService : MailService
    {
        public ElectroTechMailService()
        {
            // Configuración para Gmail
            SenderMail = "garciacami728@gmail.com"; 
            Password = "fytt qdpa esoh viqk"; 
            Host = "smtp.gmail.com";
            Port = 587;
            ssl = true;

            // Inicializar el cliente SMTP
            initializeSmtpClient();
        }

        /// <summary>
        /// Envía un correo con la contraseña al usuario
        /// </summary>
        /// <param name="destinatario">Correo del destinatario</param>
        /// <param name="nombreUsuario">Nombre de usuario</param>
        /// <param name="contrasena">Contraseña actual</param>
        /// <returns>True si el envío es exitoso, False en caso contrario</returns>
        public bool EnviarContrasena(string destinatario, string nombreUsuario, string contrasena)
        {
            try
            {
                string asunto = "ElectroTech - Recuperación de Contraseña";
                string cuerpo = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ padding: 20px; }}
                        .header {{ background-color: #2E5D7D; color: white; padding: 10px; text-align: center; }}
                        .content {{ padding: 15px; }}
                        .footer {{ font-size: 12px; color: #666; margin-top: 20px; text-align: center; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>ElectroTech - Recuperación de Contraseña</h2>
                        </div>
                        <div class='content'>
                            <p>Estimado(a) usuario,</p>
                            <p>Hemos recibido una solicitud de recuperación de contraseña para su cuenta en ElectroTech.</p>
                            <p>A continuación, se detallan sus credenciales de acceso:</p>
                            <p><strong>Usuario:</strong> {nombreUsuario}</p>
                            <p><strong>Contraseña:</strong> {contrasena}</p>
                            <p>Por motivos de seguridad, le recomendamos cambiar su contraseña después de iniciar sesión.</p>
                            <p>Si usted no solicitó esta recuperación, por favor ignore este correo.</p>
                        </div>
                        <div class='footer'>
                            <p>Este es un correo automático, por favor no responda a este mensaje.</p>
                            <p>&copy; {DateTime.Now.Year} ElectroTech - Sistema de Gestión de Inventario</p>
                        </div>
                    </div>
                </body>
                </html>";

                List<string> destinatarios = new List<string> { destinatario };
                SendMail(asunto, cuerpo, destinatarios);

                Logger.LogInfo($"Correo de recuperación enviado exitosamente a {destinatario}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al enviar correo de recuperación a {destinatario}");
                return false;
            }
        }
    }
}