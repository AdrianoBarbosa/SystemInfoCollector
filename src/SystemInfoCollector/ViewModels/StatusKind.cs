namespace SystemInfoCollector.ViewModels
{
    /// <summary>
    /// Tipo da mensagem exibida na barra de status.
    /// </summary>
    public enum StatusKind
    {
        /// <summary>
        /// Estado normal ou operação concluída com sucesso.
        /// </summary>
        Information,

        /// <summary>
        /// Operação concluída com ressalvas.
        /// </summary>
        Warning,

        /// <summary>
        /// Operação que não pôde ser concluída.
        /// </summary>
        Error,
    }
}
