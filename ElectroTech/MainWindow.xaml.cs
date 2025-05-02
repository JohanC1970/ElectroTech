using ElectroTech.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ElectroTech
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ExecuteDatabaseScript();
        }

        private async void ExecuteDatabaseScript()
        {
            try
            {
                // Verificar si la conexión es correcta
                bool canConnect = DatabaseInitializer.TestConnection();
                if (!canConnect)
                {
                    MessageBox.Show("No se puede conectar a la base de datos. Verifique la configuración.",
                        "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Preguntar al usuario si desea crear las tablas
                var result = MessageBox.Show("¿Desea crear las tablas de la base de datos?",
                    "Creación de tablas", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Ejecutar el script
                    bool success = await DatabaseInitializer.ExecuteDatabaseScriptAsync();

                    if (success)
                    {
                        MessageBox.Show("Las tablas se han creado exitosamente.",
                            "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Verificar si se necesita crear un usuario administrador
                        if (!DatabaseInitializer.AdminUserExists())
                        {
                            // Aquí podrías mostrar un formulario para crear el usuario administrador
                            // O crear uno con valores predeterminados
                            DatabaseInitializer.CreateAdminUser("admin", "Admin123", "Administrador", "admin@electrotech.com");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Error al crear las tablas de la base de datos.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}
