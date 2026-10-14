using SystemInfoCollector.Services.Collectors;
using Xunit;

namespace SystemInfoCollector.Tests
{
    public class NetFrameworkVersionMapperTests
    {
        [Theory]
        [InlineData(null, "Não detectado (anterior à 4.5)")]
        [InlineData(378388, "Não detectado (anterior à 4.5)")]
        [InlineData(378389, "4.5")]
        [InlineData(378675, "4.5.1")]
        [InlineData(379893, "4.5.2")]
        [InlineData(393295, "4.6")]
        [InlineData(394254, "4.6.1")]
        [InlineData(394802, "4.6.2")]
        [InlineData(460798, "4.7")]
        [InlineData(461308, "4.7.1")]
        [InlineData(461808, "4.7.2")]
        [InlineData(528040, "4.8")]
        [InlineData(528449, "4.8")]
        [InlineData(533320, "4.8.1 ou superior")]
        [InlineData(533509, "4.8.1 ou superior")]
        public void GetVersionName_ReturnsReadableVersionForRelease(int? release, string expectedVersion)
        {
            var version = NetFrameworkVersionMapper.GetVersionName(release);

            Assert.Equal(expectedVersion, version);
        }
    }
}
