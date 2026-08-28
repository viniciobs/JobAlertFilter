using JobAlertFilter.Extensions;
using JobAlertFilter.Models;
using JobAlertFilter.Options;
using Microsoft.Extensions.Options;

namespace JobAlertFilter.Services;

public class JobAnalyzer(
    PromptLoader promptLoader,
    OllamaService ollama,
    IOptions<ProfileOptions> profile)
{
    public async Task<AnalysisResult> AnalyzeEmailAsync(string emailHtml, CancellationToken cancellationToken)
    {
        var plainText = emailHtml.ToPlainText();

        var replacements = profile.Value.ToReplacements();
        replacements["EmailContent"] = plainText;

        var prompt = await promptLoader.LoadAsync("job-analysis", replacements);

        return await ollama.AnalyzeAsync(prompt, cancellationToken);
    }
}