using SystemInfoCollector.Models;

namespace SystemInfoCollector.Services
{
    /// <summary>
    /// Persiste o resultado da coleta em arquivo.
    /// </summary>
    public interface ISystemInfoRepository
    {
        /// <summary>
        /// Caminho completo do arquivo de dados.
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// Indica se o arquivo de dados existe.
        /// </summary>
        bool Exists();

        /// <summary>
        /// Grava a coleta, substituindo o conteúdo anterior.
        /// </summary>
        void Save(SystemInfo systemInfo);

        /// <summary>
        /// Lê a coleta gravada no arquivo.
        /// </summary>
        /// <exception cref="System.IO.FileNotFoundException">O arquivo não existe.</exception>
        /// <exception cref="System.IO.InvalidDataException">O conteúdo do arquivo é inválido.</exception>
        SystemInfo Load();
    }
}
