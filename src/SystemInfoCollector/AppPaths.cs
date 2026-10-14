using System;
using System.IO;

namespace SystemInfoCollector
{
    /// <summary>
    /// Ponto único de definição dos caminhos usados pela aplicação.
    /// </summary>
    internal static class AppPaths
    {
        /// <summary>
        /// Nome da pasta da aplicação dentro de <c>%LocalAppData%</c>.
        /// </summary>
        public const string ApplicationFolderName = "SystemInfoCollector";

        /// <summary>
        /// Nome do arquivo onde a coleta é gravada.
        /// </summary>
        public const string DataFileName = "system-info.json";

        /// <summary>
        /// Pasta de dados da aplicação. Fica em <c>%LocalAppData%</c> porque a pasta do executável
        /// pode não ter permissão de escrita (ex.: quando instalado em Program Files).
        /// </summary>
        public static string DataDirectory =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationFolderName);

        /// <summary>
        /// Caminho completo do arquivo de dados.
        /// </summary>
        public static string DataFilePath => Path.Combine(DataDirectory, DataFileName);
    }
}
