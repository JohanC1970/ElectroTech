using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ElectroTech.Views
{
    /// <summary>
    /// Lógica de interacción para InventarioPage.xaml
    /// </summary>
    public partial class InventarioPage : Page
    {
        private readonly ProductoService _productoService;
        private List<Producto> _productos;

        /// <summary>
        /// Constructor de la página de inventario
        /// </summary>
        public InventarioPage()
        {
            InitializeComponent();

            // Inicializar servicios
            _productoService = new ProductoService();

            // Seleccionar "Entrada" por defecto
            cmbTipoMovimiento.SelectedIndex = 0;

            // Cargar datos iniciales
            CargarProductos();
            CargarInventario();

            // Eventos
            this.Loaded += InventarioPage_Loaded;
        }

        /// <summary>
        /// Evento que se ejecuta cuando la página se ha cargado
        /// </summary>
        private void InventarioPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Refrescar datos
                CargarInventario();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar la página de Inventario");
                MessageBox.Show("Error al cargar el inventario: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Carga los productos para el ComboBox
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
        /// Carga el inventario de productos
        /// </summary>
        private void CargarInventario()
        {
            try
            {
                _productos = _productoService.ObtenerTodos();

                // Asignar a la vista
                dgInventario.ItemsSource = _productos;

                // Actualizar contador
                ActualizarResumen();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar inventario");
                MessageBox.Show("Error al cargar el inventario: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Actualiza el resumen del inventario
        /// </summary>
        private void ActualizarResumen()
        {
            int totalProductos = dgInventario.Items.Count;
            txtTotalProductos.Text = $"Total: {totalProductos} productos";

            // Contar productos bajo stock
            int bajoStock = 0;
            foreach (Producto producto in _productos)
            {
                if (producto.RequiereReposicion)
                {
                    bajoStock++;
                }
            }
            txtProductosBajoStock.Text = $"Bajo stock: {bajoStock} productos";
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

        #region Eventos de controles

        private void cmbProducto_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var producto = cmbProducto.SelectedItem as Producto;
                if (producto != null)
                {
                    txtStockActual.Text = producto.CantidadDisponible.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al seleccionar producto");
            }
        }

        private void txtCantidad_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Solo permitir números enteros
            e.Handled = !EsNumeroEnteroValido(e.Text);
        }

        private void btnAjustar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar selección de producto
                var producto = cmbProducto.SelectedItem as Producto;
                if (producto == null)
                {
                    MessageBox.Show("Por favor, seleccione un producto.",
                        "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validar cantidad
                if (string.IsNullOrWhiteSpace(txtCantidad.Text) || !int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
                {
                    MessageBox.Show("Por favor, ingrese una cantidad válida (mayor que cero).",
                        "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Obtener tipo de movimiento
                var selectedItem = cmbTipoMovimiento.SelectedItem as ComboBoxItem;
                if (selectedItem == null)
                {
                    MessageBox.Show("Por favor, seleccione un tipo de movimiento.",
                        "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                char tipoMovimiento = selectedItem.Tag.ToString()[0];

                // Confirmar ajuste
                string mensajeConfirmacion = tipoMovimiento == 'E'
                    ? $"¿Está seguro que desea agregar {cantidad} unidades al inventario del producto '{producto.Nombre}'?"
                    : $"¿Está seguro que desea retirar {cantidad} unidades del inventario del producto '{producto.Nombre}'?";

                var resultado = MessageBox.Show(mensajeConfirmacion,
                    "Confirmar ajuste",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resultado == MessageBoxResult.No)
                {
                    return;
                }

                // Realizar ajuste
                string errorMessage;
                bool actualizado = _productoService.ActualizarStock(producto.IdProducto, cantidad, tipoMovimiento, out errorMessage);

                if (actualizado)
                {
                    MessageBox.Show($"Inventario ajustado con éxito. El stock actual del producto '{producto.Nombre}' ha sido actualizado.",
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Recargar inventario
                    CargarInventario();
                    cmbProducto.ItemsSource = _productos;

                    // Actualizar el stock mostrado si el mismo producto sigue seleccionado
                    if (cmbProducto.SelectedItem is Producto productoSeleccionado && productoSeleccionado.IdProducto == producto.IdProducto)
                    {
                        // Buscar el producto actualizado en la lista
                        foreach (Producto p in _productos)
                        {
                            if (p.IdProducto == producto.IdProducto)
                            {
                                txtStockActual.Text = p.CantidadDisponible.ToString();
                                break;
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"Error al ajustar el inventario: {errorMessage}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al ajustar inventario");
                MessageBox.Show("Error al ajustar el inventario: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}