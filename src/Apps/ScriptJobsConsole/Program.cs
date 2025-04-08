namespace ScriptJobsConsole;

internal class Program
{
    static internal async Task Main(string[] args)
    {
        bool unattendedExecution = false;
        ScriptJobBase? scriptJob = null;

        scriptJob = new Scripts.AlphaTemp();

        await ScriptJobExecutionEngine.ExecuteAsync(
            scriptJob,
            unattendedExecution: unattendedExecution);
    }
}

