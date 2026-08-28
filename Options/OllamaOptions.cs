namespace JobAlertFilter.Options;

public record OllamaOptions: IOptionValidator
{
    public required string BaseUrl { get; init; }
    public required string Model { get; init; }
    public int TimeoutSeconds { get; init; }

    public bool IsValid() =>
        Uri.TryCreate(BaseUrl, UriKind.Absolute, out _) &&
        !string.IsNullOrWhiteSpace(Model) &&
        TimeoutSeconds > 0;
}