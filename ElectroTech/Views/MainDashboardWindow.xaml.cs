using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using ElectroTech.Views.Productos;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ElectroTech.Views
{
    /// <summary>
    /// Lógica de interacción para MainDashboardWindow.xaml
    /// </summary>
    public partial class MainDashboardWindow : Window
    {
        private readonly Usuario _usuarioActual;

        /// <summary>
        /// Constructor de la ventana principal del dashboard
        /// </summary>
        public MainDashboardWindow()
        {
            InitializeComponent();

            // Obtener el usuario actual
            _usuarioActual = LoginView.UsuarioActual;
            if (_usuarioActual == null)
            {
                MessageBox.Show("Sesión no válida. Por favor, inicie sesión nuevamente.",
                    "Error de Sesión", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            // Configurar información del usuario
            ConfigurarInterfazSegunUsuario();

            // Mostrar la página de bienvenida/dashboard inicial
            mainFrame.Navigate(new DashboardPage());

            // Registrar eventos
            this.Loaded += MainDashboardWindow_Loaded;
            this.Closing += MainDashboardWindow_Closing;
        }

        /// <summary>
        /// Configura la interfaz según el nivel de acceso del usuario
        /// </summary>
        private void ConfigurarInterfazSegunUsuario()
        {
            // Establecer nombre y nivel del usuario
            txtUsuarioNombre.Text = _usuarioActual.NombreCompleto;
            txtUsuarioNivel.Text = _usuarioActual.NombreNivel;

            // Configurar visibilidad de opciones según el nivel de usuario
            // Nivel 1: Administrador (acceso completo)
            // Nivel 2: Paramétrico (sin acceso a configuración)
            // Nivel 3: Esporádico (solo reportes y consultas)
            if (_usuarioActual.Nivel == 1) // Administrador
            {
                // Mostrar todas las opciones
                lblConfiguracion.Visibility = Visibility.Visible;
                btnUsuarios.Visibility = Visibility.Visible;
                btnBitacora.Visibility = Visibility.Visible;
                btnConfiguracion.Visibility = Visibility.Visible;
            }
            else if (_usuarioActual.Nivel == 3) // Esporádico
            {
                // Deshabilitar opciones de transacciones y entidades (solo reportes)
                DeshabilitarOpcionesTransacciones();
                DeshabilitarOpcionesEntidades();
            }
        }

        /// <summary>
        /// Deshabilita las opciones de transacciones para usuarios con nivel 3
        /// </summary>
        private void DeshabilitarOpcionesTransacciones()
        {
            btnVentas.IsEnabled = false;
            btnCompras.IsEnabled = false;
            btnDevoluciones.IsEnabled = false;
            btnInventario.IsEnabled = false;
        }

        /// <summary>
        /// Deshabilita las opciones de entidades para usuarios con nivel 3
        /// </summary>
        private void DeshabilitarOpcionesEntidades()
        {
            btnProductos.IsEnabled = false;
            btnCategorias.IsEnabled = false;
            btnProveedores.IsEnabled = false;
            btnClientes.IsEnabled = false;
            btnEmpleados.IsEnabled = false;
        }

        /// <summary>
        /// Evento que se ejecuta cuando la ventana se ha cargado
        /// </summary>
        private void MainDashboardWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Logger.LogInfo($"Usuario {_usuarioActual.NombreUsuario} ha accedido al sistema.");
        }

        /// <summary>
        /// Evento que se ejecuta cuando la ventana se está cerrando
        /// </summary>
        private void MainDashboardWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // Registrar la salida del usuario
                var authService = new AuthService();
                authService.RegistrarSalida(_usuarioActual.IdUsuario);

                Logger.LogInfo($"Usuario {_usuarioActual.NombreUsuario} ha cerrado sesión.");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al registrar la salida del usuario");
            }
        }

        #region Eventos de navegación

        private void btnProductos_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new ProductosPage());
        }

        private void btnCategorias_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar página de categorías
            MessageBox.Show("Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnProveedores_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar página de proveedores
            MessageBox.Show("Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar página de clientes
            MessageBox.Show("Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnEmpleados_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar página de empleados
            MessageBox.Show("Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnVentas_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar página de ventas
            MessageBox.Show("Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnCompras_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar página de compras
            MessageBox.Show("Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnDevoluciones_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar página de devoluciones
            MessageBox.Show("Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnInventario_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar página de ajustes de inventario
            MessageBox.Show("Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnReporteVentas_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar página de reporte de ventas
            MessageBox.Show("Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnReporteInventario_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar página de reporte de inventario
            MessageBox.Show("Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnReporteProductos_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar página de reporte de productos más vendidos
            MessageBox.Show("Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnReporteClientes_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar página de reporte de clientes frecuentes
            MessageBox.Show("Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar página de gestión de usuarios
            MessageBox.Show("Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnBitacora_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar página de bitácora de accesos
            MessageBox.Show("Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implementar página de configuración del sistema
            MessageBox.Show("Funcionalidad en desarrollo", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Preguntar al usuario si desea cerrar sesión
                var result = MessageBox.Show("¿Está seguro que desea cerrar sesión?",
                    "Cerrar Sesión", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Registrar la salida
                    var authService = new AuthService();
                    authService.RegistrarSalida(_usuarioActual.IdUsuario);

                    // Volver a la pantalla de login
                    var loginWindow = new LoginView();
                    loginWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cerrar sesión");
                MessageBox.Show("Error al cerrar sesión: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}