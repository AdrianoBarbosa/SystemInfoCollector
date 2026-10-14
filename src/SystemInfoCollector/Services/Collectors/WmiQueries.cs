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
        /// Configurações de adaptadores de rede com IP habilitado.
        /// </summary>
        /// <remarks>
        /// <c>Index</c> é a propriedade-chave da classe e precisa permanecer na projeção:
        /// sem ela o WMI não preenche <c>__PATH</c> e <c>GetRelated</c> deixa de funcionar.
        /// </remarks>
        public const string NetworkAdapterConfiguration =
            "SELECT Index, Description, IPAddress, MACAddress FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = TRUE";

        /// <summary>
        /// Classe associada à configuração de rede que contém o nome amigável da conexão.
        /// </summary>
        public const string NetworkAdapterClassName = "Win32_NetworkAdapter";
    }
}
