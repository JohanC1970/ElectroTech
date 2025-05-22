using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ElectroTech.Views.Devoluciones
{
    /// <summary>
    /// Lógica de interacción para DevolucionDetailWindow.xaml
    /// </summary>
    public partial class DevolucionDetailWindow : Window
    {
        private readonly DevolucionService _devolucionService;
        private readonly VentaService _ventaService;
        private readonly Devolucion _devolucion;
        private readonly Venta _venta;
        private readonly bool _esNuevo;
        private readonly bool _soloLectura;

        /// <summary>
        /// Constructor para una nueva devolución a partir de una venta
        /// </summary>
        public DevolucionDetailWindow(Venta venta)
        {
            InitializeComponent();

            _devolucionService = new DevolucionService();
            _ventaService = new VentaService();
            _venta = venta;
            _devolucion = new Devolucion { IdVenta = venta.IdVenta, Fecha = DateTime.Now };
            _esNuevo = true;
            _soloLectura = false;

            // Configurar ventana para nueva devolución
            txtTitulo.Text = "Nueva Devolución";
            ConfigurarVentana();
            CargarDatosVenta();
        }

        /// <summary>
        /// Constructor para editar una devolución existente
        /// </summary>
        public DevolucionDetailWindow(Devolucion devolucion, bool soloLectura = false)
        {
            InitializeComponent();

            _devolucionService = new DevolucionService();
            _ventaService = new VentaService();
            _devolucion = devolucion;
            _esNuevo = false;
            _soloLectura = soloLectura;

            // Obtener la venta relacionada con la devolución
            _venta = _ventaService.ObtenerPorId(devolucion.IdVenta);

            // Configurar ventana para edición
            txtTitulo.Text = soloLectura ? "Detalle de Devolución" : "Editar Devolución";

            if (!soloLectura && devolucion.Estado == 'P')
            {
                pnlEstadoDevolucion.Visibility = Visibility.Visible;
            }

            // Si es solo lectura, mostrar nota de crédito para devoluciones procesadas
            if (soloLectura && devolucion.Estado == 'P')
            {
                btnGenerarNotaCredito.Visibility = Visibility.Visible;
            }

            ConfigurarVentana();
            CargarDatosVenta();
            CargarDatosDevolucion();
        }

        /// <summary>
        /// Configura la ventana y carga datos iniciales
        /// </summary>
        private void ConfigurarVentana()
        {
            try
            {
                // Si es solo lectura, deshabilitar campos editables
                if (_soloLectura)
                {
                    dpFechaDevolucion.IsEnabled = false;
                    txtMontoDevolucion.IsReadOnly = true;
                    txtMotivo.IsReadOnly = true;
                    btnGuardar.Visibility = Visibility.Collapsed;
                    btnCancelar.Content = "Cerrar";
                }

                // Configurar fecha actual si es nueva devolución
                if (_esNuevo)
                {
                    dpFechaDevolucion.SelectedDate = DateTime.Now;
                    // Predeterminar el monto igual al total de la venta
                    if (_venta != null)
                    {
                        txtMontoDevolucion.Text = _venta.Total.ToString("0.00");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al configurar ventana de devolución");
                MessageBox.Show("Error al configurar la ventana: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Carga los datos de la venta relacionada
        /// </summary>
        private void CargarDatosVenta()
        {
            try
            {
                if (_venta != null)
                {
                    // Mostrar información de la venta
                    txtNumeroVenta.Text = _venta.NumeroFactura;
                    txtFechaVenta.Text = _venta.Fecha.ToString("dd/MM/yyyy");
                    txtCliente.Text = _venta.Cliente?.NombreCompleto ?? "Cliente no disponible";
                    txtTotalVenta.Text = _venta.Total.ToString("C");

                    // Calcular días transcurridos
                    int diasTranscurridos = (int)(DateTime.Now - _venta.Fecha).TotalDays;
                    txtDiasTranscurridos.Text = $"{diasTranscurridos} días";

                    // Verificar si está dentro del plazo permitido (30 días)
                    bool dentroDePlazo = diasTranscurridos <= 30;
                    txtEstadoValidacion.Text = dentroDePlazo
                        ? "Dentro del plazo permitido (30 días)"
                        : "Fuera del plazo permitido (máximo 30 días)";
                    txtEstadoValidacion.Foreground = dentroDePlazo
                        ? Brushes.Green
                        : Brushes.Red;

                    // Si está fuera de plazo y es nueva devolución, mostrar mensaje y deshabilitar campos
                    if (!dentroDePlazo && _esNuevo)
                    {
                        MostrarError("No se puede crear una devolución para una venta con más de 30 días de antigüedad.");
                        dpFechaDevolucion.IsEnabled = false;
                        txtMontoDevolucion.IsEnabled = false;
                        txtMotivo.IsEnabled = false;
                        btnGuardar.IsEnabled = false;
                    }

                    // Cargar productos de la venta
                    dgProductosVenta.ItemsSource = _venta.Detalles;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar datos de la venta");
                MessageBox.Show("Error al cargar los datos de la venta: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Carga los datos de la devolución en el formulario (modo edición)
        /// </summary>
        private void CargarDatosDevolucion()
        {
            try
            {
                // Cargar datos básicos
                dpFechaDevolucion.SelectedDate = _devolucion.Fecha;
                txtMontoDevolucion.Text = _devolucion.MontoDevuelto.ToString("0.00");
                txtMotivo.Text = _devolucion.Motivo;

                // Seleccionar estado en el combo si corresponde
                if (pnlEstadoDevolucion.Visibility == Visibility.Visible)
                {
                    cmbEstadoDevolucion.SelectedIndex = _devolucion.Estado == 'P' ? 0 : 1;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar datos de la devolución");
                MessageBox.Show("Error al cargar los datos de la devolución: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Valida y guarda la devolución
        /// </summary>
        private void GuardarDevolucion()
        {
            try
            {
                // Ocultar mensaje de error previo
                txtError.Visibility = Visibility.Collapsed;

                // Validar campos obligatorios
                if (dpFechaDevolucion.SelectedDate == null ||
                    string.IsNullOrWhiteSpace(txtMontoDevolucion.Text) ||
                    string.IsNullOrWhiteSpace(txtMotivo.Text))
                {
                    MostrarError("Por favor, complete todos los campos obligatorios.");
                    return;
                }

                // Parsear valores numéricos
                double montoDevolucion;
                if (!double.TryParse(txtMontoDevolucion.Text, out montoDevolucion) || montoDevolucion <= 0)
                {
                    MostrarError("El monto a devolver debe ser un número mayor que cero.");
                    return;
                }

                // Validar que el monto no sea mayor al total de la venta
                if (montoDevolucion > _venta.Total)
                {
                    MostrarError("El monto a devolver no puede ser mayor al total de la venta.");
                    return;
                }

                // Validar que la fecha de devolución no sea anterior a la fecha de venta
                if (dpFechaDevolucion.SelectedDate < _venta.Fecha)
                {
                    MostrarError("La fecha de devolución no puede ser anterior a la fecha de venta.");
                    return;
                }

                // Validar que la fecha de devolución no sea posterior a la fecha actual
                if (dpFechaDevolucion.SelectedDate > DateTime.Now)
                {
                    MostrarError("La fecha de devolución no puede ser posterior a la fecha actual.");
                    return;
                }

                // Validar plazo de devolución (30 días)
                TimeSpan plazo = dpFechaDevolucion.SelectedDate.Value - _venta.Fecha;
                if (plazo.TotalDays > 30)
                {
                    MostrarError("No se pueden realizar devoluciones después de 30 días de la compra.");
                    return;
                }

                // Completar datos de la devolución
                _devolucion.Fecha = dpFechaDevolucion.SelectedDate.Value;
                _devolucion.MontoDevuelto = montoDevolucion;
                _devolucion.Motivo = txtMotivo.Text.Trim();

                // Si está en modo edición y se muestra el combo de estado, actualizar estado
                if (!_esNuevo && pnlEstadoDevolucion.Visibility == Visibility.Visible)
                {
                    _devolucion.Estado = cmbEstadoDevolucion.SelectedIndex == 0 ? 'P' : 'R';
                }

                // Guardar devolución
                string errorMessage;
                bool resultado;

                if (_esNuevo)
                {
                    int idDevolucion = _devolucionService.CrearDevolucion(_devolucion, out errorMessage);
                    resultado = idDevolucion > 0;

                    if (resultado)
                    {
                        _devolucion.IdDevolucion = idDevolucion;
                    }
                }
                else
                {
                    resultado = _devolucionService.ActualizarDevolucion(_devolucion, out errorMessage);
                }

                // Verificar resultado
                if (resultado)
                {
                    MessageBox.Show($"Devolución {(_esNuevo ? "creada" : "actualizada")} con éxito.",
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Cerrar ventana con resultado positivo
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MostrarError($"Error al {(_esNuevo ? "crear" : "actualizar")} la devolución: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al guardar devolución {(_esNuevo ? "nueva" : "existente")}");
                MostrarError("Error al guardar la devolución: " + ex.Message);
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

        #region Eventos de controles

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            GuardarDevolucion();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Cerrar ventana sin guardar
            DialogResult = false;
            Close();
        }

        private void dpFechaDevolucion_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // Si se cambia la fecha, verificar plazo
            if (dpFechaDevolucion.SelectedDate.HasValue && _venta != null)
            {
                TimeSpan plazo = dpFechaDevolucion.SelectedDate.Value - _venta.Fecha;
                bool dentroDePlazo = plazo.TotalDays <= 30;

                txtEstadoValidacion.Text = dentroDePlazo
                    ? "Dentro del plazo permitido (30 días)"
                    : "Fuera del plazo permitido (máximo 30 días)";
                txtEstadoValidacion.Foreground = dentroDePlazo
                    ? Brushes.Green
                    : Brushes.Red;
            }
        }

        private void txtMontoDevolucion_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Solo permitir números y punto decimal
            e.Handled = !EsNumeroDecimalValido(e.Text);
        }

        private void txtMontoDevolucion_LostFocus(object sender, RoutedEventArgs e)
        {
            // Formatear valor numérico al perder el foco
            if (!string.IsNullOrEmpty(txtMontoDevolucion.Text))
            {
                double valor;

                if (double.TryParse(txtMontoDevolucion.Text, out valor))
                {
                    txtMontoDevolucion.Text = valor.ToString("0.00");
                }
            }
        }

        private void btnGenerarNotaCredito_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Generar y mostrar nota de crédito
                string notaCredito = _devolucionService.GenerarNotaCredito(_devolucion.IdDevolucion);

                if (!string.IsNullOrEmpty(notaCredito))
                {
                    // Mostrar ventana con nota de crédito
                    var notaCreditoWindow = new NotaCreditoWindow(notaCredito);
                    notaCreditoWindow.Owner = this;
                    notaCreditoWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("No se pudo generar la nota de crédito.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al generar nota de crédito");
                MessageBox.Show("Error al generar nota de crédito: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Métodos de validación

        /// <summary>
        /// Verifica si un texto es un número decimal válido
        /// </summary>
        private bool EsNumeroDecimalValido(string texto)
        {
            // Permitir solo dígitos y un punto decimal
            Regex regex = new Regex(@"^[0-9]+(\.)?[0-9]*$");

            // Si el texto actual ya contiene un punto, no permitir otro
            TextBox textBox = txtMontoDevolucion; // Referencia directa al control
            if (texto == "." && textBox.Text.Contains("."))
            {
                return false;
            }

            return regex.IsMatch(texto);
        }

        #endregion
    }
}