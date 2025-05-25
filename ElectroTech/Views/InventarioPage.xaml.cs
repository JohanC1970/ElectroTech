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
                // Validar entrada de datos en la UI
                if (!ValidarFormularioAjuste(out string mensajeValidacion))
                {
                    MessageBox.Show(mensajeValidacion, "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Obtener datos del formulario
                var producto = cmbProducto.SelectedItem as Producto;
                int cantidad = int.Parse(txtCantidad.Text);
                char tipoMovimiento = ((ComboBoxItem)cmbTipoMovimiento.SelectedItem).Tag.ToString()[0];

                // Validar stock suficiente para salidas (validación adicional en UI)
                if (tipoMovimiento == 'S' && !_productoService.TieneSuficienteStock(producto.IdProducto, cantidad))
                {
                    int stockActual = _productoService.ObtenerStockActual(producto.IdProducto);
                    MessageBox.Show($"No hay suficiente stock disponible.\n\n" +
                                  $"Stock actual: {stockActual}\n" +
                                  $"Cantidad solicitada: {cantidad}",
                        "Stock Insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Mostrar confirmación
                if (!MostrarConfirmacionAjuste(producto, cantidad, tipoMovimiento))
                {
                    return;
                }

                // Ejecutar ajuste
                string errorMessage;
                bool actualizado = _productoService.ActualizarStock(producto.IdProducto, cantidad, tipoMovimiento, out errorMessage);

                // Mostrar resultado
                if (actualizado)
                {
                    MostrarExitoAjuste(producto, cantidad, tipoMovimiento);
                    LimpiarFormulario();
                    RefrescarDatos();
                }
                else
                {
                    MostrarErrorAjuste(errorMessage);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error en la interfaz al ajustar inventario");
                MessageBox.Show($"Error inesperado en la interfaz:\n\n{ex.Message}\n\n" +
                               "Si el problema persiste, contacte al administrador del sistema.",
                    "Error del Sistema", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Valida que todos los campos del formulario estén correctos.
        /// </summary>
        /// <param name="mensajeError">Mensaje de error si la validación falla.</param>
        /// <returns>True si la validación es exitosa, False en caso contrario.</returns>
        private bool ValidarFormularioAjuste(out string mensajeError)
        {
            mensajeError = string.Empty;

            // Validar selección de producto
            if (cmbProducto.SelectedItem == null)
            {
                mensajeError = "Por favor, seleccione un producto.";
                return false;
            }

            // Validar cantidad
            if (string.IsNullOrWhiteSpace(txtCantidad.Text) ||
                !int.TryParse(txtCantidad.Text, out int cantidad) ||
                cantidad <= 0)
            {
                mensajeError = "Por favor, ingrese una cantidad válida (mayor que cero).";
                return false;
            }

            // Validar tipo de movimiento
            if (cmbTipoMovimiento.SelectedItem == null)
            {
                mensajeError = "Por favor, seleccione un tipo de movimiento.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Muestra el diálogo de confirmación para el ajuste.
        /// </summary>
        /// <param name="producto">Producto a ajustar.</param>
        /// <param name="cantidad">Cantidad del ajuste.</param>
        /// <param name="tipoMovimiento">Tipo de movimiento.</param>
        /// <returns>True si el usuario confirma, False en caso contrario.</returns>
        private bool MostrarConfirmacionAjuste(Producto producto, int cantidad, char tipoMovimiento)
        {
            int stockActual = _productoService.ObtenerStockActual(producto.IdProducto);
            int stockDespues = tipoMovimiento == 'E' ? stockActual + cantidad : stockActual - cantidad;

            string mensajeConfirmacion = tipoMovimiento == 'E'
                ? $"¿Está seguro que desea agregar {cantidad} unidades al inventario?"
                : $"¿Está seguro que desea retirar {cantidad} unidades del inventario?";

            mensajeConfirmacion += $"\n\nProducto: {producto.Nombre}\n" +
                                  $"Stock actual: {stockActual}\n" +
                                  $"Stock después del ajuste: {stockDespues}";

            var resultado = MessageBox.Show(mensajeConfirmacion,
                "Confirmar Ajuste de Inventario",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            return resultado == MessageBoxResult.Yes;
        }

        /// <summary>
        /// Muestra el mensaje de éxito del ajuste.
        /// </summary>
        /// <param name="producto">Producto ajustado.</param>
        /// <param name="cantidad">Cantidad del ajuste.</param>
        /// <param name="tipoMovimiento">Tipo de movimiento.</param>
        private void MostrarExitoAjuste(Producto producto, int cantidad, char tipoMovimiento)
        {
            string tipoMovimientoTexto = tipoMovimiento == 'E' ? "Entrada" : "Salida";

            MessageBox.Show($"¡Inventario ajustado con éxito!\n\n" +
                          $"Producto: {producto.Nombre}\n" +
                          $"Movimiento: {tipoMovimientoTexto} de {cantidad} unidades\n" +
                          $"El stock ha sido actualizado correctamente.",
                "Ajuste Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Muestra el mensaje de error del ajuste.
        /// </summary>
        /// <param name="errorMessage">Mensaje de error.</param>
        private void MostrarErrorAjuste(string errorMessage)
        {
            MessageBox.Show($"No se pudo ajustar el inventario:\n\n{errorMessage}",
                "Error en Ajuste", MessageBoxButton.OK, MessageBoxImage.Error);
        }


        /// <summary>
        /// Limpia el formulario después de un ajuste exitoso.
        /// </summary>
        private void LimpiarFormulario()
        {
            txtCantidad.Text = "1";
            cmbProducto.SelectedIndex = -1;
            txtStockActual.Text = "0";
            cmbTipoMovimiento.SelectedIndex = 0; // Entrada por defecto
        }

        /// <summary>
        /// Refresca los datos del inventario y productos.
        /// </summary>
        private void RefrescarDatos()
        {
            CargarInventario();
            CargarProductos();
        }


        #endregion
    }
}