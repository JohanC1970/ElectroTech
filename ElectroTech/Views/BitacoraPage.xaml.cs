using ElectroTech.Models;
using ElectroTech.Services; // Añadir el using para BitacoraService
using ElectroTech.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ElectroTech.Views.Admin
{
    public partial class BitacoraPage : Page
    {
        private readonly BitacoraService _bitacoraService; // Usar el servicio
        private List<BitacoraUsuarioViewModel> _registrosBitacoraMostrados;

        public BitacoraPage()
        {
            InitializeComponent();
            _bitacoraService = new BitacoraService(); // Instanciar el servicio
            dpFechaInicio.SelectedDate = DateTime.Today.AddMonths(-1);
            dpFechaFin.SelectedDate = DateTime.Today;
            CargarBitacora();
        }

        private void CargarBitacora()
        {
            try
            {
                DateTime? fechaInicio = dpFechaInicio.SelectedDate;
                DateTime? fechaFin = dpFechaFin.SelectedDate;

                // Ajustar fechaFin para que incluya todo el día
                if (fechaFin.HasValue)
                {
                    fechaFin = fechaFin.Value.Date.AddDays(1).AddTicks(-1);
                }

                // Validar que fechaInicio no sea mayor que fechaFin
                if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio.Value.Date > fechaFin.Value.Date)
                {
                    MessageBox.Show("La fecha de inicio no puede ser posterior a la fecha de fin.", "Error en Filtro", MessageBoxButton.OK, MessageBoxImage.Warning);
                    dgBitacora.ItemsSource = null; // Limpiar DataGrid
                    _registrosBitacoraMostrados = new List<BitacoraUsuarioViewModel>();
                    ActualizarContador();
                    return;
                }


                // Obtener los datos reales desde el servicio
                var registros = _bitacoraService.ObtenerRegistros(fechaInicio, fechaFin);

                if (registros != null)
                {
                    _registrosBitacoraMostrados = registros.Select(r => new BitacoraUsuarioViewModel(r)).ToList();
                    dgBitacora.ItemsSource = _registrosBitacoraMostrados;
                }
                else
                {
                    dgBitacora.ItemsSource = null;
                    _registrosBitacoraMostrados = new List<BitacoraUsuarioViewModel>();
                }
                ActualizarContador();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar la bitácora de accesos"); //
                MessageBox.Show($"Error al cargar la bitácora: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                dgBitacora.ItemsSource = null;
                _registrosBitacoraMostrados = new List<BitacoraUsuarioViewModel>();
                ActualizarContador();
            }
        }

        private void ActualizarContador()
        {
            txtTotalRegistros.Text = $"Total: {(_registrosBitacoraMostrados?.Count ?? 0)} registros";
        }

        private void btnFiltrar_Click(object sender, RoutedEventArgs e)
        {
            CargarBitacora();
        }

        private void btnLimpiarFiltro_Click(object sender, RoutedEventArgs e)
        {
            dpFechaInicio.SelectedDate = DateTime.Today.AddMonths(-1);
            dpFechaFin.SelectedDate = DateTime.Today;
            CargarBitacora();
        }
    }

    // ViewModel para mostrar en el DataGrid de Bitácora (sin cambios, ya lo tenías)
    public class BitacoraUsuarioViewModel
    {
        public int IdBitacora { get; set; } //
        public Usuario Usuario { get; set; } //
        public DateTime FechaHora { get; set; } //
        public string IpAcceso { get; set; } //
        public Models.Enums.TipoAccion TipoAccion { get; set; } //

        public string TipoAccionDescripcion => TipoAccion == Models.Enums.TipoAccion.Entrada ? "Entrada" : "Salida"; //

        public BitacoraUsuarioViewModel(BitacoraUsuario model)
        {
            IdBitacora = model.IdBitacora; //
            Usuario = model.Usuario; //
            FechaHora = model.FechaHora; //
            IpAcceso = model.IpAcceso; //
            TipoAccion = model.TipoAccion; //
        }
    }
}