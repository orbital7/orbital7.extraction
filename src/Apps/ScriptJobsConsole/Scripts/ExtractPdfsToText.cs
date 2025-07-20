namespace ScriptJobsConsole.Scripts;

public class ExtractPdfsToText :
    SynchronousScriptJobBase
{
    protected override void Execute()
    {
        var list = new List<(string, string)>();

        var pdfFolderPath = "TODO";
        var pdfFilePaths = Directory.GetFiles(pdfFolderPath, "*.pdf");
        var pdfExtractorService = new PdfExtractorService();

        int i = 0;
        foreach (var pdfFilePath in pdfFilePaths)
        {
            i++;
            Console.WriteLine($"Extracting {i}/{pdfFilePaths.Length}: {pdfFilePath}");

            var extractedText = pdfExtractorService.ExtractPageText(pdfFilePath);
            list.Add((pdfFilePath, string.Join(Environment.NewLine, extractedText)));
        }
    }
}
