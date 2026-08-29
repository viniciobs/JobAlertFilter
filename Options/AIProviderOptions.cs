namespace JobAlertFilter.Options;

public record AIProviderOptions: IOptionValidator
{
    public required string Provider { get; init; }
    public string? APIKey { get; init; }
    public required string BaseUrl { get; init; }
    public required string Model { get; init; }
    public int TimeoutSeconds { get; init; }

    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(Provider) &&
        Uri.TryCreate(BaseUrl, UriKind.Absolute, out _) &&
        !string.IsNullOrWhiteSpace(Model) &&
        TimeoutSeconds > 0;
}