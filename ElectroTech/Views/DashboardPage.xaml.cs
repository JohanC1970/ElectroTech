using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; // Necesario para ObservableCollection
// using System.Data; // Ya no es necesario aquí si los servicios manejan todo
using System.Linq; // Para .Select en la actividad reciente (opcional)
using System.Windows;
using System.Windows.Controls;

namespace ElectroTech.Views
{
    public partial class DashboardPage : Page
    {
        public class ActividadItem // Movida aquí o puedes ponerla en su propio archivo en Models
        {
            public string Descripcion { get; set; }
            public string Usuario { get; set; } // Podría ser NombreCompleto del Usuario/Empleado
            public string Fecha { get; set; }
            public string Tipo { get; set; } // Ej: "Venta", "Producto Nuevo", "Alerta Stock"
        }

        private readonly ProductoService _productoService;
        private readonly VentaService _ventaService; // Añadir VentaService
        private readonly ClienteService _clienteService; // Añadir ClienteService
        private readonly BitacoraService _bitacoraService; // Añadir BitacoraService para actividad reciente

        public DashboardPage()
        {
            InitializeComponent();

            _productoService = new ProductoService();
            _ventaService = new VentaService(); // Instanciar
            _clienteService = new ClienteService(); // Instanciar
            _bitacoraService = new BitacoraService(); // Instanciar

            // Cargar datos iniciales se hace en Loaded
            this.Loaded += DashboardPage_Loaded;
        }

        private void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CargarInformacionDashboard();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar la página de Dashboard"); //
                MessageBox.Show("Error al cargar la información del dashboard: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarInformacionDashboard()
        {
            try
            {
                CargarContadores();
                CargarProductosBajoStock();
                CargarActividadReciente(); // Esto seguirá siendo una simulación o una consulta simple
            }
            catch (Exception ex)
            {
                // El error ya se registra en los métodos individuales, aquí solo para la UI
                MessageBox.Show("Ocurrió un error al cargar algunos datos del dashboard: " + ex.Message,
                                "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CargarContadores()
        {
            try
            {
                int totalProductos = _productoService.ObtenerTotalProductos(); //
                txtTotalProductos.Text = totalProductos.ToString();

                double totalVentasMes = _ventaService.ObtenerTotalVentasMes(); //
                txtTotalVentas.Text = totalVentasMes.ToString("C");

                int totalClientes = _clienteService.ObtenerTotalClientesActivos(); //
                txtTotalClientes.Text = totalClientes.ToString();

                int totalAlertas = _productoService.ObtenerTotalProductosBajoStock(); //
                txtTotalAlertas.Text = totalAlertas.ToString();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar contadores del dashboard"); //
                // Mostrar valores por defecto o mensaje de error en los TextBlocks
                txtTotalProductos.Text = "N/A";
                txtTotalVentas.Text = "N/A";
                txtTotalClientes.Text = "N/A";
                txtTotalAlertas.Text = "N/A";
            }
        }

        private void CargarProductosBajoStock()
        {
            try
            {
                List<Producto> productosBajoStock = _productoService.ObtenerProductosBajoStock(); //
                lstProductosBajoStock.ItemsSource = productosBajoStock;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar productos con stock bajo"); //
                lstProductosBajoStock.ItemsSource = null; // Limpiar en caso de error
            }
        }

        private void CargarActividadReciente()
        {
            try
            {
                // Obtener las últimas 5-10 acciones de la bitácora
                // Opcional: filtrar solo acciones "importantes" si es necesario
                var actividadReciente = new ObservableCollection<ActividadItem>();
                var registrosBitacora = _bitacoraService.ObtenerRegistros(DateTime.Now.AddDays(-7), DateTime.Now) // Últimos 7 días
                                          .OrderByDescending(r => r.FechaHora) //
                                          .Take(5) // Tomar los 5 más recientes
                                          .ToList();

                foreach (var registro in registrosBitacora)
                {
                    string descripcionAccion = "";
                    switch (registro.TipoAccion) //
                    {
                        case Models.Enums.TipoAccion.Entrada: //
                            descripcionAccion = "Inició sesión";
                            break;
                        case Models.Enums.TipoAccion.Salida: //
                            descripcionAccion = "Cerró sesión";
                            break;
                        // Podrías añadir más tipos de acciones si tu bitácora los registra (ej. Creación de Venta, etc.)
                    }
                    
                    actividadReciente.Add(new ActividadItem
                    {
                        Descripcion = descripcionAccion,
                        Usuario = registro.Usuario?.NombreCompleto ?? registro.Usuario?.NombreUsuario ?? "N/A", //
                        Fecha = registro.FechaHora.ToString("dd/MM/yyyy HH:mm"), //
                        Tipo = registro.TipoAccion.ToString() //
                    });
                }
                
                // Simulación si la bitácora no tiene suficientes datos o para complementar
                if (actividadReciente.Count == 0) {
                     actividadReciente.Add(new ActividadItem { Descripcion = "Venta realizada #V00123", Usuario = "Carlos Mendoza", Fecha = DateTime.Now.AddHours(-1).ToString("dd/MM/yyyy HH:mm"), Tipo = "Venta" });
                     actividadReciente.Add(new ActividadItem { Descripcion = "Nuevo producto: Monitor LED", Usuario = "Ana García", Fecha = DateTime.Now.AddHours(-3).ToString("dd/MM/yyyy HH:mm"), Tipo = "Producto" });
                }


                lstActividad.ItemsSource = actividadReciente;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar actividad reciente"); //
                lstActividad.ItemsSource = null;
            }
        }
    }
}