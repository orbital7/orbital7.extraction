namespace ScriptJobsConsole;

internal class Program
{
    static internal async Task Main(
        string[] args)
    {
        bool unattendedExecution = false;
        ScriptJobBase? scriptJob = null;

        //scriptJob = new Scripts.ExtractEmailMessages();
        scriptJob = new Scripts.ExportEmailMessages();

        await ScriptJobRunner.ExecuteAsync(
            scriptJob,
            unattendedExecution: unattendedExecution);
    }
}

