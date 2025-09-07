namespace scommon;

/// <summary>
/// Validation pipeline options
/// </summary>
public class ValidationPipelineOptions
{
    /// <summary>
    /// Should commands be validated? Defaults to 'false'.
    /// </summary>
    public bool ValidateCommand { get; set; } = false;

    /// <summary>
    /// Should events be validated? Defaults to 'false'.
    /// </summary>
    public bool ValidateEvent { get; set; } = false;

    /// <summary>
    /// Should commands be validated? Defaults to 'false'.
    /// </summary>
    public bool ValidateQuery { get; set; } = false;
    
}