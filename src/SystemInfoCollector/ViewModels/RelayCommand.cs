using System;
using System.Windows.Input;

namespace SystemInfoCollector.ViewModels
{
    /// <summary>
    /// Comando síncrono que delega a execução e a habilitação a funções.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// Cria o comando.
        /// </summary>
        /// <param name="execute">Ação executada pelo comando.</param>
        /// <param name="canExecute">Condição de habilitação; quando omitida, o comando fica sempre habilitado.</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Disparado quando a habilitação do comando pode ter mudado.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Indica se o comando pode ser executado.
        /// </summary>
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

        /// <summary>
        /// Executa o comando.
        /// </summary>
        public void Execute(object parameter) => _execute();

        /// <summary>
        /// Solicita à interface que reavalie a habilitação do comando.
        /// </summary>
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
