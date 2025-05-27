using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using ElectroTech.Views.Admin;
using ElectroTech.Views.Compras;
using ElectroTech.Views.Devoluciones;
using ElectroTech.Views.Empleados;
using ElectroTech.Views.Perfil; 
using ElectroTech.Views.Productos;
using ElectroTech.Views.Reportes;
// using ElectroTech.Views.Configuracion; // Ya no se necesita
// using ElectroTech.Views.Usuarios; // Ya no se necesita, se reemplaza por Perfil
using ElectroTech.Views.Ventas;
// using ElectroTech.Views.Bitacora; // Si tienes una página de Bitácora
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
            _usuarioActual = LoginView.UsuarioActual; //
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
            mainFrame.Navigate(new DashboardPage()); //

            // Registrar eventos
            this.Loaded += MainDashboardWindow_Loaded; //
            this.Closing += MainDashboardWindow_Closing; //
        }


        /// <summary>
        /// Configura la interfaz según el nivel de acceso del usuario
        /// </summary>
        private void ConfigurarInterfazSegunUsuario()
        {
            // Establecer nombre y nivel del usuario
            txtUsuarioNombre.Text = _usuarioActual.NombreCompleto; //
            txtUsuarioNivel.Text = _usuarioActual.NombreNivel; //


            // Botón Inicio siempre visible para usuarios logueados
            btnInicioDashboard.Visibility = Visibility.Visible;
            btnInicioDashboard.IsEnabled = true;

            // Configurar visibilidad y estado de los elementos de menú según el nivel de usuario

            // Sección de Entidades
            ConfigurarBotonMenu(btnProductos, PermisosHelper.Modulo.Productos); //
            ConfigurarBotonMenu(btnCategorias, PermisosHelper.Modulo.Categorias); //
            ConfigurarBotonMenu(btnProveedores, PermisosHelper.Modulo.Proveedores); //
            ConfigurarBotonMenu(btnClientes, PermisosHelper.Modulo.Clientes); //
            ConfigurarBotonMenu(btnEmpleados, PermisosHelper.Modulo.Empleados); //

            // Sección de Transacciones
            ConfigurarBotonMenu(btnVentas, PermisosHelper.Modulo.Ventas); //
            ConfigurarBotonMenu(btnCompras, PermisosHelper.Modulo.Compras); //
            ConfigurarBotonMenu(btnDevoluciones, PermisosHelper.Modulo.Devoluciones); //
            ConfigurarBotonMenu(btnInventario, PermisosHelper.Modulo.Inventario); //

            // Sección de Reportes
            ConfigurarBotonMenu(btnReporteVentas, PermisosHelper.Modulo.ReporteVentas); //
            ConfigurarBotonMenu(btnReporteInventario, PermisosHelper.Modulo.ReporteInventario); //
            ConfigurarBotonMenu(btnReporteProductos, PermisosHelper.Modulo.ReporteProductos); //
            ConfigurarBotonMenu(btnReporteClientes, PermisosHelper.Modulo.ReporteClientes); //

            // Sección de Perfil y Bitácora
            // Asumiendo que tienes un botón btnMiPerfil en XAML
            ConfigurarBotonMenu(btnMiPerfil, PermisosHelper.Modulo.MiPerfil); //
            ConfigurarBotonMenu(btnBitacora, PermisosHelper.Modulo.Bitacora); //

            // Ocultar la etiqueta "Configuración" ya que se eliminó el módulo
            
            // btnConfiguracion ya no existe, no es necesario configurarlo
        }

        /// <summary>
        /// Configura un botón del menú según los permisos del usuario actual.
        /// </summary>
        /// <param name="boton">Botón a configurar.</param>
        /// <param name="modulo">Módulo al que corresponde el botón.</param>
        private void ConfigurarBotonMenu(Button boton, PermisosHelper.Modulo modulo)
        {
            // Si el botón es null (por ejemplo, si se eliminó del XAML pero aún se referencia aquí), no hacer nada.
            if (boton == null) return;

            bool tienePermiso = PermisosHelper.TienePermiso(_usuarioActual, modulo); //

            boton.IsEnabled = tienePermiso;
            boton.Visibility = tienePermiso ? Visibility.Visible : Visibility.Collapsed; // Ocultar si no tiene permiso

            if (!tienePermiso)
            {
                // Adicionalmente, podrías querer ocultarlo completamente en lugar de solo deshabilitarlo.
                // boton.Visibility = Visibility.Collapsed; 
                // O si prefieres mantenerlo visible pero claramente inactivo:
                boton.Opacity = 0.5; // Ejemplo de cómo hacerlo visualmente inactivo
                boton.ToolTip = "No tiene permisos para acceder a este módulo";
            }
            else
            {
                boton.Opacity = 1.0;
                boton.ToolTip = null;
            }
        }

        /// <summary>
        /// Evento que se ejecuta cuando la ventana se ha cargado
        /// </summary>
        private void MainDashboardWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Logger.LogInfo($"Usuario {_usuarioActual.NombreUsuario} ha accedido al sistema."); //
        }

        /// <summary>
        /// Evento que se ejecuta cuando la ventana se está cerrando
        /// </summary>
        private void MainDashboardWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // Registrar la salida del usuario
                var authService = new AuthService(); //
                authService.RegistrarSalida(_usuarioActual.IdUsuario); //

                Logger.LogInfo($"Usuario {_usuarioActual.NombreUsuario} ha cerrado sesión."); //
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al registrar la salida del usuario"); //
            }
        }

        #region Eventos de navegación

        private void btnInicioDashboard_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new DashboardPage()); //
        }

        private void btnProductos_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new ProductosPage()); //
        }

        private void btnCategorias_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new CategoriasPage()); //
        }

        private void btnProveedores_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new ProveedoresPage()); //
        }

        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new ClientesPage()); //
        }

        private void btnEmpleados_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new EmpleadosPage()); //
        }

        private void btnVentas_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new VentasPage()); //
        }

        private void btnCompras_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new ComprasPage()); //
        }

        private void btnDevoluciones_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new DevolucionesPage()); //
        }

        private void btnInventario_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new InventarioPage()); //
        }

        private void btnReporteVentas_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new ReporteVentasPage()); //
        }

        private void btnReporteInventario_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new ReporteInventarioPage()); //
        }

        private void btnReporteProductos_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new ReporteProductosPage()); //
        }

        private void btnReporteClientes_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new ReporteClientesPage()); //
        }

        // Cambiado de btnUsuarios_Click
        private void btnMiPerfil_Click(object sender, RoutedEventArgs e)
        {
           
            mainFrame.Navigate(new MiPerfilPage());
        }

        private void btnBitacora_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new BitacoraPage());  
        }

     

        private void btnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("¿Está seguro que desea cerrar sesión?",
                    "Cerrar Sesión", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var authService = new AuthService(); //
                    authService.RegistrarSalida(_usuarioActual.IdUsuario); //

                    var loginWindow = new LoginView(); //
                    loginWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cerrar sesión"); //
                MessageBox.Show("Error al cerrar sesión: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}