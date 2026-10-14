using System.Windows;
using System.Windows.Threading;

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
