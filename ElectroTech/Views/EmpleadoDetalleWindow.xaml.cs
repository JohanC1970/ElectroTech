using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using static ElectroTech.Helpers.PermisosHelper;

namespace ElectroTech.Views.Empleados
{
    /// <summary>
    /// Lógica de interacción para EmpleadoDetailWindow.xaml
    /// </summary>
    public partial class EmpleadoDetailWindow : Window
    {
        private readonly EmpleadoService _empleadoService;
        private readonly AuthService _authService;
        private Empleado _empleadoActual;
        private bool _esEdicion;

        /// <summary>
        /// Constructor para crear un nuevo empleado
        /// </summary>
        public EmpleadoDetailWindow()
        {
            InitializeComponent();

            _empleadoService = new EmpleadoService();
            _authService = new AuthService();
            _empleadoActual = new Empleado();
            _esEdicion = false;

            ConfigurarFormulario();
            ConfigurarEventos();
        }

        /// <summary>
        /// Constructor para editar un empleado existente
        /// </summary>
        /// <param name="empleado">Empleado a editar</param>
        public EmpleadoDetailWindow(Empleado empleado)
        {
            InitializeComponent();

            _empleadoService = new EmpleadoService();
            _authService = new AuthService();
            _empleadoActual = empleado;
            _esEdicion = true;

            ConfigurarFormulario();
            ConfigurarEventos();
            CargarDatosEmpleado();
        }

        /// <summary>
        /// Configura el formulario inicial
        /// </summary>
        private void ConfigurarFormulario()
        {
            if (_esEdicion)
            {
                txtTitulo.Text = "Editar Empleado";
                chkCrearUsuario.Content = "Gestionar usuario del sistema";
                chkCrearUsuario.ToolTip = "Marque esta opción para crear o actualizar el usuario asociado";
                txtPassword.Visibility = Visibility.Collapsed;
                
                var lblPassword = FindName("lblPassword") as FrameworkElement;
                lblPassword?.SetValue(VisibilityProperty, Visibility.Collapsed);

            }
            else
            {
                txtTitulo.Text = "Nuevo Empleado";
                dpFechaContratacion.SelectedDate = DateTime.Now;
                chkCrearUsuario.IsChecked = true;
                chkCrearUsuario.IsEnabled = false;
            }

            // Enfocar el primer campo
            txtNombre.Focus();
        }

        /// <summary>
        /// Configura los eventos adicionales
        /// </summary>
        private void ConfigurarEventos()
        {
            // Evento para generar nombre de usuario automáticamente
            txtNombre.LostFocus += GenerarNombreUsuarioAutomatico;
            txtApellido.LostFocus += GenerarNombreUsuarioAutomatico;

            // Validación numérica para salario
            txtSalarioBase.PreviewTextInput += ValidarEntradaNumerica;

            // Validación numérica para documento
            txtNumeroDocumento.PreviewTextInput += ValidarEntradaNumerica;
        }

        /// <summary>
        /// Carga los datos del empleado en modo edición
        /// </summary>
        private void CargarDatosEmpleado()
        {
            if (_empleadoActual == null) return;

            try
            {
                // Datos personales
                txtNombre.Text = _empleadoActual.Nombre;
                txtApellido.Text = _empleadoActual.Apellido;
                txtNumeroDocumento.Text = _empleadoActual.NumeroDocumento;
                txtDireccion.Text = _empleadoActual.Direccion;
                txtTelefono.Text = _empleadoActual.Telefono;

                // Seleccionar tipo de documento
                foreach (ComboBoxItem item in cmbTipoDocumento.Items)
                {
                    if (item.Content.ToString() == _empleadoActual.TipoDocumento)
                    {
                        cmbTipoDocumento.SelectedItem = item;
                        break;
                    }
                }

                // Datos laborales
                dpFechaContratacion.SelectedDate = _empleadoActual.FechaContratacion;
                txtSalarioBase.Text = _empleadoActual.SalarioBase.ToString("F2");
                chkActivo.IsChecked = _empleadoActual.Activo;

                // Si tiene usuario asociado, mostrar la opción
                if (_empleadoActual.IdUsuario > 0)
                {
                    chkCrearUsuario.IsChecked = true;
                    CargarDatosUsuario();
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
        /// Carga los datos del usuario asociado (en modo edición)
        /// </summary>
        private void CargarDatosUsuario()
        {
            try
            {
                if (_empleadoActual.IdUsuario > 0)
                {
                    var usuario = _authService.ObtenerPorId(_empleadoActual.IdUsuario);
                    if (usuario != null)
                    {
                        txtNombreUsuario.Text = usuario.NombreUsuario;
                        txtCorreoUsuario.Text = usuario.Correo;

                        // Seleccionar nivel de usuario
                        foreach (ComboBoxItem item in cmbNivelUsuario.Items)
                        {
                            if (Convert.ToInt32(item.Tag) == usuario.Nivel)
                            {
                                cmbNivelUsuario.SelectedItem = item;
                                break;
                            }
                        }

                        // Seleccionar estado de usuario
                        foreach (ComboBoxItem item in cmbEstadoUsuario.Items)
                        {
                            if (item.Tag.ToString() == usuario.Estado.ToString())
                            {
                                cmbEstadoUsuario.SelectedItem = item;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar datos del usuario");
            }
        }

        /// <summary>
        /// Genera automáticamente un nombre de usuario basado en nombre y apellido
        /// </summary>
        private void GenerarNombreUsuarioAutomatico(object sender, RoutedEventArgs e)
        {
            if (chkCrearUsuario.IsChecked == true && string.IsNullOrWhiteSpace(txtNombreUsuario.Text))
            {
                string nombre = txtNombre.Text.Trim();
                string apellido = txtApellido.Text.Trim();

                if (!string.IsNullOrEmpty(nombre) && !string.IsNullOrEmpty(apellido))
                {
                    // Generar nombre de usuario: primera letra del nombre + apellido
                    string nombreUsuario = (nombre.Substring(0, 1) + apellido).ToLower();

                    // Limpiar caracteres especiales
                    nombreUsuario = Regex.Replace(nombreUsuario, @"[^a-z0-9]", "");

                    txtNombreUsuario.Text = nombreUsuario;
                }
            }

        }

        /// <summary>
        /// Valida que solo se ingresen números en campos numéricos
        /// </summary>
        private void ValidarEntradaNumerica(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox == txtSalarioBase)
            {
                // Permitir números y punto decimal
                e.Handled = !Regex.IsMatch(e.Text, @"^[0-9.]+$") ||
                           (e.Text == "." && textBox.Text.Contains("."));
            }
            else
            {
                // Solo números
                e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
            }
        }

        /// <summary>
        /// Evento del checkbox para crear usuario
        /// </summary>
        private void chkCrearUsuario_Checked(object sender, RoutedEventArgs e)
        {
            GenerarNombreUsuarioAutomatico(sender, e);
        }

        /// <summary>
        /// Evento del checkbox para no crear usuario
        /// </summary>
        private void chkCrearUsuario_Unchecked(object sender, RoutedEventArgs e)
        {
            txtNombreUsuario.Clear();
            txtCorreoUsuario.Clear();
            cmbNivelUsuario.SelectedIndex = 1; // Paramétrico por defecto
            cmbEstadoUsuario.SelectedIndex = 0; // Activo por defecto
        }

        /// <summary>
        /// Valida los datos del formulario
        /// </summary>
        /// <returns>True si los datos son válidos, False en caso contrario</returns>
        private bool ValidarFormulario()
        {
            // Validaciones de Empleado (sin cambios)
            if (string.IsNullOrWhiteSpace(txtNombre.Text)) { MessageBox.Show("El nombre es obligatorio."); txtNombre.Focus(); return false; }
            if (string.IsNullOrWhiteSpace(txtApellido.Text)) { MessageBox.Show("El apellido es obligatorio."); txtApellido.Focus(); return false; }
            if (string.IsNullOrWhiteSpace(txtNumeroDocumento.Text)) { MessageBox.Show("El documento es obligatorio."); txtNumeroDocumento.Focus(); return false; }
            if (string.IsNullOrWhiteSpace(txtTelefono.Text)) { MessageBox.Show("El teléfono es obligatorio."); txtTelefono.Focus(); return false; }
            if (!dpFechaContratacion.SelectedDate.HasValue) { MessageBox.Show("La fecha de contratación es obligatoria."); dpFechaContratacion.Focus(); return false; }
            if (dpFechaContratacion.SelectedDate.Value > DateTime.Now) { MessageBox.Show("La fecha de contratación no puede ser futura."); dpFechaContratacion.Focus(); return false; }
            if (!double.TryParse(txtSalarioBase.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out double salario) || salario <= 0) { MessageBox.Show("El salario base debe ser un número mayor que cero."); txtSalarioBase.Focus(); return false; }

            // Validaciones de Usuario (si está marcado)
            if (chkCrearUsuario.IsChecked == true)
            {
                if (string.IsNullOrWhiteSpace(txtNombreUsuario.Text)) { MessageBox.Show("El nombre de usuario es obligatorio."); txtNombreUsuario.Focus(); return false; }
                if (txtNombreUsuario.Text.Length < 3) { MessageBox.Show("El nombre de usuario debe tener al menos 3 caracteres."); txtNombreUsuario.Focus(); return false; }
                if (!Regex.IsMatch(txtNombreUsuario.Text, @"^[a-zA-Z0-9_]+$")) { MessageBox.Show("El nombre de usuario solo puede contener letras, números y guiones bajos."); txtNombreUsuario.Focus(); return false; }
                if (char.IsDigit(txtNombreUsuario.Text[0])) { MessageBox.Show("El nombre de usuario no puede iniciar con un número."); txtNombreUsuario.Focus(); return false; }
                if (string.IsNullOrWhiteSpace(txtCorreoUsuario.Text)) { MessageBox.Show("El correo es obligatorio."); txtCorreoUsuario.Focus(); return false; }
                if (!ValidarFormatoCorreo(txtCorreoUsuario.Text)) { MessageBox.Show("El formato del correo no es válido."); txtCorreoUsuario.Focus(); return false; }
                if (cmbNivelUsuario.SelectedItem == null) { MessageBox.Show("Debe seleccionar un nivel."); cmbNivelUsuario.Focus(); return false; }
                if (cmbEstadoUsuario.SelectedItem == null) { MessageBox.Show("Debe seleccionar un estado."); cmbEstadoUsuario.Focus(); return false; }

                // *** NUEVA VALIDACIÓN: Contraseña (solo si es nuevo) ***
                if (!_esEdicion && string.IsNullOrWhiteSpace(txtPassword.Password))
                {
                    MessageBox.Show("La contraseña inicial es obligatoria para nuevos usuarios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPassword.Focus();
                    return false;
                }
                if (!_esEdicion && txtPassword.Password.Length < 6) // Ejemplo: mínimo 6 caracteres
                {
                    MessageBox.Show("La contraseña debe tener al menos 6 caracteres.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPassword.Focus();
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Obtiene los datos del empleado desde el formulario
        /// </summary>
        /// <returns>Objeto Empleado con los datos del formulario</returns>
        private Empleado ObtenerDatosEmpleado()
        {
            // Tu código actual parece OK
            var empleado = new Empleado
            {
                IdEmpleado = _empleadoActual?.IdEmpleado ?? 0,
                TipoDocumento = ((ComboBoxItem)cmbTipoDocumento.SelectedItem).Content.ToString(),
                NumeroDocumento = txtNumeroDocumento.Text.Trim(),
                Nombre = txtNombre.Text.Trim(),
                Apellido = txtApellido.Text.Trim(),
                Direccion = string.IsNullOrWhiteSpace(txtDireccion.Text) ? null : txtDireccion.Text.Trim(),
                Telefono = txtTelefono.Text.Trim(),
                FechaContratacion = dpFechaContratacion.SelectedDate.Value,
                SalarioBase = double.Parse(txtSalarioBase.Text, NumberStyles.Any, CultureInfo.CurrentCulture),
                IdUsuario = _empleadoActual?.IdUsuario ?? 0,
                Activo = chkActivo.IsChecked == true
            };
            return empleado;
        }

        /// <summary>
        /// Obtiene los datos del usuario desde el formulario
        /// </summary>
        /// <returns>Objeto Usuario con los datos del formulario</returns>
        // En EmpleadoDetalleWindow.xaml.cs

        private Usuario ObtenerDatosUsuario()
        {
            if (chkCrearUsuario.IsChecked != true) return null;

            // Validar que los ComboBox tengan un item seleccionado
            if (cmbNivelUsuario.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar un nivel para el usuario.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null; // Indica que hay un error de validación
            }
            if (cmbEstadoUsuario.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar un estado para el usuario.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null; // Indica que hay un error de validación
            }

            var usuario = new Usuario
            {
                // Si es edición y el empleado ya tiene un IdUsuario, lo usamos.
                // Si es creación o el empleado no tiene IdUsuario, se asignará al crear/asociar.
                IdUsuario = (_esEdicion && _empleadoActual != null) ? _empleadoActual.IdUsuario : 0,
                NombreUsuario = txtNombreUsuario.Text.Trim(),
                // Asegúrate de que el Tag de ComboBoxItem sea el valor numérico/char correcto
                Nivel = Convert.ToInt32(((ComboBoxItem)cmbNivelUsuario.SelectedItem).Tag),
                NombreCompleto = $"{txtNombre.Text.Trim()} {txtApellido.Text.Trim()}",
                Correo = txtCorreoUsuario.Text.Trim(),
                Estado = ((ComboBoxItem)cmbEstadoUsuario.SelectedItem).Tag.ToString()[0],
                // Clave, FechaCreacion, UltimaConexion se manejan en Servicio/Repositorio
            };
            return usuario;
        }

        /// <summary>
        /// Valida el formato del correo electrónico
        /// </summary>
        /// <param name="correo">Correo a validar</param>
        /// <returns>True si el formato es válido</returns>
        private bool ValidarFormatoCorreo(string correo)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(correo);
                return addr.Address == correo;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Evento del botón Guardar
        /// </summary>

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFormulario()) return;

            btnGuardar.IsEnabled = false;
            btnGuardar.Content = "Guardando...";

            var empleado = ObtenerDatosEmpleado(); // Este ya debería tener el IdUsuario correcto si es edición
            var usuario = ObtenerDatosUsuario(); // Este obtiene los datos del formulario para el usuario
                                                 // Si chkCrearUsuario no está marcado, usuario será null.

            string errorMessage;
            // bool resultado; // Ya no la necesitamos directamente aquí para el mensaje

            try
            {
                if (_esEdicion)
                {
                    // Llamamos al nuevo método que maneja la actualización de ambos
                    if (_empleadoService.ActualizarEmpleadoYUsuario(empleado, usuario, out errorMessage)) // Asumiendo que _esEdicion se pasa al servicio o se infiere
                    {
                        MessageBox.Show("Empleado y/o usuario asociado actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show($"Error al guardar: {errorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else // Es creación
                {
                    if (usuario == null)
                    {
                        MessageBox.Show("Es necesario proporcionar datos de usuario para un nuevo empleado si la opción está marcada.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        btnGuardar.IsEnabled = true;
                        btnGuardar.Content = "Guardar";
                        return;
                    }
                    string contraseña = txtPassword.Password; //

                    var empleadoCreado = _empleadoService.RegistrarEmpleadoYUsuario(empleado, usuario, contraseña, out errorMessage); //

                    if (empleadoCreado != null)
                    {
                        MessageBox.Show($"Empleado y Usuario creados correctamente.\nUsuario: {usuario.NombreUsuario}\nID Empleado: {empleadoCreado.IdEmpleado}", //
                                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show($"Error al guardar: {errorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al guardar empleado"); //
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnGuardar.IsEnabled = true;
                btnGuardar.Content = "Guardar";
            }
        }



        /// <summary>
        /// Evento del botón Cancelar
        /// </summary>
        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Verificar si hay cambios sin guardar
            if (HayCambiosSinGuardar())
            {
                var resultado = MessageBox.Show(
                    "¿Está seguro que desea cancelar? Se perderán los cambios no guardados.",
                    "Confirmar cancelación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resultado == MessageBoxResult.No)
                {
                    return;
                }
            }

            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// Verifica si hay cambios sin guardar en el formulario
        /// </summary>
        /// <returns>True si hay cambios, False en caso contrario</returns>
        private bool HayCambiosSinGuardar()
        {
            if (_esEdicion && _empleadoActual != null)
            {
                // En modo edición, comparar con los datos originales
                return txtNombre.Text.Trim() != _empleadoActual.Nombre ||
                       txtApellido.Text.Trim() != _empleadoActual.Apellido ||
                       txtNumeroDocumento.Text.Trim() != _empleadoActual.NumeroDocumento ||
                       txtTelefono.Text.Trim() != _empleadoActual.Telefono ||
                       (txtDireccion.Text.Trim() != (_empleadoActual.Direccion ?? "")) ||
                       dpFechaContratacion.SelectedDate != _empleadoActual.FechaContratacion ||
                       double.Parse(txtSalarioBase.Text) != _empleadoActual.SalarioBase ||
                       chkActivo.IsChecked != _empleadoActual.Activo;
            }
            else
            {
                // En modo creación, verificar si se ha ingresado algún dato
                return !string.IsNullOrWhiteSpace(txtNombre.Text) ||
                       !string.IsNullOrWhiteSpace(txtApellido.Text) ||
                       !string.IsNullOrWhiteSpace(txtNumeroDocumento.Text) ||
                       !string.IsNullOrWhiteSpace(txtTelefono.Text) ||
                       !string.IsNullOrWhiteSpace(txtDireccion.Text) ||
                       txtSalarioBase.Text != "0" ||
                       chkCrearUsuario.IsChecked == true;
            }
        }

        /// <summary>
        /// Maneja el evento de cierre de la ventana
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (this.DialogResult != true && HayCambiosSinGuardar())
            {
                var resultado = MessageBox.Show(
                    "¿Está seguro que desea salir? Se perderán los cambios no guardados.",
                    "Confirmar salida",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resultado == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            base.OnClosing(e);
        }
    }
}