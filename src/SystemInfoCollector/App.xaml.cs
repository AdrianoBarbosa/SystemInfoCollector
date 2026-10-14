using System.Windows;
using System.Windows.Threading;
using SystemInfoCollector.Services;
using SystemInfoCollector.Services.Collectors;
using SystemInfoCollector.ViewModels;
using SystemInfoCollector.Views;

namespace SystemInfoCollector
{
    /// <summary>
    /// Ponto de entrada da aplicação.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Título usado nas janelas e mensagens da aplicação.
        /// </summary>
        internal const string ApplicationTitle = "Coletor de Informações do Sistema";

        /// <summary>
        /// Cria a aplicação e registra o tratamento de erros não previstos.
        /// </summary>
        public App()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        /// <summary>
        /// Monta as dependências e abre a janela principal.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var systemInfoService = new SystemInfoService(
                new OperatingSystemCollector(),
                new ProcessorCollector(),
                new NetFrameworkCollector(),
                new NetworkInterfaceCollector(),
                new NetAdapterCollector());
            var repository = new SystemInfoRepository(AppPaths.DataFilePath);
            var viewModel = new MainViewModel(systemInfoService, repository, new DialogService());

            MainWindow = new MainWindow { DataContext = viewModel };
            MainWindow.Show();
        }

        // Rede de segurança: erros não previstos são informados ao usuário sem encerrar a aplicação.
        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                $"Ocorreu um erro inesperado: {e.Exception.Message}",
                ApplicationTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
