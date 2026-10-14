namespace SystemInfoCollector.Models
{
    /// <summary>
    /// Dados do sistema operacional do computador local.
    /// Valores <c>null</c> indicam que a informação não pôde ser obtida.
    /// </summary>
    public class OperatingSystemInfo
    {
        /// <summary>
        /// Nome do sistema operacional (ex.: "Microsoft Windows 11 Pro").
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Versão do sistema operacional (ex.: "10.0.26300").
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Arquitetura do sistema operacional (ex.: "64 bits").
        /// </summary>
        public string Architecture { get; set; }

        /// <summary>
        /// Nome do computador na rede.
        /// </summary>
        public string ComputerName { get; set; }
    }
}
