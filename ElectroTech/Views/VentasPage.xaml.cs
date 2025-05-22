using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using ElectroTech.Views.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ElectroTech.Views.Ventas
{
    /// <summary>
    /// Lógica de interacción para VentasPage.xaml
    /// </summary>
    public partial class VentasPage : Page
    {
        private readonly VentaService _ventaService;
        private List<Venta> _ventas;
        private List<Venta> _ventasFiltradas;
        private Venta _ventaSeleccionada;

        /// <summary>
        /// Constructor de la página de ventas
        /// </summary>
        public VentasPage()
        {
            InitializeComponent();

            // Inicializar servicios
            _ventaService = new VentaService();

            // Inicializar filtros
            InicializarFiltros();

            // Cargar datos iniciales
            CargarVentas();

            // Eventos
            this.Loaded += VentasPage_Loaded;
        }

        /// <summary>
        /// Evento que se ejecuta cuando la página se ha cargado
        /// </summary>
        private void VentasPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Refrescar datos
                CargarVentas();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar la página de Ventas");
                MessageBox.Show("Error al cargar las ventas: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Inicializa los filtros de la página
        /// </summary>
        private void InicializarFiltros()
        {
            // Configurar fechas por defecto (último mes)
            dpFechaInicio.SelectedDate = DateTime.Now.AddMonths(-1);
            dpFechaFin.SelectedDate = DateTime.Now;

            // Seleccionar filtro de estado por defecto
            cmbEstado.SelectedIndex = 0; // Todos
        }

        /// <summary>
        /// Carga la lista de ventas
        /// </summary>
        private void CargarVentas()
        {
            try
            {
                // Obtener fechas del filtro
                DateTime fechaInicio = dpFechaInicio.SelectedDate ?? DateTime.Now.AddMonths(-1);
                DateTime fechaFin = dpFechaFin.SelectedDate ?? DateTime.Now;
                fechaFin = fechaFin.AddDays(1).AddSeconds(-1); // Incluir todo el día final

                // Obtener ventas en el rango de fechas
                _ventas = _ventaService.ObtenerPorFechas(fechaInicio, fechaFin);

                // Aplicar filtro de estado
                AplicarFiltroEstado();

                // Actualizar contador
                ActualizarContadores();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar ventas");
                MessageBox.Show("Error al cargar las ventas: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Aplica el filtro de estado seleccionado
        /// </summary>
        private void AplicarFiltroEstado()
        {
            if (_ventas == null)
                return;

            if (cmbEstado.SelectedIndex < 0)
                cmbEstado.SelectedIndex = 0;

            var selectedItem = cmbEstado.SelectedItem as ComboBoxItem;
            string filtroEstado = selectedItem?.Tag?.ToString() ?? "T";

            if (filtroEstado == "T") // "Todos"
            {
                _ventasFiltradas = _ventas;
            }
            else
            {
                char estado = filtroEstado[0];
                _ventasFiltradas = _ventas.Where(v => v.Estado == estado).ToList();
            }

            // Aplicar búsqueda si hay texto
            if (!string.IsNullOrWhiteSpace(txtBuscar.Text))
            {
                AplicarBusqueda(txtBuscar.Text);
            }
            else
            {
                // Asignar ventas filtradas al DataGrid
                dgVentas.ItemsSource = _ventasFiltradas;
            }

            // Actualizar contador
            ActualizarContadores();
        }

        /// <summary>
        /// Aplica un filtro de búsqueda por texto
        /// </summary>
        private void AplicarBusqueda(string termino)
        {
            if (_ventasFiltradas == null)
                return;

            termino = termino.ToLower();

            var resultados = _ventasFiltradas.Where(v =>
                v.NumeroFactura.ToLower().Contains(termino) ||
                (v.Cliente?.NombreCompleto?.ToLower().Contains(termino) ?? false) ||
                (v.Empleado?.NombreCompleto?.ToLower().Contains(termino) ?? false) ||
                v.Total.ToString().Contains(termino) ||
                (v.Observaciones?.ToLower().Contains(termino) ?? false)
            ).ToList();

            dgVentas.ItemsSource = resultados;
            ActualizarContadores(resultados);
        }

        /// <summary>
        /// Actualiza los contadores de ventas
        /// </summary>
        private void ActualizarContadores(List<Venta> ventasAContar = null)
        {
            // Si no se proporciona una lista específica, usar la lista filtrada actual
            var lista = ventasAContar ?? _ventasFiltradas ?? new List<Venta>();

            // Actualizar contador de ventas
            int totalVentas = lista.Count;
            txtTotalVentas.Text = $"Total: {totalVentas} ventas";

            // Actualizar monto total (solo de ventas completadas)
            double montoTotal = lista.Where(v => v.Estado == 'C').Sum(v => v.Total);
            txtMontoTotal.Text = $"Monto: {montoTotal:C}";
        }

        #region Eventos de controles

        private void dpFechaInicio_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CargarVentas();
        }

        private void dpFechaFin_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CargarVentas();
        }

        private void cmbEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_ventas != null)
            {
                AplicarFiltroEstado();
            }
        }

        private void txtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AplicarBusqueda(txtBuscar.Text);
            }
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            AplicarBusqueda(txtBuscar.Text);
        }

        private void dgVentas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ventaSeleccionada = dgVentas.SelectedItem as Venta;
        }

        private void btnNuevaVenta_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Abrir ventana de nueva venta
                var nuevaVentana = new VentaDetailWindow();
                nuevaVentana.Owner = Window.GetWindow(this);

                // Si se cerró con resultado positivo, recargar ventas
                if (nuevaVentana.ShowDialog() == true)
                {
                    CargarVentas();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al abrir ventana de nueva venta");
                MessageBox.Show("Error al abrir la ventana de nueva venta: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnVer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener la venta seleccionada
                var venta = ((FrameworkElement)sender).DataContext as Venta;
                if (venta == null) return;

                // Abrir ventana de visualización (solo lectura)
                var ventanaDetalle = new VentaDetailWindow(venta, true);
                ventanaDetalle.Owner = Window.GetWindow(this);
                ventanaDetalle.ShowDialog();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al abrir ventana de visualización de venta");
                MessageBox.Show("Error al abrir la ventana de visualización: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener la venta seleccionada
                var venta = ((FrameworkElement)sender).DataContext as Venta;
                if (venta == null) return;

                // Verificar si se puede editar (solo Pendientes)
                if (venta.Estado != 'P')
                {
                    MessageBox.Show("Solo se pueden editar ventas en estado Pendiente.",
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Abrir ventana de edición
                var ventanaDetalle = new VentaDetailWindow(venta);
                ventanaDetalle.Owner = Window.GetWindow(this);

                // Si se cerró con resultado positivo, recargar ventas
                if (ventanaDetalle.ShowDialog() == true)
                {
                    CargarVentas();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al abrir ventana de edición de venta");
                MessageBox.Show("Error al abrir la ventana de edición: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCompletar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener la venta seleccionada
                var venta = ((FrameworkElement)sender).DataContext as Venta;
                if (venta == null) return;

                // Verificar si se puede completar
                if (venta.Estado != 'P')
                {
                    MessageBox.Show("Solo se pueden completar ventas en estado Pendiente.",
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Confirmar acción
                var resultado = MessageBox.Show(
                    $"¿Está seguro que desea completar la venta #{venta.NumeroFactura}?\n\n" +
                    "Esta acción no se puede deshacer.",
                    "Confirmar acción",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    // Completar venta
                    string errorMessage;
                    bool completada = _ventaService.CompletarVenta(venta.IdVenta, out errorMessage);

                    if (completada)
                    {
                        MessageBox.Show("Venta completada con éxito.",
                            "Información", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Recargar ventas
                        CargarVentas();
                    }
                    else
                    {
                        MessageBox.Show($"Error al completar la venta: {errorMessage}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al completar venta");
                MessageBox.Show("Error al completar la venta: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAnular_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener la venta seleccionada
                var venta = ((FrameworkElement)sender).DataContext as Venta;
                if (venta == null) return;

                // Verificar si se puede anular
                if (venta.Estado == 'A')
                {
                    MessageBox.Show("La venta ya está anulada.",
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Confirmar acción
                var resultado = MessageBox.Show(
                    $"¿Está seguro que desea anular la venta #{venta.NumeroFactura}?\n\n" +
                    "Esta acción no se puede deshacer y restaurará el stock de los productos.",
                    "Confirmar acción",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (resultado == MessageBoxResult.Yes)
                {
                    // Anular venta
                    string errorMessage;
                    bool anulada = _ventaService.AnularVenta(venta.IdVenta, out errorMessage);

                    if (anulada)
                    {
                        MessageBox.Show("Venta anulada con éxito.",
                            "Información", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Recargar ventas
                        CargarVentas();
                    }
                    else
                    {
                        MessageBox.Show($"Error al anular la venta: {errorMessage}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al anular venta");
                MessageBox.Show("Error al anular la venta: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnFactura_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener la venta seleccionada
                var venta = ((FrameworkElement)sender).DataContext as Venta;
                if (venta == null) return;

                // Verificar si se puede generar factura (solo completadas)
                if (venta.Estado != 'C')
                {
                    MessageBox.Show("Solo se pueden generar facturas para ventas completadas.",
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Generar factura
                string factura = _ventaService.GenerarFactura(venta.IdVenta);

                // Mostrar factura en una ventana
                var ventanaFactura = new FacturaView(factura, venta);
                ventanaFactura.Owner = Window.GetWindow(this);
                ventanaFactura.ShowDialog();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al generar factura");
                MessageBox.Show("Error al generar la factura: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}