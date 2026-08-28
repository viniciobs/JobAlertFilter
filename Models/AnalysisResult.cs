namespace JobAlertFilter.Models;

public record AnalysisResult
{
    public bool IsMatch { get; init; }
    public int ConfidenceScore { get; init; }
    public List<string> MatchedCriteria { get; init; } = [];
    public List<string> MissingOrConcerns { get; init; } = [];
    public string Recommendation { get; init; } = "";
    public string Reasoning { get; init; } = "";
}