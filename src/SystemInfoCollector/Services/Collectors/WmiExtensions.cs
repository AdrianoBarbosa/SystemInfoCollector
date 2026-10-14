using System.Management;

namespace SystemInfoCollector.Services.Collectors
{
    /// <summary>
    /// Leitura segura de propriedades de objetos WMI.
    /// </summary>
    internal static class WmiExtensions
    {
        /// <summary>
        /// Lê uma propriedade como texto, sem espaços nas extremidades.
        /// </summary>
        /// <returns>O texto, ou <c>null</c> se a propriedade não existir ou estiver vazia.</returns>
        public static string GetString(this ManagementBaseObject managementObject, string propertyName)
        {
            var text = GetValue(managementObject, propertyName)?.ToString().Trim();
            return string.IsNullOrEmpty(text) ? null : text;
        }

        /// <summary>
        /// Lê uma propriedade do tipo lista de textos.
        /// </summary>
        /// <returns>Os valores, ou uma lista vazia se a propriedade não existir ou estiver nula.</returns>
        public static string[] GetStringArray(this ManagementBaseObject managementObject, string propertyName)
        {
            return GetValue(managementObject, propertyName) as string[] ?? new string[0];
        }

        private static object GetValue(ManagementBaseObject managementObject, string propertyName)
        {
            try
            {
                return managementObject[propertyName];
            }
            catch (ManagementException ex) when (ex.ErrorCode == ManagementStatus.NotFound)
            {
                return null;
            }
        }
    }
}
