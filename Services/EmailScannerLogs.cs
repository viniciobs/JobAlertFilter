using Microsoft.Extensions.Logging;

namespace JobAlertFilter.Services;

public static partial class EmailScannerLogs
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Scanning: [ Account: {Email}, From: {SearchFromEmail} ]")]
    public static partial void Start(this ILogger logger, string email, string searchFromEmail);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Found {Count} unread emails.")]
    public static partial void EmailsFound(this ILogger logger, int count);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Processing email {Index} of {Count}.")]
    public static partial void Processing(this ILogger logger, int index, int count);
}