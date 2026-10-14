using System.Windows;

namespace SystemInfoCollector.Views
{
    /// <summary>
    /// Janela que exibe os dados lidos do arquivo de coleta. O botão Fechar usa
    /// <c>IsCancel</c>, por isso não há lógica no code-behind.
    /// </summary>
    public partial class DataWindow : Window
    {
        /// <summary>
        /// Cria a janela.
        /// </summary>
        public DataWindow()
        {
            InitializeComponent();
        }
    }
}
