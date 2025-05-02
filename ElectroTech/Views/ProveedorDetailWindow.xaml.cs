using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using System;
using System.Windows;

namespace ElectroTech.Views
{
    /// <summary>
    /// Lógica de interacción para ProveedorDetailWindow.xaml
    /// </summary>
    public partial class ProveedorDetailWindow : Window
    {
        private readonly ProveedorService _proveedorService;
        private readonly Proveedor _proveedor;
        private readonly bool _esNuevo;

        /// <summary>
        /// Constructor para un nuevo proveedor
        /// </summary>
        public ProveedorDetailWindow()
        {
            InitializeComponent();

            _proveedorService = new ProveedorService();
            _proveedor = new Proveedor();
            _esNuevo = true;

            // Configurar ventana para nuevo proveedor
            txtTitulo.Text = "Nuevo Proveedor";
            ConfigurarVentana();
        }

        /// <summary>
        /// Constructor para editar un proveedor existente
        /// </summary>
        public ProveedorDetailWindow(Proveedor proveedor)
        {
            InitializeComponent();

            _proveedorService = new ProveedorService();
            _proveedor = proveedor;
            _esNuevo = false;

            // Configurar ventana para edición
            txtTitulo.Text = "Editar Proveedor";
            ConfigurarVentana();
            CargarDatosProveedor();
        }

        /// <summary>
        /// Configura la ventana y carga datos iniciales
        /// </summary>
        private void ConfigurarVentana()
        {
            // Nada que configurar específicamente para esta ventana
        }

        /// <summary>
        /// Carga los datos del proveedor en el formulario (modo edición)
        /// </summary>
        private void CargarDatosProveedor()
        {
            try
            {
                // Cargar datos básicos
                txtNombre.Text = _proveedor.Nombre;
                txtContacto.Text = _proveedor.Contacto;
                txtTelefono.Text = _proveedor.Telefono;
                txtCorreo.Text = _proveedor.Correo;
                txtDireccion.Text = _proveedor.Direccion;
                txtCondicionesPago.Text = _proveedor.CondicionesPago;
                chkActivo.IsChecked = _proveedor.Activo;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar datos del proveedor");
                MessageBox.Show("Error al cargar los datos del proveedor: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Valida y guarda el proveedor
        /// </summary>
        private void GuardarProveedor()
        {
            try
            {
                // Ocultar mensaje de error previo
                txtError.Visibility = Visibility.Collapsed;

                // Validar campos obligatorios
                if (string.IsNullOrWhiteSpace(txtNombre.Text))
                {
                    MostrarError("Por favor, ingrese el nombre del proveedor.");
                    return;
                }

                // Completar datos del proveedor
                _proveedor.Nombre = txtNombre.Text.Trim();
                _proveedor.Contacto = string.IsNullOrWhiteSpace(txtContacto.Text) ? null : txtContacto.Text.Trim();
                _proveedor.Telefono = string.IsNullOrWhiteSpace(txtTelefono.Text) ? null : txtTelefono.Text.Trim();
                _proveedor.Correo = string.IsNullOrWhiteSpace(txtCorreo.Text) ? null : txtCorreo.Text.Trim();
                _proveedor.Direccion = string.IsNullOrWhiteSpace(txtDireccion.Text) ? null : txtDireccion.Text.Trim();
                _proveedor.CondicionesPago = string.IsNullOrWhiteSpace(txtCondicionesPago.Text) ? null : txtCondicionesPago.Text.Trim();
                _proveedor.Activo = chkActivo.IsChecked ?? true;

                // Guardar proveedor
                string errorMessage;
                bool resultado;

                if (_esNuevo)
                {
                    int idProveedor = _proveedorService.CrearProveedor(_proveedor, out errorMessage);
                    resultado = idProveedor > 0;

                    if (resultado)
                    {
                        _proveedor.IdProveedor = idProveedor;
                    }
                }
                else
                {
                    resultado = _proveedorService.ActualizarProveedor(_proveedor, out errorMessage);
                }

                // Verificar resultado
                if (resultado)
                {
                    MessageBox.Show($"Proveedor {(_esNuevo ? "creado" : "actualizado")} con éxito.",
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Cerrar ventana con resultado positivo
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MostrarError($"Error al {(_esNuevo ? "crear" : "actualizar")} el proveedor: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al guardar proveedor {(_esNuevo ? "nuevo" : "existente")}");
                MostrarError("Error al guardar el proveedor: " + ex.Message);
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
            GuardarProveedor();
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