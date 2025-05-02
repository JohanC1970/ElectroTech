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
    /// Lógica de interacción para ClientesPage.xaml
    /// </summary>
    public partial class ClientesPage : Page
    {
        private readonly ClienteService _clienteService;
        private List<Cliente> _clientes;
        private Cliente _clienteSeleccionado;

        /// <summary>
        /// Constructor de la página de clientes
        /// </summary>
        public ClientesPage()
        {
            InitializeComponent();

            // Inicializar servicios
            _clienteService = new ClienteService();

            // Cargar datos iniciales
            CargarClientes();

            // Eventos
            this.Loaded += ClientesPage_Loaded;
        }

        /// <summary>
        /// Evento que se ejecuta cuando la página se ha cargado
        /// </summary>
        private void ClientesPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Refrescar datos
                CargarClientes();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar la página de Clientes");
                MessageBox.Show("Error al cargar los clientes: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Carga la lista de clientes
        /// </summary>
        private void CargarClientes()
        {
            try
            {
                _clientes = _clienteService.ObtenerTodos();

                // Asignar a la vista
                dgClientes.ItemsSource = _clientes;

                // Actualizar contador
                ActualizarContadorClientes();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar clientes");
                MessageBox.Show("Error al cargar los clientes: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Actualiza el contador de clientes
        /// </summary>
        private void ActualizarContadorClientes()
        {
            int totalClientes = dgClientes.Items.Count;
            txtTotalClientes.Text = $"Total: {totalClientes} clientes";
        }

        /// <summary>
        /// Busca clientes según el término ingresado
        /// </summary>
        private void BuscarClientes()
        {
            try
            {
                string termino = txtBuscar.Text.Trim();

                if (string.IsNullOrEmpty(termino))
                {
                    // Si no hay término, mostrar todos los clientes
                    CargarClientes();
                    return;
                }

                // Buscar clientes
                var clientesFiltrados = _clienteService.Buscar(termino);

                dgClientes.ItemsSource = clientesFiltrados;
                ActualizarContadorClientes();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al buscar clientes con término '{txtBuscar.Text}'");
                MessageBox.Show("Error al buscar clientes: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Eventos de controles

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            BuscarClientes();
        }

        private void txtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuscarClientes();
            }
        }

        private void dgClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _clienteSeleccionado = dgClientes.SelectedItem as Cliente;
        }

        private void btnNuevoCliente_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Abrir ventana de nuevo cliente
                var nuevaVentana = new ClienteDetailWindow();
                nuevaVentana.Owner = Window.GetWindow(this);

                // Si se cerró con resultado positivo, recargar clientes
                if (nuevaVentana.ShowDialog() == true)
                {
                    CargarClientes();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al abrir ventana de nuevo cliente");
                MessageBox.Show("Error al abrir la ventana de nuevo cliente: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener el cliente seleccionado
                var cliente = ((FrameworkElement)sender).DataContext as Cliente;

                if (cliente != null)
                {
                    // Abrir ventana de edición
                    var editarVentana = new ClienteDetailWindow(cliente);
                    editarVentana.Owner = Window.GetWindow(this);

                    // Si se cerró con resultado positivo, recargar clientes
                    if (editarVentana.ShowDialog() == true)
                    {
                        CargarClientes();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al abrir ventana de edición de cliente");
                MessageBox.Show("Error al abrir la ventana de edición: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener el cliente seleccionado
                var cliente = ((FrameworkElement)sender).DataContext as Cliente;

                if (cliente != null)
                {
                    // Confirmar eliminación
                    var resultado = MessageBox.Show(
                        $"¿Está seguro que desea eliminar el cliente '{cliente.NombreCompleto}'?",
                        "Confirmar eliminación",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (resultado == MessageBoxResult.Yes)
                    {
                        // Eliminar cliente
                        string errorMessage;
                        bool eliminado = _clienteService.EliminarCliente(cliente.IdCliente, out errorMessage);

                        if (eliminado)
                        {
                            MessageBox.Show("Cliente eliminado con éxito.",
                                "Información", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Recargar clientes
                            CargarClientes();
                        }
                        else
                        {
                            MessageBox.Show($"Error al eliminar el cliente: {errorMessage}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al eliminar cliente");
                MessageBox.Show("Error al eliminar el cliente: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}