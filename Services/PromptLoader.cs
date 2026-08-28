namespace JobAlertFilter.Services;

public partial class PromptLoader
{
    private readonly string promptDirectory = Path.Combine(AppContext.BaseDirectory, "Prompts");

    public async Task<string> LoadAsync(string promptName, Dictionary<string, string> replacements)
    {
        var path = Path.Combine(promptDirectory, $"{promptName}.md");

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Prompt file not found: {path}");
        }

        var template = await File.ReadAllTextAsync(path);

        foreach (var (key, value) in replacements)
        {
            template = template.Replace($"{{{{{key}}}}}", value);
        }

        var remaining = RemainingPlaceholders.Matches(template);

        if (remaining.Count > 0)
        {
            var keys = string.Join(", ", remaining.Select(m => m.Groups[1].Value));
            throw new InvalidOperationException($"Unfilled placeholders in prompt: {keys}");
        }

        return template;
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial System.Text.RegularExpressions.Regex RemainingPlaceholders { get; }
}