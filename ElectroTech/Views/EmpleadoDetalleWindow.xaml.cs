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

                // Seleccionar nivel de usuario por defecto (Paramétrico)
                cmbNivelUsuario.SelectedIndex = 1; // Paramétrico por defecto para empleados normales

                // En modo nuevo, marcar como activo por defecto
                if (_esNuevo)
                {
                    chkActivo.IsChecked = true;
                }
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
                    var usuario = _authService.ObtenerPorId(_empleado.IdUsuario);
                    if (usuario != null)
                    {
                        chkAsociarUsuario.IsChecked = true;
                        txtUsuario.Text = usuario.NombreUsuario;

                        // Seleccionar el nivel de usuario correcto
                        for (int i = 0; i < cmbNivelUsuario.Items.Count; i++)
                        {
                            ComboBoxItem item = cmbNivelUsuario.Items[i] as ComboBoxItem;
                            if (Convert.ToInt32(item.Tag) == usuario.Nivel)
                            {
                                cmbNivelUsuario.SelectedIndex = i;
                                break;
                            }
                        }
                    }
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
                OcultarError();

                // Validar campos obligatorios
                if (!ValidarCamposObligatorios())
                {
                    return;
                }

                // Validar campos de usuario si está marcado
                if (chkAsociarUsuario.IsChecked == true)
                {
                    if (!ValidarCamposUsuario())
                    {
                        return;
                    }
                }

                // Parsear valores numéricos
                if (!double.TryParse(txtSalarioBase.Text, out double salarioBase) || salarioBase <= 0)
                {
                    MostrarError("El salario base debe ser un número mayor que cero.");
                    return;
                }

                // Completar datos del empleado
                _empleado.Nombre = txtNombre.Text.Trim();
                _empleado.Apellido = txtApellido.Text.Trim();
                _empleado.TipoDocumento = ((ComboBoxItem)cmbTipoDocumento.SelectedItem).Content.ToString();
                _empleado.NumeroDocumento = txtNumeroDocumento.Text.Trim();
                _empleado.Direccion = string.IsNullOrWhiteSpace(txtDireccion.Text) ? null : txtDireccion.Text.Trim();
                _empleado.Telefono = txtTelefono.Text.Trim();
                _empleado.FechaContratacion = dpFechaContratacion.SelectedDate.Value;
                _empleado.SalarioBase = salarioBase;
                _empleado.Activo = chkActivo.IsChecked ?? true;

                // Datos de usuario si se va a crear/actualizar
                string nombreUsuario = null;
                int nivelUsuario = 0;

                if (chkAsociarUsuario.IsChecked == true)
                {
                    nombreUsuario = txtUsuario.Text.Trim();
                    nivelUsuario = Convert.ToInt32(((ComboBoxItem)cmbNivelUsuario.SelectedItem).Tag);
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
                    string mensaje = $"Empleado {(_esNuevo ? "creado" : "actualizado")} con éxito.";

                    // Si se creó un usuario, agregar información adicional
                    if (chkAsociarUsuario.IsChecked == true && _esNuevo)
                    {
                        // Generar contraseña temporal para mostrar al usuario
                        string claveInicial = GenerarClaveInicial(nombreUsuario, _empleado.NumeroDocumento);
                        mensaje += $"\n\nCredenciales de acceso:\nUsuario: {nombreUsuario}\nContraseña temporal: {claveInicial}";
                        mensaje += "\n\nPor favor, anote estas credenciales. Se recomienda cambiar la contraseña en el primer acceso.";
                    }

                    MessageBox.Show(mensaje, "Información", MessageBoxButton.OK, MessageBoxImage.Information);

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
        /// Valida los campos obligatorios del empleado
        /// </summary>
        private bool ValidarCamposObligatorios()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MostrarError("El nombre del empleado es obligatorio.");
                txtNombre.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtApellido.Text))
            {
                MostrarError("El apellido del empleado es obligatorio.");
                txtApellido.Focus();
                return false;
            }

            if (cmbTipoDocumento.SelectedItem == null)
            {
                MostrarError("Debe seleccionar un tipo de documento.");
                cmbTipoDocumento.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtNumeroDocumento.Text))
            {
                MostrarError("El número de documento es obligatorio.");
                txtNumeroDocumento.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtTelefono.Text))
            {
                MostrarError("El teléfono del empleado es obligatorio.");
                txtTelefono.Focus();
                return false;
            }

            if (dpFechaContratacion.SelectedDate == null)
            {
                MostrarError("La fecha de contratación es obligatoria.");
                dpFechaContratacion.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtSalarioBase.Text))
            {
                MostrarError("El salario base es obligatorio.");
                txtSalarioBase.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida los campos del usuario si se va a crear/actualizar
        /// </summary>
        private bool ValidarCamposUsuario()
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text))
            {
                MostrarError("Si va a asociar un usuario, debe ingresar un nombre de usuario.");
                txtUsuario.Focus();
                return false;
            }

            if (cmbNivelUsuario.SelectedItem == null)
            {
                MostrarError("Si va a asociar un usuario, debe seleccionar un nivel de usuario.");
                cmbNivelUsuario.Focus();
                return false;
            }

            // Validar que el nombre de usuario no exista (solo en modo nuevo o si cambió el nombre)
            if (_esNuevo)
            {
                if (_authService.ExisteNombreUsuario(txtUsuario.Text.Trim()))
                {
                    MostrarError("El nombre de usuario ya existe, por favor elija otro.");
                    txtUsuario.Focus();
                    return false;
                }
            }
            else if (_empleado.IdUsuario > 0)
            {
                // En modo edición, verificar si cambió el nombre de usuario
                var usuarioActual = _authService.ObtenerPorId(_empleado.IdUsuario);
                if (usuarioActual != null && usuarioActual.NombreUsuario != txtUsuario.Text.Trim())
                {
                    if (_authService.ExisteNombreUsuario(txtUsuario.Text.Trim()))
                    {
                        MostrarError("El nombre de usuario ya existe, por favor elija otro.");
                        txtUsuario.Focus();
                        return false;
                    }
                }
            }

            // Validar formato del nombre de usuario
            if (!ValidarFormatoUsuario(txtUsuario.Text.Trim()))
            {
                MostrarError("El nombre de usuario debe tener entre 3 y 50 caracteres y solo puede contener letras, números y guiones bajos.");
                txtUsuario.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida el formato del nombre de usuario
        /// </summary>
        private bool ValidarFormatoUsuario(string nombreUsuario)
        {
            if (string.IsNullOrEmpty(nombreUsuario) || nombreUsuario.Length < 3 || nombreUsuario.Length > 50)
            {
                return false;
            }

            // Solo letras, números y guiones bajos
            Regex regex = new Regex(@"^[a-zA-Z0-9_]+$");
            return regex.IsMatch(nombreUsuario);
        }

        /// <summary>
        /// Genera una clave inicial para el usuario
        /// </summary>
        private string GenerarClaveInicial(string nombreUsuario, string numeroDocumento)
        {
            string inicialUsuario = nombreUsuario.Length >= 3 ? nombreUsuario.Substring(0, 3) : nombreUsuario;
            string inicialDocumento = numeroDocumento.Length >= 3 ? numeroDocumento.Substring(numeroDocumento.Length - 3) : numeroDocumento;
            return $"{inicialUsuario}{inicialDocumento}123";
        }

        /// <summary>
        /// Muestra un mensaje de error en la interfaz
        /// </summary>
        private void MostrarError(string mensaje)
        {
            txtError.Text = mensaje;
            borderError.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Oculta el mensaje de error
        /// </summary>
        private void OcultarError()
        {
            borderError.Visibility = Visibility.Collapsed;
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
            var textBox = sender as TextBox;
            string fullText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            // Permitir solo números y un punto decimal
            Regex regex = new Regex(@"^\d*\.?\d*$");
            e.Handled = !regex.IsMatch(fullText);
        }

        private void chkAsociarUsuario_Checked(object sender, RoutedEventArgs e)
        {
            // Mostrar campos de usuario
            pnlUsuario.Visibility = Visibility.Visible;
            pnlNivelUsuario.Visibility = Visibility.Visible;
            txtInfoPassword.Visibility = Visibility.Visible;

            // Generar sugerencia de nombre de usuario si los campos están llenos
            if (!string.IsNullOrEmpty(txtNombre.Text) && !string.IsNullOrEmpty(txtApellido.Text))
            {
                string sugerencia = GenerarSugerenciaNombreUsuario(txtNombre.Text, txtApellido.Text);
                if (string.IsNullOrEmpty(txtUsuario.Text))
                {
                    txtUsuario.Text = sugerencia;
                }
            }
        }

        private void chkAsociarUsuario_Unchecked(object sender, RoutedEventArgs e)
        {
            // Ocultar campos de usuario
            pnlUsuario.Visibility = Visibility.Collapsed;
            pnlNivelUsuario.Visibility = Visibility.Collapsed;
            txtInfoPassword.Visibility = Visibility.Collapsed;

            // Limpiar campos
            txtUsuario.Text = "";
            cmbNivelUsuario.SelectedIndex = 1; // Paramétrico por defecto
        }

        #endregion

        #region Métodos de validación

        /// <summary>
        /// Verifica si un texto es un número de teléfono válido
        /// </summary>
        private bool EsTelefonoValido(string texto)
        {
            // Permitir dígitos, +, -, () y espacios
            Regex regex = new Regex(@"^[0-9\+\-\(\)\s]*$");
            return regex.IsMatch(texto);
        }

        /// <summary>
        /// Genera una sugerencia de nombre de usuario basada en nombre y apellido
        /// </summary>
        private string GenerarSugerenciaNombreUsuario(string nombre, string apellido)
        {
            // Tomar primera letra del nombre y hasta 7 caracteres del apellido
            string inicial = nombre.Length > 0 ? nombre.Substring(0, 1).ToLower() : "";
            string apellidoLimpio = apellido.Replace(" ", "").ToLower();
            string apellidoCorto = apellidoLimpio.Length > 7 ? apellidoLimpio.Substring(0, 7) : apellidoLimpio;

            return inicial + apellidoCorto;
        }

        #endregion
    }
}