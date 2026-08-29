using JobAlertFilter.Extensions;
using JobAlertFilter.Models;
using JobAlertFilter.Options;
using JobAlertFilter.Services.Providers.Abstractions;
using Microsoft.Extensions.Options;

namespace JobAlertFilter.Services;

public class JobAnalyzer(
    FileContentLoader promptLoader,
    IAiService aiService,
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

        return await aiService.AnalyzeAsync(prompt, cancellationToken);
    }
}