using System.Windows;

namespace SystemInfoCollector.Views
{
    /// <summary>
    /// Janela principal. Todo o comportamento fica no <see cref="ViewModels.MainViewModel"/>.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Cria a janela.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
