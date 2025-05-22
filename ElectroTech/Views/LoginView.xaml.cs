using ElectroTech.DataAccess;
using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ElectroTech.Views
{
    /// <summary>
    /// Lógica de interacción para LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        private readonly AuthService _authService;
        private static Usuario _usuarioActual;

        /// <summary>
        /// Constructor de la ventana de login
        /// </summary>
        public LoginView()
        {
            InitializeComponent();
            _authService = new AuthService();
            txtUsuario.Focus();

            



        }

        /// <summary>
        /// Propiedad estática para acceder al usuario autenticado desde otras partes de la aplicación
        /// </summary>
        public static Usuario UsuarioActual
        {
            get { return _usuarioActual; }
        }

        /// <summary>
        /// Evento clic del botón Ingresar
        /// </summary>
        private void btnIngresar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string nombreUsuario = txtUsuario.Text.Trim();
                string contrasena = txtContrasena.Password;

                // Validaciones básicas
                if (string.IsNullOrEmpty(nombreUsuario) || string.IsNullOrEmpty(contrasena))
                {
                    MostrarError("Por favor, complete todos los campos.");
                    return;
                }

                // Intenta autenticar al usuario
                var usuario = _authService.Autenticar(nombreUsuario, contrasena);

                if (usuario != null)
                {
                    _usuarioActual = usuario;

                    Logger.LogInfo($"Usuario {nombreUsuario} ha iniciado sesión exitosamente.");

                    // Cargar y mostrar la ventana principal

                    var MainDashboardWindow = new MainDashboardWindow();
                    MainDashboardWindow.Owner = this;
                    MainDashboardWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    MainDashboardWindow.ShowDialog();
                    // Cerrar la ventana de login
                    //this.Hide();
                    this.Close();

                    //var mainWindow = new MainWindow();
                    //Application.Current.MainWindow = mainWindow;
                    //mainWindow.Show();
                    //this.Close();
                }
                else
                {
                    MostrarError("Nombre de usuario o contraseña incorrectos.");
                    txtContrasena.Password = string.Empty;
                    txtContrasena.Focus();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error en el inicio de sesión");
                MostrarError("Error al iniciar sesión. Por favor, inténtelo de nuevo más tarde.");
            }
        }

        /// <summary>
        /// Evento clic del botón Recuperar Contraseña
        /// </summary>
        private void btnRecuperarContrasena_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Abrir ventana de recuperación de contraseña
                var recuperarContrasenaView = new RecuperarContrasenaView();
                recuperarContrasenaView.Owner = this;
                recuperarContrasenaView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                recuperarContrasenaView.ShowDialog();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al abrir la ventana de recuperación de contraseña");
                MostrarError("Error al abrir la ventana de recuperación de contraseña.");
            }
        }

        /// <summary>
        /// Muestra un mensaje de error en la interfaz
        /// </summary>
        private void MostrarError(string mensaje)
        {
            txtError.Text = mensaje;
            txtError.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Evento que se dispara cuando la ventana se está cerrando
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            // Si se cerró la ventana sin autenticar, cerrar la aplicación
            if (_usuarioActual == null && Application.Current.MainWindow == this)
            {
                Application.Current.Shutdown();
            }
        }
    }
}