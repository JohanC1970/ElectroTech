using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ElectroTech.Views.Productos
{
    /// <summary>
    /// Lógica de interacción para ProductosPage.xaml
    /// </summary>
    public partial class ProductosPage : Page
    {
        private readonly ProductoService _productoService;
        private List<Producto> _productos;
        private List<Categoria> _categorias;
        private Producto _productoSeleccionado;

        /// <summary>
        /// Constructor de la página de productos
        /// </summary>
        public ProductosPage()
        {
            InitializeComponent();

            // Inicializar servicios
            _productoService = new ProductoService();

            // Cargar datos iniciales
            CargarCategorias();
            CargarProductos();

            // Eventos
            this.Loaded += ProductosPage_Loaded;
        }

        /// <summary>
        /// Evento que se ejecuta cuando la página se ha cargado
        /// </summary>
        private void ProductosPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Refrescar datos
                CargarProductos();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar la página de Productos");
                MessageBox.Show("Error al cargar los productos: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Carga las categorías para el filtro
        /// </summary>
        private void CargarCategorias()
        {
            try
            {
                // TODO: Obtener las categorías de un servicio real
                // Por ahora simulamos la carga de categorías
                _categorias = new List<Categoria>
                {
                    new Categoria { IdCategoria = 0, Nombre = "Todas las categorías" },
                    new Categoria { IdCategoria = 1, Nombre = "Computadoras" },
                    new Categoria { IdCategoria = 2, Nombre = "Smartphones" },
                    new Categoria { IdCategoria = 3, Nombre = "Tablets" },
                    new Categoria { IdCategoria = 4, Nombre = "Accesorios" },
                    new Categoria { IdCategoria = 5, Nombre = "Componentes" }
                };

                cmbCategoria.ItemsSource = _categorias;
                cmbCategoria.SelectedIndex = 0; // Seleccionar "Todas las categorías"
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar categorías");
                MessageBox.Show("Error al cargar las categorías: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Carga la lista de productos
        /// </summary>
        private void CargarProductos()
        {
            try
            {
                _productos = _productoService.ObtenerTodos();

                // Asignar a la vista
                dgProductos.ItemsSource = _productos;

                // Actualizar contador
                ActualizarContadorProductos();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar productos");
                MessageBox.Show("Error al cargar los productos: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Actualiza el contador de productos
        /// </summary>
        private void ActualizarContadorProductos()
        {
            int totalProductos = dgProductos.Items.Count;
            txtTotalProductos.Text = $"Total: {totalProductos} productos";
        }

        /// <summary>
        /// Filtra los productos por categoría
        /// </summary>
        private void FiltrarPorCategoria(int idCategoria)
        {
            try
            {
                if (idCategoria == 0) // "Todas las categorías"
                {
                    // Mostrar todos los productos
                    CargarProductos();
                }
                else
                {
                    // Filtrar por categoría
                    var productosFiltrados = _productoService.ObtenerPorCategoria(idCategoria);
                    dgProductos.ItemsSource = productosFiltrados;
                    ActualizarContadorProductos();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al filtrar productos por categoría {idCategoria}");
                MessageBox.Show("Error al filtrar los productos: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Busca productos según el término ingresado
        /// </summary>
        private void BuscarProductos()
        {
            try
            {
                string termino = txtBuscar.Text.Trim();

                if (string.IsNullOrEmpty(termino))
                {
                    // Si no hay término, mostrar todos o filtrar por categoría
                    if (cmbCategoria.SelectedIndex == 0)
                    {
                        CargarProductos();
                    }
                    else
                    {
                        FiltrarPorCategoria(((Categoria)cmbCategoria.SelectedItem).IdCategoria);
                    }
                    return;
                }

                // Buscar productos
                var productosFiltrados = _productoService.Buscar(termino);

                // Si hay una categoría seleccionada, aplicar ese filtro también
                if (cmbCategoria.SelectedIndex > 0)
                {
                    int idCategoria = ((Categoria)cmbCategoria.SelectedItem).IdCategoria;
                    productosFiltrados = productosFiltrados.Where(p => p.IdCategoria == idCategoria).ToList();
                }

                dgProductos.ItemsSource = productosFiltrados;
                ActualizarContadorProductos();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al buscar productos con término '{txtBuscar.Text}'");
                MessageBox.Show("Error al buscar productos: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Eventos de controles

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            BuscarProductos();
        }

        private void txtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuscarProductos();
            }
        }

        private void cmbCategoria_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCategoria.SelectedItem != null)
            {
                int idCategoria = ((Categoria)cmbCategoria.SelectedItem).IdCategoria;
                FiltrarPorCategoria(idCategoria);
            }
        }

        private void dgProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _productoSeleccionado = dgProductos.SelectedItem as Producto;
        }

        private void btnNuevoProducto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Abrir ventana de nuevo producto
                var nuevaVentana = new ProductoDetailWindow();
                nuevaVentana.Owner = Window.GetWindow(this);

                // Si se cerró con resultado positivo, recargar productos
                if (nuevaVentana.ShowDialog() == true)
                {
                    CargarProductos();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al abrir ventana de nuevo producto");
                MessageBox.Show("Error al abrir la ventana de nuevo producto: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener el producto seleccionado
                var producto = ((FrameworkElement)sender).DataContext as Producto;

                if (producto != null)
                {
                    // Abrir ventana de edición
                    var editarVentana = new ProductoDetailWindow(producto);
                    editarVentana.Owner = Window.GetWindow(this);

                    // Si se cerró con resultado positivo, recargar productos
                    if (editarVentana.ShowDialog() == true)
                    {
                        CargarProductos();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al abrir ventana de edición de producto");
                MessageBox.Show("Error al abrir la ventana de edición: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener el producto seleccionado
                var producto = ((FrameworkElement)sender).DataContext as Producto;

                if (producto != null)
                {
                    // Confirmar eliminación
                    var resultado = MessageBox.Show(
                        $"¿Está seguro que desea eliminar el producto '{producto.Nombre}'?",
                        "Confirmar eliminación",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (resultado == MessageBoxResult.Yes)
                    {
                        // Eliminar producto
                        string errorMessage;
                        bool eliminado = _productoService.EliminarProducto(producto.IdProducto, out errorMessage);

                        if (eliminado)
                        {
                            MessageBox.Show("Producto eliminado con éxito.",
                                "Información", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Recargar productos
                            CargarProductos();
                        }
                        else
                        {
                            MessageBox.Show($"Error al eliminar el producto: {errorMessage}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al eliminar producto");
                MessageBox.Show("Error al eliminar el producto: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}