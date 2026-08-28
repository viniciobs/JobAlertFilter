using System.Text;
using JobAlertFilter.Extensions;
using JobAlertFilter.Models;

namespace JobAlertFilter.Services;

public class ResultWriter(FileContentLoader fileContentLoader)
{
    private const string OutputFolder = "./Results";

    public async Task WriteAsync(IList<AnalysisResult> result, CancellationToken cancellationToken)
    {
        var timestamp = DateTime.Now;
        var dateDir = Path.Combine(OutputFolder, timestamp.ToString("yyyy-MM-dd"));

        Directory.CreateDirectory(dateDir);

        var fileName = $"{timestamp:HHmmss}.md";
        var filePath = Path.Combine(dateDir, fileName);

        var fileContent = new StringBuilder("# Analysis Results");
        fileContent.AppendLine();

        foreach (var item in result)
        {
            var content = await fileContentLoader
                .LoadAsync("analysis-result-template", item.ToReplacements());

            fileContent.AppendLine(content);

            fileContent.AppendLine("---");
        }

        await File.WriteAllTextAsync(filePath, fileContent.ToString(), cancellationToken);
    }
}