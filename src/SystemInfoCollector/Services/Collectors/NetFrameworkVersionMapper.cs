using System.Collections.Generic;

namespace SystemInfoCollector.Services.Collectors
{
    /// <summary>
    /// Converte o valor <c>Release</c> do registro na versão legível do .NET Framework,
    /// conforme a tabela oficial da Microsoft.
    /// </summary>
    internal static class NetFrameworkVersionMapper
    {
        /// <summary>
        /// Texto usado quando não há .NET Framework 4.5 ou superior instalado.
        /// </summary>
        public const string NotDetectedText = "Não detectado (anterior à 4.5)";

        // Ordenada do maior para o menor: a primeira faixa cujo mínimo for atendido define a versão.
        // A faixa mais alta é "ou superior" porque versões futuras não podem ser distinguidas pelo número.
        private static readonly IReadOnlyList<KeyValuePair<int, string>> MinimumReleaseVersions =
            new List<KeyValuePair<int, string>>
            {
                new KeyValuePair<int, string>(533320, "4.8.1 ou superior"),
                new KeyValuePair<int, string>(528040, "4.8"),
                new KeyValuePair<int, string>(461808, "4.7.2"),
                new KeyValuePair<int, string>(461308, "4.7.1"),
                new KeyValuePair<int, string>(460798, "4.7"),
                new KeyValuePair<int, string>(394802, "4.6.2"),
                new KeyValuePair<int, string>(394254, "4.6.1"),
                new KeyValuePair<int, string>(393295, "4.6"),
                new KeyValuePair<int, string>(379893, "4.5.2"),
                new KeyValuePair<int, string>(378675, "4.5.1"),
                new KeyValuePair<int, string>(378389, "4.5"),
            };

        /// <summary>
        /// Retorna a versão correspondente ao valor <c>Release</c>.
        /// </summary>
        /// <param name="release">Valor lido do registro, ou <c>null</c> se ausente.</param>
        public static string GetVersionName(int? release)
        {
            if (release.HasValue)
            {
                foreach (var minimumRelease in MinimumReleaseVersions)
                {
                    if (release.Value >= minimumRelease.Key)
                    {
                        return minimumRelease.Value;
                    }
                }
            }

            return NotDetectedText;
        }
    }
}
