using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
//using System.Windows.Shapes; // Comentado si no se usa para el gráfico

namespace ElectroTech.Views.Reportes
{
    public partial class ReporteVentasPage : Page
    {
        private readonly VentaService _ventaService;
        private readonly EmpleadoService _empleadoService;
        private readonly ClienteService _clienteService;
        private readonly ProductoService _productoService; // Mantener si se usa en otros cálculos
        private List<Venta> _ventasDelPeriodo;
        private Dictionary<string, object> _datosReporte;

        public ReporteVentasPage()
        {
            InitializeComponent();
            _ventaService = new VentaService();
            _empleadoService = new EmpleadoService();
            _clienteService = new ClienteService();
            _productoService = new ProductoService();
            InicializarFiltros();
        }

        private void InicializarFiltros()
        {
            try
            {
                dpFechaFin.SelectedDate = DateTime.Today;
                dpFechaInicio.SelectedDate = DateTime.Today.AddMonths(-1);

                var empleados = _empleadoService.ObtenerTodos();
                empleados.Insert(0, new Empleado { IdEmpleado = 0, Nombre = "Todos", Apellido = "" });
                cbEmpleado.ItemsSource = empleados;
                cbEmpleado.SelectedIndex = 0;

                var clientes = _clienteService.ObtenerTodos();
                clientes.Insert(0, new Cliente { IdCliente = 0, Nombre = "Todos", Apellido = "" });
                cbCliente.ItemsSource = clientes;
                cbCliente.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al inicializar filtros del reporte de ventas");
                MessageBox.Show("Error al cargar los filtros: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnGenerar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFiltros())
                return;

            DateTime fechaInicioSeleccionada = dpFechaInicio.SelectedDate.Value;
            DateTime fechaFinSeleccionada = dpFechaFin.SelectedDate.Value;
            int? idEmpleadoSeleccionado = cbEmpleado.SelectedValue as int?;
            int? idClienteSeleccionado = cbCliente.SelectedValue as int?;

            try
            {
                MostrarCargando(true);
                btnExportar.IsEnabled = false;

                await Task.Run(() => GenerarReporte(fechaInicioSeleccionada, fechaFinSeleccionada, idEmpleadoSeleccionado, idClienteSeleccionado));

                MostrarDatos();
                btnExportar.IsEnabled = true;
                txtEstadoReporte.Text = $"Reporte generado: {(_ventasDelPeriodo?.Count ?? 0)} ventas encontradas";

                Logger.LogInfo($"Reporte de ventas generado: {fechaInicioSeleccionada:dd/MM/yyyy} - {fechaFinSeleccionada:dd/MM/yyyy}");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al generar reporte de ventas");
                MessageBox.Show("Error al generar el reporte: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtEstadoReporte.Text = "Error al generar el reporte.";
            }
            finally
            {
                MostrarCargando(false);
            }
        }

        private bool ValidarFiltros()
        {
            if (!dpFechaInicio.SelectedDate.HasValue)
            {
                MessageBox.Show("Debe seleccionar una fecha de inicio", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!dpFechaFin.SelectedDate.HasValue)
            {
                MessageBox.Show("Debe seleccionar una fecha de fin", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (dpFechaInicio.SelectedDate > dpFechaFin.SelectedDate)
            {
                MessageBox.Show("La fecha de inicio no puede ser mayor a la fecha fin", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void GenerarReporte(DateTime fechaInicio, DateTime fechaFin, int? idEmpleado, int? idCliente)
        {
            try
            {
                var fechaFinAjustada = fechaFin.AddDays(1).AddTicks(-1);

                _ventasDelPeriodo = _ventaService.ObtenerPorFechas(fechaInicio, fechaFinAjustada);

                if (idEmpleado.HasValue && idEmpleado.Value > 0)
                {
                    _ventasDelPeriodo = _ventasDelPeriodo.Where(v => v.IdEmpleado == idEmpleado.Value).ToList();
                }
                if (idCliente.HasValue && idCliente.Value > 0)
                {
                    _ventasDelPeriodo = _ventasDelPeriodo.Where(v => v.IdCliente == idCliente.Value).ToList();
                }
                _ventasDelPeriodo = _ventasDelPeriodo.Where(v => v.Estado == 'C').ToList();

                _datosReporte = CalcularDatosReporte(fechaInicio, fechaFin);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error en la lógica interna de GenerarReporte");
                throw new Exception("Error al procesar los datos del reporte", ex);
            }
        }

        private Dictionary<string, object> CalcularDatosReporte(DateTime fechaInicioPeriodo, DateTime fechaFinPeriodo)
        {
            var datos = new Dictionary<string, object>();

            if (_ventasDelPeriodo == null)
            {
                _ventasDelPeriodo = new List<Venta>();
            }

            // Guardar las fechas usadas para el reporte para la exportación
            datos["FechaInicioReporte"] = fechaInicioPeriodo;
            datos["FechaFinReporte"] = fechaFinPeriodo;

            datos["TotalVentas"] = _ventasDelPeriodo.Sum(v => v.Total);
            datos["NumeroVentas"] = _ventasDelPeriodo.Count;
            datos["PromedioVenta"] = _ventasDelPeriodo.Count > 0 ? _ventasDelPeriodo.Average(v => v.Total) : 0;

            var diasPeriodo = (fechaFinPeriodo - fechaInicioPeriodo).Days + 1;
            var fechaInicioAnterior = fechaInicioPeriodo.AddDays(-diasPeriodo);
            var fechaFinAnterior = fechaInicioPeriodo.AddDays(-1);

            var ventasPeriodoAnterior = _ventaService.ObtenerPorFechas(fechaInicioAnterior, fechaFinAnterior)
                                      ?.Where(v => v.Estado == 'C').ToList() ?? new List<Venta>();

            var totalAnterior = ventasPeriodoAnterior.Sum(v => v.Total);
            var totalActual = Convert.ToDouble(datos["TotalVentas"]);
            var crecimiento = totalAnterior > 0 ? ((totalActual - totalAnterior) / totalAnterior) * 100 : (totalActual > 0 ? 100 : 0);
            datos["Crecimiento"] = crecimiento;

            // SECCIÓN COMENTADA/ELIMINADA: TopProductos
            // var totalGeneralVentas = totalActual;
            // datos["TopProductos"] = CalcularTopProductos(totalGeneralVentas); // Ya no llamamos a esto

            datos["VentasPorEmpleado"] = CalcularVentasPorEmpleado(totalActual); // Pasamos totalActual
            datos["VentasPorDia"] = CalcularVentasPorDia();

            return datos;
        }

        // MÉTODO COMENTADO/ELIMINADO: CalcularTopProductos
        /*
        private List<object> CalcularTopProductos(double totalGeneralVentas)
        {
            // ... (contenido original del método) ...
            // Si decides mantenerlo oculto en lugar de eliminarlo completamente,
            // simplemente no lo llames desde CalcularDatosReporte
            return new List<object>(); // Devolver lista vacía si se comenta la lógica
        }
        */

        private List<object> CalcularVentasPorEmpleado(double totalGeneralVentas)
        {
            if (_ventasDelPeriodo == null) return new List<object>();

            var ventasPorEmpleado = _ventasDelPeriodo
                .GroupBy(v => v.IdEmpleado)
                .Select(g => new
                {
                    IdEmpleado = g.Key,
                    NumeroVentas = g.Count(),
                    TotalVendido = g.Sum(v => v.Total)
                })
                .OrderByDescending(e => e.TotalVendido)
                .ToList();

            return ventasPorEmpleado.Select(e => {
                var empleadoInfo = _empleadoService.ObtenerPorId(e.IdEmpleado);
                return new
                {
                    Empleado = empleadoInfo?.NombreCompleto ?? $"Empleado ID: {e.IdEmpleado}",
                    e.NumeroVentas,
                    e.TotalVendido,
                    PromedioVenta = e.NumeroVentas > 0 ? e.TotalVendido / e.NumeroVentas : 0,
                    Comisiones = e.TotalVendido * 0.02,
                    PorcentajeTotal = totalGeneralVentas > 0 ? (e.TotalVendido / totalGeneralVentas) * 100 : 0
                };
            }).Cast<object>().ToList();
        }

        private Dictionary<DateTime, double> CalcularVentasPorDia()
        {
            if (_ventasDelPeriodo == null) return new Dictionary<DateTime, double>();
            return _ventasDelPeriodo
                .GroupBy(v => v.Fecha.Date)
                .ToDictionary(g => g.Key, g => g.Sum(v => v.Total));
        }

        private void MostrarDatos()
        {
            if (_datosReporte == null || _ventasDelPeriodo == null)
            {
                Dispatcher.Invoke(() => {
                    txtEstadoReporte.Text = "No hay datos para mostrar o ocurrió un error previo.";
                    dgVentas.ItemsSource = null;
                     // Aunque se elimine la tab, es bueno limpiar el ItemsSource
                    dgVentasEmpleado.ItemsSource = null;
                    canvasGrafico.Children.Clear();
                });
                return;
            }

            Dispatcher.Invoke(() =>
            {
                txtTotalVentas.Text = ((double)_datosReporte["TotalVentas"]).ToString("C");
                txtNumeroVentas.Text = _datosReporte["NumeroVentas"].ToString();
                txtPromedioVenta.Text = ((double)_datosReporte["PromedioVenta"]).ToString("C");
                var crecimiento = (double)_datosReporte["Crecimiento"];
                txtCrecimiento.Text = $"{crecimiento:F1}%";
                txtCrecimiento.Foreground = crecimiento >= 0 ? Brushes.Green : Brushes.Red;

                dgVentas.ItemsSource = _ventasDelPeriodo;

                // SECCIÓN COMENTADA/ELIMINADA: Asignación a dgTopProductos
                // if (_datosReporte.ContainsKey("TopProductos"))
                // {
                //     dgTopProductos.ItemsSource = (List<object>)_datosReporte["TopProductos"];
                // }
                // else
                // {
                //     dgTopProductos.ItemsSource = null; // Limpiar por si acaso
                // }
                 // Siempre limpiar si la tab se va a quitar

                if (_datosReporte.ContainsKey("VentasPorEmpleado"))
                {
                    dgVentasEmpleado.ItemsSource = (List<object>)_datosReporte["VentasPorEmpleado"];
                }
                else
                {
                    dgVentasEmpleado.ItemsSource = null;
                }


                DibujarGraficoVentas();
            });
        }

        private void DibujarGraficoVentas()
        {
            try
            {
                canvasGrafico.Children.Clear();
                if (_datosReporte == null || !_datosReporte.ContainsKey("VentasPorDia")) return;

                var ventasPorDia = (Dictionary<DateTime, double>)_datosReporte["VentasPorDia"];
                if (ventasPorDia.Count == 0) return;

                var width = canvasGrafico.ActualWidth > 0 ? canvasGrafico.ActualWidth : 800;
                var height = canvasGrafico.ActualHeight > 0 ? canvasGrafico.ActualHeight : 300;
                var maxVenta = ventasPorDia.Values.Any() ? ventasPorDia.Values.Max() : 1.0;
                if (maxVenta == 0) maxVenta = 1.0;

                var minFecha = ventasPorDia.Keys.Min();
                var maxFecha = ventasPorDia.Keys.Max();
                var rangoFechas = (maxFecha - minFecha).TotalDays;

                if (rangoFechas < 1) rangoFechas = 1;

                var puntos = new PointCollection();
                foreach (var kvp in ventasPorDia.OrderBy(k => k.Key))
                {
                    var x = ((kvp.Key - minFecha).TotalDays / rangoFechas) * (width - 60) + 30;
                    var y = height - 30 - ((kvp.Value / maxVenta) * (height - 60));
                    puntos.Add(new Point(x, y));
                }

                if (puntos.Any())
                {
                    var polyline = new Polyline { Points = puntos, Stroke = Brushes.Blue, StrokeThickness = 2 };
                    canvasGrafico.Children.Add(polyline);
                    foreach (var punto in puntos)
                    {
                        var circle = new Ellipse { Width = 6, Height = 6, Fill = Brushes.Blue };
                        Canvas.SetLeft(circle, punto.X - 3);
                        Canvas.SetTop(circle, punto.Y - 3);
                        canvasGrafico.Children.Add(circle);
                    }
                }
                var ejeX = new Line { X1 = 30, Y1 = height - 30, X2 = width - 30, Y2 = height - 30, Stroke = Brushes.Gray, StrokeThickness = 1 };
                var ejeY = new Line { X1 = 30, Y1 = 30, X2 = 30, Y2 = height - 30, Stroke = Brushes.Gray, StrokeThickness = 1 };
                canvasGrafico.Children.Add(ejeX);
                canvasGrafico.Children.Add(ejeY);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al dibujar gráfico de ventas");
            }
        }

        private void MostrarCargando(bool mostrar)
        {
            Dispatcher.Invoke(() =>
            {
                loadingPanel.Visibility = mostrar ? Visibility.Visible : Visibility.Collapsed;
                btnGenerar.IsEnabled = !mostrar;
            });
        }

        private void btnExportar_Click(object sender, RoutedEventArgs e)
        {
            if (_datosReporte == null || !_datosReporte.Any() || _ventasDelPeriodo == null)
            {
                MessageBox.Show("Debe generar el reporte antes de exportar", "Información",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "Archivo PDF (*.pdf)|*.pdf|Archivo de texto (*.txt)|*.txt|Archivo CSV (*.csv)|*.csv", // PDF como primera opción
                DefaultExt = "pdf",
                FileName = $"ReporteVentas_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                string filePath = saveDialog.FileName;
                try
                {
                    switch (saveDialog.FilterIndex)
                    {
                        case 1: // PDF
                            ExportarVentasPDF(filePath);
                            break;
                        case 2: // TXT
                            ExportarTexto(filePath); // Tu método existente
                            break;
                        case 3: // CSV
                            ExportarCSV(filePath);   // Tu método existente
                            break;
                    }
                    MessageBox.Show("Reporte exportado exitosamente", "Éxito",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "Error al exportar reporte de ventas");
                    MessageBox.Show("Error al exportar el reporte: " + ex.Message, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportarVentasPDF(string filePath)
        {
            Document document = new Document(PageSize.A4.Rotate(), 25, 25, 30, 30); // Hoja horizontal, márgenes
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));

            // Definir fuentes
            BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font titleFont = new Font(baseFont, 18, Font.BOLD);
            Font subTitleFont = new Font(baseFont, 14, Font.BOLD);
            Font headerFont = new Font(baseFont, 9, Font.BOLD, BaseColor.WHITE);
            Font cellFont = new Font(baseFont, 8, Font.NORMAL);
            Font smallItalicFont = new Font(baseFont, 8, Font.ITALIC);

            document.Open();

            // Título Principal
            Paragraph mainTitle = new Paragraph("Reporte General de Ventas", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 15f
            };
            document.Add(mainTitle);

            // Información de Filtros
            DateTime fechaInicioUsada = _datosReporte.ContainsKey("FechaInicioReporte") ? (DateTime)_datosReporte["FechaInicioReporte"] : dpFechaInicio.SelectedDate.GetValueOrDefault();
            DateTime fechaFinUsada = _datosReporte.ContainsKey("FechaFinReporte") ? (DateTime)_datosReporte["FechaFinReporte"] : dpFechaFin.SelectedDate.GetValueOrDefault();
            string empleadoFiltro = (cbEmpleado.SelectedItem as Empleado)?.IdEmpleado == 0 ? "Todos" : (cbEmpleado.SelectedItem as Empleado)?.NombreCompleto ?? "Todos";
            string clienteFiltro = (cbCliente.SelectedItem as Cliente)?.IdCliente == 0 ? "Todos" : (cbCliente.SelectedItem as Cliente)?.NombreCompleto ?? "Todos";

            document.Add(new Paragraph($"Período: {fechaInicioUsada:dd/MM/yyyy} - {fechaFinUsada:dd/MM/yyyy}", smallItalicFont));
            document.Add(new Paragraph($"Empleado: {empleadoFiltro}", smallItalicFont));
            document.Add(new Paragraph($"Cliente: {clienteFiltro}", smallItalicFont));
            document.Add(new Paragraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", smallItalicFont) { SpacingAfter = 20f });

            // Sección de Resumen General
            if (_datosReporte != null)
            {
                Paragraph resumenTitle = new Paragraph("Resumen General", subTitleFont) { SpacingAfter = 10f };
                document.Add(resumenTitle);

                PdfPTable resumenTable = new PdfPTable(2); // 2 columnas para Métrica y Valor
                resumenTable.WidthPercentage = 70; // No ocupar todo el ancho
                resumenTable.HorizontalAlignment = Element.ALIGN_LEFT;
                resumenTable.SetWidths(new float[] { 3f, 2f });


                resumenTable.AddCell(new PdfPCell(new Phrase("Total de Ventas:", cellFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER });
                resumenTable.AddCell(new PdfPCell(new Phrase(((double)_datosReporte["TotalVentas"]).ToString("C", CultureInfo.CurrentCulture), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = iTextSharp.text.Rectangle.NO_BORDER });

                resumenTable.AddCell(new PdfPCell(new Phrase("Número de Ventas:", cellFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER });
                resumenTable.AddCell(new PdfPCell(new Phrase(_datosReporte["NumeroVentas"].ToString(), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = iTextSharp.text.Rectangle.NO_BORDER });

                resumenTable.AddCell(new PdfPCell(new Phrase("Promedio por Venta:", cellFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER });
                resumenTable.AddCell(new PdfPCell(new Phrase(((double)_datosReporte["PromedioVenta"]).ToString("C", CultureInfo.CurrentCulture), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = iTextSharp.text.Rectangle.NO_BORDER });

                resumenTable.AddCell(new PdfPCell(new Phrase("Crecimiento vs Período Anterior:", cellFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER });
                resumenTable.AddCell(new PdfPCell(new Phrase($"{((double)_datosReporte["Crecimiento"]):F1}%", cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = iTextSharp.text.Rectangle.NO_BORDER });
                document.Add(resumenTable);
                document.Add(new Paragraph(" ") { SpacingAfter = 15f }); // Espacio
            }


            // Sección Detalle de Ventas
            if (_ventasDelPeriodo != null && _ventasDelPeriodo.Any())
            {
                document.Add(new Paragraph("Detalle de Ventas", subTitleFont) { SpacingAfter = 10f });
                PdfPTable ventasTable = new PdfPTable(8); // Ajustar número de columnas
                ventasTable.WidthPercentage = 100;
                ventasTable.SetWidths(new float[] { 1.2f, 1f, 2f, 2f, 1.2f, 1f, 1f, 1f }); // Ajustar anchos

                string[] ventasHeaders = { "Factura", "Fecha", "Cliente", "Empleado", "Método Pago", "Subtotal", "Total", "Estado" };
                foreach (string header in ventasHeaders)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = new BaseColor(76, 175, 80), // Verde
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 4
                    };
                    ventasTable.AddCell(cell);
                }

                foreach (var venta in _ventasDelPeriodo)
                {
                    ventasTable.AddCell(new PdfPCell(new Phrase(venta.NumeroFactura, cellFont)) { Padding = 3 });
                    ventasTable.AddCell(new PdfPCell(new Phrase(venta.Fecha.ToString("dd/MM/yy"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 3 });
                    ventasTable.AddCell(new PdfPCell(new Phrase(venta.Cliente?.NombreCompleto ?? "N/A", cellFont)) { Padding = 3 });
                    ventasTable.AddCell(new PdfPCell(new Phrase(venta.Empleado?.NombreCompleto ?? "N/A", cellFont)) { Padding = 3 });
                    ventasTable.AddCell(new PdfPCell(new Phrase(venta.MetodoPagoNombre ?? "N/A", cellFont)) { Padding = 3 });
                    ventasTable.AddCell(new PdfPCell(new Phrase(venta.Subtotal.ToString("C", CultureInfo.CurrentCulture), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
                    ventasTable.AddCell(new PdfPCell(new Phrase(venta.Total.ToString("C", CultureInfo.CurrentCulture), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
                    ventasTable.AddCell(new PdfPCell(new Phrase(venta.EstadoDescripcion, cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 3 });
                }
                document.Add(ventasTable);
                document.Add(new Paragraph(" ") { SpacingAfter = 15f });
            }

            // Sección Productos Más Vendidos (si existe en _datosReporte)
            if (_datosReporte != null && _datosReporte.ContainsKey("TopProductos") && _datosReporte["TopProductos"] is List<object> topProductos && topProductos.Any())
            {
                document.Add(new Paragraph("Productos Más Vendidos", subTitleFont) { SpacingAfter = 10f });
                PdfPTable topProductosTable = new PdfPTable(7);
                topProductosTable.WidthPercentage = 100;
                topProductosTable.SetWidths(new float[] { 0.5f, 1f, 2.5f, 1.5f, 1f, 1.2f, 1.2f });

                string[] topProdHeaders = { "Pos.", "Código", "Producto", "Categoría", "Cant.", "Total", "% Ventas" };
                foreach (string header in topProdHeaders)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = new BaseColor(76, 175, 80), // Verde
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 4
                    };
                    topProductosTable.AddCell(cell);
                }
                foreach (dynamic item in topProductos)
                {
                    topProductosTable.AddCell(new PdfPCell(new Phrase(item.Posicion.ToString(), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
                    topProductosTable.AddCell(new PdfPCell(new Phrase(item.Codigo.ToString(), cellFont)) { Padding = 3 });
                    topProductosTable.AddCell(new PdfPCell(new Phrase(item.ProductoNombre.ToString(), cellFont)) { Padding = 3 });
                    topProductosTable.AddCell(new PdfPCell(new Phrase(item.Categoria.ToString(), cellFont)) { Padding = 3 });
                    topProductosTable.AddCell(new PdfPCell(new Phrase(item.CantidadVendida.ToString(), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
                    topProductosTable.AddCell(new PdfPCell(new Phrase(item.TotalVendido.ToString("C", CultureInfo.CurrentCulture), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
                    topProductosTable.AddCell(new PdfPCell(new Phrase(item.PorcentajeTotal.ToString("F2", CultureInfo.CurrentCulture) + "%", cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
                }
                document.Add(topProductosTable);
                document.Add(new Paragraph(" ") { SpacingAfter = 15f });
            }

            // Sección Ventas por Empleado (si existe en _datosReporte)
            if (_datosReporte != null && _datosReporte.ContainsKey("VentasPorEmpleado") && _datosReporte["VentasPorEmpleado"] is List<object> ventasPorEmpleado && ventasPorEmpleado.Any())
            {
                document.Add(new Paragraph("Ventas por Empleado", subTitleFont) { SpacingAfter = 10f });
                PdfPTable empleadoTable = new PdfPTable(6);
                empleadoTable.WidthPercentage = 100;
                empleadoTable.SetWidths(new float[] { 2.5f, 1f, 1.2f, 1.2f, 1f, 1f });

                string[] empleadoHeaders = { "Empleado", "N° Ventas", "Total Vendido", "Prom. Venta", "Comisiones", "% Ventas" };
                foreach (string header in empleadoHeaders)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = new BaseColor(76, 175, 80), // Verde
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 4
                    };
                    empleadoTable.AddCell(cell);
                }
                foreach (dynamic item in ventasPorEmpleado)
                {
                    empleadoTable.AddCell(new PdfPCell(new Phrase(item.Empleado.ToString(), cellFont)) { Padding = 3 });
                    empleadoTable.AddCell(new PdfPCell(new Phrase(item.NumeroVentas.ToString(), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
                    empleadoTable.AddCell(new PdfPCell(new Phrase(item.TotalVendido.ToString("C", CultureInfo.CurrentCulture), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
                    empleadoTable.AddCell(new PdfPCell(new Phrase(item.PromedioVenta.ToString("C", CultureInfo.CurrentCulture), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
                    empleadoTable.AddCell(new PdfPCell(new Phrase(item.Comisiones.ToString("C", CultureInfo.CurrentCulture), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
                    empleadoTable.AddCell(new PdfPCell(new Phrase(item.PorcentajeTotal.ToString("F2", CultureInfo.CurrentCulture) + "%", cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
                }
                document.Add(empleadoTable);
            }

            document.Close();
            writer.Close();
        }

        private void ExportarTexto(string fileName)
        {
            var sb = new StringBuilder();
            // Asegúrate de que estas claves existan en _datosReporte o maneja la posibilidad de que no
            DateTime fechaInicioUsada = _datosReporte.ContainsKey("FechaInicioReporte") ? (DateTime)_datosReporte["FechaInicioReporte"] : dpFechaInicio.SelectedDate.GetValueOrDefault();
            DateTime fechaFinUsada = _datosReporte.ContainsKey("FechaFinReporte") ? (DateTime)_datosReporte["FechaFinReporte"] : dpFechaFin.SelectedDate.GetValueOrDefault();

            sb.AppendLine("=============================================================");
            sb.AppendLine("                      REPORTE DE VENTAS");
            sb.AppendLine("=============================================================");
            sb.AppendLine($"Período: {fechaInicioUsada:dd/MM/yyyy} - {fechaFinUsada:dd/MM/yyyy}");
            sb.AppendLine($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine();

            sb.AppendLine("RESUMEN GENERAL");
            sb.AppendLine("-------------------------------------------------------------");
            sb.AppendLine($"Total de Ventas: {((double)_datosReporte["TotalVentas"]):C}");
            sb.AppendLine($"Número de Ventas: {_datosReporte["NumeroVentas"]}");
            sb.AppendLine($"Promedio por Venta: {((double)_datosReporte["PromedioVenta"]):C}");
            sb.AppendLine($"Crecimiento: {((double)_datosReporte["Crecimiento"]):F1}%");
            sb.AppendLine();

            // SECCIÓN COMENTADA/ELIMINADA: Exportación de Top Productos
            /*
            sb.AppendLine("PRODUCTOS MÁS VENDIDOS (Top 20)");
            sb.AppendLine("-------------------------------------------------------------");
            sb.AppendLine("Pos. | Código     | Producto                      | Categoría      | Cant. | Total Vendido | % del Total");
            if (_datosReporte.ContainsKey("TopProductos"))
            {
                var topProductos = (List<object>)_datosReporte["TopProductos"];
                foreach (dynamic producto in topProductos)
                {
                    sb.AppendLine($"{producto.Posicion.ToString().PadRight(4)} | {producto.Codigo.PadRight(10)} | {producto.ProductoNombre.PadRight(29)} | {producto.Categoria.PadRight(14)} | {producto.CantidadVendida.ToString().PadLeft(5)} | {producto.TotalVendido.ToString("C").PadLeft(13)} | {producto.PorcentajeTotal.ToString("F1").PadLeft(10)}%");
                }
            }
            sb.AppendLine();
            */

            sb.AppendLine("VENTAS POR EMPLEADO");
            sb.AppendLine("-------------------------------------------------------------");
            sb.AppendLine("Empleado                      | N° Ventas | Total Vendido | Prom. Venta | Comisiones  | % del Total");
            if (_datosReporte.ContainsKey("VentasPorEmpleado"))
            {
                var ventasEmpleado = (List<object>)_datosReporte["VentasPorEmpleado"];
                foreach (dynamic empleado in ventasEmpleado)
                {
                    sb.AppendLine($"{empleado.Empleado.PadRight(29)} | {empleado.NumeroVentas.ToString().PadLeft(9)} | {empleado.TotalVendido.ToString("C").PadLeft(13)} | {empleado.PromedioVenta.ToString("C").PadLeft(11)} | {empleado.Comisiones.ToString("C").PadLeft(11)} | {empleado.PorcentajeTotal.ToString("F1").PadLeft(10)}%");
                }
            }
            sb.AppendLine();

            sb.AppendLine("DETALLE DE VENTAS");
            sb.AppendLine("-------------------------------------------------------------");
            sb.AppendLine("Factura   | Fecha      | Cliente                | Empleado         | Subtotal  | Descuento | Total     | Estado");
            if (_ventasDelPeriodo != null)
            {
                foreach (var venta in _ventasDelPeriodo)
                {
                    sb.AppendLine($"{venta.NumeroFactura.PadRight(9)} | {venta.Fecha:dd/MM/yyyy} | {(venta.Cliente?.NombreCompleto ?? "N/A").PadRight(22)} | {(venta.Empleado?.NombreCompleto ?? "N/A").PadRight(16)} | {venta.Subtotal.ToString("C").PadLeft(9)} | {venta.Descuento.ToString("C").PadLeft(9)} | {venta.Total.ToString("C").PadLeft(9)} | {venta.EstadoDescripcion}");
                }
            }
            File.WriteAllText(fileName, sb.ToString(), Encoding.UTF8);
        }

        private void ExportarCSV(string fileName)
        {
            var sb = new StringBuilder();
            DateTime fechaInicioUsada = _datosReporte.ContainsKey("FechaInicioReporte") ? (DateTime)_datosReporte["FechaInicioReporte"] : dpFechaInicio.SelectedDate.GetValueOrDefault();
            DateTime fechaFinUsada = _datosReporte.ContainsKey("FechaFinReporte") ? (DateTime)_datosReporte["FechaFinReporte"] : dpFechaFin.SelectedDate.GetValueOrDefault();

            sb.AppendLine($"\"Reporte de Ventas del {fechaInicioUsada:dd/MM/yyyy} al {fechaFinUsada:dd/MM/yyyy}\"");
            sb.AppendLine($"\"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\"");
            sb.AppendLine();

            sb.AppendLine("\"Resumen General\"");
            sb.AppendLine("\"Métrica\",\"Valor\"");
            sb.AppendLine($"\"Total de Ventas\",\"{((double)_datosReporte["TotalVentas"]):C}\"");
            sb.AppendLine($"\"Número de Ventas\",\"{_datosReporte["NumeroVentas"]}\"");
            sb.AppendLine($"\"Promedio por Venta\",\"{((double)_datosReporte["PromedioVenta"]):C}\"");
            sb.AppendLine($"\"Crecimiento (%)\",\"{((double)_datosReporte["Crecimiento"]):F1}\"");
            sb.AppendLine();

            // SECCIÓN COMENTADA/ELIMINADA: Exportación CSV de Top Productos
            /*
            sb.AppendLine("\"Productos Más Vendidos (Top 20)\"");
            sb.AppendLine("\"Posición\",\"Código\",\"Producto\",\"Categoría\",\"Cantidad Vendida\",\"Total Vendido\",\"% del Total\"");
            if (_datosReporte.ContainsKey("TopProductos"))
            {
                var topProductos = (List<object>)_datosReporte["TopProductos"];
                foreach (dynamic producto in topProductos)
                {
                    sb.AppendLine($"\"{producto.Posicion}\",\"{producto.Codigo}\",\"{producto.ProductoNombre}\",\"{producto.Categoria}\",\"{producto.CantidadVendida}\",\"{producto.TotalVendido:F2}\",\"{producto.PorcentajeTotal:F1}\"");
                }
            }
            sb.AppendLine();
            */

            sb.AppendLine("\"Ventas por Empleado\"");
            sb.AppendLine("\"Empleado\",\"Número de Ventas\",\"Total Vendido\",\"Promedio por Venta\",\"Comisiones\",\"% del Total\"");
            if (_datosReporte.ContainsKey("VentasPorEmpleado"))
            {
                var ventasEmpleado = (List<object>)_datosReporte["VentasPorEmpleado"];
                foreach (dynamic empleado in ventasEmpleado)
                {
                    sb.AppendLine($"\"{empleado.Empleado}\",\"{empleado.NumeroVentas}\",\"{empleado.TotalVendido:F2}\",\"{empleado.PromedioVenta:F2}\",\"{empleado.Comisiones:F2}\",\"{empleado.PorcentajeTotal:F1}\"");
                }
            }
            sb.AppendLine();

            sb.AppendLine("\"Detalle de Ventas\"");
            sb.AppendLine("\"Factura\",\"Fecha\",\"Cliente\",\"Empleado\",\"Método Pago\",\"Subtotal\",\"Descuento\",\"Impuestos\",\"Total\",\"Estado\"");
            if (_ventasDelPeriodo != null)
            {
                foreach (var venta in _ventasDelPeriodo)
                {
                    sb.AppendLine($"\"{venta.NumeroFactura}\",\"{venta.Fecha:dd/MM/yyyy}\"," +
                                  $"\"{(venta.Cliente?.NombreCompleto ?? "")}\"," +
                                  $"\"{(venta.Empleado?.NombreCompleto ?? "")}\"," +
                                  $"\"{venta.MetodoPagoNombre ?? ""}\"," +
                                  $"\"{venta.Subtotal:F2}\",\"{venta.Descuento:F2}\",\"{venta.Impuestos:F2}\",\"{venta.Total:F2}\",\"{venta.EstadoDescripcion}\"");
                }
            }
            File.WriteAllText(fileName, sb.ToString(), Encoding.UTF8);
        }
    }
}