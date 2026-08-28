using System.Net.Http.Json;
using System.Text.Json;
using JobAlertFilter.Models;
using JobAlertFilter.Options;
using Microsoft.Extensions.Options;

namespace JobAlertFilter.Services;

public class OllamaService
{
    private readonly HttpClient client;
    private readonly OllamaOptions opts;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OllamaService(IOptions<OllamaOptions> opts)
    {
        this.opts = opts.Value;

        client = new HttpClient { BaseAddress = new Uri(this.opts.BaseUrl) };
    }

    public async Task<AnalysisResult> AnalyzeAsync(string prompt, CancellationToken cancellationToken)
    {
        var request = new
        {
            model = opts.Model,
            prompt,
            stream = false,
            options = new { temperature = 0.1 }
        };

        using var response = await client.PostAsJsonAsync("/api/generate", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var ollamaResponse = await response.Content.ReadFromJsonAsync<OllamaApiResponse>(cancellationToken);
        var json = ollamaResponse?.Response ?? "{}";

        return JsonSerializer.Deserialize<AnalysisResult>(json, SerializerOptions)
            ?? new AnalysisResult();
    }
}