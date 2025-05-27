using ElectroTech.Helpers;
using ElectroTech.Models;
using ElectroTech.Services;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization; // Para CultureInfo en el formateo de números
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes; // Necesario para Line, Rectangle, Ellipse

namespace ElectroTech.Views.Reportes
{
    public partial class ReporteInventarioPage : Page
    {
        private readonly ProductoService _productoService;
        private readonly CategoriaService _categoriaService;
        private List<Producto> _productosParaReporte;
        private Dictionary<string, object> _datosResumen;

        public ReporteInventarioPage()
        {
            InitializeComponent();
            _productoService = new ProductoService();
            _categoriaService = new CategoriaService();
            _productosParaReporte = new List<Producto>();
            _datosResumen = new Dictionary<string, object>();
            InicializarFiltros();
        }

        private void InicializarFiltros()
        {
            try
            {
                var categorias = _categoriaService.ObtenerTodas();
                categorias.Insert(0, new Categoria { IdCategoria = 0, Nombre = "Todas las Categorías" });
                cbCategoria.ItemsSource = categorias;
                cbCategoria.SelectedIndex = 0;
                cbEstadoStock.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al inicializar filtros del reporte de inventario");
                MessageBox.Show("Error al cargar los filtros: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnGenerar_Click(object sender, RoutedEventArgs e)
        {
            int? idCategoriaSeleccionada = cbCategoria.SelectedValue as int?;
            string tagEstadoStock = (cbEstadoStock.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "T";

            try
            {
                MostrarCargando(true);
                btnExportar.IsEnabled = false;

                await Task.Run(() => GenerarDatosReporte(idCategoriaSeleccionada, tagEstadoStock));

                MostrarDatosEnUI(); // Esto incluye el gráfico
                btnExportar.IsEnabled = true;
                txtEstadoReporte.Text = $"Reporte generado: {_productosParaReporte.Count} productos encontrados.";
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al generar reporte de inventario");
                MessageBox.Show("Error al generar el reporte: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtEstadoReporte.Text = "Error al generar el reporte.";
            }
            finally
            {
                MostrarCargando(false);
            }
        }

        private void GenerarDatosReporte(int? idCategoria, string estadoStockTag)
        {
            try
            {
                _productosParaReporte = _productoService.ObtenerTodos();

                if (idCategoria.HasValue && idCategoria.Value > 0)
                {
                    _productosParaReporte = _productosParaReporte.Where(p => p.IdCategoria == idCategoria.Value).ToList();
                }

                switch (estadoStockTag)
                {
                    case "N":
                        _productosParaReporte = _productosParaReporte.Where(p => p.CantidadDisponible >= p.StockMinimo && p.CantidadDisponible > 0).ToList();
                        break;
                    case "B":
                        _productosParaReporte = _productosParaReporte.Where(p => p.CantidadDisponible < p.StockMinimo && p.CantidadDisponible > 0).ToList();
                        break;
                    case "S":
                        _productosParaReporte = _productosParaReporte.Where(p => p.CantidadDisponible == 0).ToList();
                        break;
                }

                CalcularDatosResumen();
                CalcularValorInventarioPorCategoria(); // Nuevo método para datos del gráfico
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error procesando datos para reporte de inventario");
                throw new Exception("Error al procesar los datos del reporte de inventario.", ex);
            }
        }

        private void CalcularDatosResumen()
        {
            _datosResumen.Clear();
            List<Producto> todosLosProductosActivos = _productoService.ObtenerTodos();

            _datosResumen["ValorTotalInventario"] = _productosParaReporte.Sum(p => p.ValorInventario);
            _datosResumen["TotalProductosActivos"] = todosLosProductosActivos.Count;
            _datosResumen["ProductosCoincidentesFiltro"] = _productosParaReporte.Count;

            var productosBajoStockGlobal = todosLosProductosActivos.Where(p => p.RequiereReposicion && p.Activo).ToList();
            _datosResumen["ProductosBajoStockGlobal"] = productosBajoStockGlobal.Count;
            _datosResumen["ListaProductosBajoStockGlobal"] = productosBajoStockGlobal
                .Select(p => new { // Objeto anónimo
                    p.Codigo,
                    p.Nombre,
                    p.NombreCategoria,
                    p.StockMinimo,
                    p.CantidadDisponible,
                    DiferenciaStock = p.StockMinimo - p.CantidadDisponible
                })
                .Cast<object>() // Convertir cada elemento a object
                .ToList();      // Crear una List<object>
        }

        private void CalcularValorInventarioPorCategoria()
        {
            // Usaremos _productosParaReporte (ya filtrados) para el gráfico
            var valorPorCategoriaTemporal = _productosParaReporte
                .GroupBy(p => p.NombreCategoria ?? "Sin Categoría")
                .Select(g => new // Esto sigue siendo un tipo anónimo
                {
                    Categoria = g.Key,
                    ValorTotal = g.Sum(p => p.ValorInventario)
                })
                .Where(item => item.ValorTotal > 0)
                .OrderByDescending(item => item.ValorTotal)
                .ToList(); // valorPorCategoriaTemporal es List<TipoAnonimo>

            // Convierte la lista de tipos anónimos a List<dynamic> ANTES de guardarla
            _datosResumen["ValorInventarioPorCategoria"] = valorPorCategoriaTemporal.Cast<dynamic>().ToList();
        }


        private void MostrarDatosEnUI()
        {
            Dispatcher.Invoke(() =>
            {
                if (_datosResumen == null || !_datosResumen.Any())
                {
                    txtEstadoReporte.Text = "No hay datos de resumen para mostrar.";
                    dgInventarioProductos.ItemsSource = null;
                    dgProductosBajoStockDetalle.ItemsSource = null;
                    txtValorTotalInventario.Text = "$0.00";
                    txtTotalProductosActivos.Text = "0";
                    txtProductosBajoStock.Text = "0";
                    canvasInventarioCategoria.Children.Clear();
                    return;
                }

                txtValorTotalInventario.Text = ((decimal)_datosResumen["ValorTotalInventario"]).ToString("C");
                txtTotalProductosActivos.Text = _datosResumen["TotalProductosActivos"].ToString();
                txtProductosBajoStock.Text = _datosResumen["ProductosBajoStockGlobal"].ToString();

                dgInventarioProductos.ItemsSource = _productosParaReporte;

                if (_datosResumen.ContainsKey("ListaProductosBajoStockGlobal"))
                {
                    // dgProductosBajoStockDetalle.ItemsSource espera una colección, y List<object> funciona bien.
                    dgProductosBajoStockDetalle.ItemsSource = (List<object>)_datosResumen["ListaProductosBajoStockGlobal"];
                }
                else
                {
                    dgProductosBajoStockDetalle.ItemsSource = null;
                }

                if (_datosResumen.ContainsKey("ValorInventarioPorCategoria"))
                {
                    // Ahora _datosResumen["ValorInventarioPorCategoria"] es List<dynamic>, el cast funcionará.
                    DibujarGraficoInventarioPorCategoria((List<dynamic>)_datosResumen["ValorInventarioPorCategoria"]);
                }
                else
                {
                    canvasInventarioCategoria.Children.Clear();
                }
            });
        }

        private void DibujarGraficoInventarioPorCategoria(List<dynamic> datosGrafico)
        {
            canvasInventarioCategoria.Children.Clear();
            if (datosGrafico == null || !datosGrafico.Any()) return;

            double canvasWidth = canvasInventarioCategoria.ActualWidth;
            double canvasHeight = canvasInventarioCategoria.ActualHeight;

            if (canvasWidth <= 0 || canvasHeight <= 0)
            {
                // Si el canvas aún no tiene dimensiones, usar valores por defecto o esperar a que se cargue
                canvasWidth = 600; // Valor por defecto
                canvasHeight = 300; // Valor por defecto
                // Podrías forzar una actualización del layout si es necesario aquí
                // o llamar a este método de nuevo en el evento SizeChanged del Canvas.
            }


            double padding = 30; // Espacio para etiquetas y ejes
            double graphHeight = canvasHeight - (2 * padding);
            double graphWidth = canvasWidth - (2 * padding);

            decimal maxValor = datosGrafico.Max(item => (decimal)item.ValorTotal);
            if (maxValor == 0) maxValor = 1; // Evitar división por cero

            int numBarras = datosGrafico.Count;
            if (numBarras == 0) return;

            double anchoBarra = graphWidth / (numBarras * 1.5); // 1.5 para espaciado entre barras
            double espacioBarra = anchoBarra * 0.5;

            // Eje Y (Valor)
            Line ejeY = new Line { X1 = padding, Y1 = padding, X2 = padding, Y2 = canvasHeight - padding, Stroke = Brushes.Gray, StrokeThickness = 1 };
            canvasInventarioCategoria.Children.Add(ejeY);

            // Eje X (Categorías)
            Line ejeX = new Line { X1 = padding, Y1 = canvasHeight - padding, X2 = canvasWidth - padding, Y2 = canvasHeight - padding, Stroke = Brushes.Gray, StrokeThickness = 1 };
            canvasInventarioCategoria.Children.Add(ejeX);

            // Etiquetas Eje Y (simplificado, podrías añadir más marcas)
            TextBlock maxValLabel = new TextBlock { Text = maxValor.ToString("C0", CultureInfo.CurrentCulture), FontSize = 10, Foreground = Brushes.DarkSlateGray };
            Canvas.SetLeft(maxValLabel, padding - maxValLabel.ActualWidth - 5 > 0 ? padding - maxValLabel.ActualWidth - 5 : 0); // Ajustar para que no se salga
            Canvas.SetTop(maxValLabel, padding - (maxValLabel.ActualHeight / 2));
            canvasInventarioCategoria.Children.Add(maxValLabel);

            TextBlock zeroValLabel = new TextBlock { Text = "$0", FontSize = 10, Foreground = Brushes.DarkSlateGray };
            Canvas.SetLeft(zeroValLabel, padding - zeroValLabel.ActualWidth - 5 > 0 ? padding - zeroValLabel.ActualWidth - 5 : 0);
            Canvas.SetTop(zeroValLabel, canvasHeight - padding - (zeroValLabel.ActualHeight / 2));
            canvasInventarioCategoria.Children.Add(zeroValLabel);


            for (int i = 0; i < numBarras; i++)
            {
                var item = datosGrafico[i];
                decimal valor = item.ValorTotal;
                string categoria = item.Categoria;

                double alturaBarra = (double)(valor / maxValor) * graphHeight;
                if (alturaBarra < 0) alturaBarra = 0; // Asegurar que no sea negativo

                System.Windows.Shapes.Rectangle barra = new System.Windows.Shapes.Rectangle
                {
                    Width = anchoBarra,
                    Height = alturaBarra,
                    Fill = GetColorForBar(i), // Función para obtener colores diferentes
                    ToolTip = $"{categoria}: {valor:C}"
                };

                Canvas.SetLeft(barra, padding + (i * (anchoBarra + espacioBarra)) + (espacioBarra / 2));
                Canvas.SetBottom(barra, padding); // Canvas.SetBottom alinea con el borde inferior del canvas menos el padding
                canvasInventarioCategoria.Children.Add(barra);

                // Etiqueta de Categoría (Eje X)
                TextBlock etiquetaCategoria = new TextBlock
                {
                    Text = TruncateString(categoria, 15), // Truncar si es muy largo
                    FontSize = 10,
                    Foreground = Brushes.DarkSlateGray,
                    ToolTip = categoria
                };
                etiquetaCategoria.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity)); // Para obtener ActualWidth
                double etiquetaX = padding + (i * (anchoBarra + espacioBarra)) + (espacioBarra / 2) + (anchoBarra / 2) - (etiquetaCategoria.ActualWidth / 2);

                Canvas.SetLeft(etiquetaCategoria, etiquetaX);
                Canvas.SetTop(etiquetaCategoria, canvasHeight - padding + 5); // Debajo del eje X
                canvasInventarioCategoria.Children.Add(etiquetaCategoria);
            }
        }

        private string TruncateString(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }

       
        private void CanvasInventarioCategoria_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Volver a dibujar el gráfico si ya hay datos cargados
            if (_datosResumen != null && _datosResumen.ContainsKey("ValorInventarioPorCategoria") &&
                _datosResumen["ValorInventarioPorCategoria"] is List<dynamic> datosGrafico && datosGrafico.Any())
            {
                DibujarGraficoInventarioPorCategoria(datosGrafico);
            }
        }


        private Brush GetColorForBar(int index)
        {
            // Lista de colores predefinidos para las barras
            Brush[] colores = { Brushes.SteelBlue, Brushes.MediumSeaGreen, Brushes.Tomato, Brushes.Orange, Brushes.SlateGray, Brushes.CornflowerBlue, Brushes.LightCoral };
            return colores[index % colores.Length];
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
            if ((_productosParaReporte == null || !_productosParaReporte.Any()) &&
                (_datosResumen == null || !_datosResumen.Any())) // Modificado para chequear ambas listas
            {
                MessageBox.Show("Debe generar el reporte antes de exportar.", "Información",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "Archivo PDF (*.pdf)|*.pdf|Archivo CSV (*.csv)|*.csv|Archivo de texto (*.txt)|*.txt",
                DefaultExt = "pdf",
                FileName = $"ReporteInventario_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                string filePath = saveDialog.FileName;
                try
                {
                    switch (saveDialog.FilterIndex)
                    {
                        case 1: // PDF
                            ExportarInventarioPDF(filePath);
                            break;
                        case 2: // CSV
                            ExportarInventarioCSV(filePath); // Tu método existente
                            break;
                        case 3: // Texto
                            ExportarInventarioTexto(filePath); // Tu método existente
                            break;
                    }
                    MessageBox.Show("Reporte de inventario exportado exitosamente.", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, "Error al exportar reporte de inventario");
                    MessageBox.Show("Error al exportar el reporte: " + ex.Message, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportarInventarioPDF(string filePath)
        {
            Document document = new Document(PageSize.A4.Rotate(), 25, 25, 30, 30);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));

            BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font titleFont = new Font(baseFont, 18, Font.BOLD);
            Font subTitleFont = new Font(baseFont, 14, Font.BOLD);
            Font headerFont = new Font(baseFont, 9, Font.BOLD, BaseColor.WHITE);
            Font cellFont = new Font(baseFont, 8, Font.NORMAL);
            Font smallItalicFont = new Font(baseFont, 8, Font.ITALIC);

            document.Open();

            Paragraph mainTitle = new Paragraph("Reporte de Inventario", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 15f
            };
            document.Add(mainTitle);

            document.Add(new Paragraph($"Filtro Categoría: {(cbCategoria.SelectedItem as Categoria)?.Nombre ?? "Todas"}", smallItalicFont));
            document.Add(new Paragraph($"Filtro Estado Stock: {(cbEstadoStock.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos"}", smallItalicFont));
            document.Add(new Paragraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", smallItalicFont) { SpacingAfter = 20f });

            // Resumen
            if (_datosResumen != null && _datosResumen.Any())
            {
                document.Add(new Paragraph("Resumen del Inventario", subTitleFont) { SpacingAfter = 10f });
                PdfPTable resumenTable = new PdfPTable(2);
                resumenTable.WidthPercentage = 70;
                resumenTable.HorizontalAlignment = Element.ALIGN_LEFT;
                resumenTable.SetWidths(new float[] { 3f, 2f });

                resumenTable.AddCell(new PdfPCell(new Phrase("Valor Total Inventario (Filtrado):", cellFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER });
                resumenTable.AddCell(new PdfPCell(new Phrase(((decimal)_datosResumen["ValorTotalInventario"]).ToString("C", CultureInfo.CurrentCulture), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = iTextSharp.text.Rectangle.NO_BORDER });

                resumenTable.AddCell(new PdfPCell(new Phrase("Total Productos Activos (Sistema):", cellFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER });
                resumenTable.AddCell(new PdfPCell(new Phrase(_datosResumen["TotalProductosActivos"].ToString(), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = iTextSharp.text.Rectangle.NO_BORDER });

                resumenTable.AddCell(new PdfPCell(new Phrase("Productos Bajo Stock Mínimo (Sistema):", cellFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER });
                resumenTable.AddCell(new PdfPCell(new Phrase(_datosResumen["ProductosBajoStockGlobal"].ToString(), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = iTextSharp.text.Rectangle.NO_BORDER });

                resumenTable.AddCell(new PdfPCell(new Phrase("Productos Listados (Según Filtro):", cellFont)) { Border = iTextSharp.text.Rectangle.NO_BORDER });
                resumenTable.AddCell(new PdfPCell(new Phrase(_datosResumen["ProductosCoincidentesFiltro"].ToString(), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = iTextSharp.text.Rectangle.NO_BORDER });
                document.Add(resumenTable);
                document.Add(new Paragraph(" ") { SpacingAfter = 15f });
            }

            // Detalle de Productos Filtrados
            if (_productosParaReporte != null && _productosParaReporte.Any())
            {
                document.Add(new Paragraph("Detalle de Productos en Inventario (Filtrados)", subTitleFont) { SpacingAfter = 10f });
                PdfPTable productosTable = new PdfPTable(7); // Código, Producto, Categoría, Stock Mín, Stock Actual, P.Compra, Valor Inv.
                productosTable.WidthPercentage = 100;
                productosTable.SetWidths(new float[] { 1f, 2.5f, 1.5f, 0.8f, 0.8f, 1f, 1.2f });

                string[] prodHeaders = { "Código", "Producto", "Categoría", "Mín.", "Actual", "P. Compra", "Valor Stock" };
                foreach (string header in prodHeaders)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = new BaseColor(0, 105, 92), // Verde azulado oscuro
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 4
                    };
                    productosTable.AddCell(cell);
                }

                foreach (var p in _productosParaReporte)
                {
                    productosTable.AddCell(new PdfPCell(new Phrase(p.Codigo, cellFont)) { Padding = 3 });
                    productosTable.AddCell(new PdfPCell(new Phrase(p.Nombre, cellFont)) { Padding = 3 });
                    productosTable.AddCell(new PdfPCell(new Phrase(p.NombreCategoria ?? "N/A", cellFont)) { Padding = 3 });
                    productosTable.AddCell(new PdfPCell(new Phrase(p.StockMinimo.ToString(), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
                    productosTable.AddCell(new PdfPCell(new Phrase(p.CantidadDisponible.ToString(), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
                    productosTable.AddCell(new PdfPCell(new Phrase(p.PrecioCompra.ToString("C", CultureInfo.CurrentCulture), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
                    productosTable.AddCell(new PdfPCell(new Phrase(p.ValorInventario.ToString("C", CultureInfo.CurrentCulture), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
                }
                document.Add(productosTable);
                document.Add(new Paragraph(" ") { SpacingAfter = 15f });
            }

            // Productos con Bajo Stock (Global)
            if (_datosResumen != null && _datosResumen.ContainsKey("ListaProductosBajoStockGlobal") && _datosResumen["ListaProductosBajoStockGlobal"] is List<object> bajoStockGlobal && bajoStockGlobal.Any())
            {
                document.Add(new Paragraph("Productos con Bajo Stock (Global)", subTitleFont) { SpacingAfter = 10f });
                PdfPTable bajoStockTable = new PdfPTable(6);
                bajoStockTable.WidthPercentage = 100;
                bajoStockTable.SetWidths(new float[] { 1f, 2.5f, 1.5f, 0.8f, 0.8f, 1f });

                string[] bajoStockHeaders = { "Código", "Producto", "Categoría", "Mín.", "Actual", "Diferencia" };
                foreach (string header in bajoStockHeaders)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = new BaseColor(0, 105, 92), // Verde azulado oscuro
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 4
                    };
                    bajoStockTable.AddCell(cell);
                }
                foreach (dynamic p in bajoStockGlobal)
                {
                    bajoStockTable.AddCell(new PdfPCell(new Phrase(p.Codigo.ToString(), cellFont)) { Padding = 3 });
                    bajoStockTable.AddCell(new PdfPCell(new Phrase(p.Nombre.ToString(), cellFont)) { Padding = 3 });
                    bajoStockTable.AddCell(new PdfPCell(new Phrase(p.NombreCategoria.ToString(), cellFont)) { Padding = 3 });
                    bajoStockTable.AddCell(new PdfPCell(new Phrase(p.StockMinimo.ToString(), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
                    bajoStockTable.AddCell(new PdfPCell(new Phrase(p.CantidadDisponible.ToString(), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
                    bajoStockTable.AddCell(new PdfPCell(new Phrase(p.DiferenciaStock.ToString(), cellFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 3 });
                }
                document.Add(bajoStockTable);
            }

            document.Close();
            writer.Close();
        }

        private void ExportarInventarioCSV(string fileName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("\"Reporte de Inventario\"");
            sb.AppendLine($"\"Generado el:\",\"{DateTime.Now:dd/MM/yyyy HH:mm:ss}\"");
            sb.AppendLine($"\"Filtro Categoría:\",\"{(cbCategoria.SelectedItem as Categoria)?.Nombre ?? "Todas"}\"");
            sb.AppendLine($"\"Filtro Estado Stock:\",\"{(cbEstadoStock.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos"}\"");
            sb.AppendLine();

            sb.AppendLine("\"Resumen\"");
            sb.AppendLine($"\"Métrica\",\"Valor\"");
            sb.AppendLine($"\"Valor Total Inventario (Productos Filtrados):\",\"{((decimal)_datosResumen["ValorTotalInventario"]):C}\"");
            sb.AppendLine($"\"Total Productos Activos (Sistema):\",\"{_datosResumen["TotalProductosActivos"]}\"");
            sb.AppendLine($"\"Productos Bajo Stock Mínimo (Sistema):\",\"{_datosResumen["ProductosBajoStockGlobal"]}\"");
            sb.AppendLine($"\"Productos Coincidentes con Filtro Actual:\",\"{_datosResumen["ProductosCoincidentesFiltro"]}\"");
            sb.AppendLine();

            sb.AppendLine("\"Detalle de Productos Filtrados\"");
            sb.AppendLine("\"Código\",\"Producto\",\"Categoría\",\"Stock Mínimo\",\"Stock Actual\",\"Precio Compra\",\"Valor Inventario\",\"Estado\"");
            foreach (var p in _productosParaReporte)
            {
                sb.AppendLine($"\"{p.Codigo}\",\"{p.Nombre}\",\"{p.NombreCategoria}\",\"{p.StockMinimo}\",\"{p.CantidadDisponible}\",\"{p.PrecioCompra:F2}\",\"{p.ValorInventario:F2}\",\"{(p.RequiereReposicion ? "Bajo Stock" : "Normal")}\"");
            }
            sb.AppendLine();

            sb.AppendLine("\"Productos con Bajo Stock (Global)\"");
            sb.AppendLine("\"Código\",\"Producto\",\"Categoría\",\"Stock Mínimo\",\"Stock Actual\",\"Diferencia\"");
            if (_datosResumen.TryGetValue("ListaProductosBajoStockGlobal", out object bajoStockObj) && bajoStockObj is List<object> bajoStockGlobal)
            {
                foreach (dynamic p in bajoStockGlobal)
                {
                    sb.AppendLine($"\"{p.Codigo}\",\"{p.Nombre}\",\"{p.NombreCategoria}\",\"{p.StockMinimo}\",\"{p.CantidadDisponible}\",\"{p.DiferenciaStock}\"");
                }
            }


            File.WriteAllText(fileName, sb.ToString(), Encoding.UTF8);
        }

        private void ExportarInventarioTexto(string fileName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("========================================================================================");
            sb.AppendLine("                                REPORTE DE INVENTARIO                                   ");
            sb.AppendLine("========================================================================================");
            sb.AppendLine($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine($"Filtro Categoría: {(cbCategoria.SelectedItem as Categoria)?.Nombre ?? "Todas"}");
            sb.AppendLine($"Filtro Estado Stock: {(cbEstadoStock.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos"}");
            sb.AppendLine();

            sb.AppendLine("RESUMEN");
            sb.AppendLine("----------------------------------------------------------------------------------------");
            sb.AppendLine($"Valor Total Inventario (Productos Filtrados): {((decimal)_datosResumen["ValorTotalInventario"]):C}");
            sb.AppendLine($"Total Productos Activos (Sistema):           {_datosResumen["TotalProductosActivos"]}");
            sb.AppendLine($"Productos Bajo Stock Mínimo (Sistema):       {_datosResumen["ProductosBajoStockGlobal"]}");
            sb.AppendLine($"Productos Coincidentes con Filtro Actual:    {_datosResumen["ProductosCoincidentesFiltro"]}");
            sb.AppendLine();

            sb.AppendLine("DETALLE DE PRODUCTOS FILTRADOS");
            sb.AppendLine("----------------------------------------------------------------------------------------");
            sb.AppendLine("Código     | Producto                               | Categoría        | Min | Actual | P.Compra | Valor Inv. | Estado     ");
            sb.AppendLine("-----------|----------------------------------------|------------------|-----|--------|----------|------------|------------");
            foreach (var p in _productosParaReporte)
            {
                string nombreProducto = p.Nombre ?? "";
                string nombreCategoria = p.NombreCategoria ?? "";
                sb.AppendLine($"{p.Codigo.PadRight(10)} | {nombreProducto.PadRight(38).Substring(0, Math.Min(nombreProducto.Length, 38))} | {nombreCategoria.PadRight(16).Substring(0, Math.Min(nombreCategoria.Length, 16))} | {p.StockMinimo.ToString().PadLeft(3)} | {p.CantidadDisponible.ToString().PadLeft(6)} | {p.PrecioCompra.ToString("C").PadLeft(8)} | {p.ValorInventario.ToString("C").PadLeft(10)} | {(p.RequiereReposicion ? "Bajo Stock" : "Normal").PadRight(10)}");
            }
            sb.AppendLine();

            sb.AppendLine("PRODUCTOS CON BAJO STOCK (GLOBAL)");
            sb.AppendLine("----------------------------------------------------------------------------------------");
            sb.AppendLine("Código     | Producto                               | Categoría        | Min | Actual | Diferencia");
            sb.AppendLine("-----------|----------------------------------------|------------------|-----|--------|-----------");
            if (_datosResumen.TryGetValue("ListaProductosBajoStockGlobal", out object bajoStockObj) && bajoStockObj is List<object> bajoStockGlobal)
            {
                foreach (dynamic p in bajoStockGlobal)
                {
                    string nombreProducto = p.Nombre ?? "";
                    string nombreCategoria = p.NombreCategoria ?? "";
                    sb.AppendLine($"{p.Codigo.PadRight(10)} | {nombreProducto.PadRight(38).Substring(0, Math.Min(nombreProducto.Length, 38))} | {nombreCategoria.PadRight(16).Substring(0, Math.Min(nombreCategoria.Length, 16))} | {p.StockMinimo.ToString().PadLeft(3)} | {p.CantidadDisponible.ToString().PadLeft(6)} | {p.DiferenciaStock.ToString().PadLeft(9)}");
                }
            }
            File.WriteAllText(fileName, sb.ToString(), Encoding.UTF8);
        }
    }
}