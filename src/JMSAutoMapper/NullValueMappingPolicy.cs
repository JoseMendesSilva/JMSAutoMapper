namespace JMSAutoMapper
{
    /// <summary>
    /// Define a política de mapeamento para valores nulos em tipos de valor não anuláveis.
    /// </summary>
    public enum NullValueMappingPolicy
    {
        /// <summary>Lança uma exceção se um valor nulo for mapeado para um tipo de valor não anulável.</summary>
        Throw,
        /// <summary>Usa o valor padrão do tipo de destino (ex: 0 para int, false para bool).</summary>
        Default,
        /// <summary>Ignora o mapeamento da propriedade se o valor de origem for nulo.</summary>
        Ignore
    }
}