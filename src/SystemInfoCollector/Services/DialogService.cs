using System.Windows;

namespace SystemInfoCollector.Services
{
    /// <summary>
    /// Exibe mensagens ao usuário com caixas de diálogo do WPF.
    /// </summary>
    public class DialogService : IDialogService
    {
        /// <inheritdoc />
        public void ShowInformation(string message) => Show(message, MessageBoxImage.Information);

        /// <inheritdoc />
        public void ShowWarning(string message) => Show(message, MessageBoxImage.Warning);

        /// <inheritdoc />
        public void ShowError(string message) => Show(message, MessageBoxImage.Error);

        private static void Show(string message, MessageBoxImage image)
        {
            var owner = Application.Current?.MainWindow;
            if (owner == null)
            {
                MessageBox.Show(message, App.ApplicationTitle, MessageBoxButton.OK, image);
            }
            else
            {
                MessageBox.Show(owner, message, App.ApplicationTitle, MessageBoxButton.OK, image);
            }
        }
    }
}
