using JobAlertFilter.Models;

namespace JobAlertFilter.Services.Abstractions;

public interface IJobAnalyzer
{
    public Task<IList<AnalysisResult>> AnalyzeAsync(string data, CancellationToken cancellationToken);
}