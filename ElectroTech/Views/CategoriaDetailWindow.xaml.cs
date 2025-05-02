using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using System;
using System.Windows;

namespace ElectroTech.Views
{
    /// <summary>
    /// Lógica de interacción para CategoriaDetailWindow.xaml
    /// </summary>
    public partial class CategoriaDetailWindow : Window
    {
        private readonly CategoriaService _categoriaService;
        private readonly Categoria _categoria;
        private readonly bool _esNueva;

        /// <summary>
        /// Constructor para una nueva categoría
        /// </summary>
        public CategoriaDetailWindow()
        {
            InitializeComponent();

            _categoriaService = new CategoriaService();
            _categoria = new Categoria();
            _esNueva = true;

            // Configurar ventana para nueva categoría
            txtTitulo.Text = "Nueva Categoría";
            ConfigurarVentana();
        }

        /// <summary>
        /// Constructor para editar una categoría existente
        /// </summary>
        public CategoriaDetailWindow(Categoria categoria)
        {
            InitializeComponent();

            _categoriaService = new CategoriaService();
            _categoria = categoria;
            _esNueva = false;

            // Configurar ventana para edición
            txtTitulo.Text = "Editar Categoría";
            ConfigurarVentana();
            CargarDatosCategoria();
        }

        /// <summary>
        /// Configura la ventana y carga datos iniciales
        /// </summary>
        private void ConfigurarVentana()
        {
            // Nada que configurar específicamente para esta ventana
        }

        /// <summary>
        /// Carga los datos de la categoría en el formulario (modo edición)
        /// </summary>
        private void CargarDatosCategoria()
        {
            try
            {
                // Cargar datos básicos
                txtNombre.Text = _categoria.Nombre;
                txtDescripcion.Text = _categoria.Descripcion;
                chkActiva.IsChecked = _categoria.Activa;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar datos de la categoría");
                MessageBox.Show("Error al cargar los datos de la categoría: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Valida y guarda la categoría
        /// </summary>
        private void GuardarCategoria()
        {
            try
            {
                // Ocultar mensaje de error previo
                txtError.Visibility = Visibility.Collapsed;

                // Validar campos obligatorios
                if (string.IsNullOrWhiteSpace(txtNombre.Text))
                {
                    MostrarError("Por favor, ingrese el nombre de la categoría.");
                    return;
                }

                // Validar longitud máxima del nombre
                if (txtNombre.Text.Length > 50)
                {
                    MostrarError("El nombre de la categoría no puede exceder los 50 caracteres.");
                    return;
                }

                // Validar longitud máxima de la descripción
                if (!string.IsNullOrEmpty(txtDescripcion.Text) && txtDescripcion.Text.Length > 200)
                {
                    MostrarError("La descripción de la categoría no puede exceder los 200 caracteres.");
                    return;
                }

                // Completar datos de la categoría
                _categoria.Nombre = txtNombre.Text.Trim();
                _categoria.Descripcion = string.IsNullOrWhiteSpace(txtDescripcion.Text) ? null : txtDescripcion.Text.Trim();
                _categoria.Activa = chkActiva.IsChecked ?? true;

                // Guardar categoría
                string errorMessage;
                bool resultado;

                if (_esNueva)
                {
                    int idCategoria = _categoriaService.CrearCategoria(_categoria, out errorMessage);
                    resultado = idCategoria > 0;

                    if (resultado)
                    {
                        _categoria.IdCategoria = idCategoria;
                    }
                }
                else
                {
                    resultado = _categoriaService.ActualizarCategoria(_categoria, out errorMessage);
                }

                // Verificar resultado
                if (resultado)
                {
                    MessageBox.Show($"Categoría {(_esNueva ? "creada" : "actualizada")} con éxito.",
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Cerrar ventana con resultado positivo
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MostrarError($"Error al {(_esNueva ? "crear" : "actualizar")} la categoría: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al guardar categoría {(_esNueva ? "nueva" : "existente")}");
                MostrarError("Error al guardar la categoría: " + ex.Message);
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
            GuardarCategoria();
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