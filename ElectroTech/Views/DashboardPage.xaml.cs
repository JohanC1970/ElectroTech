using ElectroTech.DataAccess;
using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace ElectroTech.Views
{
    /// <summary>
    /// Lógica de interacción para DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        // Clase para la actividad reciente
        public class ActividadItem
        {
            public string Descripcion { get; set; }
            public string Usuario { get; set; }
            public string Fecha { get; set; }
        }

        // Servicio de productos
        private readonly ProductoService _productoService;

        /// <summary>
        /// Constructor de la página de dashboard
        /// </summary>
        public DashboardPage()
        {
            InitializeComponent();

            // Inicializar servicios
            _productoService = new ProductoService();

            // Cargar datos iniciales
            CargarInformacionDashboard();

            // Eventos
            this.Loaded += DashboardPage_Loaded;
        }

        /// <summary>
        /// Evento que se ejecuta cuando la página se ha cargado
        /// </summary>
        private void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Refrescar datos
                CargarInformacionDashboard();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar la página de Dashboard");
                MessageBox.Show("Error al cargar la información del dashboard: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Carga la información para el dashboard
        /// </summary>
        private void CargarInformacionDashboard()
        {
            try
            {
                // Cargar contadores
                CargarContadores();

                // Cargar productos con stock bajo
                CargarProductosBajoStock();

                // Cargar actividad reciente
                CargarActividadReciente();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar información del dashboard");
                throw;
            }
        }

        /// <summary>
        /// Carga los contadores del dashboard
        /// </summary>
        private void CargarContadores()
        {
            try
            {
                // Obtener total de productos
                int totalProductos = _productoService.ObtenerTotalProductos();
                txtTotalProductos.Text = totalProductos.ToString();

                // Obtener total de ventas del mes (simulado por ahora)
                double totalVentas = 15750.50; // Esto debería obtenerse de un servicio real
                txtTotalVentas.Text = totalVentas.ToString("C");

                // Obtener total de clientes (simulado por ahora)
                int totalClientes = 35; // Esto debería obtenerse de un servicio real
                txtTotalClientes.Text = totalClientes.ToString();

                // Obtener total de alertas (productos con stock bajo)
                int totalAlertas = _productoService.ObtenerTotalProductosBajoStock();
                txtTotalAlertas.Text = totalAlertas.ToString();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar contadores del dashboard");
                throw;
            }
        }

        /// <summary>
        /// Carga los productos con stock bajo
        /// </summary>
        private void CargarProductosBajoStock()
        {
            try
            {
                // Obtener productos con stock bajo
                List<Producto> productosBajoStock = _productoService.ObtenerProductosBajoStock();

                // Asignar a la lista
                lstProductosBajoStock.ItemsSource = productosBajoStock;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar productos con stock bajo");
                throw;
            }
        }

        /// <summary>
        /// Carga la actividad reciente (simulada por ahora)
        /// </summary>
        private void CargarActividadReciente()
        {
            try
            {
                // Crear lista de actividad (simulada)
                var actividad = new ObservableCollection<ActividadItem>
                {
                    new ActividadItem
                    {
                        Descripcion = "Venta realizada #V00123",
                        Usuario = "Carlos Mendoza",
                        Fecha = DateTime.Now.AddHours(-1).ToString("dd/MM/yyyy HH:mm")
                    },
                    new ActividadItem
                    {
                        Descripcion = "Nuevo producto registrado: Monitor LED 24\"",
                        Usuario = "Ana García",
                        Fecha = DateTime.Now.AddHours(-3).ToString("dd/MM/yyyy HH:mm")
                    },
                    new ActividadItem
                    {
                        Descripcion = "Compra a proveedor #C00098",
                        Usuario = "Juan Pérez",
                        Fecha = DateTime.Now.AddHours(-5).ToString("dd/MM/yyyy HH:mm")
                    },
                    new ActividadItem
                    {
                        Descripcion = "Actualización de inventario",
                        Usuario = "María Rodríguez",
                        Fecha = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy HH:mm")
                    },
                    new ActividadItem
                    {
                        Descripcion = "Devolución procesada #D00034",
                        Usuario = "Luis Torres",
                        Fecha = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy HH:mm")
                    }
                };

                // Asignar a la lista
                lstActividad.ItemsSource = actividad;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar actividad reciente");
                throw;
            }
        }
    }
}