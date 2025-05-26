using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ElectroTech.Views.Ventas
{
    /// <summary>
    /// Lógica de interacción para VentaDetailWindow.xaml
    /// </summary>
    public partial class VentaDetailWindow : Window
    {
        private readonly VentaService _ventaService;
        private readonly ClienteService _clienteService;
        private readonly EmpleadoService _empleadoService;
        private readonly ProductoService _productoService;
        private readonly MetodoPagoService _metodoPagoService;

        private Venta _venta;
        private ObservableCollection<DetalleVenta> _detallesVenta;
        private List<Cliente> _clientes;
        private List<Empleado> _empleados;
        private List<Producto> _productos;
        private List<MetodoPago> _metodosPago;
        private bool _esNuevo;
        private bool _soloLectura;
        private bool _actualizandoUI;

        private const double PORCENTAJE_IMPUESTO = 0.04; // 4% de IVA

        /// <summary>
        /// Constructor para una nueva venta
        /// </summary>
        /// <summary>
        /// Constructor para una nueva venta
        /// </summary>
        public VentaDetailWindow()
        {
            InitializeComponent();

            // Inicializar servicios
            _ventaService = new VentaService();
            _clienteService = new ClienteService();
            _empleadoService = new EmpleadoService();
            _productoService = new ProductoService();
            _metodoPagoService = new MetodoPagoService();

            // Inicializar _detallesVenta ANTES de configurar la ventana
            _detallesVenta = new ObservableCollection<DetalleVenta>();

            // Configurar ventana para nueva venta
            _venta = new Venta();
            _esNuevo = true;
            _soloLectura = false;

            txtTitulo.Text = "Nueva Venta";
            ConfigurarVentana();
        }

        /// <summary>
        /// Constructor para editar o ver una venta existente
        /// </summary>
        /// <param name="venta">Venta a editar o ver</param>
        /// <param name="soloLectura">Indica si es solo para visualización</param>
        public VentaDetailWindow(Venta venta, bool soloLectura = false)
        {
            InitializeComponent();

            // Inicializar servicios
            _ventaService = new VentaService();
            _clienteService = new ClienteService();
            _empleadoService = new EmpleadoService();
            _productoService = new ProductoService();
            _metodoPagoService = new MetodoPagoService();

            // Inicializar _detallesVenta ANTES de configurar la ventana
            _detallesVenta = new ObservableCollection<DetalleVenta>();

            // Configurar ventana para edición o visualización
            _venta = venta;
            _esNuevo = false;
            _soloLectura = soloLectura;

            txtTitulo.Text = soloLectura ? $"Ver Venta #{venta.NumeroFactura}" : $"Editar Venta #{venta.NumeroFactura}";

            // Mostrar panel de estado
            pnlEstado.Visibility = Visibility.Visible;

            ConfigurarVentana();
            CargarDatosVenta();
        }

        /// <summary>
        /// Configura la ventana y carga datos iniciales
        /// </summary>
        private void ConfigurarVentana()
        {
            try
            {
                // Cargar listas
                CargarClientes();
                CargarEmpleados();
                CargarProductos();
                CargarMetodosPago();

                // Configurar detalles de venta
                _detallesVenta = new ObservableCollection<DetalleVenta>();
                dgProductos.ItemsSource = _detallesVenta;

                // Establecer columna de productos
                dgcProducto.ItemsSource = _productos;

                // Configurar modo solo lectura si aplica
                if (_soloLectura)
                {
                    ConfigurarModoSoloLectura();
                }
                else if (_esNuevo)
                {
                    // Generar número de factura nuevo
                    _venta.NumeroFactura = _ventaService.GenerarNumeroFactura();
                    txtNumeroFactura.Text = _venta.NumeroFactura;

                    // Establecer fecha actual
                    _venta.Fecha = DateTime.Now;
                    dpFecha.SelectedDate = _venta.Fecha;

                    // Establecer estado (Pendiente)
                    _venta.Estado = 'P';
                }
                else
                {
                    // Configurar edición (solo ventas pendientes)
                    if (_venta.Estado != 'P')
                    {
                        ConfigurarModoSoloLectura();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al configurar ventana de venta");
                MessageBox.Show("Error al configurar la ventana: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Configura la ventana en modo solo lectura
        /// </summary>
        private void ConfigurarModoSoloLectura()
        {
            // Establecer controles en modo solo lectura
            dpFecha.IsEnabled = false;
            cmbCliente.IsEnabled = false;
            cmbEmpleado.IsEnabled = false;
            cmbMetodoPago.IsEnabled = false;
            btnAgregarProducto.IsEnabled = false;
            btnQuitarProducto.IsEnabled = false;
            dgProductos.IsReadOnly = true;
            txtDescuento.IsReadOnly = true;
            txtImpuestos.IsEnabled = true;
            txtObservaciones.IsReadOnly = true;
            btnGuardar.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Carga la lista de clientes
        /// </summary>
        private void CargarClientes()
        {
            try
            {
                // Obtener clientes activos
                _clientes = _clienteService.ObtenerTodos();
                cmbCliente.ItemsSource = _clientes;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar clientes");
                throw;
            }
        }

        /// <summary>
        /// Carga la lista de empleados
        /// </summary>
        private void CargarEmpleados()
        {
            try
            {
                // Obtener empleados activos
                _empleados = _empleadoService.ObtenerTodos();
                cmbEmpleado.ItemsSource = _empleados;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar empleados");
                throw;
            }
        }

        /// <summary>
        /// Carga la lista de productos
        /// </summary>
        private void CargarProductos()
        {
            try
            {
                // Obtener productos activos
                _productos = _productoService.ObtenerTodos();

                // Configurar lista de productos para el DataGrid
                dgcProducto.ItemsSource = _productos;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar productos");
                throw;
            }
        }

        /// <summary>
        /// Carga la lista de métodos de pago
        /// </summary>
        private void CargarMetodosPago()
        {
            try
            {
                // Obtener métodos de pago activos
                _metodosPago = _metodoPagoService.ObtenerTodos();
                cmbMetodoPago.ItemsSource = _metodosPago;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar métodos de pago");
                throw;
            }
        }

        /// <summary>
        /// Carga los datos de la venta en el formulario
        /// </summary>
        private void CargarDatosVenta()
        {
            try
            {
                _actualizandoUI = true;

                // Cargar datos básicos
                txtNumeroFactura.Text = _venta.NumeroFactura;
                dpFecha.SelectedDate = _venta.Fecha;
                txtEstado.Text = _venta.EstadoDescripcion;
                txtObservaciones.Text = _venta.Observaciones;
                txtDescuento.Text = _venta.Descuento.ToString("0.00");
              

                // Seleccionar cliente
                for (int i = 0; i < _clientes.Count; i++)
                {
                    if (_clientes[i].IdCliente == _venta.IdCliente)
                    {
                        cmbCliente.SelectedIndex = i;
                        break;
                    }
                }

                // Seleccionar empleado
                for (int i = 0; i < _empleados.Count; i++)
                {
                    if (_empleados[i].IdEmpleado == _venta.IdEmpleado)
                    {
                        cmbEmpleado.SelectedIndex = i;
                        break;
                    }
                }

                // Seleccionar método de pago
                for (int i = 0; i < _metodosPago.Count; i++)
                {
                    if (_metodosPago[i].IdMetodoPago == _venta.IdMetodoPago)
                    {
                        cmbMetodoPago.SelectedIndex = i;
                        break;
                    }
                }

                // Cargar detalles
                _detallesVenta.Clear();
                if (_venta.Detalles != null)
                {
                    foreach (var detalle in _venta.Detalles)
                    {
                        // Asignar el objeto Producto completo
                        var producto = _productos.FirstOrDefault(p => p.IdProducto == detalle.IdProducto);
                        if (producto != null)
                        {
                            detalle.Producto = producto;
                        }
                        _detallesVenta.Add(detalle);
                    }
                }

                // Actualizar totales
                ActualizarTotales();

                _actualizandoUI = false;
            }
            catch (Exception ex)
            {
                _actualizandoUI = false;
                Logger.LogException(ex, "Error al cargar datos de la venta");
                throw;
            }
        }

        /// <summary>
        /// Valida y guarda la venta
        /// </summary>
        private void GuardarVenta()
        {
            try
            {
                // Ocultar mensaje de error previo
                txtError.Visibility = Visibility.Collapsed;

                // Validar datos básicos
                if (
                    string.IsNullOrWhiteSpace(txtNumeroFactura.Text) ||
                    dpFecha.SelectedDate == null ||
                    cmbCliente.SelectedItem == null ||
                    cmbEmpleado.SelectedItem == null ||
                    cmbMetodoPago.SelectedItem == null ||
                    _detallesVenta.Count == 0
                )
                {
                    MostrarError("Por favor, complete todos los campos obligatorios y agregue al menos un producto.");
                    return;
                }

                // Validar detalles
                foreach (var detalle in _detallesVenta)
                {
                    if (detalle.Producto == null)
                    {
                        MostrarError("Todos los detalles deben tener un producto seleccionado.");
                        return;
                    }

                    if (detalle.Cantidad <= 0)
                    {
                        MostrarError("La cantidad de todos los productos debe ser mayor que cero.");
                        return;
                    }

                    // Verificar stock (solo para ventas nuevas o productos agregados)
                    if (_esNuevo || !ExisteDetalleEnOriginal(detalle))
                    {
                        if (detalle.Cantidad > detalle.Producto.CantidadDisponible)
                        {
                            MostrarError($"No hay suficiente stock del producto '{detalle.Producto.Nombre}'. " +
                                $"Disponible: {detalle.Producto.CantidadDisponible}, Solicitado: {detalle.Cantidad}.");
                            return;
                        }
                    }
                }

                // Comprobar descuentos
                double descuentoTotal = 0;
                double subtotalProductos = 0;

                foreach (var detalle in _detallesVenta)
                {
                    subtotalProductos += detalle.Subtotal;
                    descuentoTotal += detalle.Descuento;
                }

                // Validar descuento adicional
                double descuentoAdicional = string.IsNullOrEmpty(txtDescuento.Text) ? 0 : double.Parse(txtDescuento.Text);
               

                // Completar datos de la venta
                _venta.Fecha = dpFecha.SelectedDate.Value;
                _venta.IdCliente = ((Cliente)cmbCliente.SelectedItem).IdCliente;
                _venta.IdEmpleado = ((Empleado)cmbEmpleado.SelectedItem).IdEmpleado;
                _venta.IdMetodoPago = ((MetodoPago)cmbMetodoPago.SelectedItem).IdMetodoPago;
                _venta.Observaciones = txtObservaciones.Text;
                _venta.Descuento = descuentoAdicional;
                _venta.Detalles = _detallesVenta.ToList();

                // Calcular totales
                _venta.CalcularTotal();

                // Guardar venta
                string errorMessage;
                bool resultado;

                if (_esNuevo)
                {
                    int idVenta = _ventaService.CrearVenta(_venta, out errorMessage);
                    resultado = idVenta > 0;
                    if (resultado)
                    {
                        _venta.IdVenta = idVenta;
                    }
                }
                else
                {
                    resultado = _ventaService.ActualizarVenta(_venta, out errorMessage);
                }

                // Verificar resultado
                if (resultado)
                {
                    MessageBox.Show($"Venta {(_esNuevo ? "creada" : "actualizada")} con éxito.",
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Cerrar ventana con resultado positivo
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MostrarError($"Error al {(_esNuevo ? "crear" : "actualizar")} la venta: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al guardar venta {(_esNuevo ? "nueva" : "existente")}");
                MostrarError("Error al guardar la venta: " + ex.Message);
            }
        }

        /// <summary>
        /// Verifica si un detalle de venta ya existía en la venta original
        /// </summary>
        /// <param name="detalle">Detalle a verificar</param>
        /// <returns>True si ya existía, False si es nuevo</returns>
        private bool ExisteDetalleEnOriginal(DetalleVenta detalle)
        {
            if (_esNuevo || _venta.Detalles == null)
                return false;

            return _venta.Detalles.Any(d =>
                d.IdProducto == detalle.IdProducto &&
                d.Cantidad == detalle.Cantidad);
        }

        /// <summary>
        /// Agrega un nuevo detalle de venta a la lista
        /// </summary>
        private void AgregarDetalle()
        {
            var nuevoDetalle = new DetalleVenta
            {
                Cantidad = 1,
                PrecioUnitario = 0,
                Descuento = 0
            };

            _detallesVenta.Add(nuevoDetalle);
            dgProductos.SelectedIndex = _detallesVenta.Count - 1;
        }

        /// <summary>
        /// Quita un detalle de venta de la lista
        /// </summary>
        private void QuitarDetalle()
        {
            if (dgProductos.SelectedItem == null)
                return;

            int indice = dgProductos.SelectedIndex;
            _detallesVenta.RemoveAt(indice);
            ActualizarTotales();
        }

        /// <summary>
        /// Actualiza los totales de la venta
        /// </summary>
        /// <summary>
        /// Actualiza los totales de la venta
        /// </summary>
        private void ActualizarTotales()
        {
            // Validar que la colección esté inicializada
            if (_detallesVenta == null)
            {
                Logger.LogWarning("_detallesVenta es null en ActualizarTotales()");
                return;
            }

            _actualizandoUI = true;

            try
            {
                // Calcular subtotal de productos
                double subtotal = 0;
                foreach (var detalle in _detallesVenta)
                {
                    // Validar que el detalle no sea null
                    if (detalle != null)
                    {
                        subtotal += detalle.Subtotal;
                    }
                }

                // Obtener descuento adicional con validación
                double descuento = 0;
                if (!string.IsNullOrEmpty(txtDescuento?.Text))
                {
                    double.TryParse(txtDescuento.Text, out descuento);
                }

                // Calcular base gravable (subtotal - descuento)
                double baseGravable = subtotal - descuento;

                // Calcular impuestos automáticamente sobre la base gravable
                double impuestos = baseGravable * PORCENTAJE_IMPUESTO;

                // Calcular total
                double total = baseGravable + impuestos;

                // Actualizar UI con validación de controles
                if (txtSubtotal != null)
                    txtSubtotal.Text = subtotal.ToString("C");

                if (txtImpuestos != null)
                    txtImpuestos.Text = impuestos.ToString("C");

                if (txtTotal != null)
                    txtTotal.Text = total.ToString("C");

                // Actualizar el objeto venta si existe
                if (_venta != null)
                {
                    _venta.Subtotal = subtotal;
                    _venta.Impuestos = impuestos;
                    _venta.Total = total;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error en ActualizarTotales");
            }
            finally
            {
                _actualizandoUI = false;
            }
        }



        /// <summary>
        /// Actualiza el subtotal de un detalle de venta
        /// </summary>
        /// <param name="detalle">Detalle a actualizar</param>
        private void ActualizarSubtotalDetalle(DetalleVenta detalle)
        {
            if (detalle.Cantidad <= 0 || detalle.PrecioUnitario <= 0)
            {
                detalle.Subtotal = 0;
                return;
            }

            detalle.Subtotal = (detalle.Cantidad * detalle.PrecioUnitario) - detalle.Descuento;
            ActualizarTotales();
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

        private void btnAgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            AgregarDetalle();
        }

        private void btnQuitarProducto_Click(object sender, RoutedEventArgs e)
        {
            QuitarDetalle();
        }

        private void dgProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnQuitarProducto.IsEnabled = dgProductos.SelectedItem != null;
        }

        private void txtCantidad_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Solo permitir números enteros positivos
            Regex regex = new Regex(@"^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void txtCantidad_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_actualizandoUI) return;

            try
            {
                var textBox = sender as TextBox;
                var detalle = textBox.DataContext as DetalleVenta;

                if (detalle != null && !string.IsNullOrEmpty(textBox.Text))
                {
                    int cantidad;
                    if (int.TryParse(textBox.Text, out cantidad))
                    {
                        detalle.Cantidad = cantidad;
                        ActualizarSubtotalDetalle(detalle);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cambiar cantidad");
            }
        }

        private void txtDescuento_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Solo permitir números y punto decimal
            Regex regex = new Regex(@"^[0-9]+(\.[0-9]*)?$");
            e.Handled = !regex.IsMatch(txtDescuento.Text + e.Text);
        }

        private void txtDescuento_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_actualizandoUI) return;

            try
            {
                var textBox = sender as TextBox;
                var detalle = textBox.DataContext as DetalleVenta;

                if (detalle != null && !string.IsNullOrEmpty(textBox.Text))
                {
                    double descuento;
                    if (double.TryParse(textBox.Text, out descuento))
                    {
                        detalle.Descuento = descuento;
                        ActualizarSubtotalDetalle(detalle);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cambiar descuento");
            }
        }

        private void txtVentaDescuento_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_actualizandoUI) return;
            ActualizarTotales();
        }

        private void txtImpuestos_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Solo permitir números y punto decimal
            Regex regex = new Regex(@"^[0-9]+(\.[0-9]*)?$");
            e.Handled = !regex.IsMatch(txtImpuestos.Text + e.Text);
        }

        private void txtImpuestos_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_actualizandoUI) return;
            ActualizarTotales();
        }

        private void cmbCliente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCliente.SelectedItem != null)
            {
                var cliente = cmbCliente.SelectedItem as Cliente;
                if (cliente != null)
                {
                    _venta.Cliente = cliente;
                    _venta.IdCliente = cliente.IdCliente;
                }
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            GuardarVenta();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #endregion
    }
}