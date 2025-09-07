namespace scommon;

public class ContextTransactionPipelineOptions
{
    
    /// <summary>
    /// Should a transaction be open on commands? Defaults to 'false'.
    /// </summary>
    public bool BeginTransactionOnCommand { get; set; } = false;

    /// <summary>
    /// Should a transaction be open on events? Defaults to 'false'.
    /// </summary>
    public bool BeginTransactionOnEvent { get; set; } = false;
}
