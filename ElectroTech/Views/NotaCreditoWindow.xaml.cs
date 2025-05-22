using ElectroTech.Helpers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Printing;

namespace ElectroTech.Views.Devoluciones
{
    /// <summary>
    /// Lógica de interacción para NotaCreditoWindow.xaml
    /// </summary>
    public partial class NotaCreditoWindow : Window
    {
        private readonly string _notaCredito;

        /// <summary>
        /// Constructor de la ventana de nota de crédito
        /// </summary>
        /// <param name="notaCredito">Texto de la nota de crédito</param>
        public NotaCreditoWindow(string notaCredito)
        {
            InitializeComponent();
            _notaCredito = notaCredito;
            txtNotaCredito.Text = notaCredito;
        }

        /// <summary>
        /// Imprime la nota de crédito
        /// </summary>
        private void btnImprimir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Crear documento para impresión
                FlowDocument flowDocument = new FlowDocument();
                flowDocument.FontFamily = new FontFamily("Consolas");
                flowDocument.FontSize = 12;

                // Párrafo con el contenido de la nota de crédito
                Paragraph paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run(_notaCredito));
                flowDocument.Blocks.Add(paragraph);

                // Configurar impresión
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Crear documentos de impresión
                    IDocumentPaginatorSource dps = flowDocument;

                    // Imprimir
                    printDialog.PrintDocument(dps.DocumentPaginator, "Nota de Crédito");

                    // Informar al usuario
                    MessageBox.Show("Nota de crédito enviada a impresión correctamente.",
                        "Impresión", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al imprimir nota de crédito");
                MessageBox.Show("Error al imprimir la nota de crédito: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Cierra la ventana
        /// </summary>
        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}