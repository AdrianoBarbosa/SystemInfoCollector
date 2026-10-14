using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SystemInfoCollector.Models
{
    /// <summary>
    /// Dados de uma interface de rede com endereço IP configurado.
    /// </summary>
    public class NetworkInterfaceInfo
    {
        /// <summary>
        /// Nome da conexão como exibido pelo Windows (ex.: "Ethernet", "Wi-Fi").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descrição do adaptador de rede.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Endereços IP (IPv4 e IPv6) da interface. Nunca é <c>null</c>.
        /// </summary>
        public List<string> IpAddresses { get; set; } = new List<string>();

        /// <summary>
        /// Endereço físico (MAC) do adaptador.
        /// </summary>
        public string MacAddress { get; set; }

        // Um arquivo editado manualmente pode trazer "ipAddresses": null.
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (IpAddresses == null)
            {
                IpAddresses = new List<string>();
            }
        }
    }
}
