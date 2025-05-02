using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ElectroTech.Views.Empleados
{
    /// <summary>
    /// Lógica de interacción para EmpleadosPage.xaml
    /// </summary>
    public partial class EmpleadosPage : Page
    {
        private readonly EmpleadoService _empleadoService;
        private List<Empleado> _empleados;
        private Empleado _empleadoSeleccionado;

        /// <summary>
        /// Constructor de la página de empleados
        /// </summary>
        public EmpleadosPage()
        {
            InitializeComponent();

            // Inicializar servicios
            _empleadoService = new EmpleadoService();

            // Cargar datos iniciales
            CargarEmpleados();

            // Eventos
            this.Loaded += EmpleadosPage_Loaded;
        }

        /// <summary>
        /// Evento que se ejecuta cuando la página se ha cargado
        /// </summary>
        private void EmpleadosPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Refrescar datos
                CargarEmpleados();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar la página de Empleados");
                MessageBox.Show("Error al cargar los empleados: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Carga la lista de empleados
        /// </summary>
        private void CargarEmpleados()
        {
            try
            {
                _empleados = _empleadoService.ObtenerTodos();

                // Asignar a la vista
                dgEmpleados.ItemsSource = _empleados;

                // Actualizar contador
                ActualizarContadorEmpleados();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar empleados");
                MessageBox.Show("Error al cargar los empleados: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Actualiza el contador de empleados
        /// </summary>
        private void ActualizarContadorEmpleados()
        {
            int totalEmpleados = dgEmpleados.Items.Count;
            txtTotalEmpleados.Text = $"Total: {totalEmpleados} empleados";
        }

        /// <summary>
        /// Busca empleados según el término ingresado
        /// </summary>
        private void BuscarEmpleados()
        {
            try
            {
                string termino = txtBuscar.Text.Trim();

                if (string.IsNullOrEmpty(termino))
                {
                    // Si no hay término, mostrar todos
                    CargarEmpleados();
                    return;
                }

                // Buscar empleados
                var empleadosFiltrados = _empleadoService.Buscar(termino);

                dgEmpleados.ItemsSource = empleadosFiltrados;
                ActualizarContadorEmpleados();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al buscar empleados con término '{txtBuscar.Text}'");
                MessageBox.Show("Error al buscar empleados: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Eventos de controles

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            BuscarEmpleados();
        }

        private void txtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuscarEmpleados();
            }
        }

        private void dgEmpleados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _empleadoSeleccionado = dgEmpleados.SelectedItem as Empleado;
        }

        private void btnNuevoEmpleado_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Abrir ventana de nuevo empleado
                var nuevaVentana = new EmpleadoDetailWindow();
                nuevaVentana.Owner = Window.GetWindow(this);

                // Si se cerró con resultado positivo, recargar empleados
                if (nuevaVentana.ShowDialog() == true)
                {
                    CargarEmpleados();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al abrir ventana de nuevo empleado");
                MessageBox.Show("Error al abrir la ventana de nuevo empleado: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener el empleado seleccionado
                var empleado = ((FrameworkElement)sender).DataContext as Empleado;

                if (empleado != null)
                {
                    // Abrir ventana de edición
                    var editarVentana = new EmpleadoDetailWindow(empleado);
                    editarVentana.Owner = Window.GetWindow(this);

                    // Si se cerró con resultado positivo, recargar empleados
                    if (editarVentana.ShowDialog() == true)
                    {
                        CargarEmpleados();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al abrir ventana de edición de empleado");
                MessageBox.Show("Error al abrir la ventana de edición: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obtener el empleado seleccionado
                var empleado = ((FrameworkElement)sender).DataContext as Empleado;

                if (empleado != null)
                {
                    // Confirmar eliminación
                    var resultado = MessageBox.Show(
                        $"¿Está seguro que desea eliminar al empleado '{empleado.NombreCompleto}'?",
                        "Confirmar eliminación",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (resultado == MessageBoxResult.Yes)
                    {
                        // Eliminar empleado
                        string errorMessage;
                        bool eliminado = _empleadoService.EliminarEmpleado(empleado.IdEmpleado, out errorMessage);

                        if (eliminado)
                        {
                            MessageBox.Show("Empleado eliminado con éxito.",
                                "Información", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Recargar empleados
                            CargarEmpleados();
                        }
                        else
                        {
                            MessageBox.Show($"Error al eliminar el empleado: {errorMessage}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al eliminar empleado");
                MessageBox.Show("Error al eliminar el empleado: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}