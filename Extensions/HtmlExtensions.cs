using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace JobAlertFilter.Extensions;

public static partial class HtmlExtensions
{
    private static readonly int MaxLength = 8_000;

    public static string? ToPlainText(this string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var jobCards = doc.DocumentNode.SelectNodes("//td[@data-test-id='job-card']");

        if (jobCards is null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        for (var i = 0; i < jobCards.Count; i++)
        {
            var jobCard = jobCards[i];
            var jobUrl = jobCard.InnerHtml.ExtractJobUrls();
            var text = jobCard.InnerText;
            var result = TrimWhiteSpaces.Replace(text, " ").Trim();

            if (result.Length > MaxLength)
            {
                result = result[..MaxLength] + "\n[Content truncated...]";
            }

            builder.AppendLine($"{i + 1}: {result} | {jobUrl}");
        }

        return builder.ToString();
    }

    public static string ExtractJobUrls(this string emailHtml)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(emailHtml);

        var anchorNodes = doc.DocumentNode.SelectNodes("//a[@href]");

        if (anchorNodes is null)
        {
            return string.Empty;
        }

        foreach (var node in anchorNodes)
        {
            var url = node.GetAttributeValue("href", string.Empty);

            if (string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            var isJobUrl = url.StartsWith("https://www.linkedin.com") && url.Contains("jobs/view");

            if (isJobUrl is false)
            {
                continue;
            }

            var uri = new Uri(url);
            var builder = new UriBuilder(uri)
            {
                Query = string.Empty
            };

            return WebUtility.HtmlDecode(builder.Uri.GetLeftPart(UriPartial.Path));
        }

        return string.Empty;
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"\s+")]
    private static partial System.Text.RegularExpressions.Regex TrimWhiteSpaces { get; }
}