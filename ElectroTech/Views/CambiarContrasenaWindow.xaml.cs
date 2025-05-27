using ElectroTech.Models;
using ElectroTech.Services;
using ElectroTech.Helpers;
using System;
using System.Linq;
using System.Windows;

namespace ElectroTech.Views.Perfil
{
    public partial class CambiarContrasenaWindow : Window
    {
        private readonly Usuario _usuario;
        private readonly AuthService _authService;

        public CambiarContrasenaWindow(Usuario usuario)
        {
            InitializeComponent();
            _usuario = usuario;
            _authService = new AuthService();
            txtContrasenaActual.Focus();
        }

        private void btnConfirmarCambio_Click(object sender, RoutedEventArgs e)
        {
            txtErrorContrasena.Visibility = Visibility.Collapsed;
            string contrasenaActual = txtContrasenaActual.Password;
            string nuevaContrasena = txtNuevaContrasena.Password;
            string confirmarNuevaContrasena = txtConfirmarNuevaContrasena.Password;

            if (string.IsNullOrWhiteSpace(contrasenaActual) ||
                string.IsNullOrWhiteSpace(nuevaContrasena) ||
                string.IsNullOrWhiteSpace(confirmarNuevaContrasena))
            {
                MostrarError("Todos los campos son obligatorios.");
                return;
            }

            if (nuevaContrasena.Length < 6) // O la longitud mínima que definas
            {
                MostrarError("La nueva contraseña debe tener al menos 6 caracteres.");
                return;
            }

            // Opcional: Añadir validación de formato de contraseña (mayúscula, minúscula, número)
            // if (!PasswordValidator.CumpleFormato(nuevaContrasena)) 
            // {
            //     MostrarError("La contraseña debe incluir mayúsculas, minúsculas y números.");
            //     return;
            // }


            if (nuevaContrasena != confirmarNuevaContrasena)
            {
                MostrarError("La nueva contraseña y la confirmación no coinciden.");
                txtNuevaContrasena.Clear();
                txtConfirmarNuevaContrasena.Clear();
                txtNuevaContrasena.Focus();
                return;
            }

            string mensajeError;
            // Usar el método de AuthService que verifica la contraseña actual antes de cambiarla
            bool cambiado = _authService.CambiarClave(_usuario, contrasenaActual, nuevaContrasena, out mensajeError); //

            if (cambiado)
            {
                MessageBox.Show("Contraseña actualizada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                Logger.LogInfo($"Usuario {_usuario.NombreUsuario} cambió su contraseña."); //
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MostrarError($"Error al cambiar la contraseña: {mensajeError}");
                txtContrasenaActual.Clear();
                txtNuevaContrasena.Clear();
                txtConfirmarNuevaContrasena.Clear();
                txtContrasenaActual.Focus();
                Logger.LogError($"Error al cambiar contraseña para {_usuario.NombreUsuario}: {mensajeError}"); //
            }
        }

        private void btnCancelarCambio_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void MostrarError(string mensaje)
        {
            txtErrorContrasena.Text = mensaje;
            txtErrorContrasena.Visibility = Visibility.Visible;
        }
    }
}