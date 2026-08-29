using System.Net.Http.Json;
using System.Text.Json;
using JobAlertFilter.Models;
using JobAlertFilter.Options;
using JobAlertFilter.Services.Providers.Abstractions;
using Microsoft.Extensions.Options;

namespace JobAlertFilter.Services.Providers;

public class OllamaService: IAiService
{
    private readonly HttpClient client;
    private readonly AIProviderOptions opts;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly object ResponseSchema = new
    {
        type = "array",
        items = new
        {
            type = "object",
            properties = new
            {
                title = new
                {
                    type = "string"
                },
                url = new
                {
                    type = "string"
                },
                confidenceScore = new
                {
                    type = "integer",
                    minimum = 0,
                    maximum = 100
                },
                matchedCriteria = new
                {
                    type = "array",
                    items = new
                    {
                        type = "string"
                    }
                },
                missingOrConcerns = new
                {
                    type = "array",
                    items = new
                    {
                        type = "string"
                    }
                },
                recommendation = new
                {
                    type = "string",
                    @enum = new[] { "Apply", "Maybe", "Skip" }
                },
                reasoning = new
                {
                    type = "string"
                }
            }
        },
        required = new[]
        {
            "title",
            "confidenceScore",
            "matchedCriteria",
            "missingOrConcerns",
            "recommendation",
            "reasoning",
            "url"
        }
    };

    public OllamaService(IOptions<AIProviderOptions> opts)
    {
        this.opts = opts.Value;

        client = new HttpClient
        {
            BaseAddress = new Uri(this.opts.BaseUrl),
            Timeout = Timeout.InfiniteTimeSpan
        };
    }

    public async Task<IList<AnalysisResult>> AnalyzeAsync(
        string prompt,
        CancellationToken cancellationToken)
    {
        var request = new
        {
            model = opts.Model,
            prompt,
            stream = false,
            format = ResponseSchema,
            options = new
            {
                temperature = 0.1,
                top_p = 0.9,
                repeat_penalty = 1.1
            }
        };

        using var response = await client.PostAsJsonAsync(
            "/api/generate",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var ollamaResponse =
            await response.Content.ReadFromJsonAsync<OllamaApiResponse>(
                cancellationToken);

        if (string.IsNullOrWhiteSpace(ollamaResponse?.Response))
        {
            throw new InvalidOperationException(
                "Ollama returned an empty response.");
        }

        return JsonSerializer.Deserialize<IList<AnalysisResult>>(
            ollamaResponse.Response,
            SerializerOptions)
            ?? throw new JsonException(
                "Ollama returned an invalid AnalysisResult.");
    }
}