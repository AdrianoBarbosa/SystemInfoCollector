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
        public async Task CollectData_WithoutWarnings_ShowsInformation()
        {
            var viewModel = CreateViewModel();

            await viewModel.CollectDataCommand.ExecuteAsync();

            var information = Assert.Single(_dialogService.Informations);
            Assert.Contains(_repository.FilePath, information);
            Assert.Empty(_dialogService.Warnings);
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
        public void ShowData_WhenFileWasDeleted_ShowsErrorAndDisablesShowData()
        {
            _repository.FileExists = true;
            var viewModel = CreateViewModel();
            _repository.FileExists = false;
            _repository.LoadException = new FileNotFoundException();

            viewModel.ShowDataCommand.Execute(null);

            Assert.Equal("O arquivo de dados não existe mais. Colete novamente.", Assert.Single(_dialogService.Errors));
            Assert.False(viewModel.CanShowData);
        }

        [Fact]
        public void ShowData_WhenFileIsCorrupted_ShowsErrorAndKeepsShowDataEnabled()
        {
            _repository.FileExists = true;
            _repository.LoadException = new InvalidDataException();
            var viewModel = CreateViewModel();

            viewModel.ShowDataCommand.Execute(null);

            Assert.Equal("O arquivo de dados está corrompido. Colete novamente.", Assert.Single(_dialogService.Errors));
            Assert.True(viewModel.CanShowData);
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

        private MainViewModel CreateViewModel()
        {
            return new MainViewModel(_systemInfoService, _repository, _dialogService);
        }
    }
}
