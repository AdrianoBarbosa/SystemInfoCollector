using SystemInfoCollector.Services.Collectors;
using Xunit;

namespace SystemInfoCollector.Tests
{
    public class NetworkInterfaceCollectorTests
    {
        [Theory]
        [InlineData("00155D000001", "00:15:5D:00:00:01")]
        [InlineData(null, null)]
        [InlineData("ABC", "ABC")]
        public void FormatMacAddress_InsertsSeparators(string macAddress, string expected)
        {
            Assert.Equal(expected, NetworkInterfaceCollector.FormatMacAddress(macAddress));
        }

        [Theory]
        [InlineData("fe80::aaaa:bbbb:cccc:dddd%40", "fe80::aaaa:bbbb:cccc:dddd")]
        [InlineData("172.16.0.1", "172.16.0.1")]
        public void RemoveZoneIndex_StripsIpv6Zone(string ipAddress, string expected)
        {
            Assert.Equal(expected, NetworkInterfaceCollector.RemoveZoneIndex(ipAddress));
        }
    }
}
