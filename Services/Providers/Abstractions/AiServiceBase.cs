using System.Net.Http.Headers;
using System.Text.Json;
using JobAlertFilter.Options;
using Microsoft.Extensions.Options;

namespace JobAlertFilter.Services.Providers.Abstractions;

public abstract class AiServiceBase
{
    protected readonly HttpClient client;
    protected readonly AIProviderOptions opts;

    protected static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected static readonly object ResponseSchema = new
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
        },
        additionalProperties = false
    };

    public AiServiceBase(IOptions<AIProviderOptions> opts)
    {
        this.opts = opts.Value;
        client = new HttpClient
        {
            BaseAddress = new Uri(this.opts.BaseUrl),
            Timeout = TimeSpan.FromSeconds(this.opts.TimeoutSeconds)
        };

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

        if (!string.IsNullOrWhiteSpace(this.opts.APIKey))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.opts.APIKey);
        }
    }
}