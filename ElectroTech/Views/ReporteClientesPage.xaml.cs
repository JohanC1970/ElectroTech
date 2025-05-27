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

namespace ElectroTech.Views.Reportes
{
    public partial class ReporteClientesPage : Page
    {
        private readonly VentaService _ventaService;
        private readonly ClienteService _clienteService;
        private List<object> _topClientesReporte;

        public ReporteClientesPage()
        {
            InitializeComponent();
            _ventaService = new VentaService();
            _clienteService = new ClienteService();
            _topClientesReporte = new List<object>();
            InicializarFiltros();
        }

        private void InicializarFiltros()
        {
            try
            {
                dpFechaFin.SelectedDate = DateTime.Today;
                dpFechaInicio.SelectedDate = DateTime.Today.AddMonths(-1);
                cbOrdenarPor.SelectedIndex = 0; // Por defecto, "Mayor Cantidad de Compras"
                txtTopN.Text = "10";
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al inicializar filtros del Reporte de Clientes Frecuentes");
                MessageBox.Show("Error al cargar los filtros: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidarFiltros(out DateTime fechaInicio, out DateTime fechaFin, out string ordenarPorTag, out int topN)
        {
            fechaInicio = DateTime.MinValue;
            fechaFin = DateTime.MinValue;
            ordenarPorTag = "Cantidad";
            topN = 10;

            if (!dpFechaInicio.SelectedDate.HasValue)
            {
                MessageBox.Show("Debe seleccionar una fecha de inicio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!dpFechaFin.SelectedDate.HasValue)
            {
                MessageBox.Show("Debe seleccionar una fecha de fin.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (dpFechaInicio.SelectedDate.Value > dpFechaFin.SelectedDate.Value)
            {
                MessageBox.Show("La fecha de inicio no puede ser mayor a la fecha de fin.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (cbOrdenarPor.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar un criterio de ordenación.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!int.TryParse(txtTopN.Text, out topN) || topN <= 0)
            {
                MessageBox.Show("El valor de 'Top N' debe ser un número entero positivo.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            fechaInicio = dpFechaInicio.SelectedDate.Value;
            fechaFin = dpFechaFin.SelectedDate.Value;
            ordenarPorTag = (cbOrdenarPor.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Cantidad";
            return true;
        }

        private async void btnGenerar_Click(object sender, RoutedEventArgs e)
        {
            DateTime fechaInicio, fechaFin;
            string ordenarPorTag;
            int topN;

            if (!ValidarFiltros(out fechaInicio, out fechaFin, out ordenarPorTag, out topN))
                return;

            try
            {
                MostrarCargando(true);
                btnExportar.IsEnabled = false;

                _topClientesReporte = await Task.Run(() =>
                    GenerarDatosClientesFrecuentes(fechaInicio, fechaFin, ordenarPorTag, topN)
                );

                MostrarDatosEnUI();
                btnExportar.IsEnabled = _topClientesReporte.Any();
                txtEstadoReporte.Text = $"Reporte generado: Mostrando los {Math.Min(topN, _topClientesReporte.Count)} clientes más frecuentes.";
                Logger.LogInfo($"Reporte de clientes frecuentes generado para el período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}, Orden: {ordenarPorTag}, Top: {topN}");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al generar reporte de clientes frecuentes");
                MessageBox.Show("Error al generar el reporte: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                txtEstadoReporte.Text = "Error al generar el reporte.";
            }
            finally
            {
                MostrarCargando(false);
            }
        }

        private List<object> GenerarDatosClientesFrecuentes(DateTime fechaInicio, DateTime fechaFin, string ordenarPor, int topN)
        {
            var fechaFinAjustada = fechaFin.AddDays(1).AddTicks(-1);
            var ventasDelPeriodo = _ventaService.ObtenerPorFechas(fechaInicio, fechaFinAjustada);

            if (ventasDelPeriodo == null || !ventasDelPeriodo.Any())
            {
                Logger.LogInfo("GenerarDatosClientesFrecuentes: No se encontraron ventas para el período especificado.");
                return new List<object>();
            }

            // Filtrar por estado completado y que tengan IdCliente válido
            ventasDelPeriodo = ventasDelPeriodo.Where(v => v.Estado == 'C' && v.IdCliente > 0).ToList();

            if (!ventasDelPeriodo.Any())
            {
                Logger.LogInfo("GenerarDatosClientesFrecuentes: No se encontraron ventas completadas ('C') con clientes válidos.");
                return new List<object>();
            }

            var clientesAgrupados = ventasDelPeriodo
                .GroupBy(v => v.IdCliente)
                .Select(g => new
                {
                    IdCliente = g.Key,
                    CantidadCompras = g.Count(),
                    MontoTotalComprado = g.Sum(v => v.Total),
                    FechaUltimaCompra = g.Max(v => v.Fecha)
                });

            // Ordenar según el criterio
            if (ordenarPor == "Monto")
            {
                clientesAgrupados = clientesAgrupados.OrderByDescending(c => c.MontoTotalComprado).ThenByDescending(c => c.CantidadCompras);
            }
            else // Por defecto (o si es "Cantidad")
            {
                clientesAgrupados = clientesAgrupados.OrderByDescending(c => c.CantidadCompras).ThenByDescending(c => c.MontoTotalComprado);
            }

            var topClientesAgrupados = clientesAgrupados.Take(topN).ToList();

            if (!topClientesAgrupados.Any())
            {
                Logger.LogInfo("GenerarDatosClientesFrecuentes: No hay clientes después de agrupar y tomar el top N.");
                return new List<object>();
            }

            var resultadoFinal = new List<object>();
            int posicion = 1;
            foreach (var clienteAgrupado in topClientesAgrupados)
            {
                var infoCliente = _clienteService.ObtenerPorId(clienteAgrupado.IdCliente); // Asume que tienes este método
                resultadoFinal.Add(new
                {
                    Posicion = posicion++,
                    NombreCliente = infoCliente?.NombreCompleto ?? $"Cliente ID: {clienteAgrupado.IdCliente}",
                    DocumentoCliente = infoCliente != null ? $"{infoCliente.TipoDocumento} {infoCliente.NumeroDocumento}" : "N/A",
                    clienteAgrupado.CantidadCompras,
                    clienteAgrupado.MontoTotalComprado,
                    clienteAgrupado.FechaUltimaCompra
                });
            }

            Logger.LogInfo($"GenerarDatosClientesFrecuentes: Se generó una lista de {resultadoFinal.Count} clientes.");
            return resultadoFinal;
        }

        private void MostrarDatosEnUI()
        {
            Dispatcher.Invoke(() =>
            {
                dgTopClientes.ItemsSource = _topClientesReporte;
            });
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
            if (_topClientesReporte == null || !_topClientesReporte.Any())
            {
                MessageBox.Show("Debe generar el reporte antes de exportar.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Archivo PDF (*.pdf)|*.pdf|Archivo CSV (*.csv)|*.csv|Archivo de texto (*.txt)|*.txt",
                DefaultExt = "pdf",
                FileName = $"ReporteClientesFrecuentes_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                string filePath = saveDialog.FileName;
                try
                {
                    switch (saveDialog.FilterIndex)
                    {
                        case 1: // PDF
                            ExportarClientesPDF(filePath); // Llama a tu método existente, ahora modificado
                            break;
                        case 2: // CSV
                            ExportarClientesCSV(filePath); // Tu método existente
                            break;
                        case 3: // TXT
                            ExportarClientesTXT(filePath); // Tu método existente
                            break;
                    }

                    MessageBox.Show("Reporte exportado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "Error al exportar reporte de clientes frecuentes");
                    MessageBox.Show("Error al exportar el reporte: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // El método ExportarClientesPDF que ya tenías, lo revisamos para asegurar que esté bien:
        private void ExportarClientesPDF(string filePath)
        {
            Document document = new Document(PageSize.A4.Rotate(), 25, 25, 30, 30);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));

            BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font titleFont = new Font(baseFont, 16, Font.BOLD);
            Font subTitleFont = new Font(baseFont, 14, Font.BOLD);
            Font headerFont = new Font(baseFont, 9, Font.BOLD, BaseColor.WHITE);
            Font cellFont = new Font(baseFont, 8, Font.NORMAL);
            Font smallItalicFont = new Font(baseFont, 8, Font.ITALIC);

            document.Open();

            Paragraph title = new Paragraph("Reporte de Clientes Más Frecuentes", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 15f
            };
            document.Add(title);

            document.Add(new Paragraph($"Período: {dpFechaInicio.SelectedDate:dd/MM/yyyy} - {dpFechaFin.SelectedDate:dd/MM/yyyy}", smallItalicFont));
            document.Add(new Paragraph($"Ordenado por: {(cbOrdenarPor.SelectedItem as ComboBoxItem)?.Content}", smallItalicFont));
            document.Add(new Paragraph($"Top N: {txtTopN.Text}", smallItalicFont));
            document.Add(new Paragraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", smallItalicFont) { SpacingAfter = 10f });

            if (_topClientesReporte != null && _topClientesReporte.Any())
            {
                PdfPTable table = new PdfPTable(6);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 0.5f, 2.5f, 1.2f, 1f, 1.5f, 1f });

                string[] headers = { "Pos.", "Cliente", "Documento", "Cant. Compras", "Monto Total", "Última Compra" };
                foreach (string header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = new BaseColor(0, 123, 255),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 4
                    };
                    table.AddCell(cell);
                }

                foreach (dynamic item in _topClientesReporte)
                {
                    table.AddCell(new PdfPCell(new Phrase(item.Posicion.ToString(), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
                    table.AddCell(new PdfPCell(new Phrase(item.NombreCliente.ToString(), cellFont)) { Padding = 3 });
                    table.AddCell(new PdfPCell(new Phrase(item.DocumentoCliente.ToString(), cellFont)) { Padding = 3 });
                    table.AddCell(new PdfPCell(new Phrase(item.CantidadCompras.ToString(), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
                    table.AddCell(new PdfPCell(new Phrase(item.MontoTotalComprado.ToString("C", CultureInfo.CurrentCulture), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
                    table.AddCell(new PdfPCell(new Phrase(((DateTime)item.FechaUltimaCompra).ToString("dd/MM/yyyy"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 3 });
                }
                document.Add(table);
            }
            else
            {
                document.Add(new Paragraph("No hay datos de clientes para mostrar según los filtros seleccionados.", cellFont));
            }

            document.Close();
            writer.Close();
        }

        private void ExportarClientesCSV(string filePath)
        {
            if (_topClientesReporte == null || !_topClientesReporte.Any())
            {
                MessageBox.Show("No hay datos para exportar.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            StringBuilder sb = new StringBuilder();

            // Encabezados del reporte y filtros aplicados
            sb.AppendLine($"\"Reporte de Clientes Más Frecuentes al {DateTime.Now:dd/MM/yyyy HH:mm:ss}\"");
            sb.AppendLine($"\"Período: {dpFechaInicio.SelectedDate:dd/MM/yyyy} - {dpFechaFin.SelectedDate:dd/MM/yyyy}\"");
            sb.AppendLine($"\"Ordenado por: {(cbOrdenarPor.SelectedItem as ComboBoxItem)?.Content}\"");
            sb.AppendLine($"\"Top N: {txtTopN.Text}\"");
            sb.AppendLine(); // Línea en blanco para separar

            // Encabezados de las columnas de datos
            sb.AppendLine($"\"Pos.\",\"Cliente\",\"Documento\",\"Cant. Compras\",\"Monto Total Comprado\",\"Última Compra\"");

            // Datos
            foreach (dynamic item in _topClientesReporte)
            {
                sb.AppendLine(
                    $"\"{item.Posicion}\"," +
                    $"\"{item.NombreCliente?.ToString().Replace("\"", "\"\"") ?? ""}\"," + // Manejo de comillas en nombres
                    $"\"{item.DocumentoCliente?.ToString().Replace("\"", "\"\"") ?? ""}\"," +
                    $"\"{item.CantidadCompras}\"," +
                    $"\"{item.MontoTotalComprado:F2}\"," + // Formato numérico para CSV
                    $"\"{(item.FechaUltimaCompra as DateTime?)?.ToString("dd/MM/yyyy") ?? ""}\""
                );
            }

            try
            {
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al escribir archivo CSV para Reporte Clientes: {filePath}");
                throw; // Relanza para que el catch en btnExportar_Click lo maneje
            }
        }

        private void ExportarClientesTXT(string filePath)
        {
            if (_topClientesReporte == null || !_topClientesReporte.Any())
            {
                MessageBox.Show("No hay datos para exportar.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var sb = new StringBuilder();
            string titulo = "REPORTE DE CLIENTES MÁS FRECUENTES";

            sb.AppendLine(new string('=', titulo.Length + 4));
            sb.AppendLine($"  {titulo}  ");
            sb.AppendLine(new string('=', titulo.Length + 4));
            sb.AppendLine($"Período: {dpFechaInicio.SelectedDate:dd/MM/yyyy} - {dpFechaFin.SelectedDate:dd/MM/yyyy}");
            sb.AppendLine($"Ordenado por: {(cbOrdenarPor.SelectedItem as ComboBoxItem)?.Content}");
            sb.AppendLine($"Top N: {txtTopN.Text}");
            sb.AppendLine($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine();

            // Definir anchos para formato de texto y encabezados
            int wPos = 4, wCli = 35, wDoc = 18, wCant = 10, wMonto = 18, wFecha = 12;
            string headerFormat = $"{{0,-{wPos}}} | {{1,-{wCli}}} | {{2,-{wDoc}}} | {{3,{wCant}}} | {{4,{wMonto}}} | {{5,-{wFecha}}}";

            sb.AppendLine(string.Format(headerFormat,
                "Pos.", "Cliente", "Documento", "Compras", "Monto Total", "Últ. Compra"));
            sb.AppendLine(new string('-', wPos + wCli + wDoc + wCant + wMonto + wFecha + (5 * 3))); // 5 separadores " | "

            foreach (dynamic item in _topClientesReporte)
            {
                string nombreCliente = item.NombreCliente?.ToString() ?? "N/A";
                string documentoCliente = item.DocumentoCliente?.ToString() ?? "N/A";
                string fechaUltimaCompra = (item.FechaUltimaCompra as DateTime?)?.ToString("dd/MM/yyyy") ?? "N/A";

                sb.AppendLine(string.Format(headerFormat,
                    item.Posicion,
                    TruncateString(nombreCliente, wCli),
                    TruncateString(documentoCliente, wDoc),
                    item.CantidadCompras.ToString().PadLeft(wCant),
                    item.MontoTotalComprado.ToString("C", CultureInfo.CurrentCulture).PadLeft(wMonto),
                    fechaUltimaCompra.PadRight(wFecha)
                ));
            }

            try
            {
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al escribir archivo TXT para Reporte Clientes: {filePath}");
                throw; // Relanza para que el catch en btnExportar_Click lo maneje
            }
        }

        private string TruncateString(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
                return input;
            return input.Substring(0, maxLength - 3) + "..."; // Agrega "..." si se trunca

        }
    }
}