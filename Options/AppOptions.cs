namespace JobAlertFilter.Options;

public record AppOptions: IOptionValidator
{
    public required string Email { get; init; }
    public required string AppPassword { get; init; }
    public required string SearchFromEmail { get; init; }
    public int ImapOperationTimeoutSeconds { get; init; } = 30;
    public int ProcessSingleEmailTimeoutMinutes { get; init; } = 5;

    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(Email) &&
        !string.IsNullOrWhiteSpace(AppPassword) &&
        !string.IsNullOrWhiteSpace(SearchFromEmail) &&
        ImapOperationTimeoutSeconds > 0 &&
        ProcessSingleEmailTimeoutMinutes > 0;
};
