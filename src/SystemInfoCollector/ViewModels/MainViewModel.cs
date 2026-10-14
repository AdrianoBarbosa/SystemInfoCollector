using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SystemInfoCollector.Services;

namespace SystemInfoCollector.ViewModels
{
    /// <summary>
    /// Estado e comandos da janela principal.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private const string FileNotFoundMessage = "O arquivo de dados não existe mais. Colete novamente.";
        private const string CorruptedFileMessage = "O arquivo de dados está corrompido. Colete novamente.";

        private readonly ISystemInfoService _systemInfoService;
        private readonly ISystemInfoRepository _repository;
        private readonly IDialogService _dialogService;
        private bool _isBusy;
        private bool _canShowData;
        private string _statusMessage;

        /// <summary>
        /// Cria o ViewModel com os serviços de coleta, persistência e diálogo.
        /// </summary>
        public MainViewModel(ISystemInfoService systemInfoService, ISystemInfoRepository repository, IDialogService dialogService)
        {
            _systemInfoService = systemInfoService ?? throw new ArgumentNullException(nameof(systemInfoService));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            CollectDataCommand = new AsyncRelayCommand(CollectDataAsync, () => !IsBusy);
            ShowDataCommand = new RelayCommand(ShowData, () => CanShowData && !IsBusy);

            // Um arquivo gerado em execução anterior já habilita a exibição.
            RefreshCanShowData();
        }

        /// <summary>
        /// Coleta os dados do computador e grava o arquivo.
        /// </summary>
        public AsyncRelayCommand CollectDataCommand { get; }

        /// <summary>
        /// Lê o arquivo e exibe os dados coletados.
        /// </summary>
        public RelayCommand ShowDataCommand { get; }

        /// <summary>
        /// Indica que uma coleta está em andamento.
        /// </summary>
        /// <remarks>
        /// É o estado exposto à interface (indicador de progresso e habilitação dos botões).
        /// O <see cref="AsyncRelayCommand.IsExecuting"/> do comando é apenas a proteção interna contra reentrada.
        /// </remarks>
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    CollectDataCommand.RaiseCanExecuteChanged();
                    ShowDataCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Indica se o arquivo de dados existe e pode ser exibido.
        /// </summary>
        public bool CanShowData
        {
            get => _canShowData;
            private set
            {
                if (SetProperty(ref _canShowData, value))
                {
                    ShowDataCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Mensagem de status exibida na janela principal.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        private async Task CollectDataAsync()
        {
            IsBusy = true;
            StatusMessage = "Coletando dados...";
            try
            {
                var systemInfo = await Task.Run(() => _systemInfoService.Collect());
                await Task.Run(() => _repository.Save(systemInfo));

                StatusMessage = $"Dados salvos em {_repository.FilePath}";
                if (systemInfo.Warnings.Any())
                {
                    _dialogService.ShowWarning(
                        "Coleta concluída com avisos:" + Environment.NewLine
                        + string.Join(Environment.NewLine, systemInfo.Warnings.Select(warning => "• " + warning)));
                }
                else
                {
                    _dialogService.ShowInformation($"Coleta concluída. Dados salvos em {_repository.FilePath}");
                }
            }
            catch (IOException ex)
            {
                ReportSaveFailure(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                ReportSaveFailure(ex);
            }
            finally
            {
                IsBusy = false;
                RefreshCanShowData();
            }
        }

        private void ShowData()
        {
            try
            {
                _dialogService.ShowData(_repository.Load());
            }
            catch (FileNotFoundException)
            {
                _dialogService.ShowError(FileNotFoundMessage);
            }
            catch (InvalidDataException)
            {
                _dialogService.ShowError(CorruptedFileMessage);
            }
            catch (IOException ex)
            {
                ReportReadFailure(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                ReportReadFailure(ex);
            }
            finally
            {
                // O arquivo pode ter sido apagado fora da aplicação.
                RefreshCanShowData();
            }
        }

        private void RefreshCanShowData()
        {
            CanShowData = _repository.Exists();
        }

        private void ReportSaveFailure(Exception exception)
        {
            StatusMessage = "Não foi possível salvar os dados.";
            _dialogService.ShowError($"Não foi possível salvar os dados em {_repository.FilePath}: {exception.Message}");
        }

        private void ReportReadFailure(Exception exception)
        {
            _dialogService.ShowError($"Não foi possível ler {_repository.FilePath}: {exception.Message}");
        }
    }
}
