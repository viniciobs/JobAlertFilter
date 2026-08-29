using JobAlertFilter.Extensions;
using JobAlertFilter.Models;
using JobAlertFilter.Options;
using Microsoft.Extensions.Options;

namespace JobAlertFilter.Services;

public class JobAnalyzer(
    FileContentLoader promptLoader,
    OllamaService ollama,
    IOptions<ProfileOptions> profile)
{
    public async Task<IList<AnalysisResult>> AnalyzeEmailAsync(string emailHtml, CancellationToken cancellationToken)
    {
        var plainText = emailHtml.ToPlainText();

        if (string.IsNullOrWhiteSpace(plainText))
        {
            return [];
        }

        var replacements = profile.Value.ToReplacements();
        replacements["EmailContent"] = plainText;

        var prompt = await promptLoader.LoadAsync("prompt-template", replacements);

        return await ollama.AnalyzeAsync(prompt, cancellationToken);
    }
}