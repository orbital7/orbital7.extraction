namespace ScriptJobsConsole;

public class ExtractionConfig
{
    public string? SyncfusionLicenseKey { get; set; }

    public MicrosoftEntraIdAppConfig EmailExtractionAppConfig { get; set; } = new();

    public MicrosoftEntraIdAppTokenInfo EmailExtractionAppTokenInfo { get; set; } = new();

    public List<EmailExtractionExportTarget> EmailExtractionTargets { get; set; } = new();

    public static ExtractionConfig Load<TAssemblyClass>(
        string? environmentVariableName = null)
        where TAssemblyClass : class
    {
        return ConfigurationHelper.GetConfigurationWithUserSecrets<ExtractionConfig, TAssemblyClass>(
            environmentVariableName) ?? new ExtractionConfig();
    }
}
