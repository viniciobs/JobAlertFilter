namespace JobAlertFilter.Configuration;

public record AppConfiguration
{
    public required string Email { get; init; }
    public required string AppPassword { get; init; }
    public required string SearchFromEmail { get; init; }

    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(Email) &&
        !string.IsNullOrWhiteSpace(AppPassword) &&
        !string.IsNullOrWhiteSpace(SearchFromEmail);
};
