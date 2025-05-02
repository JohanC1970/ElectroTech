using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ElectroTech.Views
{
    /// <summary>
    /// Lógica de interacción para CategoriasPage.xaml
    /// </summary>
    public partial class CategoriasPage : Page
    {
        private readonly CategoriaService _categoriaService;
        private List<Categoria> _categorias;
        private Categoria _categoriaSeleccionada;

        /// <summary>
        /// Constructor de la página de categorías
        /// </summary>
        public CategoriasPage()
        {
            InitializeComponent();

            // Inicializar servicios
            _categoriaService = new CategoriaService();

            // Cargar datos iniciales
            CargarCategorias();

            // Eventos
            this.Loaded += CategoriasPage_Loaded;
        }

        /// <summary>
        /// Evento que se ejecuta cuando la página se ha cargado
        /// </summary>
        private void CategoriasPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Refrescar datos
                CargarCategorias();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar la página de Categorías");
                MessageBox.Show("Error al cargar las categorías: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Carga la lista de categorías
        /// </summary>
        private void CargarCategorias()
        {
            try
            {
                _categorias = _categoriaService.ObtenerTodas();

                // Asignar a la vista
                dgCategorias.ItemsSource = _categorias;

                // Actualizar contador
                ActualizarContadorCategorias();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar categorías");
                MessageBox.Show("Error al cargar las categorías: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Actualiza el contador de categorías
        /// </summary>
        private void ActualizarContadorCategorias()
        {
            int totalCategorias = dgCategorias.Items.Count;
            txtTotalCategorias.Text = $"Total: {totalCategorias} categorías";
        }

        /// <summary>
        /// Busca categorías según el término ingresado
        /// </summary>
        private void BuscarCategorias()
        {
            try
            {
                string termino = txtBuscar.Text.Trim();

                if (string.IsNullOrEmpty(termino))
                {
                    // Si no hay término, mostrar todas las categorías
                    CargarCategorias();
                    return;
                }

                // Buscar categorías
                var categoriasFiltradas = _categoriaService.Buscar(termino);

                dgCategorias.ItemsSource = categoriasFiltradas;
                ActualizarContadorCategorias();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al buscar categorías con término '{txtBuscar.Text}'");
                MessageBox.Show("Error al buscar categorías: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Eventos de controles

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            BuscarCategorias();
        }

        private void txtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuscarCategorias();
            }
        }

        private void dgCategorias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _categoriaSeleccionada = dgCategorias.SelectedItem as Categoria;
        }

        private void btnNuevaCategoria_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Abrir ventana de nueva categoría
                var nuevaVentana = new CategoriaDetailWindow();
                nuevaVentana.Owner = Window.GetWindow(this);

                // Si se cerró con resultado positivo, recargar categorías
                if (nuevaVentana.ShowDialog() == true)
                {
                    CargarCategorias();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al abrir ventana de nueva categoría");
                MessageBox.Show("Error al abrir la ventana de nueva categoría: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener la categoría seleccionada
                var categoria = ((FrameworkElement)sender).DataContext as Categoria;

                if (categoria != null)
                {
                    // Abrir ventana de edición
                    var editarVentana = new CategoriaDetailWindow(categoria);
                    editarVentana.Owner = Window.GetWindow(this);

                    // Si se cerró con resultado positivo, recargar categorías
                    if (editarVentana.ShowDialog() == true)
                    {
                        CargarCategorias();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al abrir ventana de edición de categoría");
                MessageBox.Show("Error al abrir la ventana de edición: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener la categoría seleccionada
                var categoria = ((FrameworkElement)sender).DataContext as Categoria;

                if (categoria != null)
                {
                    // Confirmar eliminación
                    var resultado = MessageBox.Show(
                        $"¿Está seguro que desea eliminar la categoría '{categoria.Nombre}'?",
                        "Confirmar eliminación",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (resultado == MessageBoxResult.Yes)
                    {
                        // Eliminar categoría
                        string errorMessage;
                        bool eliminado = _categoriaService.EliminarCategoria(categoria.IdCategoria, out errorMessage);

                        if (eliminado)
                        {
                            MessageBox.Show("Categoría eliminada con éxito.",
                                "Información", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Recargar categorías
                            CargarCategorias();
                        }
                        else
                        {
                            MessageBox.Show($"Error al eliminar la categoría: {errorMessage}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al eliminar categoría");
                MessageBox.Show("Error al eliminar la categoría: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}