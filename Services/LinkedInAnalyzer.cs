using JobAlertFilter.Extensions;
using JobAlertFilter.Models;
using JobAlertFilter.Options;
using JobAlertFilter.Services.Abstractions;
using JobAlertFilter.Services.Providers.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobAlertFilter.Services;

public class LinkedInAnalyzer(FileContentLoader promptLoader,
    IAiService aiService,
    LinkedInJobScraper jobPageScraper,
    IOptions<ProfileOptions> profile,
    IOptions<AppOptions> appOptions,
    ILogger<LinkedInAnalyzer> logger): IJobAnalyzer
{
    public async Task<IList<AnalysisResult>> AnalyzeAsync(string emailHtml, CancellationToken cancellationToken)
    {
        var urls = emailHtml.ToJobUrls();

        if (urls is null || urls.Count == 0)
        {
            return [];
        }

        var results = new List<AnalysisResult>(urls.Count);

        var replacements = profile.Value.ToReplacements();

        for (var i = 0; i < urls.Count; i++)
        {
            var url = urls[i];
            cancellationToken.ThrowIfCancellationRequested();

            LinkedInAnalyzerLogs.Start(logger, url, i + 1, urls.Count);

            try
            {
                var jobContent = await jobPageScraper.GetJobContentAsync(url, cancellationToken);

                if (string.IsNullOrWhiteSpace(jobContent))
                {
                    continue;
                }

                replacements["JobContent"] = jobContent;

                var prompt = await promptLoader.LoadAsync("linkedin-prompt-template", replacements);
                var analysis = await aiService.AnalyzeAsync<AnalysisResult>(prompt, cancellationToken);

                if (analysis is null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(analysis.Url))
                {
                    analysis = analysis with { Url = url };
                }

                results.Add(analysis);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                LinkedInAnalyzerLogs.Timeout(logger, url);
            }
            catch (Exception ex)
            {
                LinkedInAnalyzerLogs.Error(logger, ex, url);
            }

            if (i < urls.Count - 1)
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(appOptions.Value.JobPageRequestDelaySeconds),
                    cancellationToken);
            }
        }

        return results;
    }
}