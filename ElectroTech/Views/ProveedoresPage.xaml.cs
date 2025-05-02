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
    /// Lógica de interacción para ProveedoresPage.xaml
    /// </summary>
    public partial class ProveedoresPage : Page
    {
        private readonly ProveedorService _proveedorService;
        private List<Proveedor> _proveedores;
        private Proveedor _proveedorSeleccionado;

        /// <summary>
        /// Constructor de la página de proveedores
        /// </summary>
        public ProveedoresPage()
        {
            InitializeComponent();

            // Inicializar servicios
            _proveedorService = new ProveedorService();

            // Cargar datos iniciales
            CargarProveedores();

            // Eventos
            this.Loaded += ProveedoresPage_Loaded;
        }

        /// <summary>
        /// Evento que se ejecuta cuando la página se ha cargado
        /// </summary>
        private void ProveedoresPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Refrescar datos
                CargarProveedores();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar la página de Proveedores");
                MessageBox.Show("Error al cargar los proveedores: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Carga la lista de proveedores
        /// </summary>
        private void CargarProveedores()
        {
            try
            {
                _proveedores = _proveedorService.ObtenerTodos();

                // Asignar a la vista
                dgProveedores.ItemsSource = _proveedores;

                // Actualizar contador
                ActualizarContadorProveedores();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar proveedores");
                MessageBox.Show("Error al cargar los proveedores: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Actualiza el contador de proveedores
        /// </summary>
        private void ActualizarContadorProveedores()
        {
            int totalProveedores = dgProveedores.Items.Count;
            txtTotalProveedores.Text = $"Total: {totalProveedores} proveedores";
        }

        /// <summary>
        /// Busca proveedores según el término ingresado
        /// </summary>
        private void BuscarProveedores()
        {
            try
            {
                string termino = txtBuscar.Text.Trim();

                if (string.IsNullOrEmpty(termino))
                {
                    // Si no hay término, mostrar todos los proveedores
                    CargarProveedores();
                    return;
                }

                // Buscar proveedores
                var proveedoresFiltrados = _proveedorService.Buscar(termino);

                dgProveedores.ItemsSource = proveedoresFiltrados;
                ActualizarContadorProveedores();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al buscar proveedores con término '{txtBuscar.Text}'");
                MessageBox.Show("Error al buscar proveedores: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Eventos de controles

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            BuscarProveedores();
        }

        private void txtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuscarProveedores();
            }
        }

        private void dgProveedores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _proveedorSeleccionado = dgProveedores.SelectedItem as Proveedor;
        }

        private void btnNuevoProveedor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Abrir ventana de nuevo proveedor
                var nuevaVentana = new ProveedorDetailWindow();
                nuevaVentana.Owner = Window.GetWindow(this);

                // Si se cerró con resultado positivo, recargar proveedores
                if (nuevaVentana.ShowDialog() == true)
                {
                    CargarProveedores();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al abrir ventana de nuevo proveedor");
                MessageBox.Show("Error al abrir la ventana de nuevo proveedor: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener el proveedor seleccionado
                var proveedor = ((FrameworkElement)sender).DataContext as Proveedor;

                if (proveedor != null)
                {
                    // Abrir ventana de edición
                    var editarVentana = new ProveedorDetailWindow(proveedor);
                    editarVentana.Owner = Window.GetWindow(this);

                    // Si se cerró con resultado positivo, recargar proveedores
                    if (editarVentana.ShowDialog() == true)
                    {
                        CargarProveedores();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al abrir ventana de edición de proveedor");
                MessageBox.Show("Error al abrir la ventana de edición: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener el proveedor seleccionado
                var proveedor = ((FrameworkElement)sender).DataContext as Proveedor;

                if (proveedor != null)
                {
                    // Confirmar eliminación
                    var resultado = MessageBox.Show(
                        $"¿Está seguro que desea eliminar el proveedor '{proveedor.Nombre}'?",
                        "Confirmar eliminación",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (resultado == MessageBoxResult.Yes)
                    {
                        // Eliminar proveedor
                        string errorMessage;
                        bool eliminado = _proveedorService.EliminarProveedor(proveedor.IdProveedor, out errorMessage);

                        if (eliminado)
                        {
                            MessageBox.Show("Proveedor eliminado con éxito.",
                                "Información", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Recargar proveedores
                            CargarProveedores();
                        }
                        else
                        {
                            MessageBox.Show($"Error al eliminar el proveedor: {errorMessage}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al eliminar proveedor");
                MessageBox.Show("Error al eliminar el proveedor: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}