using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ElectroTech.Views
{
    /// <summary>
    /// Lógica de interacción para ClienteDetailWindow.xaml
    /// </summary>
    public partial class ClienteDetailWindow : Window
    {
        private readonly ClienteService _clienteService;
        private readonly Cliente _cliente;
        private readonly bool _esNuevo;

        /// <summary>
        /// Constructor para un nuevo cliente
        /// </summary>
        public ClienteDetailWindow()
        {
            InitializeComponent();

            _clienteService = new ClienteService();
            _cliente = new Cliente();
            _esNuevo = true;

            // Configurar ventana para nuevo cliente
            txtTitulo.Text = "Nuevo Cliente";
            ConfigurarVentana();
        }

        /// <summary>
        /// Constructor para editar un cliente existente
        /// </summary>
        public ClienteDetailWindow(Cliente cliente)
        {
            InitializeComponent();

            _clienteService = new ClienteService();
            _cliente = cliente;
            _esNuevo = false;

            // Configurar ventana para edición
            txtTitulo.Text = "Editar Cliente";
            ConfigurarVentana();
            CargarDatosCliente();
        }

        /// <summary>
        /// Configura la ventana y carga datos iniciales
        /// </summary>
        private void ConfigurarVentana()
        {
            // Seleccionar por defecto el tipo de documento DNI
            if (cmbTipoDocumento.Items.Count > 0)
            {
                cmbTipoDocumento.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Carga los datos del cliente en el formulario (modo edición)
        /// </summary>
        private void CargarDatosCliente()
        {
            try
            {
                // Cargar datos básicos
                txtNombre.Text = _cliente.Nombre;
                txtApellido.Text = _cliente.Apellido;
                txtTelefono.Text = _cliente.Telefono;
                txtCorreo.Text = _cliente.Correo;
                txtDireccion.Text = _cliente.Direccion;
                txtNumeroDocumento.Text = _cliente.NumeroDocumento;
                chkActivo.IsChecked = _cliente.Activo;

                // Seleccionar tipo de documento
                foreach (ComboBoxItem item in cmbTipoDocumento.Items)
                {
                    if (item.Content.ToString() == _cliente.TipoDocumento)
                    {
                        cmbTipoDocumento.SelectedItem = item;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar datos del cliente");
                MessageBox.Show("Error al cargar los datos del cliente: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Valida y guarda el cliente
        /// </summary>
        private void GuardarCliente()
        {
            try
            {
                // Ocultar mensaje de error previo
                txtError.Visibility = Visibility.Collapsed;

                // Validar campos obligatorios
                if (cmbTipoDocumento.SelectedItem == null ||
                    string.IsNullOrWhiteSpace(txtNumeroDocumento.Text) ||
                    string.IsNullOrWhiteSpace(txtNombre.Text) ||
                    string.IsNullOrWhiteSpace(txtApellido.Text))
                {
                    MostrarError("Por favor, complete todos los campos obligatorios.");
                    return;
                }

                // Completar datos del cliente
                _cliente.TipoDocumento = ((ComboBoxItem)cmbTipoDocumento.SelectedItem).Content.ToString();
                _cliente.NumeroDocumento = txtNumeroDocumento.Text.Trim();
                _cliente.Nombre = txtNombre.Text.Trim();
                _cliente.Apellido = txtApellido.Text.Trim();
                _cliente.Telefono = string.IsNullOrWhiteSpace(txtTelefono.Text) ? null : txtTelefono.Text.Trim();
                _cliente.Correo = string.IsNullOrWhiteSpace(txtCorreo.Text) ? null : txtCorreo.Text.Trim();
                _cliente.Direccion = string.IsNullOrWhiteSpace(txtDireccion.Text) ? null : txtDireccion.Text.Trim();
                _cliente.Activo = chkActivo.IsChecked ?? true;

                // Guardar cliente
                string errorMessage;
                bool resultado;

                if (_esNuevo)
                {
                    int idCliente = _clienteService.CrearCliente(_cliente, out errorMessage);
                    resultado = idCliente > 0;

                    if (resultado)
                    {
                        _cliente.IdCliente = idCliente;
                    }
                }
                else
                {
                    resultado = _clienteService.ActualizarCliente(_cliente, out errorMessage);
                }

                // Verificar resultado
                if (resultado)
                {
                    MessageBox.Show($"Cliente {(_esNuevo ? "creado" : "actualizado")} con éxito.",
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Cerrar ventana con resultado positivo
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MostrarError($"Error al {(_esNuevo ? "crear" : "actualizar")} el cliente: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al guardar cliente {(_esNuevo ? "nuevo" : "existente")}");
                MostrarError("Error al guardar el cliente: " + ex.Message);
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
            GuardarCliente();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Cerrar ventana sin guardar
            DialogResult = false;
            Close();
        }

        #endregion
    }
}