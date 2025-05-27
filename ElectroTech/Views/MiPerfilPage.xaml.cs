using ElectroTech.Models;
using ElectroTech.Services;
using ElectroTech.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ElectroTech.Views.Perfil
{
    public partial class MiPerfilPage : Page
    {
        private Usuario _usuarioLogueado;
        private readonly AuthService _authService;

        public MiPerfilPage()
        {
            InitializeComponent();
            _authService = new AuthService();
            CargarDatosUsuario();
        }

        private void CargarDatosUsuario()
        {
            _usuarioLogueado = LoginView.UsuarioActual; //
            if (_usuarioLogueado != null)
            {
                txtNombreUsuario.Text = _usuarioLogueado.NombreUsuario; //
                txtNombreCompleto.Text = _usuarioLogueado.NombreCompleto; //
                txtCorreo.Text = _usuarioLogueado.Correo; //
                txtNivelAcceso.Text = _usuarioLogueado.NombreNivel; //
                txtFechaCreacion.Text = _usuarioLogueado.FechaCreacion.ToString("dd/MM/yyyy HH:mm"); //
                txtUltimaConexion.Text = _usuarioLogueado.UltimaConexion?.ToString("dd/MM/yyyy HH:mm") ?? "N/A"; //
            }
            else
            {
                MessageBox.Show("No se pudo cargar la información del usuario.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Opcionalmente, redirigir al login o cerrar
            }
        }

        private void btnGuardarCambios_Click(object sender, RoutedEventArgs e)
        {
            txtError.Visibility = Visibility.Collapsed;
            if (_usuarioLogueado == null) return;

            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(txtNombreCompleto.Text))
            {
                MostrarError("El nombre completo no puede estar vacío.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtCorreo.Text) || !EsFormatoCorreoValido(txtCorreo.Text))
            {
                MostrarError("Por favor, ingrese un correo electrónico válido.");
                return;
            }

            _usuarioLogueado.NombreCompleto = txtNombreCompleto.Text.Trim(); //
            _usuarioLogueado.Correo = txtCorreo.Text.Trim(); //

            string mensajeError;
            bool actualizado = _authService.ActualizarUsuario(_usuarioLogueado, out mensajeError); //

            if (actualizado)
            {
                MessageBox.Show("Información de perfil actualizada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                Logger.LogInfo($"Usuario {_usuarioLogueado.NombreUsuario} actualizó su perfil."); //
            }
            else
            {
                MostrarError($"Error al actualizar el perfil: {mensajeError}");
                Logger.LogError($"Error al actualizar perfil para {_usuarioLogueado.NombreUsuario}: {mensajeError}"); //
            }
        }

        private void btnCambiarContrasena_Click(object sender, RoutedEventArgs e)
        {

            var cambioContrasenaWindow = new CambiarContrasenaWindow(_usuarioLogueado);
            cambioContrasenaWindow.Owner = Window.GetWindow(this);
            cambioContrasenaWindow.ShowDialog();
        }

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

        private void MostrarError(string mensaje)
        {
            txtError.Text = mensaje;
            txtError.Visibility = Visibility.Visible;
        }
    }
}