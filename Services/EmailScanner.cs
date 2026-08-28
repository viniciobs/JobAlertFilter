using JobAlertFilter.Extensions;
using JobAlertFilter.Models;
using JobAlertFilter.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobAlertFilter.Services;

public class EmailScanner(
    IOptions<AppOptions> config,
    IOptions<ProfileOptions> profile,
    FileContentLoader promptLoader,
    OllamaService ollama,
    ResultWriter resultWriter,
    ILogger<EmailScanner> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        EmailScannerLogs.Start(logger, config.Value.Email, config.Value.SearchFromEmail);

        using var client = new MailKit.Net.Imap.ImapClient();

        using var connectionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        connectionCts.CancelAfter(TimeSpan.FromSeconds(config.Value.ImapOperationTimeoutSeconds));

        await client.ConnectAsync("imap.gmail.com", 993, true, connectionCts.Token);
        await client.AuthenticateAsync(config.Value.Email, config.Value.AppPassword, connectionCts.Token);

        var inbox = client.Inbox;

        await inbox.OpenAsync(MailKit.FolderAccess.ReadOnly, connectionCts.Token);

        var query = MailKit.Search.SearchQuery.NotSeen
            .And(MailKit.Search.SearchQuery.FromContains(config.Value.SearchFromEmail));

        var uids = await inbox.SearchAsync(query, connectionCts.Token);

        EmailScannerLogs.EmailsFound(logger, uids.Count);

        var analysisResults = new List<AnalysisResult>();

        for (var i = 0; i < uids.Count; i++)
        {
            var uid = uids[i];

            try
            {
                EmailScannerLogs.Processing(logger, i + 1, uids.Count, DateTime.Now);

                using var processingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                processingCts.CancelAfter(TimeSpan.FromMinutes(config.Value.ProcessSingleEmailTimeoutMinutes));

                var message = await inbox.GetMessageAsync(uid, processingCts.Token);
                var htmlBody = message.HtmlBody;

                if (string.IsNullOrWhiteSpace(htmlBody))
                {
                    continue;
                }

                var result = await AnalyzeEmailAsync(htmlBody, processingCts.Token);
                analysisResults.Add(result);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning("Timed out processing message {Uid}", uid);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process message {Uid}", uid);
            }
        }

        if (client.IsConnected)
        {
            await client.DisconnectAsync(true, CancellationToken.None);
        }

        await resultWriter.WriteAsync(analysisResults, cancellationToken);

        EmailScannerLogs.Finished(logger, DateTime.Now);
    }

    private async Task<AnalysisResult> AnalyzeEmailAsync(string emailHtml, CancellationToken cancellationToken)
    {
        var plainText = emailHtml.ToPlainText();

        var replacements = profile.Value.ToReplacements();
        replacements["EmailContent"] = plainText;

        var prompt = await promptLoader.LoadAsync("prompt-template", replacements);

        return await ollama.AnalyzeAsync(prompt, cancellationToken);
    }
}