using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ElectroTech.Views.Productos
{
    /// <summary>
    /// Lógica de interacción para ProductoDetailWindow.xaml
    /// </summary>
    public partial class ProductoDetailWindow : Window
    {
        private readonly ProductoService _productoService;
        private readonly Producto _producto;
        private readonly bool _esNuevo;
        private List<Categoria> _categorias;

        /// <summary>
        /// Constructor para un nuevo producto
        /// </summary>
        public ProductoDetailWindow()
        {
            InitializeComponent();

            _productoService = new ProductoService();
            _producto = new Producto();
            _esNuevo = true;

            // Configurar ventana para nuevo producto
            txtTitulo.Text = "Nuevo Producto";
            ConfigurarVentana();
        }

        /// <summary>
        /// Constructor para editar un producto existente
        /// </summary>
        public ProductoDetailWindow(Producto producto)
        {
            InitializeComponent();

            _productoService = new ProductoService();
            _producto = producto;
            _esNuevo = false;

            // Configurar ventana para edición
            txtTitulo.Text = "Editar Producto";
            pnlStockActual.Visibility = Visibility.Visible;
            pnlCamposEdicion.Visibility = Visibility.Visible;

            ConfigurarVentana();
            CargarDatosProducto();
        }

        /// <summary>
        /// Configura la ventana y carga datos iniciales
        /// </summary>
        private void ConfigurarVentana()
        {
            try
            {
                // Cargar categorías
                CargarCategorias();

                // Configurar validaciones
                txtPrecioCompra.LostFocus += txtPrecio_LostFocus;
                txtPrecioVenta.LostFocus += txtPrecio_LostFocus;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al configurar ventana de producto");
                MessageBox.Show("Error al configurar la ventana: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Carga las categorías en el combo
        /// </summary>
        private void CargarCategorias()
        {
            try
            {
                var categoriaService = new CategoriaService();
                _categorias = categoriaService.ObtenerTodas();
                cmbCategoria.ItemsSource = _categorias; 
                cmbCategoria.SelectedValuePath = "IdCategoria";
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar categorías");
                MessageBox.Show("Error al cargar las categorías: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Carga los datos del producto en el formulario (modo edición)
        /// </summary>
        private void CargarDatosProducto()
        {
            try
            {
                // Cargar datos básicos
                txtCodigo.Text = _producto.Codigo;
                txtNombre.Text = _producto.Nombre;
                txtDescripcion.Text = _producto.Descripcion;
                txtModelo.Text = _producto.Modelo;
                txtUbicacionAlmacen.Text = _producto.UbicacionAlmacen;
                txtPrecioCompra.Text = _producto.PrecioCompra.ToString();
                txtPrecioVenta.Text = _producto.PrecioVenta.ToString();
                txtStockMinimo.Text = _producto.StockMinimo.ToString();
                txtStockActual.Text = _producto.CantidadDisponible.ToString();

                // Seleccionar categoría
                for (int i = 0; i < _categorias.Count; i++)
                {
                    if (_categorias[i].IdCategoria == _producto.IdCategoria)
                    {
                        cmbCategoria.SelectedIndex = i;
                        break;
                    }
                }

               txtMarca.Text = _producto.Marca;

                // Estado
                chkActivo.IsChecked = _producto.Activo;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar datos del producto");
                MessageBox.Show("Error al cargar los datos del producto: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Valida y guarda el producto
        /// </summary>
        private void GuardarProducto()
        {
            try
            {
                // Ocultar mensaje de error previo
                txtError.Visibility = Visibility.Collapsed;

                // Validar campos obligatorios
                if (string.IsNullOrWhiteSpace(txtCodigo.Text) ||
                    string.IsNullOrWhiteSpace(txtNombre.Text) ||
                    cmbCategoria.SelectedItem == null ||
                    string.IsNullOrWhiteSpace(txtPrecioCompra.Text) ||
                    string.IsNullOrWhiteSpace(txtPrecioVenta.Text) ||
                    string.IsNullOrWhiteSpace(txtStockMinimo.Text))
                {
                    MostrarError("Por favor, complete todos los campos obligatorios.");
                    return;
                }

                // Parsear valores numéricos
                decimal precioCompra, precioVenta;
                int stockMinimo;

                if (!decimal.TryParse(txtPrecioCompra.Text, out precioCompra) || precioCompra <= 0)
                {
                    MostrarError("El precio de compra debe ser un número mayor que cero.");
                    return;
                }

                if (!decimal.TryParse(txtPrecioVenta.Text, out precioVenta) || precioVenta <= 0)
                {
                    MostrarError("El precio de venta debe ser un número mayor que cero.");
                    return;
                }

                if (!int.TryParse(txtStockMinimo.Text, out stockMinimo) || stockMinimo < 0)
                {
                    MostrarError("El stock mínimo debe ser un número entero no negativo.");
                    return;
                }

                // Validar regla de negocio: precio de venta > precio de compra
                if (precioVenta <= precioCompra)
                {
                    MostrarError("El precio de venta debe ser mayor al precio de compra.");
                    return;
                }

                // Completar datos del producto
                _producto.Codigo = txtCodigo.Text.Trim();
                _producto.Nombre = txtNombre.Text.Trim();
                _producto.Descripcion = txtDescripcion.Text.Trim();
                _producto.IdCategoria = ((Categoria)cmbCategoria.SelectedItem).IdCategoria;
                _producto.NombreCategoria = ((Categoria)cmbCategoria.SelectedItem).Nombre;
                _producto.Marca = string.IsNullOrWhiteSpace(txtMarca.Text) ? null : txtMarca.Text.Trim();
                _producto.Marca = string.IsNullOrWhiteSpace(txtMarca.Text) ? null : txtMarca.Text.Trim();
                _producto.Modelo = txtModelo.Text.Trim();
                _producto.UbicacionAlmacen = txtUbicacionAlmacen.Text.Trim();
                _producto.PrecioCompra = precioCompra;
                _producto.PrecioVenta = precioVenta;
                _producto.StockMinimo = stockMinimo;

                // Valores específicos según modo
                if (!_esNuevo)
                {
                    _producto.Activo = chkActivo.IsChecked ?? true;
                }

                // Guardar producto
                string errorMessage;
                bool resultado;

                if (_esNuevo)
                {
                    int idProducto = _productoService.CrearProducto(_producto, out errorMessage);
                    resultado = idProducto > 0;

                    if (resultado)
                    {
                        _producto.IdProducto = idProducto;
                    }
                }
                else
                {
                    resultado = _productoService.ActualizarProducto(_producto, out errorMessage);
                }

                // Verificar resultado
                if (resultado)
                {
                    MessageBox.Show($"Producto {(_esNuevo ? "creado" : "actualizado")} con éxito.",
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Cerrar ventana con resultado positivo
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MostrarError($"Error al {(_esNuevo ? "crear" : "actualizar")} el producto: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al guardar producto {(_esNuevo ? "nuevo" : "existente")}");
                MostrarError("Error al guardar el producto: " + ex.Message);
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
            GuardarProducto();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Cerrar ventana sin guardar
            DialogResult = false;
            Close();
        }

        private void txtPrecioCompra_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Solo permitir números y punto decimal
            e.Handled = !EsNumeroDecimalValido(e.Text);
        }

        private void txtPrecioVenta_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Solo permitir números y punto decimal
            e.Handled = !EsNumeroDecimalValido(e.Text);
        }

        private void txtStockMinimo_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Solo permitir números enteros
            e.Handled = !EsNumeroEnteroValido(e.Text);
        }

        private void txtPrecio_LostFocus(object sender, RoutedEventArgs e)
        {
            // Formatear valor numérico al perder el foco
            TextBox textBox = sender as TextBox;

            if (!string.IsNullOrEmpty(textBox.Text))
            {
                decimal valor;

                if (decimal.TryParse(textBox.Text, out valor))
                {
                    textBox.Text = valor.ToString("0.00");
                }
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
            if (texto == "." && ((TextBox)((TextCompositionEventArgs)System.Windows.Application.Current.MainWindow.FindResource("currentEvent")).Source).Text.Contains("."))
            {
                return false;
            }

            return regex.IsMatch(texto);
        }

        /// <summary>
        /// Verifica si un texto es un número entero válido
        /// </summary>
        private bool EsNumeroEnteroValido(string texto)
        {
            // Permitir solo dígitos
            Regex regex = new Regex(@"^[0-9]+$");
            return regex.IsMatch(texto);
        }

        #endregion
    }
}