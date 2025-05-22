using ElectroTech.Helpers;
using ElectroTech.Models;
using System;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ElectroTech.Views.Ventas
{
    /// <summary>
    /// Lógica de interacción para FacturaView.xaml
    /// </summary>
    public partial class FacturaView : Window
    {
        private string _contenidoFactura;
        private Venta _venta;

        /// <summary>
        /// Constructor para mostrar una factura
        /// </summary>
        /// <param name="contenidoFactura">Texto de la factura</param>
        /// <param name="venta">Venta asociada a la factura</param>
        public FacturaView(string contenidoFactura, Venta venta)
        {
            InitializeComponent();

            _contenidoFactura = contenidoFactura;
            _venta = venta;

            // Configurar título
            this.Title = $"Factura - {venta.NumeroFactura}";

            // Cargar factura
            CargarFactura();
        }

        /// <summary>
        /// Carga el contenido de la factura en el RichTextBox
        /// </summary>
        private void CargarFactura()
        {
            try
            {
                // Limpiar contenido actual
                rtbFactura.Document.Blocks.Clear();

                // Crear nuevo párrafo con formato monoespaciado
                Paragraph paragraph = new Paragraph();
                paragraph.FontFamily = new System.Windows.Media.FontFamily("Consolas");
                paragraph.FontSize = 12;
                paragraph.LineHeight = 16;

                // Agregar el texto de la factura
                paragraph.Inlines.Add(new Run(_contenidoFactura));

                // Agregar párrafo al documento
                rtbFactura.Document.Blocks.Add(paragraph);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al cargar factura");
                MessageBox.Show("Error al cargar la factura: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Imprime la factura
        /// </summary>
        private void ImprimirFactura()
        {
            try
            {
                // Configurar impresión
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Establecer orientación y tamaño de página
                    printDialog.PrintTicket.PageOrientation = PageOrientation.Portrait;
                    printDialog.PrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4);

                    // Crear documento para impresión
                    FlowDocument flowDocument = new FlowDocument();
                    flowDocument.FontFamily = new System.Windows.Media.FontFamily("Consolas");
                    flowDocument.FontSize = 12;
                    flowDocument.LineHeight = 16;
                    flowDocument.PagePadding = new Thickness(50);

                    // Agregar contenido de la factura
                    Paragraph paragraph = new Paragraph(new Run(_contenidoFactura));
                    flowDocument.Blocks.Add(paragraph);

                    // Configurar paginación
                    flowDocument.ColumnWidth = printDialog.PrintableAreaWidth;
                    IDocumentPaginatorSource paginatorSource = flowDocument;

                    // Imprimir documento
                    printDialog.PrintDocument(paginatorSource.DocumentPaginator, $"Factura - {_venta.NumeroFactura}");

                    // Mostrar mensaje de éxito
                    MessageBox.Show("Factura enviada a la impresora.",
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al imprimir factura");
                MessageBox.Show("Error al imprimir la factura: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Eventos de controles

        private void btnImprimir_Click(object sender, RoutedEventArgs e)
        {
            ImprimirFactura();
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion
    }
}