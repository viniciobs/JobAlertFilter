namespace JobAlertFilter.Options;

public record ProfileOptions: IOptionValidator
{
    public List<string> WorkModes { get; init; } = [];
    public List<string> Locations { get; init; } = [];
    public List<string> PrimaryStack { get; init; } = [];
    public List<string> SecondaryStack { get; init; } = [];
    public int MinYearsExperience { get; init; }
    public List<string> Languages { get; init; } = [];
    public List<string> Roles { get; init; } = [];
    public List<string> AvoidKeywords { get; init; } = [];
    public List<string> MustHaveKeywords { get; init; } = [];

    public bool IsValid() =>
        WorkModes.Count > 0 &&
        Locations.Count > 0 &&
        PrimaryStack.Count > 0 &&
        SecondaryStack.Count > 0 &&
        MinYearsExperience >= 0 &&
        Languages.Count > 0 &&
        Roles.Count > 0 &&
        AvoidKeywords.Count > 0 &&
        MustHaveKeywords.Count > 0;
}