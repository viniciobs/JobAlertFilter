using HtmlAgilityPack;

namespace JobAlertFilter.Extensions;

public static partial class HtmlExtensions
{
    private static readonly int MaxLength = 8_000;

    public static string ToPlainText(this string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        doc.DocumentNode.SelectNodes("//script|//style")?
            .ToList()
            .ForEach(n => n.Remove());

        var text = doc.DocumentNode.InnerText;
        var result = TrimWhiteSpaces.Replace(text, " ").Trim();

        if (result.Length > MaxLength)
        {
            result = result[..MaxLength] + "\n[Content truncated...]";
        }

        return result;
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"\s+")]
    private static partial System.Text.RegularExpressions.Regex TrimWhiteSpaces { get; }
}