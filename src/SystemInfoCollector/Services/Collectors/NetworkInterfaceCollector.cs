using System.Collections.Generic;
using System.Management;
using SystemInfoCollector.Models;

namespace SystemInfoCollector.Services.Collectors
{
    /// <summary>
    /// Coleta as interfaces de rede conectadas pelas classes WMI <c>MSFT_NetAdapter</c> e
    /// <c>MSFT_NetIPAddress</c> (namespace <c>root\StandardCimv2</c>), disponíveis em todo
    /// Windows 10 ou superior.
    /// </summary>
    /// <remarks>
    /// Estas classes listam todas as interfaces que o Windows apresenta como ativas, inclusive
    /// adaptadores virtuais (ex.: "vEthernet" do Hyper-V), e já trazem o nome da conexão.
    /// </remarks>
    public class NetworkInterfaceCollector
    {
        private const string NameProperty = "Name";
        private const string DescriptionProperty = "InterfaceDescription";
        private const string InterfaceIndexProperty = "InterfaceIndex";
        private const string MacAddressProperty = "PermanentAddress";
        private const string IpAddressProperty = "IPAddress";
        private const char ZoneIndexSeparator = '%';
        private const int MacAddressLength = 12;

        /// <summary>
        /// Consulta o WMI e retorna uma entrada por adaptador conectado que tenha endereço IP.
        /// </summary>
        public List<NetworkInterfaceInfo> Collect()
        {
            var ipAddressesByInterface = CollectIpAddresses();
            var networkInterfaces = new List<NetworkInterfaceInfo>();

            using (var searcher = new ManagementObjectSearcher(WmiQueries.StandardCimNamespace, WmiQueries.ConnectedNetAdapters))
            using (var results = searcher.Get())
            {
                foreach (var adapter in results)
                {
                    using (adapter)
                    {
                        var interfaceIndex = adapter[InterfaceIndexProperty] as uint?;
                        if (interfaceIndex == null || !ipAddressesByInterface.TryGetValue(interfaceIndex.Value, out var ipAddresses))
                        {
                            continue;
                        }

                        networkInterfaces.Add(new NetworkInterfaceInfo
                        {
                            Name = adapter.GetString(NameProperty),
                            Description = adapter.GetString(DescriptionProperty),
                            IpAddresses = ipAddresses,
                            MacAddress = FormatMacAddress(adapter.GetString(MacAddressProperty)),
                        });
                    }
                }
            }

            return networkInterfaces;
        }

        /// <summary>
        /// Converte o MAC sem separadores (ex.: "00155D000001") para o formato usado pelas classes
        /// <c>Win32_*</c> (ex.: "00:15:5D:00:00:01").
        /// </summary>
        internal static string FormatMacAddress(string macAddress)
        {
            if (macAddress == null || macAddress.Length != MacAddressLength)
            {
                return macAddress;
            }

            var pairs = new string[MacAddressLength / 2];
            for (var i = 0; i < pairs.Length; i++)
            {
                pairs[i] = macAddress.Substring(i * 2, 2);
            }

            return string.Join(":", pairs);
        }

        /// <summary>
        /// Remove o índice de zona de endereços IPv6 (ex.: "fe80::1%17" → "fe80::1"),
        /// mantendo o mesmo formato das classes <c>Win32_*</c>.
        /// </summary>
        internal static string RemoveZoneIndex(string ipAddress)
        {
            var separatorIndex = ipAddress.IndexOf(ZoneIndexSeparator);
            return separatorIndex < 0 ? ipAddress : ipAddress.Substring(0, separatorIndex);
        }

        private static Dictionary<uint, List<string>> CollectIpAddresses()
        {
            var ipAddressesByInterface = new Dictionary<uint, List<string>>();

            using (var searcher = new ManagementObjectSearcher(WmiQueries.StandardCimNamespace, WmiQueries.NetIpAddresses))
            using (var results = searcher.Get())
            {
                foreach (var address in results)
                {
                    using (address)
                    {
                        var interfaceIndex = address[InterfaceIndexProperty] as uint?;
                        var ipAddress = address.GetString(IpAddressProperty);
                        if (interfaceIndex == null || ipAddress == null)
                        {
                            continue;
                        }

                        if (!ipAddressesByInterface.TryGetValue(interfaceIndex.Value, out var ipAddresses))
                        {
                            ipAddresses = new List<string>();
                            ipAddressesByInterface.Add(interfaceIndex.Value, ipAddresses);
                        }

                        ipAddresses.Add(RemoveZoneIndex(ipAddress));
                    }
                }
            }

            return ipAddressesByInterface;
        }
    }
}
