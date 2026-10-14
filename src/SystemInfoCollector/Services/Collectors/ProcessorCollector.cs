using System.Collections.Generic;
using System.Management;
using SystemInfoCollector.Models;

namespace SystemInfoCollector.Services.Collectors
{
    /// <summary>
    /// Coleta os processadores físicos pela classe WMI <c>Win32_Processor</c>.
    /// </summary>
    public class ProcessorCollector
    {
        /// <summary>
        /// Consulta o WMI e retorna um item por processador físico.
        /// </summary>
        public List<ProcessorInfo> Collect()
        {
            var processors = new List<ProcessorInfo>();

            using (var searcher = new ManagementObjectSearcher(WmiQueries.Processor))
            using (var results = searcher.Get())
            {
                foreach (var processor in results)
                {
                    using (processor)
                    {
                        processors.Add(new ProcessorInfo
                        {
                            Name = processor.GetString("Name"),
                            Description = processor.GetString("Description"),
                        });
                    }
                }
            }

            return processors;
        }
    }
}
