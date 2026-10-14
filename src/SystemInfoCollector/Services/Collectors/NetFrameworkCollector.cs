using System;
using Microsoft.Win32;
using SystemInfoCollector.Models;

namespace SystemInfoCollector.Services.Collectors
{
    /// <summary>
    /// Identifica a versão do .NET Framework 4.x instalada a partir do registro do Windows.
    /// </summary>
    public class NetFrameworkCollector
    {
        private const string NdpSubKeyPath = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full";
        private const string ReleaseValueName = "Release";

        /// <summary>
        /// Lê o registro e retorna a versão instalada.
        /// </summary>
        public NetFrameworkInfo Collect()
        {
            var release = ReadRelease();
            return new NetFrameworkInfo
            {
                Release = release,
                Version = NetFrameworkVersionMapper.GetVersionName(release),
            };
        }

        private static int? ReadRelease()
        {
            // A view explícita garante a mesma leitura em processos de 32 e 64 bits.
            var registryView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;

            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
            using (var ndpKey = baseKey.OpenSubKey(NdpSubKeyPath))
            {
                return ndpKey?.GetValue(ReleaseValueName) as int?;
            }
        }
    }
}
