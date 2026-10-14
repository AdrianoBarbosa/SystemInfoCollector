using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SystemInfoCollector.ViewModels
{
    /// <summary>
    /// Comando assíncrono que fica desabilitado enquanto está em execução,
    /// impedindo execuções simultâneas.
    /// </summary>
    /// <remarks>
    /// O delegate é responsável por tratar as próprias exceções: como <see cref="Execute"/> é
    /// <c>async void</c>, uma exceção não tratada chega ao dispatcher da aplicação.
    /// </remarks>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;
        private bool _isExecuting;

        /// <summary>
        /// Cria o comando.
        /// </summary>
        /// <param name="execute">Operação assíncrona executada pelo comando.</param>
        /// <param name="canExecute">Condição adicional de habilitação; quando omitida, depende apenas de não estar em execução.</param>
        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Disparado quando a habilitação do comando pode ter mudado.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Indica se o comando está em execução.
        /// </summary>
        public bool IsExecuting
        {
            get => _isExecuting;
            private set
            {
                _isExecuting = value;
                RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Indica se o comando pode ser executado.
        /// </summary>
        public bool CanExecute(object parameter) => !IsExecuting && (_canExecute == null || _canExecute());

        /// <summary>
        /// Executa o comando a partir da interface.
        /// </summary>
        public async void Execute(object parameter) => await ExecuteAsync();

        /// <summary>
        /// Executa o comando e aguarda sua conclusão. Ignora a chamada se o comando não puder ser executado.
        /// </summary>
        public async Task ExecuteAsync()
        {
            if (!CanExecute(null))
            {
                return;
            }

            IsExecuting = true;
            try
            {
                await _execute();
            }
            finally
            {
                IsExecuting = false;
            }
        }

        /// <summary>
        /// Solicita à interface que reavalie a habilitação do comando.
        /// </summary>
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
