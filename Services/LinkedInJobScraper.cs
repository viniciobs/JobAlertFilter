using System.Text;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace JobAlertFilter.Services;

public partial class LinkedInJobScraper(ILogger<LinkedInJobScraper> logger)
{
    private const int MaxContentLength = 8_000;

    private static readonly HttpClient Client = new()
    {
        Timeout = TimeSpan.FromSeconds(30),
        DefaultRequestHeaders =
        {
            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36" },
            { "Accept-Language", "en-US,en;q=0.9" },
        }
    };

    public async Task<string?> GetJobContentAsync(string jobUrl, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await Client.GetAsync(jobUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("LinkedIn returned {StatusCode} for {Url}", response.StatusCode, jobUrl);
                return null;
            }

            var html = await response.Content.ReadAsStringAsync(cancellationToken);
            var content = ExtractJobContent(html);

            if (content is null)
            {
                logger.LogWarning("Could not extract job content from {Url}", jobUrl);
            }

            return content;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Request to {Url} timed out", jobUrl);
            return null;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Request to {Url} failed", jobUrl);
            return null;
        }
    }

    private static string? ExtractJobContent(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var title = GetText(doc, "//h1[contains(@class,'topcard__title') or contains(@class,'top-card-layout__title')]");
        var company = GetText(doc, "//a[contains(@class,'topcard__org-name-link')]//span | //span[contains(@class,'topcard__flavor') and not(contains(@class,'bullet'))]");
        var location = GetText(doc, "//span[contains(@class,'topcard__flavor--bullet')]");
        var description = doc.DocumentNode.SelectSingleNode(
            "//div[contains(@class,'show-more-less-html__markup') or contains(@class,'description__text')]");

        if (description is null)
        {
            return null;
        }

        return BuildContent(title, company, location, description.InnerHtml);
    }

    private static string? GetText(HtmlDocument doc, string xpath) =>
        Normalize(doc.DocumentNode.SelectSingleNode(xpath)?.InnerText);

    private static string BuildContent(string? title, string? company, string? location, string? descriptionHtml)
    {
        var builder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(title)) builder.AppendLine($"Title: {title}");
        if (!string.IsNullOrWhiteSpace(company)) builder.AppendLine($"Company: {company}");
        if (!string.IsNullOrWhiteSpace(location)) builder.AppendLine($"Location: {location}");
        builder.AppendLine();
        builder.AppendLine(HtmlToText(descriptionHtml));

        var content = builder.ToString();

        return content.Length > MaxContentLength
            ? content[..MaxContentLength] + "\n[Content truncated...]"
            : content;
    }

    private static string HtmlToText(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return Normalize(doc.DocumentNode.InnerText);
    }

    private static string Normalize(string? text) =>
        text is null
            ? string.Empty
            : CollapseWhitespace.Replace(System.Net.WebUtility.HtmlDecode(text), " ").Trim();

    [System.Text.RegularExpressions.GeneratedRegex(@"\s+")]
    private static partial System.Text.RegularExpressions.Regex CollapseWhitespace { get; }
}