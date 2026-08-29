using JobAlertFilter.Models;

namespace JobAlertFilter.Services.Providers.Abstractions;

public interface IAiService
{
    Task<IList<AnalysisResult>> AnalyzeAsync(string prompt, CancellationToken cancellationToken);
}