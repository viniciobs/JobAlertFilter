using JobAlertFilter.Extensions;
using JobAlertFilter.Models;
using JobAlertFilter.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobAlertFilter.Services;

public class EmailScanner(
    IOptions<AppOptions> config,
    IOptions<ProfileOptions> profile,
    PromptLoader promptLoader,
    OllamaService ollama,
    ILogger<EmailScanner> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        EmailScannerLogs.Start(logger, config.Value.Email, config.Value.SearchFromEmail);

        using var client = new MailKit.Net.Imap.ImapClient();

        await client.ConnectAsync("imap.gmail.com", 993, true, cancellationToken);
        await client.AuthenticateAsync(config.Value.Email, config.Value.AppPassword, cancellationToken);

        var inbox = client.Inbox;

        await inbox.OpenAsync(MailKit.FolderAccess.ReadOnly, cancellationToken);

        var query = MailKit.Search.SearchQuery.NotSeen
            .And(MailKit.Search.SearchQuery.FromContains(config.Value.SearchFromEmail));

        var uids = await inbox.SearchAsync(query, cancellationToken);

        EmailScannerLogs.EmailsFound(logger, uids.Count);

        for (var i = 0; i < uids.Count; i++)
        {
            var uid = uids[i];

            EmailScannerLogs.Processing(logger, i + 1, uids.Count);

            var message = await inbox.GetMessageAsync(uid, cancellationToken);
            var htmlBody = message.HtmlBody;

            if (string.IsNullOrWhiteSpace(htmlBody))
            {
                continue;
            }

            var analysisResult = await AnalyzeEmailAsync(htmlBody, cancellationToken);
        }

        await client.DisconnectAsync(true, cancellationToken);
    }

    private async Task<AnalysisResult> AnalyzeEmailAsync(string emailHtml, CancellationToken cancellationToken)
    {
        var plainText = emailHtml.ToPlainText();

        var replacements = profile.Value.ToReplacements();
        replacements["EmailContent"] = plainText;

        var prompt = await promptLoader.LoadAsync("job-analysis", replacements);

        return await ollama.AnalyzeAsync(prompt, cancellationToken);
    }
}