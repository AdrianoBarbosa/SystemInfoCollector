namespace SystemInfoCollector.Models
{
    /// <summary>
    /// Dados de um processador físico do computador local.
    /// </summary>
    public class ProcessorInfo
    {
        /// <summary>
        /// Nome comercial do processador.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descrição técnica do processador (família, modelo e stepping).
        /// </summary>
        public string Description { get; set; }
    }
}
