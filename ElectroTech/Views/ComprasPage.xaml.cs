using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ElectroTech.Views.Compras
{
    /// <summary>
    /// Lógica de interacción para ComprasPage.xaml
    /// </summary>
    public partial class ComprasPage : Page
    {
        private readonly CompraService _compraService;
        private List<Compra> _compras;
        private Compra _compraSeleccionada;

        /// <summary>
        /// Constructor de la página de compras
        /// </summary>
        public ComprasPage()
        {
            InitializeComponent();

            // Inicializar servicios
            _compraService = new CompraService();

            // Cargar datos iniciales
            CargarCompras();

            // Seleccionar "Todos los estados" por defecto
            cmbEstado.SelectedIndex = 0;

            // Eventos
            this.Loaded += ComprasPage_Loaded;
        }

        /// <summary>
        /// Evento que se ejecuta cuando la página se ha cargado
        /// </summary>
        private void ComprasPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Refrescar datos
                CargarCompras();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar la página de Compras");
                MessageBox.Show("Error al cargar las compras: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Carga la lista de compras
        /// </summary>
        private void CargarCompras()
        {
            try
            {
                _compras = _compraService.ObtenerTodas();

                // Asignar a la vista
                dgCompras.ItemsSource = _compras;

                // Actualizar contador
                ActualizarContadorCompras();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar compras");
                MessageBox.Show("Error al cargar las órdenes de compra: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Actualiza el contador de compras
        /// </summary>
        private void ActualizarContadorCompras()
        {
            int totalCompras = dgCompras.Items.Count;
            txtTotalCompras.Text = $"Total: {totalCompras} órdenes de compra";
        }

        /// <summary>
        /// Filtra las compras por estado
        /// </summary>
        private void FiltrarPorEstado(string estado)
        {
            try
            {
                if (string.IsNullOrEmpty(estado)) // "Todos los estados"
                {
                    // Mostrar todas las compras
                    CargarCompras();
                }
                else
                {
                    // Filtrar por estado
                    var comprasFiltradas = _compraService.ObtenerPorEstado(estado[0]);
                    dgCompras.ItemsSource = comprasFiltradas;
                    ActualizarContadorCompras();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al filtrar compras por estado {estado}");
                MessageBox.Show("Error al filtrar las compras: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Busca compras según el término ingresado
        /// </summary>
        private void BuscarCompras()
        {
            try
            {
                string termino = txtBuscar.Text.Trim();

                if (string.IsNullOrEmpty(termino))
                {
                    // Si no hay término, mostrar todos o filtrar por estado
                    if (cmbEstado.SelectedIndex == 0)
                    {
                        CargarCompras();
                    }
                    else
                    {
                        string estado = ((ComboBoxItem)cmbEstado.SelectedItem).Tag.ToString();
                        FiltrarPorEstado(estado);
                    }
                    return;
                }

                // Buscar compras
                var comprasFiltradas = _compraService.Buscar(termino);

                // Si hay un estado seleccionado, aplicar ese filtro también
                if (cmbEstado.SelectedIndex > 0)
                {
                    string estado = ((ComboBoxItem)cmbEstado.SelectedItem).Tag.ToString();
                    comprasFiltradas = comprasFiltradas.Where(c => c.Estado == estado[0]).ToList();
                }

                dgCompras.ItemsSource = comprasFiltradas;
                ActualizarContadorCompras();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al buscar compras con término '{txtBuscar.Text}'");
                MessageBox.Show("Error al buscar compras: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Eventos de controles

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            BuscarCompras();
        }

        private void txtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuscarCompras();
            }
        }

        private void cmbEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbEstado.SelectedItem != null)
            {
                string estado = ((ComboBoxItem)cmbEstado.SelectedItem).Tag.ToString();
                FiltrarPorEstado(estado);
            }
        }

        private void dgCompras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _compraSeleccionada = dgCompras.SelectedItem as Compra;
        }

        private void btnNuevaCompra_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Abrir ventana de nueva compra
                var nuevaVentana = new CompraDetailWindow();
                nuevaVentana.Owner = Window.GetWindow(this);

                // Si se cerró con resultado positivo, recargar compras
                if (nuevaVentana.ShowDialog() == true)
                {
                    CargarCompras();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al abrir ventana de nueva compra");
                MessageBox.Show("Error al abrir la ventana de nueva compra: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnVerCompra_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener la compra seleccionada
                var compra = ((FrameworkElement)sender).DataContext as Compra;

                if (compra != null)
                {
                    // Abrir ventana de visualización (modo solo lectura)
                    var verVentana = new CompraDetailWindow(compra, true);
                    verVentana.Owner = Window.GetWindow(this);
                    verVentana.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al abrir ventana de visualización de compra");
                MessageBox.Show("Error al abrir la ventana de visualización: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEditarCompra_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener la compra seleccionada
                var compra = ((FrameworkElement)sender).DataContext as Compra;

                if (compra != null)
                {
                    // Verificar que la compra esté en estado Pendiente (solo las pendientes se pueden editar)
                    if (compra.Estado != 'P')
                    {
                        MessageBox.Show("Solo se pueden editar las compras en estado Pendiente.",
                            "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    // Abrir ventana de edición
                    var editarVentana = new CompraDetailWindow(compra, false);
                    editarVentana.Owner = Window.GetWindow(this);

                    // Si se cerró con resultado positivo, recargar compras
                    if (editarVentana.ShowDialog() == true)
                    {
                        CargarCompras();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al abrir ventana de edición de compra");
                MessageBox.Show("Error al abrir la ventana de edición: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}