using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SystemInfoCollector.Models;
using SystemInfoCollector.Services;
using Xunit;

namespace SystemInfoCollector.Tests
{
    public sealed class SystemInfoRepositoryTests : IDisposable
    {
        private readonly string _directory;
        private readonly string _filePath;
        private readonly SystemInfoRepository _repository;

        public SystemInfoRepositoryTests()
        {
            _directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _filePath = Path.Combine(_directory, "system-info.json");
            _repository = new SystemInfoRepository(_filePath);
        }

        public void Dispose()
        {
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, true);
            }
        }

        [Fact]
        public void SaveAndLoad_PreservesAllValues()
        {
            var original = CreateFullSystemInfo();

            _repository.Save(original);
            var loaded = _repository.Load();

            Assert.Equal(original.CollectedAt, loaded.CollectedAt);
            Assert.Equal(original.CollectedAt.Offset, loaded.CollectedAt.Offset);
            Assert.Equal("Microsoft Windows 11 Pro", loaded.OperatingSystem.Caption);
            Assert.Equal("10.0.26300", loaded.OperatingSystem.Version);
            Assert.Equal("64 bits", loaded.OperatingSystem.Architecture);
            Assert.Equal("ESTACAO-01", loaded.OperatingSystem.ComputerName);

            var processor = Assert.Single(loaded.Processors);
            Assert.Equal("Intel(R) Core(TM) i7", processor.Name);
            Assert.Equal("Intel64 Family 6 Model 141 Stepping 1", processor.Description);

            Assert.Equal("4.8.1 ou superior", loaded.NetFramework.Version);
            Assert.Equal(533509, loaded.NetFramework.Release);

            Assert.Equal(2, loaded.NetworkInterfaces.Count);
            Assert.Equal("Wi-Fi", loaded.NetworkInterfaces[0].Name);
            Assert.Equal(new[] { "192.168.1.10", "fe80::1" }, loaded.NetworkInterfaces[0].IpAddresses);
            Assert.Equal("00:11:22:33:44:55", loaded.NetworkInterfaces[0].MacAddress);
            Assert.Equal("Ethernet", loaded.NetworkInterfaces[1].Name);

            Assert.Equal(new[] { "Aviso de exemplo" }, loaded.Warnings);
        }

        [Fact]
        public void SaveAndLoad_KeepsNullStringsAsNull()
        {
            var original = CreateFullSystemInfo();
            original.NetworkInterfaces[1].MacAddress = null;

            _repository.Save(original);
            var json = File.ReadAllText(_filePath);
            var loaded = _repository.Load();

            Assert.Contains("\"macAddress\": null", json);
            Assert.Null(loaded.NetworkInterfaces[1].MacAddress);
        }

        [Fact]
        public void Save_WritesIndentedCamelCaseUtf8WithoutBom()
        {
            _repository.Save(CreateFullSystemInfo());

            var bytes = File.ReadAllBytes(_filePath);
            var json = Encoding.UTF8.GetString(bytes);

            Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);
            Assert.Matches(new Regex("^\\{\\r?\\n  \"collectedAt\": "), json);
            Assert.Matches(new Regex("\\r?\\n  \"operatingSystem\": \\{"), json);
            Assert.Matches(new Regex("\\r?\\n  \"networkInterfaces\": \\["), json);
        }

        [Fact]
        public void Load_WithNullListsAndUnknownProperty_ReturnsEmptyLists()
        {
            WriteFile("{ \"processors\": null, \"networkInterfaces\": null, \"warnings\": null, \"propriedadeDesconhecida\": 1 }");

            var loaded = _repository.Load();

            Assert.Empty(loaded.Processors);
            Assert.Empty(loaded.NetworkInterfaces);
            Assert.Empty(loaded.Warnings);
        }

        [Fact]
        public void Load_WithNullNestedObjects_ReturnsEmptyInstances()
        {
            WriteFile("{ \"operatingSystem\": null, \"netFramework\": null, \"networkInterfaces\": [ { \"name\": \"Wi-Fi\", \"ipAddresses\": null } ] }");

            var loaded = _repository.Load();

            Assert.NotNull(loaded.OperatingSystem);
            Assert.NotNull(loaded.NetFramework);
            Assert.Empty(loaded.NetworkInterfaces.Single().IpAddresses);
        }

        [Fact]
        public void Load_WhenFileDoesNotExist_ThrowsFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() => _repository.Load());
        }

        [Theory]
        [InlineData("{ \"operatingSystem\": ")]
        [InlineData("[ 1, 2, 3 ]")]
        [InlineData("")]
        [InlineData("null")]
        public void Load_WhenContentIsInvalid_ThrowsInvalidDataException(string content)
        {
            WriteFile(content);

            Assert.Throws<InvalidDataException>(() => _repository.Load());
        }

        [Fact]
        public void Save_Twice_ReplacesContentWithoutLeavingTemporaryFile()
        {
            var first = CreateFullSystemInfo();
            var second = CreateFullSystemInfo();
            second.OperatingSystem.ComputerName = "ESTACAO-02";

            _repository.Save(first);
            _repository.Save(second);

            Assert.Equal("ESTACAO-02", _repository.Load().OperatingSystem.ComputerName);
            Assert.Equal(new[] { _filePath }, Directory.GetFiles(_directory));
        }

        [Fact]
        public void Exists_ReflectsFilePresence()
        {
            Assert.False(_repository.Exists());

            _repository.Save(CreateFullSystemInfo());
            Assert.True(_repository.Exists());

            File.Delete(_filePath);
            Assert.False(_repository.Exists());
        }

        private void WriteFile(string content)
        {
            Directory.CreateDirectory(_directory);
            File.WriteAllText(_filePath, content);
        }

        private static SystemInfo CreateFullSystemInfo()
        {
            var systemInfo = new SystemInfo
            {
                CollectedAt = new DateTimeOffset(2026, 10, 14, 10, 32, 5, 123, TimeSpan.FromHours(-3)),
                OperatingSystem = new OperatingSystemInfo
                {
                    Caption = "Microsoft Windows 11 Pro",
                    Version = "10.0.26300",
                    Architecture = "64 bits",
                    ComputerName = "ESTACAO-01",
                },
                NetFramework = new NetFrameworkInfo { Version = "4.8.1 ou superior", Release = 533509 },
            };

            systemInfo.Processors.Add(new ProcessorInfo
            {
                Name = "Intel(R) Core(TM) i7",
                Description = "Intel64 Family 6 Model 141 Stepping 1",
            });

            var wifi = new NetworkInterfaceInfo { Name = "Wi-Fi", Description = "Adaptador sem fio", MacAddress = "00:11:22:33:44:55" };
            wifi.IpAddresses.Add("192.168.1.10");
            wifi.IpAddresses.Add("fe80::1");
            systemInfo.NetworkInterfaces.Add(wifi);

            var ethernet = new NetworkInterfaceInfo { Name = "Ethernet", Description = "Adaptador cabeado", MacAddress = "00:1A:2B:3C:4D:5E" };
            ethernet.IpAddresses.Add("10.0.0.2");
            systemInfo.NetworkInterfaces.Add(ethernet);

            systemInfo.Warnings.Add("Aviso de exemplo");
            return systemInfo;
        }
    }
}
