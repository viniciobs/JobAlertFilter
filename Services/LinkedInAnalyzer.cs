using JobAlertFilter.Models;
using JobAlertFilter.Services.Abstractions;

namespace JobAlertFilter.Services;

public class LinkedInAnalyzer: IJobAnalyzer
{
    public Task<IList<AnalysisResult>> AnalyzeAsync(string content, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}