using SystemInfoCollector.Models;
using SystemInfoCollector.ViewModels;
using Xunit;

namespace SystemInfoCollector.Tests
{
    public class DataViewModelTests
    {
        [Fact]
        public void HasNetworkInterfaces_IsFalseWhenListIsEmpty()
        {
            var viewModel = new DataViewModel(new SystemInfo());

            Assert.False(viewModel.HasNetworkInterfaces);
        }

        [Fact]
        public void HasNetworkInterfaces_IsTrueWhenThereIsAnInterface()
        {
            var systemInfo = new SystemInfo();
            systemInfo.NetworkInterfaces.Add(new NetworkInterfaceInfo { Name = "Ethernet" });

            var viewModel = new DataViewModel(systemInfo);

            Assert.True(viewModel.HasNetworkInterfaces);
        }

        [Fact]
        public void HasProcessors_IsFalseWhenListIsEmpty()
        {
            var viewModel = new DataViewModel(new SystemInfo());

            Assert.False(viewModel.HasProcessors);
        }

        [Fact]
        public void NetFrameworkText_IncludesReleaseWhenAvailable()
        {
            var systemInfo = new SystemInfo
            {
                NetFramework = new NetFrameworkInfo { Version = "4.8.1 ou superior", Release = 533509 },
            };

            var viewModel = new DataViewModel(systemInfo);

            Assert.Equal("4.8.1 ou superior (release 533509)", viewModel.NetFrameworkText);
        }

        [Fact]
        public void NetFrameworkText_ShowsNotAvailableWhenVersionIsMissing()
        {
            var viewModel = new DataViewModel(new SystemInfo());

            Assert.Equal("Não disponível", viewModel.NetFrameworkText);
        }

        [Fact]
        public void HasWarnings_ReflectsWarnings()
        {
            var systemInfo = new SystemInfo();
            Assert.False(new DataViewModel(systemInfo).HasWarnings);

            systemInfo.Warnings.Add("Não foi possível coletar interfaces de rede: acesso negado");

            Assert.True(new DataViewModel(systemInfo).HasWarnings);
        }
    }
}
