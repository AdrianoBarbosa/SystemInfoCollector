namespace SystemInfoCollector.Services.Collectors
{
    /// <summary>
    /// Consultas WQL usadas na coleta. Selecionam apenas as propriedades necessárias
    /// para reduzir o tempo de resposta do WMI.
    /// </summary>
    internal static class WmiQueries
    {
        /// <summary>
        /// Dados do sistema operacional.
        /// </summary>
        public const string OperatingSystem =
            "SELECT Caption, Version, OSArchitecture, CSName FROM Win32_OperatingSystem";

        /// <summary>
        /// Dados dos processadores.
        /// </summary>
        public const string Processor =
            "SELECT Name, Description FROM Win32_Processor";

        /// <summary>
        /// Namespace WMI das classes de rede <c>MSFT_*</c>.
        /// </summary>
        public const string StandardCimNamespace = @"root\StandardCimv2";

        /// <summary>
        /// Adaptadores de rede conectados (<c>MediaConnectState = 1</c>), equivalente aos listados como ativos pelo Windows.
        /// </summary>
        public const string ConnectedNetAdapters =
            "SELECT Name, InterfaceDescription, InterfaceIndex, PermanentAddress FROM MSFT_NetAdapter WHERE MediaConnectState = 1";

        /// <summary>
        /// Endereços IP de todas as interfaces, associados pelo índice da interface.
        /// </summary>
        public const string NetIpAddresses =
            "SELECT InterfaceIndex, IPAddress FROM MSFT_NetIPAddress";
    }
}
