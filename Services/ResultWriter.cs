using System.Text;
using JobAlertFilter.Extensions;
using JobAlertFilter.Models;
using JobAlertFilter.Options;
using Microsoft.Extensions.Options;

namespace JobAlertFilter.Services;

public class ResultWriter(
    IOptions<AppOptions> appOptions,
    FileContentLoader fileContentLoader)
{
    public async Task WriteAsync(IList<AnalysisResult> result, CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(
            appOptions.Value.OutputDirectory,
            $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.md");

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