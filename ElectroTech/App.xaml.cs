using ElectroTech.DataAccess;
using ElectroTech.Helpers;
using ElectroTech.Views;
using System;
using System.Windows;

namespace ElectroTech
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configurar el control de excepciones no manejadas
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            try
            {
                // Inicializar la base de datos
                bool dbInitialized = DatabaseInitializer.Initialize();
                if (!dbInitialized)
                {
                    MessageBox.Show("Error al inicializar la base de datos. La aplicación se cerrará.",
                        "Error de Inicialización", MessageBoxButton.OK, MessageBoxImage.Error);
                    Current.Shutdown();
                    return;
                }

                // Verificar el estado de la base de datos
                DatabaseCheckResult dbCheckResult = DatabaseInitializer.CheckDatabase();

                // Actuar según el resultado de la verificación
                switch (dbCheckResult)
                {
                    case DatabaseCheckResult.OK:
                        // Mostrar ventana de login
                        var loginWindow = new LoginView();
                        MainWindow = loginWindow;
                        loginWindow.Show();
                        break;

                    case DatabaseCheckResult.ConnectionError:
                        MessageBox.Show("No se puede establecer conexión con la base de datos. " +
                            "Verifique la configuración y vuelva a intentarlo.",
                            "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                        Current.Shutdown();
                        break;

                    case DatabaseCheckResult.TablesNotExist:
                        MessageBox.Show("La estructura de la base de datos no existe. " +
                            "Se intentará crearla automáticamente.",
                            "Base de Datos no Inicializada", MessageBoxButton.OK, MessageBoxImage.Warning);

                        // Ejecutar script de creación de base de datos
                        bool scriptExecuted = DatabaseInitializer.ExecuteDatabaseScriptAsync().Result;
                        if (scriptExecuted)
                        {
                            MessageBox.Show("Base de datos creada exitosamente. " +
                                "La aplicación se reiniciará.",
                                "Información", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Reiniciar la aplicación
                            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                            Current.Shutdown();
                        }
                        else
                        {
                            MessageBox.Show("Error al crear la estructura de la base de datos. " +
                                "La aplicación se cerrará.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            Current.Shutdown();
                        }
                        break;

                    case DatabaseCheckResult.AdminUserNotExist:
                        MessageBox.Show("No existe un usuario administrador en el sistema. " +
                            "Debe crear uno para continuar.",
                            "Primer Uso", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Mostrar ventana de creación de administrador
                        //var primerUsoWindow = new PrimerUsoView();
                        //MainWindow = primerUsoWindow;
                        //primerUsoWindow.Show();
                        break;

                    default:
                        MessageBox.Show("Error desconocido al verificar la base de datos. " +
                            "La aplicación se cerrará.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Current.Shutdown();
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error en el inicio de la aplicación");
                MessageBox.Show($"Error al iniciar la aplicación: {ex.Message}",
                    "Error de Inicio", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Logger.LogException(ex, "Excepción no manejada en el dominio");
            MessageBox.Show($"Error no manejado: {ex?.Message}",
                "Error Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.LogException(e.Exception, "Excepción no manejada en el dispatcher");
            MessageBox.Show($"Error no manejado: {e.Exception.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}