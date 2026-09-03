using Microsoft.Extensions.Logging;

namespace JobAlertFilter.Services;

public static partial class LinkedInAnalyzerLogs
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Analyzing job {Url} ({Index}/{Total})")]
    public static partial void Start(this ILogger logger, string url, int index, int total);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Timed out analyzing job {Url}")]
    public static partial void Timeout(this ILogger logger, string url);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Failed to analyze job {Url}")]
    public static partial void Error(this ILogger logger, Exception ex, string url);
}