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
    /// Lógica de interacción para DevolucionesPage.xaml
    /// </summary>
    public partial class DevolucionesPage : Page
    {
        private readonly DevolucionService _devolucionService;
        private List<DevolucionViewModel> _devoluciones;
        private DevolucionViewModel _devolucionSeleccionada;

        /// <summary>
        /// Constructor de la página de devoluciones
        /// </summary>
        public DevolucionesPage()
        {
            InitializeComponent();

            // Inicializar servicios
            _devolucionService = new DevolucionService();

            // Establecer filtros iniciales
            cmbEstado.SelectedIndex = 0; // "Todos los estados"

            // Cargar datos iniciales
            CargarDevoluciones();

            // Eventos
            this.Loaded += DevolucionesPage_Loaded;
        }

        /// <summary>
        /// Evento que se ejecuta cuando la página se ha cargado
        /// </summary>
        private void DevolucionesPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Refrescar datos
                CargarDevoluciones();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar la página de Devoluciones");
                MessageBox.Show("Error al cargar las devoluciones: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Carga la lista de devoluciones
        /// </summary>
        private void CargarDevoluciones()
        {
            try
            {
                // Obtener devoluciones desde el servicio
                List<Devolucion> devoluciones = _devolucionService.ObtenerTodas();

                // Aplicar filtro de estado si está seleccionado
                if (cmbEstado.SelectedIndex > 0)
                {
                    char estadoFiltro = cmbEstado.SelectedIndex == 1 ? 'P' : 'R';
                    devoluciones = devoluciones.Where(d => d.Estado == estadoFiltro).ToList();
                }

                // Aplicar búsqueda si hay término
                string termino = txtBuscar.Text.Trim();
                if (!string.IsNullOrEmpty(termino))
                {
                    devoluciones = devoluciones.Where(d =>
                        d.IdVenta.ToString().Contains(termino) ||
                         d.Motivo.ToUpper().Contains(termino.ToUpper())
                    ).ToList();

                }

                // Convertir a ViewModel
                _devoluciones = devoluciones.Select(d => new DevolucionViewModel(d)).ToList();

                // Asignar a la vista
                dgDevoluciones.ItemsSource = _devoluciones;

                // Actualizar contador
                ActualizarContador();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar devoluciones");
                MessageBox.Show("Error al cargar las devoluciones: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Actualiza el contador de devoluciones
        /// </summary>
        private void ActualizarContador()
        {
            int total = dgDevoluciones.Items.Count;
            txtTotalDevoluciones.Text = $"Total: {total} devoluciones";
        }

        #region Eventos de controles

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            CargarDevoluciones();
        }

        private void txtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CargarDevoluciones();
            }
        }

        private void cmbEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CargarDevoluciones();
        }

        private void dgDevoluciones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _devolucionSeleccionada = dgDevoluciones.SelectedItem as DevolucionViewModel;
        }

        private void btnNuevaDevolucion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Abrir ventana de búsqueda de venta primero
                var busquedaVentaWindow = new BusquedaVentaWindow();
                busquedaVentaWindow.Owner = Window.GetWindow(this);

                // Si se seleccionó una venta, abrir ventana de nueva devolución
                if (busquedaVentaWindow.ShowDialog() == true && busquedaVentaWindow.VentaSeleccionada != null)
                {
                    var ventaSeleccionada = busquedaVentaWindow.VentaSeleccionada;

                    // Verificar si ya existe una devolución para esta venta
                    if (_devolucionService.ExisteDevolucionParaVenta(ventaSeleccionada.IdVenta))
                    {
                        MessageBox.Show("Ya existe una devolución para esta venta.",
                            "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    // Abrir ventana de nueva devolución
                    var nuevaDevolucionWindow = new DevolucionDetailWindow(ventaSeleccionada);
                    nuevaDevolucionWindow.Owner = Window.GetWindow(this);

                    if (nuevaDevolucionWindow.ShowDialog() == true)
                    {
                        // Recargar devoluciones
                        CargarDevoluciones();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al abrir ventana de nueva devolución");
                MessageBox.Show("Error al crear nueva devolución: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnVer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener la devolución seleccionada
                var devolucion = ((FrameworkElement)sender).DataContext as DevolucionViewModel;

                if (devolucion != null)
                {
                    // Abrir ventana de detalle en modo solo lectura
                    var detalleWindow = new DevolucionDetailWindow(devolucion.ToDevolucion(), true);
                    detalleWindow.Owner = Window.GetWindow(this);
                    detalleWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al abrir ventana de detalle de devolución");
                MessageBox.Show("Error al mostrar detalle de devolución: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener la devolución seleccionada
                var devolucion = ((FrameworkElement)sender).DataContext as DevolucionViewModel;

                if (devolucion != null)
                {
                    // Abrir ventana de edición
                    var editarWindow = new DevolucionDetailWindow(devolucion.ToDevolucion());
                    editarWindow.Owner = Window.GetWindow(this);

                    if (editarWindow.ShowDialog() == true)
                    {
                        // Recargar devoluciones
                        CargarDevoluciones();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al abrir ventana de edición de devolución");
                MessageBox.Show("Error al editar devolución: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnProcesar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener la devolución seleccionada
                var devolucion = ((FrameworkElement)sender).DataContext as DevolucionViewModel;

                if (devolucion != null)
                {
                    // Confirmar procesamiento
                    var resultado = MessageBox.Show(
                        $"¿Está seguro que desea procesar la devolución #{devolucion.IdDevolucion}?",
                        "Confirmar procesamiento",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (resultado == MessageBoxResult.Yes)
                    {
                        // Procesar devolución
                        string errorMessage;
                        bool procesado = _devolucionService.ProcesarDevolucion(devolucion.IdDevolucion, out errorMessage);

                        if (procesado)
                        {
                            MessageBox.Show("Devolución procesada con éxito.",
                                "Información", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Recargar devoluciones
                            CargarDevoluciones();
                        }
                        else
                        {
                            MessageBox.Show($"Error al procesar la devolución: {errorMessage}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al procesar devolución");
                MessageBox.Show("Error al procesar devolución: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }

    /// <summary>
    /// ViewModel para mostrar devoluciones en la interfaz
    /// </summary>
    public class DevolucionViewModel
    {
        public int IdDevolucion { get; set; }
        public int IdVenta { get; set; }
        public DateTime Fecha { get; set; }
        public string Motivo { get; set; }
        public double MontoDevuelto { get; set; }
        public char Estado { get; set; }
        public string EstadoDescripcion { get; set; }
        public string NombreCliente { get; set; }
        public bool PermiteEdicion { get; set; }
        public bool RequiereProcesamiento { get; set; }

        public DevolucionViewModel(Devolucion devolucion)
        {
            IdDevolucion = devolucion.IdDevolucion;
            IdVenta = devolucion.IdVenta;
            Fecha = devolucion.Fecha;
            Motivo = devolucion.Motivo;
            MontoDevuelto = devolucion.MontoDevuelto;
            Estado = devolucion.Estado;
            EstadoDescripcion = devolucion.EstadoDescripcion;

            // Estos datos tendrían que venir de un join en la base de datos
            // Por ahora usamos un placeholder
            NombreCliente = devolucion.Venta?.Cliente?.NombreCompleto ?? "Cliente no disponible";

            // Una devolución solo permite edición si está pendiente de procesar
            PermiteEdicion = Estado == 'P';
            RequiereProcesamiento = Estado == 'P';
        }

        public Devolucion ToDevolucion()
        {
            return new Devolucion
            {
                IdDevolucion = IdDevolucion,
                IdVenta = IdVenta,
                Fecha = Fecha,
                Motivo = Motivo,
                MontoDevuelto = MontoDevuelto,
                Estado = Estado
            };
        }
    }
}