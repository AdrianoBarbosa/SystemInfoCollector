using System.Windows;
using SystemInfoCollector.Models;
using SystemInfoCollector.ViewModels;
using SystemInfoCollector.Views;

namespace SystemInfoCollector.Services
{
    /// <summary>
    /// Exibe mensagens e janelas ao usuário usando o WPF.
    /// </summary>
    public class DialogService : IDialogService
    {
        /// <inheritdoc />
        public void ShowWarning(string message) => Show(message, MessageBoxImage.Warning);

        /// <inheritdoc />
        public void ShowError(string message) => Show(message, MessageBoxImage.Error);

        /// <inheritdoc />
        public void ShowData(SystemInfo systemInfo)
        {
            var dataWindow = new DataWindow
            {
                Owner = Application.Current?.MainWindow,
                DataContext = new DataViewModel(systemInfo),
            };
            dataWindow.ShowDialog();
        }

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
