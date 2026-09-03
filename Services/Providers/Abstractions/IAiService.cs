namespace JobAlertFilter.Services.Providers.Abstractions;

public interface IAiService
{
    Task<T> AnalyzeAsync<T>(string prompt, CancellationToken cancellationToken);
}