using SystemInfoCollector.Models;

namespace SystemInfoCollector.Services
{
    /// <summary>
    /// Exibe mensagens e janelas ao usuário, isolando os ViewModels da camada de interface.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Exibe uma mensagem informativa.
        /// </summary>
        void ShowInformation(string message);

        /// <summary>
        /// Exibe uma mensagem de aviso.
        /// </summary>
        void ShowWarning(string message);

        /// <summary>
        /// Exibe uma mensagem de erro.
        /// </summary>
        void ShowError(string message);

        /// <summary>
        /// Abre a janela com os dados coletados.
        /// </summary>
        void ShowData(SystemInfo systemInfo);
    }
}
