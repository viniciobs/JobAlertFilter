using System.Net;
using HtmlAgilityPack;

namespace JobAlertFilter.Services;

public static class EmailJobUrlExtractor
{
    public static IEnumerable<string> ExtractJobUrls(string emailHtml)
    {
        var jobUrls = new HashSet<string>();

        var doc = new HtmlDocument();
        doc.LoadHtml(emailHtml);

        var anchorNodes = doc.DocumentNode.SelectNodes("//a[@href]");

        if (anchorNodes is null)
        {
            return jobUrls;
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

            jobUrls.Add(
                WebUtility.HtmlDecode(
                    builder.Uri.GetLeftPart(UriPartial.Path)));
        }

        return jobUrls;
    }
}