using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using ElectroTech.Views.Empleados;
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

            // Configurar visibilidad y estado de los elementos de menú según el nivel de usuario

            // Sección de Entidades
            ConfigurarBotonMenu(btnProductos, PermisosHelper.Modulo.Productos);
            ConfigurarBotonMenu(btnCategorias, PermisosHelper.Modulo.Categorias);
            ConfigurarBotonMenu(btnProveedores, PermisosHelper.Modulo.Proveedores);
            ConfigurarBotonMenu(btnClientes, PermisosHelper.Modulo.Clientes);
            ConfigurarBotonMenu(btnEmpleados, PermisosHelper.Modulo.Empleados);

            // Sección de Transacciones
            ConfigurarBotonMenu(btnVentas, PermisosHelper.Modulo.Ventas);
            ConfigurarBotonMenu(btnCompras, PermisosHelper.Modulo.Compras);
            ConfigurarBotonMenu(btnDevoluciones, PermisosHelper.Modulo.Devoluciones);
            ConfigurarBotonMenu(btnInventario, PermisosHelper.Modulo.Inventario);

            // Sección de Reportes
            ConfigurarBotonMenu(btnReporteVentas, PermisosHelper.Modulo.ReporteVentas);
            ConfigurarBotonMenu(btnReporteInventario, PermisosHelper.Modulo.ReporteInventario);
            ConfigurarBotonMenu(btnReporteProductos, PermisosHelper.Modulo.ReporteProductos);
            ConfigurarBotonMenu(btnReporteClientes, PermisosHelper.Modulo.ReporteClientes);

            // Sección de Configuración - Solo visible para administradores
            bool esAdmin = PermisosHelper.EsAdministrador(_usuarioActual);
            lblConfiguracion.Visibility = esAdmin ? Visibility.Visible : Visibility.Collapsed;
            ConfigurarBotonMenu(btnUsuarios, PermisosHelper.Modulo.Usuarios);
            ConfigurarBotonMenu(btnBitacora, PermisosHelper.Modulo.Bitacora);
            ConfigurarBotonMenu(btnConfiguracion, PermisosHelper.Modulo.Configuracion);
        }

        /// <summary>
        /// Configura un botón del menú según los permisos del usuario actual.
        /// </summary>
        /// <param name="boton">Botón a configurar.</param>
        /// <param name="modulo">Módulo al que corresponde el botón.</param>
        private void ConfigurarBotonMenu(Button boton, PermisosHelper.Modulo modulo)
        {
            bool tienePermiso = PermisosHelper.TienePermiso(_usuarioActual, modulo);

            // Si no tiene permiso para este módulo, deshabilitar o ocultar el botón
            boton.IsEnabled = tienePermiso;

            // Opciones avanzadas: podríamos ocultar completamente los botones sin permiso
            // boton.Visibility = tienePermiso ? Visibility.Visible : Visibility.Collapsed;

            // Si queremos mantener visible pero inactivo (para indicar que existe pero no tiene acceso)
            if (!tienePermiso)
            {
                boton.Opacity = 0.5;
                boton.ToolTip = "No tiene permisos para acceder a este módulo";
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
            mainFrame.Navigate(new CategoriasPage());
        }

        private void btnProveedores_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new ProveedoresPage());
        }

        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new ClientesPage());
        }

        private void btnEmpleados_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new EmpleadosPage());
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
            mainFrame.Navigate(new InventarioPage());
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