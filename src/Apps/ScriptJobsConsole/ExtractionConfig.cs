namespace ScriptJobsConsole;

public class ExtractionConfig
{
    public MicrosoftEntraIdAppConfig EmailExtractionApp { get; set; } = new();

    public List<MicrosoftEntraIdAppExtractionTarget> EmailExtractionTargets { get; set; } = new();

    public static ExtractionConfig Load<TAssemblyClass>(
        string? environmentVariableName = null)
        where TAssemblyClass : class
    {
        return ConfigurationHelper.GetConfigurationWithUserSecrets<ExtractionConfig, TAssemblyClass>(
            environmentVariableName) ?? new ExtractionConfig();
    }
}
