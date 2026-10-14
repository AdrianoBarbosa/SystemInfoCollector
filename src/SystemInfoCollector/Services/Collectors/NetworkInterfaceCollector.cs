using System.Collections.Generic;
using System.Linq;
using System.Management;
using SystemInfoCollector.Models;

namespace SystemInfoCollector.Services.Collectors
{
    /// <summary>
    /// Coleta as interfaces de rede com IP configurado pelas classes WMI
    /// <c>Win32_NetworkAdapterConfiguration</c> e <c>Win32_NetworkAdapter</c>.
    /// </summary>
    public class NetworkInterfaceCollector
    {
        private const string DescriptionProperty = "Description";
        private const string IpAddressProperty = "IPAddress";
        private const string MacAddressProperty = "MACAddress";
        private const string ConnectionNameProperty = "NetConnectionID";

        /// <summary>
        /// Consulta o WMI e retorna uma entrada por interface com IP habilitado.
        /// </summary>
        public List<NetworkInterfaceInfo> Collect()
        {
            var networkInterfaces = new List<NetworkInterfaceInfo>();

            using (var searcher = new ManagementObjectSearcher(WmiQueries.NetworkAdapterConfiguration))
            using (var results = searcher.Get())
            {
                foreach (ManagementObject configuration in results)
                {
                    using (configuration)
                    {
                        var description = configuration.GetString(DescriptionProperty);
                        networkInterfaces.Add(new NetworkInterfaceInfo
                        {
                            Name = GetConnectionName(configuration) ?? description,
                            Description = description,
                            IpAddresses = configuration.GetStringArray(IpAddressProperty).ToList(),
                            MacAddress = configuration.GetString(MacAddressProperty),
                        });
                    }
                }
            }

            return networkInterfaces;
        }

        // A configuração não tem o nome exibido pelo Windows ("Ethernet", "Wi-Fi");
        // ele está no adaptador associado.
        private static string GetConnectionName(ManagementObject configuration)
        {
            using (var adapters = configuration.GetRelated(WmiQueries.NetworkAdapterClassName))
            {
                foreach (var adapter in adapters)
                {
                    using (adapter)
                    {
                        return adapter.GetString(ConnectionNameProperty);
                    }
                }
            }

            return null;
        }
    }
}
