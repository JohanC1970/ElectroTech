using ElectroTech.DataAccess;
using ElectroTech.Helpers;
using ElectroTech.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ElectroTech.Views
{
    /// <summary>
    /// Lógica de interacción para RecuperarContrasenaView.xaml
    /// </summary>
    public partial class RecuperarContrasenaView : Window
    {
        private readonly UsuarioRepository _usuarioRepository;
        private readonly ElectroTechMailService _mailService;

        /// <summary>
        /// Constructor de la ventana de recuperación de contraseña
        /// </summary>
        public RecuperarContrasenaView()
        {
            InitializeComponent();
            _usuarioRepository = new UsuarioRepository();
            _mailService = new ElectroTechMailService();
            txtCorreo.Focus();
        }

        /// <summary>
        /// Evento clic del botón Enviar
        /// </summary>
        private void btnEnviar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener y validar datos
                string correo = txtCorreo.Text.Trim();

                if (string.IsNullOrEmpty(correo))
                {
                    MostrarMensaje("Por favor, ingresa tu correo electrónico.", false);
                    return;
                }

                // Verificar formato básico de correo
                if (!EsFormatoCorreoValido(correo))
                {
                    MostrarMensaje("Por favor, ingresa un correo electrónico válido.", false);
                    return;
                }

                // Recuperar información del usuario y contraseña
                var datosUsuario = ObtenerDatosUsuario(correo);

                if (datosUsuario != null)
                {
                    // Enviar correo con la contraseña
                    bool enviado = _mailService.EnviarContrasena(correo, datosUsuario.Item1, datosUsuario.Item2);

                    if (enviado)
                    {
                        MostrarMensaje("Se ha enviado tu contraseña al correo electrónico registrado. " +
                            "Por favor, revisa tu bandeja de entrada.", true);

                        // Limpiar campos
                        txtCorreo.Text = string.Empty;
                    }
                    else
                    {
                        MostrarMensaje("Error al enviar el correo. Por favor, intenta nuevamente más tarde.", false);
                    }
                }
                else
                {
                    MostrarMensaje("No se encontró ninguna cuenta asociada a este correo electrónico.", false);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error en la recuperación de contraseña");
                MostrarMensaje("Error al procesar la solicitud. Por favor, inténtelo de nuevo más tarde.", false);
            }
        }

        /// <summary>
        /// Verifica si el formato del correo es válido (comprobación básica)
        /// </summary>
        private bool EsFormatoCorreoValido(string correo)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(correo);
                return addr.Address == correo;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtiene los datos del usuario a partir del correo
        /// </summary>
        /// <returns>Tupla con (nombreUsuario, contraseña) o null si no existe</returns>
        private Tuple<string, string> ObtenerDatosUsuario(string correo)
        {
            try
            {
                string query = @"
                    SELECT nombreUsuario, clave 
                    FROM Usuario 
                    WHERE correo = :correo AND estado = 'A'";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":correo", correo }
                };

                var dataTable = SqlHelper.ExecuteQuery(query, parameters);

                if (dataTable.Rows.Count > 0)
                {
                    string nombreUsuario = dataTable.Rows[0]["nombreUsuario"].ToString();
                    string claveHash = dataTable.Rows[0]["clave"].ToString();

                    // En una aplicación real, aquí debería generarse una nueva contraseña temporal
                    // y actualizarla en la base de datos, en lugar de enviar la actual (que está hasheada)

                    // Para este ejemplo, usamos una contraseña temporal
                    string contrasenaTemporal = GenerarContrasenaTemporal();
                    ActualizarContrasenaUsuario(nombreUsuario, contrasenaTemporal);

                    return new Tuple<string, string>(nombreUsuario, contrasenaTemporal);
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener datos de usuario para recuperación");
                return null;
            }
        }

        /// <summary>
        /// Genera una contraseña temporal aleatoria
        /// </summary>
        private string GenerarContrasenaTemporal()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new char[8]; // Contraseña de 8 caracteres

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            return new string(result);
        }

        /// <summary>
        /// Actualiza la contraseña del usuario en la base de datos
        /// </summary>
        private bool ActualizarContrasenaUsuario(string nombreUsuario, string nuevaContrasena)
        {
            try
            {
                // Hashear la nueva contraseña
                string hashedPassword = PasswordValidator.HashPassword(nuevaContrasena);

                string query = "UPDATE Usuario SET clave = :clave WHERE nombreUsuario = :nombreUsuario";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":clave", hashedPassword },
                    { ":nombreUsuario", nombreUsuario }
                };

                int rowsAffected = SqlHelper.ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al actualizar contraseña temporal");
                return false;
            }
        }

        /// <summary>
        /// Muestra un mensaje informativo o de error
        /// </summary>
        private void MostrarMensaje(string mensaje, bool esExito)
        {
            txtMensaje.Text = mensaje;
            txtMensaje.Foreground = esExito ? Brushes.Green : Brushes.Red;
            txtMensaje.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Evento clic del botón Volver
        /// </summary>
        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}