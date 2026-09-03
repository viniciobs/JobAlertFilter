using JobAlertFilter.Extensions;
using JobAlertFilter.Models;
using JobAlertFilter.Services.Abstractions;

namespace JobAlertFilter.Services;

public class LinkedInAnalyzer: IJobAnalyzer
{
    public async Task<IList<AnalysisResult>> AnalyzeAsync(string emailHtml, CancellationToken cancellationToken)
    {
        var urls = emailHtml.ToJobUrls();

        if (urls is null || !urls.Any())
        {
            return [];
        }

        var results = new List<AnalysisResult>(urls.Count());

        return results;
    }
}