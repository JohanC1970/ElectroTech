using ElectroTech.DataAccess;
using ElectroTech.Helpers;
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

        /// <summary>
        /// Constructor de la ventana de recuperación de contraseña
        /// </summary>
        public RecuperarContrasenaView()
        {
            InitializeComponent();
            _usuarioRepository = new UsuarioRepository();
            txtUsuario.Focus();
        }

        /// <summary>
        /// Evento clic del botón Enviar
        /// </summary>
        private void btnEnviar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener y validar datos
                string nombreUsuario = txtUsuario.Text.Trim();
                string correo = txtCorreo.Text.Trim();

                if (string.IsNullOrEmpty(nombreUsuario) || string.IsNullOrEmpty(correo))
                {
                    MostrarMensaje("Por favor, complete todos los campos.", false);
                    return;
                }

                // En una aplicación real, aquí se verificaría si el usuario existe y el correo coincide
                // Luego se enviaría un correo con instrucciones de recuperación

                // Verificar si el usuario existe
                bool existeUsuario = VerificarUsuario(nombreUsuario, correo);

                if (existeUsuario)
                {
                    // Simulamos el envío de correo
                    GenerarEnlaceRecuperacion(nombreUsuario);

                    MostrarMensaje("Se han enviado las instrucciones de recuperación a tu correo electrónico. " +
                        "Por favor, revisa tu bandeja de entrada.", true);

                    // Limpiar campos
                    txtUsuario.Text = string.Empty;
                    txtCorreo.Text = string.Empty;
                }
                else
                {
                    MostrarMensaje("No se encontró un usuario con los datos ingresados.", false);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error en la recuperación de contraseña");
                MostrarMensaje("Error al procesar la solicitud. Por favor, inténtelo de nuevo más tarde.", false);
            }
        }

        /// <summary>
        /// Verifica si existe un usuario con el nombre y correo proporcionados
        /// </summary>
        private bool VerificarUsuario(string nombreUsuario, string correo)
        {
            try
            {
                // En una aplicación real, aquí se haría una consulta a la base de datos
                // Para este ejemplo, simulamos una verificación

                string query = "SELECT COUNT(*) FROM Usuario WHERE nombreUsuario = :nombreUsuario AND correo = :correo";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":nombreUsuario", nombreUsuario },
                    { ":correo", correo }
                };

                object result = SqlHelper.ExecuteScalar(query, parameters);
                int count = Convert.ToInt32(result);

                return count > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al verificar usuario para recuperación de contraseña");
                return false;
            }
        }

        /// <summary>
        /// Genera un enlace de recuperación (simulado para este ejemplo)
        /// </summary>
        private void GenerarEnlaceRecuperacion(string nombreUsuario)
        {
            // En una aplicación real, aquí se generaría un token y se enviaría por correo
            // Para este ejemplo, solo registramos en el log

            string token = Guid.NewGuid().ToString();
            Logger.LogInfo($"Generado token de recuperación de contraseña para el usuario {nombreUsuario}: {token}");

            // En un caso real, aquí se enviaría un correo con un enlace que contenga el token
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