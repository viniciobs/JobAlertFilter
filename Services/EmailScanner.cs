using HtmlAgilityPack;
using JobAlertFilter.Configuration;
using Microsoft.Extensions.Logging;

namespace JobAlertFilter.Services;

public class EmailScanner(
    AppConfiguration config,
    ILogger<EmailScanner> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        EmailScannerLogs.Start(logger, config.Email, config.SearchFromEmail);

        using var client = new MailKit.Net.Imap.ImapClient();

        await client.ConnectAsync("imap.gmail.com", 993, true, cancellationToken);
        await client.AuthenticateAsync(config.Email, config.AppPassword, cancellationToken);

        var inbox = client.Inbox;

        await inbox.OpenAsync(MailKit.FolderAccess.ReadOnly, cancellationToken);

        var query = MailKit.Search.SearchQuery.NotSeen
            .And(MailKit.Search.SearchQuery.FromContains(config.SearchFromEmail));

        var uids = await inbox.SearchAsync(query, cancellationToken);

        EmailScannerLogs.EmailsFound(logger, uids.Count);

        foreach (var uid in uids)
        {
            var message = await inbox.GetMessageAsync(uid, cancellationToken);
            var htmlBody = message.HtmlBody;

            var doc = new HtmlDocument();
            doc.LoadHtml(htmlBody);
        }

        await client.DisconnectAsync(true, cancellationToken);
    }
}