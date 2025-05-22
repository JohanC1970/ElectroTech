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

namespace ElectroTech.Views.Compras
{
    /// <summary>
    /// Lógica de interacción para CompraDetailWindow.xaml
    /// </summary>
    public partial class CompraDetailWindow : Window
    {
        private readonly CompraService _compraService;
        private readonly ProveedorService _proveedorService;
        private readonly ProductoService _productoService;
        private readonly Compra _compra;
        private readonly bool _esNuevo;
        private readonly bool _soloLectura;
        private List<Proveedor> _proveedores;
        private List<Producto> _productos;
        private ObservableCollection<DetalleCompra> _detallesCompra;

        /// <summary>
        /// Constructor para una nueva compra
        /// </summary>
        public CompraDetailWindow()
        {
            InitializeComponent();

            _compraService = new CompraService();
            _proveedorService = new ProveedorService();
            _productoService = new ProductoService();
            _compra = new Compra();
            _esNuevo = true;
            _soloLectura = false;
            _detallesCompra = new ObservableCollection<DetalleCompra>();

            // Configurar ventana para nueva compra
            txtTitulo.Text = "Nueva Orden de Compra";
            dtpFecha.SelectedDate = DateTime.Now;
            cmbEstado.SelectedIndex = 0; // Pendiente por defecto
            pnlBotonesEdicion.Visibility = Visibility.Visible;

            ConfigurarVentana();
        }

        /// <summary>
        /// Constructor para editar o ver una compra existente
        /// </summary>
        public CompraDetailWindow(Compra compra, bool soloLectura)
        {
            InitializeComponent();

            _compraService = new CompraService();
            _proveedorService = new ProveedorService();
            _productoService = new ProductoService();
            _compra = compra;
            _esNuevo = false;
            _soloLectura = soloLectura;
            _detallesCompra = new ObservableCollection<DetalleCompra>(compra.Detalles ?? new List<DetalleCompra>());

            // Configurar ventana según modo
            if (soloLectura)
            {
                txtTitulo.Text = $"Visualización de Orden de Compra #{compra.NumeroOrden}";
                pnlBotonesVista.Visibility = Visibility.Visible;

                // Mostrar botón "Recibir Productos" solo si está pendiente
                if (compra.Estado == 'P')
                {
                    btnRecibir.Visibility = Visibility.Visible;
                }
            }
            else
            {
                txtTitulo.Text = $"Edición de Orden de Compra #{compra.NumeroOrden}";
                pnlBotonesEdicion.Visibility = Visibility.Visible;
            }

            ConfigurarVentana();
            CargarDatosCompra();
        }

        /// <summary>
        /// Configura la ventana y carga datos iniciales
        /// </summary>
        private void ConfigurarVentana()
        {
            try
            {
                // Cargar proveedores
                CargarProveedores();

                // Cargar productos
                CargarProductos();

                // Configurar lista de detalles
                dgDetalles.ItemsSource = _detallesCompra;

                // Configurar modo solo lectura si es necesario
                if (_soloLectura)
                {
                    ConfigurarModoSoloLectura();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al configurar ventana de compra");
                MessageBox.Show("Error al configurar la ventana: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Carga los proveedores en el combo
        /// </summary>
        private void CargarProveedores()
        {
            try
            {
                _proveedores = _proveedorService.ObtenerTodos();
                cmbProveedor.ItemsSource = _proveedores;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar proveedores");
                MessageBox.Show("Error al cargar los proveedores: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Carga los productos en el combo
        /// </summary>
        private void CargarProductos()
        {
            try
            {
                _productos = _productoService.ObtenerTodos();
                cmbProducto.ItemsSource = _productos;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar productos");
                MessageBox.Show("Error al cargar los productos: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Configura la ventana en modo solo lectura
        /// </summary>
        private void ConfigurarModoSoloLectura()
        {
            // Deshabilitar todos los controles de edición
            txtNumeroOrden.IsEnabled = false;
            chkGenerarNumero.IsEnabled = false;
            dtpFecha.IsEnabled = false;
            cmbEstado.IsEnabled = false;
            cmbProveedor.IsEnabled = false;
            txtObservaciones.IsEnabled = false;
            cmbProducto.IsEnabled = false;
            txtCantidad.IsEnabled = false;
            txtPrecioUnitario.IsEnabled = false;
            btnAgregarProducto.IsEnabled = false;
            txtImpuestos.IsEnabled = false;
        }

        /// <summary>
        /// Carga los datos de la compra en el formulario
        /// </summary>
        private void CargarDatosCompra()
        {
            try
            {
                // Cargar datos básicos
                txtNumeroOrden.Text = _compra.NumeroOrden;
                chkGenerarNumero.IsChecked = false;
                dtpFecha.SelectedDate = _compra.Fecha;
                txtObservaciones.Text = _compra.Observaciones;
                txtSubtotal.Text = _compra.Subtotal.ToString("N2");
                txtImpuestos.Text = _compra.Impuestos.ToString("N2");
                txtTotal.Text = _compra.Total.ToString("N2");

                // Seleccionar estado
                for (int i = 0; i < cmbEstado.Items.Count; i++)
                {
                    if (((ComboBoxItem)cmbEstado.Items[i]).Tag.ToString() == _compra.Estado.ToString())
                    {
                        cmbEstado.SelectedIndex = i;
                        break;
                    }
                }

                // Seleccionar proveedor
                cmbProveedor.SelectedValue = _compra.IdProveedor;

                // Cargar detalles (ya se hace en el constructor)
                ActualizarTotales();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar datos de la compra");
                MessageBox.Show("Error al cargar los datos de la compra: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Agrega un nuevo detalle de compra
        /// </summary>
        private void AgregarDetalle()
        {
            try
            {
                // Ocultar mensaje de error previo
                txtError.Visibility = Visibility.Collapsed;

                // Validar que se haya seleccionado un producto
                if (cmbProducto.SelectedItem == null)
                {
                    MostrarError("Debe seleccionar un producto.");
                    return;
                }

                // Validar cantidad
                int cantidad;
                if (!int.TryParse(txtCantidad.Text, out cantidad) || cantidad <= 0)
                {
                    MostrarError("La cantidad debe ser un número entero mayor que cero.");
                    return;
                }

                // Validar precio unitario
                double precioUnitario;
                if (!double.TryParse(txtPrecioUnitario.Text, out precioUnitario) || precioUnitario <= 0)
                {
                    MostrarError("El precio unitario debe ser un número mayor que cero.");
                    return;
                }

                // Verificar si el producto ya está en la lista
                var producto = (Producto)cmbProducto.SelectedItem;
                var detalleExistente = _detallesCompra.FirstOrDefault(d => d.IdProducto == producto.IdProducto);

                if (detalleExistente != null)
                {
                    // Actualizar el detalle existente
                    detalleExistente.Cantidad += cantidad;
                    detalleExistente.PrecioUnitario = precioUnitario; // Usar el último precio ingresado
                    detalleExistente.Subtotal = detalleExistente.Cantidad * detalleExistente.PrecioUnitario;

                    // Refrescar la vista
                    dgDetalles.Items.Refresh();
                }
                else
                {
                    // Crear nuevo detalle
                    var detalle = new DetalleCompra
                    {
                        IdProducto = producto.IdProducto,
                        Producto = producto,
                        Cantidad = cantidad,
                        PrecioUnitario = precioUnitario,
                        Subtotal = cantidad * precioUnitario
                    };

                    // Agregar a la lista
                    _detallesCompra.Add(detalle);
                }

                // Limpiar campos
                cmbProducto.SelectedIndex = -1;
                txtCantidad.Text = "1";
                txtPrecioUnitario.Text = "";

                // Actualizar totales
                ActualizarTotales();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al agregar detalle de compra");
                MostrarError("Error al agregar el producto: " + ex.Message);
            }
        }

        /// <summary>
        /// Elimina un detalle de compra
        /// </summary>
        private void EliminarDetalle(DetalleCompra detalle)
        {
            try
            {
                _detallesCompra.Remove(detalle);
                ActualizarTotales();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al eliminar detalle de compra");
                MessageBox.Show("Error al eliminar el producto: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Actualiza los totales de la compra
        /// </summary>
        private void ActualizarTotales()
        {
            try
            {
                // Calcular subtotal
                double subtotal = _detallesCompra.Sum(d => d.Subtotal);
                txtSubtotal.Text = subtotal.ToString("N2");

                // Obtener impuestos
                double impuestos;
                if (!double.TryParse(txtImpuestos.Text, out impuestos))
                {
                    impuestos = 0;
                }

                // Calcular total
                double total = subtotal + impuestos;
                txtTotal.Text = total.ToString("N2");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al actualizar totales");
                MessageBox.Show("Error al actualizar los totales: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Valida y guarda la compra
        /// </summary>
        private void GuardarCompra()
        {
            try
            {
                // Ocultar mensaje de error previo
                txtError.Visibility = Visibility.Collapsed;

                // Validar campos obligatorios
                if (string.IsNullOrWhiteSpace(txtNumeroOrden.Text) ||
                    dtpFecha.SelectedDate == null ||
                    cmbProveedor.SelectedItem == null)
                {
                    MostrarError("Por favor, complete todos los campos obligatorios.");
                    return;
                }

                // Validar que haya al menos un detalle
                if (_detallesCompra.Count == 0)
                {
                    MostrarError("Debe agregar al menos un producto a la compra.");
                    return;
                }

                // Completar datos de la compra
                _compra.NumeroOrden = txtNumeroOrden.Text.Trim();
                _compra.Fecha = dtpFecha.SelectedDate.Value;
                _compra.IdProveedor = ((Proveedor)cmbProveedor.SelectedItem).IdProveedor;
                _compra.Proveedor = (Proveedor)cmbProveedor.SelectedItem;
                _compra.Estado = ((ComboBoxItem)cmbEstado.SelectedItem).Tag.ToString()[0];
                _compra.Observaciones = txtObservaciones.Text.Trim();
                _compra.Subtotal = double.Parse(txtSubtotal.Text);
                _compra.Impuestos = double.Parse(txtImpuestos.Text);
                _compra.Total = double.Parse(txtTotal.Text);
                _compra.Detalles = _detallesCompra.ToList();

                // Guardar compra
                string errorMessage;
                bool resultado;

                if (_esNuevo)
                {
                    int idCompra = _compraService.CrearCompra(_compra, out errorMessage);
                    resultado = idCompra > 0;

                    if (resultado)
                    {
                        _compra.IdCompra = idCompra;
                    }
                }
                else
                {
                    resultado = _compraService.ActualizarCompra(_compra, out errorMessage);
                }

                // Verificar resultado
                if (resultado)
                {
                    MessageBox.Show($"Compra {(_esNuevo ? "creada" : "actualizada")} con éxito.",
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Cerrar ventana con resultado positivo
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MostrarError($"Error al {(_esNuevo ? "crear" : "actualizar")} la compra: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al guardar compra {(_esNuevo ? "nueva" : "existente")}");
                MostrarError("Error al guardar la compra: " + ex.Message);
            }
        }

        /// <summary>
        /// Recibe los productos de la compra
        /// </summary>
        private void RecibirProductos()
        {
            try
            {
                // Confirmar acción
                var result = MessageBox.Show(
                    "¿Está seguro que desea marcar esta compra como recibida? " +
                    "Esta acción actualizará el inventario con los productos de la compra.",
                    "Confirmar Recepción",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Intentar recibir la compra
                    string errorMessage;
                    bool recibido = _compraService.RecibirCompra(_compra.IdCompra, out errorMessage);

                    if (recibido)
                    {
                        MessageBox.Show("Compra recibida con éxito. El inventario ha sido actualizado.",
                            "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Cerrar ventana con resultado positivo
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show($"Error al recibir la compra: {errorMessage}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al recibir productos de la compra");
                MessageBox.Show("Error al recibir los productos: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Genera un número de orden automático
        /// </summary>
        private void GenerarNumeroOrden()
        {
            try
            {
                // El formato será "CO" + YYYYMMDD + consecutivo (3 dígitos)
                // Por ejemplo: CO20250102001
                string fechaParte = DateTime.Now.ToString("yyyyMMdd");
                int consecutivo = _compraService.ObtenerConsecutivoOrden();
                string numeroOrden = $"CO{fechaParte}{consecutivo:D3}";
                txtNumeroOrden.Text = numeroOrden;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al generar número de orden");
                MessageBox.Show("Error al generar el número de orden: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            GuardarCompra();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Cerrar ventana sin guardar
            DialogResult = false;
            Close();
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            // Cerrar ventana sin guardar
            DialogResult = false;
            Close();
        }

        private void btnRecibir_Click(object sender, RoutedEventArgs e)
        {
            RecibirProductos();
        }

        private void chkGenerarNumero_Checked(object sender, RoutedEventArgs e)
        {
            txtNumeroOrden.IsEnabled = false;
            GenerarNumeroOrden();
        }

        private void chkGenerarNumero_Unchecked(object sender, RoutedEventArgs e)
        {
            txtNumeroOrden.IsEnabled = true;
            txtNumeroOrden.Focus();
        }

        private void btnAgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            AgregarDetalle();
        }

        private void btnEliminarDetalle_Click(object sender, RoutedEventArgs e)
        {
            var detalle = ((FrameworkElement)sender).DataContext as DetalleCompra;
            if (detalle != null)
            {
                EliminarDetalle(detalle);
            }
        }

        private void txtCantidad_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Solo permitir números enteros
            e.Handled = !EsNumeroEnteroValido(e.Text);
        }

        private void txtPrecioUnitario_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Solo permitir números y punto decimal
            e.Handled = !EsNumeroDecimalValido(e.Text);
        }

        private void txtPrecioUnitario_LostFocus(object sender, RoutedEventArgs e)
        {
            // Formatear valor numérico al perder el foco
            TextBox textBox = sender as TextBox;

            if (!string.IsNullOrEmpty(textBox.Text))
            {
                double valor;

                if (double.TryParse(textBox.Text, out valor))
                {
                    textBox.Text = valor.ToString("N2");
                }
            }
        }

        private void txtImpuestos_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Solo permitir números y punto decimal
            e.Handled = !EsNumeroDecimalValido(e.Text);
        }

        private void txtImpuestos_LostFocus(object sender, RoutedEventArgs e)
        {
            // Formatear valor numérico al perder el foco
            TextBox textBox = sender as TextBox;

            if (!string.IsNullOrEmpty(textBox.Text))
            {
                double valor;

                if (double.TryParse(textBox.Text, out valor))
                {
                    textBox.Text = valor.ToString("N2");
                }
            }
        }

        private void txtImpuestos_TextChanged(object sender, TextChangedEventArgs e)
        {
            ActualizarTotales();
        }

        #endregion

        #region Métodos de validación

        /// <summary>
        /// Verifica si un texto es un número decimal válido
        /// </summary>
        private bool EsNumeroDecimalValido(string texto)
        {
            // Permitir solo dígitos y un punto decimal
            Regex regex = new Regex(@"^[0-9.]$");
            return regex.IsMatch(texto);
        }

        /// <summary>
        /// Verifica si un texto es un número entero válido
        /// </summary>
        private bool EsNumeroEnteroValido(string texto)
        {
            // Permitir solo dígitos
            Regex regex = new Regex(@"^[0-9]$");
            return regex.IsMatch(texto);
        }

        #endregion
    }
}