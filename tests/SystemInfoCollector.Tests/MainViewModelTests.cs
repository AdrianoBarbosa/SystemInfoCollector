using System.IO;
using System.Threading.Tasks;
using SystemInfoCollector.Models;
using SystemInfoCollector.Tests.Fakes;
using SystemInfoCollector.ViewModels;
using Xunit;

namespace SystemInfoCollector.Tests
{
    public class MainViewModelTests
    {
        private readonly FakeSystemInfoService _systemInfoService = new FakeSystemInfoService();
        private readonly FakeSystemInfoRepository _repository = new FakeSystemInfoRepository();
        private readonly FakeDialogService _dialogService = new FakeDialogService();

        [Fact]
        public async Task CollectData_SavesResultAndShowsFilePath()
        {
            var viewModel = CreateViewModel();

            await viewModel.CollectDataCommand.ExecuteAsync();

            Assert.Same(_systemInfoService.Result, _repository.SavedSystemInfo);
            Assert.Contains(_repository.FilePath, viewModel.StatusMessage);
        }

        [Fact]
        public async Task CollectData_WithWarnings_ShowsWarningListingThem()
        {
            _systemInfoService.Result = new SystemInfo();
            _systemInfoService.Result.Warnings.Add("Não foi possível coletar processador: acesso negado");
            var viewModel = CreateViewModel();

            await viewModel.CollectDataCommand.ExecuteAsync();

            var warning = Assert.Single(_dialogService.Warnings);
            Assert.Contains("Não foi possível coletar processador: acesso negado", warning);
            Assert.Empty(_dialogService.Informations);
        }

        [Fact]
        public void Constructor_ShowsReadyStatus()
        {
            var viewModel = CreateViewModel();

            Assert.Equal("Pronto.", viewModel.StatusMessage);
            Assert.Equal(StatusKind.Information, viewModel.StatusKind);
        }

        [Fact]
        public async Task CollectData_WhileRunning_ShowsCollectingStatus()
        {
            var viewModel = CreateViewModel();
            string messageDuringCollection = null;
            var kindDuringCollection = StatusKind.Error;
            _systemInfoService.OnCollect = () =>
            {
                messageDuringCollection = viewModel.StatusMessage;
                kindDuringCollection = viewModel.StatusKind;
            };

            await viewModel.CollectDataCommand.ExecuteAsync();

            Assert.Equal("Coletando dados...", messageDuringCollection);
            Assert.Equal(StatusKind.Information, kindDuringCollection);
        }

        [Fact]
        public async Task CollectData_WithoutWarnings_ShowsStatusWithoutDialog()
        {
            var viewModel = CreateViewModel();

            await viewModel.CollectDataCommand.ExecuteAsync();

            Assert.Equal(StatusKind.Information, viewModel.StatusKind);
            Assert.Contains(_repository.FilePath, viewModel.StatusMessage);
            AssertNoDialog();
        }

        [Fact]
        public async Task CollectData_WhenSaveFails_ShowsErrorWithoutThrowing()
        {
            _repository.SaveException = new IOException("Disco cheio");
            var viewModel = CreateViewModel();

            await viewModel.CollectDataCommand.ExecuteAsync();

            var error = Assert.Single(_dialogService.Errors);
            Assert.Contains("Disco cheio", error);
            Assert.Contains(_repository.FilePath, error);
        }

        [Fact]
        public async Task CollectData_WhenFinished_ReleasesBusyState()
        {
            var viewModel = CreateViewModel();

            await viewModel.CollectDataCommand.ExecuteAsync();

            Assert.False(viewModel.IsBusy);
            Assert.True(viewModel.CollectDataCommand.CanExecute(null));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Constructor_SetsCanShowDataFromFileExistence(bool fileExists)
        {
            _repository.FileExists = fileExists;

            var viewModel = CreateViewModel();

            Assert.Equal(fileExists, viewModel.CanShowData);
            Assert.Equal(fileExists, viewModel.ShowDataCommand.CanExecute(null));
        }

        [Fact]
        public async Task CollectData_WhenSaved_EnablesShowData()
        {
            var viewModel = CreateViewModel();
            Assert.False(viewModel.CanShowData);

            await viewModel.CollectDataCommand.ExecuteAsync();

            Assert.True(viewModel.CanShowData);
            Assert.True(viewModel.ShowDataCommand.CanExecute(null));
        }

        [Fact]
        public void ShowData_WhenFileWasDeleted_ShowsErrorStatusAndDisablesShowData()
        {
            _repository.FileExists = true;
            var viewModel = CreateViewModel();
            _repository.FileExists = false;
            _repository.LoadException = new FileNotFoundException();

            viewModel.ShowDataCommand.Execute(null);

            Assert.Equal(StatusKind.Error, viewModel.StatusKind);
            Assert.Equal("O arquivo de dados não existe mais. Colete novamente.", viewModel.StatusMessage);
            Assert.False(viewModel.CanShowData);
            AssertNoDialog();
        }

        [Fact]
        public void ShowData_WhenFileIsCorrupted_ShowsErrorStatusAndKeepsShowDataEnabled()
        {
            _repository.FileExists = true;
            _repository.LoadException = new InvalidDataException();
            var viewModel = CreateViewModel();

            viewModel.ShowDataCommand.Execute(null);

            Assert.Equal(StatusKind.Error, viewModel.StatusKind);
            Assert.Equal("O arquivo de dados está corrompido. Colete novamente.", viewModel.StatusMessage);
            Assert.True(viewModel.CanShowData);
            AssertNoDialog();
        }

        [Fact]
        public void ShowData_WhenFileIsValid_ShowsLoadedData()
        {
            _repository.FileExists = true;
            var viewModel = CreateViewModel();

            viewModel.ShowDataCommand.Execute(null);

            Assert.Same(_repository.LoadResult, Assert.Single(_dialogService.ShownData));
            Assert.Empty(_dialogService.Errors);
        }

        private void AssertNoDialog()
        {
            Assert.Empty(_dialogService.Informations);
            Assert.Empty(_dialogService.Warnings);
            Assert.Empty(_dialogService.Errors);
        }

        private MainViewModel CreateViewModel()
        {
            return new MainViewModel(_systemInfoService, _repository, _dialogService);
        }
    }
}
