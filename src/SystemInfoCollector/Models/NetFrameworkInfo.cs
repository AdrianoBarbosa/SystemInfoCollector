namespace SystemInfoCollector.Models
{
    /// <summary>
    /// Versão do .NET Framework 4.x instalada no Windows.
    /// </summary>
    public class NetFrameworkInfo
    {
        /// <summary>
        /// Versão em formato legível (ex.: "4.8").
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Identificador numérico <c>Release</c> lido do registro, usado para determinar a versão.
        /// </summary>
        public int? Release { get; set; }
    }
}
