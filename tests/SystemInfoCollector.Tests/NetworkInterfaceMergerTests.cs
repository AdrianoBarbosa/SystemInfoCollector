using System.Linq;
using SystemInfoCollector.Models;
using SystemInfoCollector.Services.Collectors;
using Xunit;

namespace SystemInfoCollector.Tests
{
    public class NetworkInterfaceMergerTests
    {
        [Fact]
        public void Merge_KeepsPrimaryAndAddsOnlyMissingInterfaces()
        {
            var primary = new[]
            {
                new NetworkInterfaceInfo { Name = "Wi-Fi", Description = "Adaptador sem fio", MacAddress = "00:11:22:33:44:55" },
            };
            var complementary = new[]
            {
                new NetworkInterfaceInfo { Name = "Wi-Fi (duplicado)", Description = "ADAPTADOR SEM FIO", MacAddress = "00:11:22:33:44:55" },
                new NetworkInterfaceInfo { Name = "vEthernet (Default Switch)", Description = "Hyper-V Virtual Ethernet Adapter" },
            };

            var merged = NetworkInterfaceMerger.Merge(primary, complementary);

            Assert.Equal(new[] { "Wi-Fi", "vEthernet (Default Switch)" }, merged.Select(item => item.Name));
        }

        [Fact]
        public void Merge_WhenPrimaryIsEmpty_UsesComplementary()
        {
            var complementary = new[] { new NetworkInterfaceInfo { Name = "Ethernet", Description = "Adaptador cabeado" } };

            var merged = NetworkInterfaceMerger.Merge(new NetworkInterfaceInfo[0], complementary);

            Assert.Equal("Ethernet", Assert.Single(merged).Name);
        }

        [Fact]
        public void Merge_WithoutDescription_UsesMacAddressToDetectDuplicates()
        {
            var primary = new[] { new NetworkInterfaceInfo { Name = "Ethernet", MacAddress = "00:1A:2B:3C:4D:5E" } };
            var complementary = new[] { new NetworkInterfaceInfo { Name = "Ethernet 2", MacAddress = "00:1A:2B:3C:4D:5E" } };

            var merged = NetworkInterfaceMerger.Merge(primary, complementary);

            Assert.Single(merged);
        }

        [Theory]
        [InlineData("00155D000001", "00:15:5D:00:00:01")]
        [InlineData(null, null)]
        [InlineData("ABC", "ABC")]
        public void FormatMacAddress_InsertsSeparators(string macAddress, string expected)
        {
            Assert.Equal(expected, NetAdapterCollector.FormatMacAddress(macAddress));
        }

        [Theory]
        [InlineData("fe80::aaaa:bbbb:cccc:dddd%40", "fe80::aaaa:bbbb:cccc:dddd")]
        [InlineData("172.16.0.1", "172.16.0.1")]
        public void RemoveZoneIndex_StripsIpv6Zone(string ipAddress, string expected)
        {
            Assert.Equal(expected, NetAdapterCollector.RemoveZoneIndex(ipAddress));
        }
    }
}
