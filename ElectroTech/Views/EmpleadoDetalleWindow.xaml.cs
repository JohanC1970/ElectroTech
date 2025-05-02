using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ElectroTech.Views.Empleados
{
    /// <summary>
    /// Lógica de interacción para EmpleadoDetailWindow.xaml
    /// </summary>
    public partial class EmpleadoDetailWindow : Window
    {
        private readonly EmpleadoService _empleadoService;
        private readonly AuthService _authService;
        private readonly Empleado _empleado;
        private readonly bool _esNuevo;
        private bool _crearUsuario;

        /// <summary>
        /// Constructor para un nuevo empleado
        /// </summary>
        public EmpleadoDetailWindow()
        {
            InitializeComponent();

            _empleadoService = new EmpleadoService();
            _authService = new AuthService();
            _empleado = new Empleado();
            _esNuevo = true;
            _crearUsuario = false;

            // Configurar ventana para nuevo empleado
            txtTitulo.Text = "Nuevo Empleado";
            ConfigurarVentana();
        }

        /// <summary>
        /// Constructor para editar un empleado existente
        /// </summary>
        public EmpleadoDetailWindow(Empleado empleado)
        {
            InitializeComponent();

            _empleadoService = new EmpleadoService();
            _authService = new AuthService();
            _empleado = empleado;
            _esNuevo = false;
            _crearUsuario = false;

            // Configurar ventana para edición
            txtTitulo.Text = "Editar Empleado";
            pnlCamposEdicion.Visibility = Visibility.Visible;

            ConfigurarVentana();
            CargarDatosEmpleado();
        }

        /// <summary>
        /// Configura la ventana y carga datos iniciales
        /// </summary>
        private void ConfigurarVentana()
        {
            try
            {
                // Configurar fecha actual en el DatePicker
                dpFechaContratacion.SelectedDate = DateTime.Now;

                // Seleccionar el primer tipo de documento
                cmbTipoDocumento.SelectedIndex = 0;

                // Seleccionar nivel de usuario por defecto
                cmbNivelUsuario.SelectedIndex = 2; // Esporádico
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al configurar ventana de empleado");
                MessageBox.Show("Error al configurar la ventana: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Carga los datos del empleado en el formulario (modo edición)
        /// </summary>
        private void CargarDatosEmpleado()
        {
            try
            {
                // Cargar datos básicos
                txtNombre.Text = _empleado.Nombre;
                txtApellido.Text = _empleado.Apellido;
                txtDireccion.Text = _empleado.Direccion;
                txtTelefono.Text = _empleado.Telefono;
                txtNumeroDocumento.Text = _empleado.NumeroDocumento;
                dpFechaContratacion.SelectedDate = _empleado.FechaContratacion;
                txtSalarioBase.Text = _empleado.SalarioBase.ToString("0.00");

                // Seleccionar tipo de documento
                for (int i = 0; i < cmbTipoDocumento.Items.Count; i++)
                {
                    ComboBoxItem item = cmbTipoDocumento.Items[i] as ComboBoxItem;
                    if (item.Content.ToString() == _empleado.TipoDocumento)
                    {
                        cmbTipoDocumento.SelectedIndex = i;
                        break;
                    }
                }

                // Estado
                chkActivo.IsChecked = _empleado.Activo;

                // Verificar si tiene usuario asociado
                if (_empleado.IdUsuario > 0)
                {
                    // Aquí se cargaría la información del usuario asociado
                    // y se mostrarían los controles correspondientes
                    chkAsociarUsuario.IsChecked = true;
                    // Esto debería mostrar y cargar los campos de usuario
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar datos del empleado");
                MessageBox.Show("Error al cargar los datos del empleado: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Valida y guarda el empleado
        /// </summary>
        private void GuardarEmpleado()
        {
            try
            {
                // Ocultar mensaje de error previo
                txtError.Visibility = Visibility.Collapsed;

                // Validar campos obligatorios
                if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                    string.IsNullOrWhiteSpace(txtApellido.Text) ||
                    string.IsNullOrWhiteSpace(txtNumeroDocumento.Text) ||
                    string.IsNullOrWhiteSpace(txtTelefono.Text) ||
                    string.IsNullOrWhiteSpace(txtSalarioBase.Text) ||
                    dpFechaContratacion.SelectedDate == null)
                {
                    MostrarError("Por favor, complete todos los campos obligatorios.");
                    return;
                }

                // Validar campos adicionales si se va a crear usuario
                if (_crearUsuario)
                {
                    if (string.IsNullOrWhiteSpace(txtUsuario.Text))
                    {
                        MostrarError("Si va a asociar un usuario, debe ingresar un nombre de usuario.");
                        return;
                    }

                    // Validar que el nombre de usuario no exista
                    if (_esNuevo && _authService.ExisteNombreUsuario(txtUsuario.Text))
                    {
                        MostrarError("El nombre de usuario ya existe, por favor elija otro.");
                        return;
                    }
                }

                // Parsear valores numéricos
                double salarioBase;
                if (!double.TryParse(txtSalarioBase.Text, out salarioBase) || salarioBase <= 0)
                {
                    MostrarError("El salario base debe ser un número mayor que cero.");
                    return;
                }

                // Completar datos del empleado
                _empleado.Nombre = txtNombre.Text.Trim();
                _empleado.Apellido = txtApellido.Text.Trim();
                _empleado.TipoDocumento = ((ComboBoxItem)cmbTipoDocumento.SelectedItem).Content.ToString();
                _empleado.NumeroDocumento = txtNumeroDocumento.Text.Trim();
                _empleado.Direccion = txtDireccion.Text.Trim();
                _empleado.Telefono = txtTelefono.Text.Trim();
                _empleado.FechaContratacion = dpFechaContratacion.SelectedDate.Value;
                _empleado.SalarioBase = salarioBase;

                // Datos de usuario si se va a crear
                string nombreUsuario = _crearUsuario ? txtUsuario.Text.Trim() : null;
                int nivelUsuario = _crearUsuario ? Convert.ToInt32(((ComboBoxItem)cmbNivelUsuario.SelectedItem).Tag) : 0;

                // Valores específicos según modo
                if (!_esNuevo)
                {
                    _empleado.Activo = chkActivo.IsChecked ?? true;
                }

                // Guardar empleado
                string errorMessage;
                bool resultado;

                if (_esNuevo)
                {
                    resultado = _empleadoService.CrearEmpleado(_empleado, nombreUsuario, nivelUsuario, out errorMessage);
                }
                else
                {
                    resultado = _empleadoService.ActualizarEmpleado(_empleado, nombreUsuario, nivelUsuario, out errorMessage);
                }

                // Verificar resultado
                if (resultado)
                {
                    MessageBox.Show($"Empleado {(_esNuevo ? "creado" : "actualizado")} con éxito.",
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Cerrar ventana con resultado positivo
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MostrarError($"Error al {(_esNuevo ? "crear" : "actualizar")} el empleado: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al guardar empleado {(_esNuevo ? "nuevo" : "existente")}");
                MostrarError("Error al guardar el empleado: " + ex.Message);
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
            GuardarEmpleado();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Cerrar ventana sin guardar
            DialogResult = false;
            Close();
        }

        private void txtTelefono_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Solo permitir números y símbolos de teléfono
            e.Handled = !EsTelefonoValido(e.Text);
        }

        private void txtSalarioBase_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Solo permitir números y punto decimal
            e.Handled = !EsNumeroDecimalValido(e.Text);
        }

        private void chkAsociarUsuario_Checked(object sender, RoutedEventArgs e)
        {
            // Mostrar campos de usuario
            pnlUsuario.Visibility = Visibility.Visible;
            pnlNivelUsuario.Visibility = Visibility.Visible;
            _crearUsuario = true;
        }

        private void chkAsociarUsuario_Unchecked(object sender, RoutedEventArgs e)
        {
            // Ocultar campos de usuario
            pnlUsuario.Visibility = Visibility.Collapsed;
            pnlNivelUsuario.Visibility = Visibility.Collapsed;
            _crearUsuario = false;
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
        /// Verifica si un texto es un número de teléfono válido
        /// </summary>
        private bool EsTelefonoValido(string texto)
        {
            // Permitir dígitos, +, -, () y espacios
            Regex regex = new Regex(@"^[0-9\+\-\(\)\s]*$");
            return regex.IsMatch(texto);
        }

        #endregion
    }
}