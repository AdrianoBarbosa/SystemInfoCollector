using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SystemInfoCollector.Models;

namespace SystemInfoCollector.Services
{
    /// <summary>
    /// Grava e lê a coleta em um arquivo JSON indentado, codificado em UTF-8.
    /// </summary>
    public class SystemInfoRepository : ISystemInfoRepository
    {
        private const string TemporaryFileExtension = ".tmp";

        private static readonly Encoding Utf8WithoutBom = new UTF8Encoding(false);

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Include,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateParseHandling = DateParseHandling.DateTimeOffset,
            MissingMemberHandling = MissingMemberHandling.Ignore,
        };

        /// <summary>
        /// Cria o repositório para o arquivo informado.
        /// </summary>
        /// <param name="filePath">Caminho completo do arquivo de dados.</param>
        public SystemInfoRepository(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("O caminho do arquivo é obrigatório.", nameof(filePath));
            }

            FilePath = filePath;
        }

        /// <inheritdoc />
        public string FilePath { get; }

        /// <inheritdoc />
        public bool Exists() => File.Exists(FilePath);

        /// <inheritdoc />
        public void Save(SystemInfo systemInfo)
        {
            if (systemInfo == null)
            {
                throw new ArgumentNullException(nameof(systemInfo));
            }

            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

            // Grava primeiro em um arquivo temporário para que uma falha no meio da escrita
            // não deixe o arquivo de dados truncado.
            var temporaryFilePath = FilePath + TemporaryFileExtension;
            File.WriteAllText(temporaryFilePath, JsonConvert.SerializeObject(systemInfo, SerializerSettings), Utf8WithoutBom);

            if (File.Exists(FilePath))
            {
                File.Replace(temporaryFilePath, FilePath, null);
            }
            else
            {
                File.Move(temporaryFilePath, FilePath);
            }
        }
    }
}
