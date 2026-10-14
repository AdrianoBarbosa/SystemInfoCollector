using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;
using System.Security;
using SystemInfoCollector.Models;
using SystemInfoCollector.Services.Collectors;

namespace SystemInfoCollector.Services
{
    /// <summary>
    /// Orquestra os coletores e consolida o resultado, mantendo a coleta parcial quando uma fonte falha.
    /// </summary>
    public class SystemInfoService : ISystemInfoService
    {
        private readonly OperatingSystemCollector _operatingSystemCollector;
        private readonly ProcessorCollector _processorCollector;
        private readonly NetFrameworkCollector _netFrameworkCollector;
        private readonly NetworkInterfaceCollector _networkInterfaceCollector;

        /// <summary>
        /// Cria o serviço com os coletores de cada fonte.
        /// </summary>
        public SystemInfoService(
            OperatingSystemCollector operatingSystemCollector,
            ProcessorCollector processorCollector,
            NetFrameworkCollector netFrameworkCollector,
            NetworkInterfaceCollector networkInterfaceCollector)
        {
            _operatingSystemCollector = operatingSystemCollector ?? throw new ArgumentNullException(nameof(operatingSystemCollector));
            _processorCollector = processorCollector ?? throw new ArgumentNullException(nameof(processorCollector));
            _netFrameworkCollector = netFrameworkCollector ?? throw new ArgumentNullException(nameof(netFrameworkCollector));
            _networkInterfaceCollector = networkInterfaceCollector ?? throw new ArgumentNullException(nameof(networkInterfaceCollector));
        }

        /// <inheritdoc />
        public SystemInfo Collect()
        {
            var warnings = new List<string>();

            return new SystemInfo
            {
                CollectedAt = DateTimeOffset.Now,
                OperatingSystem = TryCollect(_operatingSystemCollector.Collect, "sistema operacional", warnings, new OperatingSystemInfo()),
                Processors = TryCollect(_processorCollector.Collect, "processador", warnings, new List<ProcessorInfo>()),
                NetFramework = TryCollect(_netFrameworkCollector.Collect, ".NET Framework", warnings, new NetFrameworkInfo()),
                NetworkInterfaces = TryCollect(_networkInterfaceCollector.Collect, "interfaces de rede", warnings, new List<NetworkInterfaceInfo>()),
                Warnings = warnings,
            };
        }

        // Captura apenas as falhas esperadas ao consultar WMI ou registro
        // (serviço indisponível, permissão negada); qualquer outra é um defeito e deve propagar.
        private static T TryCollect<T>(Func<T> collect, string sectionName, List<string> warnings, T fallback)
        {
            try
            {
                return collect();
            }
            catch (ManagementException ex)
            {
                return RegisterFailure(ex);
            }
            catch (COMException ex)
            {
                return RegisterFailure(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                return RegisterFailure(ex);
            }
            catch (SecurityException ex)
            {
                return RegisterFailure(ex);
            }

            T RegisterFailure(Exception exception)
            {
                warnings.Add($"Não foi possível coletar {sectionName}: {exception.Message}");
                return fallback;
            }
        }
    }
}
