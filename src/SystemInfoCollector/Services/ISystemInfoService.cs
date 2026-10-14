using SystemInfoCollector.Models;

namespace SystemInfoCollector.Services
{
    /// <summary>
    /// Coleta as informações do computador local.
    /// </summary>
    public interface ISystemInfoService
    {
        /// <summary>
        /// Executa a coleta. Falhas em uma fonte não interrompem as demais e são registradas em
        /// <see cref="SystemInfo.Warnings"/>.
        /// </summary>
        SystemInfo Collect();
    }
}
