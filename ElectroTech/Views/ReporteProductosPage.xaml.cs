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
    public partial class ReporteProductosPage : Page
    {
        private readonly VentaService _ventaService;
        private readonly ProductoService _productoService;
        private readonly CategoriaService _categoriaService;
        private List<object> _topProductosReporte; // Para almacenar los datos del DataGrid

        public ReporteProductosPage()
        {
            InitializeComponent();
            _ventaService = new VentaService();
            _productoService = new ProductoService();
            _categoriaService = new CategoriaService();
            _topProductosReporte = new List<object>();
            InicializarFiltros();
        }

        private void InicializarFiltros()
        {
            try
            {
                dpFechaFin.SelectedDate = DateTime.Today;
                dpFechaInicio.SelectedDate = DateTime.Today.AddMonths(-1);

                var categorias = _categoriaService.ObtenerTodas();
                categorias.Insert(0, new Categoria { IdCategoria = 0, Nombre = "Todas las Categorías" });
                cbCategoria.ItemsSource = categorias;
                cbCategoria.SelectedIndex = 0;

                txtTopN.Text = "10"; // Valor por defecto
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al inicializar filtros del Reporte de Productos Más Vendidos");
                MessageBox.Show("Error al cargar los filtros: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidarFiltros(out DateTime fechaInicio, out DateTime fechaFin, out int? idCategoria, out int topN)
        {
            fechaInicio = DateTime.MinValue;
            fechaFin = DateTime.MinValue;
            idCategoria = null;
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
            if (!int.TryParse(txtTopN.Text, out topN) || topN <= 0)
            {
                MessageBox.Show("El valor de 'Top N' debe ser un número entero positivo.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            fechaInicio = dpFechaInicio.SelectedDate.Value;
            fechaFin = dpFechaFin.SelectedDate.Value;
            if (cbCategoria.SelectedValue != null && (int)cbCategoria.SelectedValue > 0)
            {
                idCategoria = (int)cbCategoria.SelectedValue;
            }
            return true;
        }

        private async void btnGenerar_Click(object sender, RoutedEventArgs e)
        {
            DateTime fechaInicio, fechaFin;
            int? idCategoria;
            int topN;

            if (!ValidarFiltros(out fechaInicio, out fechaFin, out idCategoria, out topN))
                return;

            try
            {
                MostrarCargando(true);
                btnExportar.IsEnabled = false;

                _topProductosReporte = await Task.Run(() =>
                    GenerarDatosProductosMasVendidos(fechaInicio, fechaFin, idCategoria, topN)
                );

                MostrarDatosEnUI();
                btnExportar.IsEnabled = _topProductosReporte.Any();
                txtEstadoReporte.Text = $"Reporte generado: Mostrando los {Math.Min(topN, _topProductosReporte.Count)} productos más vendidos.";
                Logger.LogInfo($"Reporte de productos más vendidos generado para el período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}, Categoría ID: {idCategoria?.ToString() ?? "Todas"}, Top: {topN}");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al generar reporte de productos más vendidos");
                MessageBox.Show("Error al generar el reporte: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                txtEstadoReporte.Text = "Error al generar el reporte.";
            }
            finally
            {
                MostrarCargando(false);
            }
        }

        // En ReporteProductosPage.xaml.cs
        // En ReporteProductosPage.xaml.cs
        // En ReporteProductosPage.xaml.cs
        private List<object> GenerarDatosProductosMasVendidos(DateTime fechaInicio, DateTime fechaFin, int? idCategoria, int topN)
        {
            var fechaFinAjustada = fechaFin.AddDays(1).AddTicks(-1);
            var ventasDelPeriodo = _ventaService.ObtenerPorFechas(fechaInicio, fechaFinAjustada);

            // Punto de depuración 1: Verificar ventasDelPeriodo iniciales
            if (ventasDelPeriodo == null || !ventasDelPeriodo.Any())
            {
                Logger.LogInfo("GenerarDatosProductosMasVendidos: (Punto 1) No se encontraron ventas iniciales para el período especificado.");
                return new List<object>(); // Devuelve lista vacía si no hay ventas
            }
            Logger.LogInfo($"GenerarDatosProductosMasVendidos: (Punto 1) Ventas iniciales encontradas: {ventasDelPeriodo.Count}");

            // Filtrar por estado completado
            ventasDelPeriodo = ventasDelPeriodo.Where(v => v.Estado == 'C').ToList();

            // Punto de depuración 2: Verificar ventas después de filtrar por estado 'C'
            if (!ventasDelPeriodo.Any())
            {
                Logger.LogInfo("GenerarDatosProductosMasVendidos: (Punto 2) No se encontraron ventas completadas ('C').");
                return new List<object>();
            }
            Logger.LogInfo($"GenerarDatosProductosMasVendidos: (Punto 2) Ventas completadas ('C') encontradas: {ventasDelPeriodo.Count}");

            // Filtrar por categoría si se seleccionó una
            if (idCategoria.HasValue && idCategoria.Value > 0)
            {
                List<Venta> ventasFiltradasPorCategoria = new List<Venta>();
                foreach (var v_iter in ventasDelPeriodo) // Renombrada 'v' para evitar conflicto con la 'v' del lambda
                {
                    if (v_iter.Detalles == null || !v_iter.Detalles.Any()) continue;

                    var detallesDeCategoria = v_iter.Detalles
                        .Where(d => {
                            var productoDeDetalle = _productoService.ObtenerPorId(d.IdProducto);
                            return productoDeDetalle != null && productoDeDetalle.IdCategoria == idCategoria.Value;
                        })
                        .ToList();

                    if (detallesDeCategoria.Any())
                    {
                        // Crear una nueva instancia de Venta y copiar propiedades manualmente
                        Venta ventaConDetallesFiltrados = new Venta
                        {
                            IdVenta = v_iter.IdVenta,
                            NumeroFactura = v_iter.NumeroFactura,
                            Fecha = v_iter.Fecha,
                            IdCliente = v_iter.IdCliente,
                            Cliente = v_iter.Cliente,
                            IdEmpleado = v_iter.IdEmpleado,
                            Empleado = v_iter.Empleado,
                            IdMetodoPago = v_iter.IdMetodoPago,
                            MetodoPagoNombre = v_iter.MetodoPagoNombre,
                            Subtotal = v_iter.Subtotal,
                            Descuento = v_iter.Descuento,
                            Impuestos = v_iter.Impuestos,
                            Total = v_iter.Total,
                            Observaciones = v_iter.Observaciones,
                            Estado = v_iter.Estado,
                            Detalles = detallesDeCategoria // Asignar la lista de detalles ya filtrados
                        };
                        ventasFiltradasPorCategoria.Add(ventaConDetallesFiltrados);
                    }
                }
                ventasDelPeriodo = ventasFiltradasPorCategoria;
            }

            // Punto de depuración 3: Verificar ventas después de filtrar por categoría
            if (!ventasDelPeriodo.Any())
            {
                Logger.LogInfo($"GenerarDatosProductosMasVendidos: (Punto 3) No se encontraron ventas después de filtrar por categoría ID: {idCategoria?.ToString() ?? "Todas"}.");
                return new List<object>();
            }
            Logger.LogInfo($"GenerarDatosProductosMasVendidos: (Punto 3) Ventas después de filtro de categoría: {ventasDelPeriodo.Count}");

            var productosVendidos = new Dictionary<int, (Producto ProductoInfo, int CantidadVendida, double TotalVendido)>();
            int contadorVentasConDetalles = 0;

            foreach (var venta in ventasDelPeriodo)
            {
                // Punto de depuración 4: Verificar si cada venta tiene detalles.
                if (venta.Detalles == null || !venta.Detalles.Any())
                {
                    Logger.LogWarning($"GenerarDatosProductosMasVendidos: (Punto 4) La venta {venta.NumeroFactura} (ID: {venta.IdVenta}) NO tiene detalles o su lista de detalles es null.");
                    continue;
                }
                contadorVentasConDetalles++;

                foreach (var detalle in venta.Detalles)
                {
                    if (detalle.IdProducto <= 0)
                    {
                        Logger.LogWarning($"GenerarDatosProductosMasVendidos: Detalle con IdProducto inválido ({detalle.IdProducto}) en venta {venta.NumeroFactura}.");
                        continue;
                    }

                    if (productosVendidos.TryGetValue(detalle.IdProducto, out var datosProd))
                    {
                        productosVendidos[detalle.IdProducto] = (datosProd.ProductoInfo, datosProd.CantidadVendida + detalle.Cantidad, datosProd.TotalVendido + detalle.Subtotal);
                    }
                    else
                    {
                        var producto = _productoService.ObtenerPorId(detalle.IdProducto);
                        // Punto de depuración 5: Verificar si se encuentra el producto y si tiene NombreCategoria.
                        if (producto != null)
                        {
                            if (string.IsNullOrEmpty(producto.NombreCategoria))
                            {
                                Logger.LogWarning($"GenerarDatosProductosMasVendidos: (Punto 5) Producto ID {producto.IdProducto} ('{producto.Nombre}') no tiene NombreCategoria asignado.");
                            }
                            productosVendidos[detalle.IdProducto] = (producto, detalle.Cantidad, detalle.Subtotal);
                        }
                        else
                        {
                            Logger.LogWarning($"GenerarDatosProductosMasVendidos: (Punto 5) Producto con ID {detalle.IdProducto} de la venta {venta.NumeroFactura} no fue encontrado por ProductoService.");
                        }
                    }
                }
            }
            Logger.LogInfo($"GenerarDatosProductosMasVendidos: (Punto 4) Ventas procesadas con detalles: {contadorVentasConDetalles} de {ventasDelPeriodo.Count}");

            // Punto de depuración 6: Verificar si se agregó algo a productosVendidos
            if (!productosVendidos.Any())
            {
                Logger.LogInfo("GenerarDatosProductosMasVendidos: (Punto 6) El diccionario productosVendidos está vacío. Posibles causas: las ventas no tenían detalles, los detalles no tenían productos válidos, o los productos no se encontraron/cargaron correctamente.");
                return new List<object>();
            }
            Logger.LogInfo($"GenerarDatosProductosMasVendidos: (Punto 6) Productos distintos acumulados en diccionario: {productosVendidos.Count}");

            double totalVentasGlobalDelPeriodo = ventasDelPeriodo.Sum(v => v.Total);

            var resultadoFinal = productosVendidos.Values
                .OrderByDescending(p => p.CantidadVendida)
                .Take(topN)
                .Select((p, index) => new
                {
                    Posicion = index + 1,
                    Codigo = p.ProductoInfo?.Codigo ?? "N/A",
                    ProductoNombre = p.ProductoInfo?.Nombre ?? "Desconocido",
                    Categoria = p.ProductoInfo?.NombreCategoria ?? "N/A",
                    p.CantidadVendida,
                    p.TotalVendido,
                    PorcentajeSobreTotalVentas = totalVentasGlobalDelPeriodo > 0 ? (p.TotalVendido / totalVentasGlobalDelPeriodo) * 100 : 0
                })
                .Cast<object>()
                .ToList();

            Logger.LogInfo($"GenerarDatosProductosMasVendidos: (Punto 7) Se generó una lista de {resultadoFinal.Count} productos para el top {topN}.");
            if (!resultadoFinal.Any() && productosVendidos.Any())
            {
                Logger.LogWarning("GenerarDatosProductosMasVendidos: (Punto 7) productosVendidos tenía datos, pero resultadoFinal está vacío. Revisa la lógica de Take o Select (poco probable si topN > 0).");
            }
            return resultadoFinal;
        }

        private void MostrarDatosEnUI()
        {
            Dispatcher.Invoke(() =>
            {
                dgTopProductos.ItemsSource = _topProductosReporte;
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
            if (_topProductosReporte == null || !_topProductosReporte.Any())
            {
                MessageBox.Show("Debe generar el reporte antes de exportar.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                // Añadir PDF a las opciones de filtro
                Filter = "Archivo PDF (*.pdf)|*.pdf|Archivo CSV (*.csv)|*.csv|Archivo de texto (*.txt)|*.txt",
                DefaultExt = "pdf", // Hacer PDF el formato por defecto
                FileName = $"ReporteProductosMasVendidos_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    string filePath = saveDialog.FileName;
                    switch (saveDialog.FilterIndex)
                    {
                        case 1: // PDF
                            ExportarProductosPDF(filePath);
                            break;
                        case 2: // CSV
                            // Llama a tu método existente ExportarProductosCSV(filePath);
                            // Necesitarías crear este método similar a como tienes para ventas.
                            ExportarDatosGenericoCSV(filePath, _topProductosReporte,
                                new string[] { "Pos.", "Código", "Producto", "Categoría", "Cant. Vendida", "Total Vendido", "% S/Total Ventas" },
                                item => new object[] { item.Posicion, item.Codigo, item.ProductoNombre, item.Categoria, item.CantidadVendida, item.TotalVendido, item.PorcentajeSobreTotalVentas });
                            break;
                        case 3: // TXT
                            // Llama a tu método existente ExportarProductosTXT(filePath);
                            ExportarDatosGenericoTXT(filePath, _topProductosReporte,
                                $"Reporte de Productos Más Vendidos al {DateTime.Now:dd/MM/yyyy HH:mm:ss}",
                                $"Período: {dpFechaInicio.SelectedDate:dd/MM/yyyy} - {dpFechaFin.SelectedDate:dd/MM/yyyy}",
                                $"Categoría: {(cbCategoria.SelectedItem as Categoria)?.Nombre ?? "Todas"}",
                                $"Top N: {txtTopN.Text}",
                                new string[] { "Pos.", "Código", "Producto", "Categoría", "Cant. Vendida", "Total Vendido", "% S/Total Ventas" },
                                item => $"{item.Posicion.ToString().PadRight(4)} | {item.Codigo.PadRight(10)} | {item.ProductoNombre.PadRight(35)} | {item.Categoria.PadRight(20)} | {item.CantidadVendida.ToString().PadLeft(12)} | {item.TotalVendido.ToString("C", CultureInfo.CurrentCulture).PadLeft(18)} | {item.PorcentajeSobreTotalVentas.ToString("F2", CultureInfo.CurrentCulture).PadLeft(15)}%");
                            break;
                    }

                    MessageBox.Show("Reporte exportado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "Error al exportar reporte de productos más vendidos");
                    MessageBox.Show("Error al exportar el reporte: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void ExportarProductosPDF(string filePath)
        {
            Document document = new Document(PageSize.A4, 25, 25, 30, 30); // Márgenes: izq, der, arr, ab
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));

            // Definir fuentes
            BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font titleFont = new Font(baseFont, 16, Font.BOLD);
            Font headerFont = new Font(baseFont, 10, Font.BOLD, BaseColor.WHITE);
            Font cellFont = new Font(baseFont, 9, Font.NORMAL);
            Font subHeaderFont = new Font(baseFont, 10, Font.ITALIC);


            document.Open();

            // Título del Reporte
            Paragraph title = new Paragraph("Reporte de Productos Más Vendidos", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20f
            };
            document.Add(title);

            // Información del Período y Filtros
            document.Add(new Paragraph($"Período: {dpFechaInicio.SelectedDate:dd/MM/yyyy} - {dpFechaFin.SelectedDate:dd/MM/yyyy}", subHeaderFont));
            document.Add(new Paragraph($"Categoría: {(cbCategoria.SelectedItem as Categoria)?.Nombre ?? "Todas"}", subHeaderFont));
            document.Add(new Paragraph($"Top N: {txtTopN.Text}", subHeaderFont));
            document.Add(new Paragraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", subHeaderFont) { SpacingAfter = 15f });


            // Crear tabla para los productos
            PdfPTable table = new PdfPTable(7); // 7 columnas
            table.WidthPercentage = 100;
            float[] widths = new float[] { 0.5f, 1f, 3f, 1.5f, 1.2f, 1.3f, 1.3f }; // Anchos relativos
            table.SetWidths(widths);

            // Encabezados de la tabla
            string[] headers = { "Pos.", "Código", "Producto", "Categoría", "Cant. Vendida", "Total Vendido", "% S/Total Ventas" };
            foreach (string header in headers)
            {
                PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                {
                    BackgroundColor = new BaseColor(63, 81, 181), // Un azul bonito
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    Padding = 5
                };
                table.AddCell(cell);
            }

            // Filas de datos
            if (_topProductosReporte != null)
            {
                foreach (dynamic item in _topProductosReporte)
                {
                    table.AddCell(new PdfPCell(new Phrase(item.Posicion.ToString(), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(item.Codigo.ToString(), cellFont)) { Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(item.ProductoNombre.ToString(), cellFont)) { Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(item.Categoria.ToString(), cellFont)) { Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(item.CantidadVendida.ToString(), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(item.TotalVendido.ToString("C", CultureInfo.CurrentCulture), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(item.PorcentajeSobreTotalVentas.ToString("F2", CultureInfo.CurrentCulture) + "%", cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
                }
            }

            document.Add(table);
            document.Close();
            writer.Close();
        }

        // Métodos genéricos para exportar CSV y TXT que puedes crear o adaptar
        private void ExportarDatosGenericoCSV(string fileName, IEnumerable<object> datos, string[] encabezados, Func<dynamic, object[]> selectorDeValores)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", encabezados.Select(h => $"\"{h.Replace("\"", "\"\"")}\""))); // Encabezados entre comillas

            foreach (var item in datos)
            {
                var valores = selectorDeValores(item);
                sb.AppendLine(string.Join(",", valores.Select(v => $"\"{(v ?? "").ToString().Replace("\"", "\"\"")}\"")));
            }
            File.WriteAllText(fileName, sb.ToString(), Encoding.UTF8);
        }

        private void ExportarDatosGenericoTXT(string fileName, IEnumerable<object> datos, string tituloReporte, string infoPeriodo, string infoCategoria, string infoTopN, string[] encabezados, Func<dynamic, string> formateadorDeLinea)
        {
            var sb = new StringBuilder();
            sb.AppendLine(new string('=', tituloReporte.Length + 4));
            sb.AppendLine($"  {tituloReporte.ToUpper()}  ");
            sb.AppendLine(new string('=', tituloReporte.Length + 4));
            sb.AppendLine(infoPeriodo);
            if (!string.IsNullOrEmpty(infoCategoria)) sb.AppendLine(infoCategoria);
            if (!string.IsNullOrEmpty(infoTopN)) sb.AppendLine(infoTopN);
            sb.AppendLine($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine();

            // Aquí podrías crear una línea de encabezado con anchos fijos como en tus otros reportes de texto.
            // Por simplicidad, solo pongo los nombres.
            sb.AppendLine(string.Join(" | ", encabezados));
            sb.AppendLine(new string('-', encabezados.Sum(h => h.Length) + (encabezados.Length - 1) * 3));


            foreach (dynamic item in datos)
            {
                sb.AppendLine(formateadorDeLinea(item));
            }
            File.WriteAllText(fileName, sb.ToString(), Encoding.UTF8);
        }


    }
}