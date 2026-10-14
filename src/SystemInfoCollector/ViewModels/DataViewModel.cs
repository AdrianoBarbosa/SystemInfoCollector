using System;
using System.Collections.Generic;
using System.Globalization;
using SystemInfoCollector.Converters;
using SystemInfoCollector.Models;

namespace SystemInfoCollector.ViewModels
{
    /// <summary>
    /// Dados de uma coleta preparados para exibição.
    /// </summary>
    public class DataViewModel : ViewModelBase
    {
        private const string CollectedAtFormat = "dd/MM/yyyy HH:mm:ss";

        /// <summary>
        /// Cria o ViewModel a partir da coleta lida do arquivo.
        /// </summary>
        public DataViewModel(SystemInfo systemInfo)
        {
            if (systemInfo == null)
            {
                throw new ArgumentNullException(nameof(systemInfo));
            }

            CollectedAtText = systemInfo.CollectedAt.ToString(CollectedAtFormat, CultureInfo.InvariantCulture);
            OperatingSystem = systemInfo.OperatingSystem;
            Processors = systemInfo.Processors;
            NetFrameworkText = FormatNetFramework(systemInfo.NetFramework);
            NetworkInterfaces = systemInfo.NetworkInterfaces;
            Warnings = systemInfo.Warnings;
        }

        /// <summary>
        /// Data e hora da coleta.
        /// </summary>
        public string CollectedAtText { get; }

        /// <summary>
        /// Dados do sistema operacional.
        /// </summary>
        public OperatingSystemInfo OperatingSystem { get; }

        /// <summary>
        /// Processadores coletados.
        /// </summary>
        public IReadOnlyList<ProcessorInfo> Processors { get; }

        /// <summary>
        /// Indica se há ao menos um processador para exibir.
        /// </summary>
        public bool HasProcessors => Processors.Count > 0;

        /// <summary>
        /// Versão do .NET Framework acompanhada do identificador numérico, quando disponível.
        /// </summary>
        public string NetFrameworkText { get; }

        /// <summary>
        /// Interfaces de rede com IP configurado.
        /// </summary>
        public IReadOnlyList<NetworkInterfaceInfo> NetworkInterfaces { get; }

        /// <summary>
        /// Indica se há ao menos uma interface de rede para exibir.
        /// </summary>
        public bool HasNetworkInterfaces => NetworkInterfaces.Count > 0;

        /// <summary>
        /// Avisos registrados durante a coleta.
        /// </summary>
        public IReadOnlyList<string> Warnings { get; }

        /// <summary>
        /// Indica se a coleta registrou avisos.
        /// </summary>
        public bool HasWarnings => Warnings.Count > 0;

        private static string FormatNetFramework(NetFrameworkInfo netFramework)
        {
            var version = string.IsNullOrWhiteSpace(netFramework.Version)
                ? NullToNotAvailableConverter.NotAvailableText
                : netFramework.Version;

            return netFramework.Release.HasValue
                ? $"{version} (release {netFramework.Release.Value})"
                : version;
        }
    }
}
