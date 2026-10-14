using System;
using System.Collections.Generic;
using System.Linq;
using SystemInfoCollector.Models;

namespace SystemInfoCollector.Services.Collectors
{
    /// <summary>
    /// Combina as interfaces de rede da fonte principal com as da fonte complementar.
    /// </summary>
    internal static class NetworkInterfaceMerger
    {
        /// <summary>
        /// Mantém todas as interfaces da fonte principal e acrescenta, da fonte complementar, apenas as
        /// que ainda não estão presentes (identificadas pela descrição do adaptador ou, na falta dela, pelo MAC).
        /// </summary>
        public static List<NetworkInterfaceInfo> Merge(
            IEnumerable<NetworkInterfaceInfo> primary,
            IEnumerable<NetworkInterfaceInfo> complementary)
        {
            var networkInterfaces = primary.ToList();
            var knownKeys = new HashSet<string>(
                networkInterfaces.Select(GetKey).Where(key => key != null),
                StringComparer.OrdinalIgnoreCase);

            foreach (var networkInterface in complementary)
            {
                var key = GetKey(networkInterface);
                if (key != null && knownKeys.Add(key))
                {
                    networkInterfaces.Add(networkInterface);
                }
            }

            return networkInterfaces;
        }

        private static string GetKey(NetworkInterfaceInfo networkInterface)
        {
            return networkInterface.Description ?? networkInterface.MacAddress;
        }
    }
}
