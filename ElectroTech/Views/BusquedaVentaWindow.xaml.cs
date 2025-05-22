using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ElectroTech.Views.Devoluciones
{
    /// <summary>
    /// Lógica de interacción para BusquedaVentaWindow.xaml
    /// </summary>
    public partial class BusquedaVentaWindow : Window
    {
        private readonly VentaService _ventaService;
        private readonly DevolucionService _devolucionService;
        private List<VentaViewModel> _ventas;

        /// <summary>
        /// Propiedad para obtener la venta seleccionada
        /// </summary>
        public Venta VentaSeleccionada { get; private set; }

        /// <summary>
        /// Constructor de la ventana de búsqueda de ventas
        /// </summary>
        public BusquedaVentaWindow()
        {
            InitializeComponent();

            // Inicializar servicios
            _ventaService = new VentaService();
            _devolucionService = new DevolucionService();

            // Cargar datos iniciales
            CargarVentas();
        }

        /// <summary>
        /// Carga la lista de ventas
        /// </summary>
        private void CargarVentas()
        {
            try
            {
                // Obtener ventas desde el servicio (solo completadas)
                List<Venta> ventas = _ventaService.ObtenerVentasCompletadas();

                // Filtrar ventas que ya tienen devolución
                var ventasConDevolucion = _devolucionService.ObtenerVentasConDevolucion();
                ventas = ventas.Where(v => !ventasConDevolucion.Contains(v.IdVenta)).ToList();

                // Aplicar búsqueda si hay término
                string termino = txtBuscar.Text.Trim();
                if (!string.IsNullOrEmpty(termino))
                {
                    ventas = ventas.Where(v =>
                        v.NumeroFactura.ToUpper().Contains(termino.ToUpper()) ||
                        v.Fecha.ToString("dd/MM/yyyy").Contains(termino) ||
                        (v.Cliente != null && v.Cliente.NombreCompleto.ToUpper().Contains(termino.ToUpper()))
                    ).ToList(); // ← correctamente cerrado
                }

                // Filtrar ventas que tienen más de 30 días (regla de negocio)
                ventas = ventas.Where(v => (DateTime.Now - v.Fecha).TotalDays <= 30).ToList();

                // Convertir a ViewModel
                _ventas = ventas.Select(v => new VentaViewModel(v)).ToList();

                // Asignar a la vista
                dgVentas.ItemsSource = _ventas;

                // Actualizar mensaje informativo
                txtInfo.Text = _ventas.Count == 0
                    ? "No se encontraron ventas disponibles para devolución"
                    : "Seleccione una venta para crear una devolución";
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar ventas");
                MessageBox.Show("Error al cargar las ventas: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Eventos de controles

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            CargarVentas();
        }

        private void txtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CargarVentas();
            }
        }

        private void dgVentas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ventaSeleccionada = dgVentas.SelectedItem as VentaViewModel;
            btnSeleccionar.IsEnabled = ventaSeleccionada != null;
        }

        private void btnSeleccionar_Click(object sender, RoutedEventArgs e)
        {
            SeleccionarVenta();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Cerrar ventana sin seleccionar
            DialogResult = false;
            Close();
        }

        #endregion

        /// <summary>
        /// Selecciona la venta actual y cierra la ventana
        /// </summary>
        private void SeleccionarVenta()
        {
            try
            {
                var ventaViewModel = dgVentas.SelectedItem as VentaViewModel;
                if (ventaViewModel != null)
                {
                    // Obtener la venta completa desde el servicio
                    VentaSeleccionada = _ventaService.ObtenerPorId(ventaViewModel.IdVenta);

                    if (VentaSeleccionada != null)
                    {
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("No se pudo cargar la información completa de la venta seleccionada.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al seleccionar venta");
                MessageBox.Show("Error al seleccionar la venta: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// ViewModel para mostrar ventas en la interfaz
    /// </summary>
    public class VentaViewModel
    {
        public int IdVenta { get; set; }
        public string NumeroFactura { get; set; }
        public DateTime Fecha { get; set; }
        public double Total { get; set; }
        public string EstadoDescripcion { get; set; }
        public string NombreCliente { get; set; }

        public VentaViewModel(Venta venta)
        {
            IdVenta = venta.IdVenta;
            NumeroFactura = venta.NumeroFactura;
            Fecha = venta.Fecha;
            Total = venta.Total;
            EstadoDescripcion = venta.EstadoDescripcion;
            NombreCliente = venta.Cliente?.NombreCompleto ?? "Cliente no disponible";
        }
    }
}