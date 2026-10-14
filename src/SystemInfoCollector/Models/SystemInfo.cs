using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SystemInfoCollector.Models
{
    /// <summary>
    /// Resultado completo de uma coleta de informações do computador local.
    /// </summary>
    public class SystemInfo
    {
        /// <summary>
        /// Momento em que a coleta foi realizada.
        /// </summary>
        public DateTimeOffset CollectedAt { get; set; }

        /// <summary>
        /// Dados do sistema operacional. Nunca é <c>null</c>.
        /// </summary>
        public OperatingSystemInfo OperatingSystem { get; set; } = new OperatingSystemInfo();

        /// <summary>
        /// Processadores físicos. Nunca é <c>null</c>.
        /// </summary>
        public List<ProcessorInfo> Processors { get; set; } = new List<ProcessorInfo>();

        /// <summary>
        /// Versão do .NET Framework instalada. Nunca é <c>null</c>.
        /// </summary>
        public NetFrameworkInfo NetFramework { get; set; } = new NetFrameworkInfo();

        /// <summary>
        /// Interfaces de rede com IP configurado. Nunca é <c>null</c>.
        /// </summary>
        public List<NetworkInterfaceInfo> NetworkInterfaces { get; set; } = new List<NetworkInterfaceInfo>();

        /// <summary>
        /// Avisos sobre partes da coleta que falharam. Nunca é <c>null</c>.
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();

        // Garante que consumidores nunca recebam objetos ou listas nulos,
        // mesmo quando o arquivo foi editado manualmente.
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            OperatingSystem = OperatingSystem ?? new OperatingSystemInfo();
            Processors = Processors ?? new List<ProcessorInfo>();
            NetFramework = NetFramework ?? new NetFrameworkInfo();
            NetworkInterfaces = NetworkInterfaces ?? new List<NetworkInterfaceInfo>();
            Warnings = Warnings ?? new List<string>();
        }
    }
}
