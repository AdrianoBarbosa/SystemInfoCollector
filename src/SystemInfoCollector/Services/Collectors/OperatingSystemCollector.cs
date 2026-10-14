using System.Management;
using SystemInfoCollector.Models;

namespace SystemInfoCollector.Services.Collectors
{
    /// <summary>
    /// Coleta os dados do sistema operacional pela classe WMI <c>Win32_OperatingSystem</c>.
    /// </summary>
    public class OperatingSystemCollector
    {
        private const string CaptionProperty = "Caption";
        private const string VersionProperty = "Version";
        private const string ArchitectureProperty = "OSArchitecture";
        private const string ComputerNameProperty = "CSName";

        /// <summary>
        /// Consulta o WMI e retorna os dados do sistema operacional.
        /// </summary>
        public OperatingSystemInfo Collect()
        {
            using (var searcher = new ManagementObjectSearcher(WmiQueries.OperatingSystem))
            using (var results = searcher.Get())
            {
                foreach (var operatingSystem in results)
                {
                    using (operatingSystem)
                    {
                        return new OperatingSystemInfo
                        {
                            Caption = operatingSystem.GetString(CaptionProperty),
                            Version = operatingSystem.GetString(VersionProperty),
                            Architecture = operatingSystem.GetString(ArchitectureProperty),
                            ComputerName = operatingSystem.GetString(ComputerNameProperty),
                        };
                    }
                }
            }

            return new OperatingSystemInfo();
        }
    }
}
