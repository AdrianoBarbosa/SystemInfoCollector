using System.Management;
using SystemInfoCollector.Models;

namespace SystemInfoCollector.Services.Collectors
{
    /// <summary>
    /// Coleta os dados do sistema operacional pela classe WMI <c>Win32_OperatingSystem</c>.
    /// </summary>
    public class OperatingSystemCollector
    {
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
                            Caption = operatingSystem.GetString("Caption"),
                            Version = operatingSystem.GetString("Version"),
                            Architecture = operatingSystem.GetString("OSArchitecture"),
                            ComputerName = operatingSystem.GetString("CSName"),
                        };
                    }
                }
            }

            return new OperatingSystemInfo();
        }
    }
}
