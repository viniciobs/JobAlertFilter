using System.Net.Http.Json;
using System.Text.Json;
using JobAlertFilter.Models;
using JobAlertFilter.Options;
using JobAlertFilter.Services.Providers.Abstractions;
using Microsoft.Extensions.Options;

namespace JobAlertFilter.Services.Providers;

public class OpenAIService(IOptions<AIProviderOptions> opts)
    : AiServiceBase(opts), IAiService
{
    public async Task<IList<AnalysisResult>> AnalyzeAsync(string prompt, CancellationToken cancellationToken)
    {
        var request = new
        {
            model = opts.Model,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = "You are a job description analyzer. Respond ONLY with a JSON array matching the requested schema."
                },
                new
                {
                    role = "user",
                    content = prompt
                }
            },
            temperature = 0.1,
            response_format = new { type = "json_object" }
        };

        using var response = await client.PostAsJsonAsync("chat/completions", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        var content = json
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return JsonSerializer.Deserialize<IList<AnalysisResult>>(
            content!, SerializerOptions)
            ?? throw new JsonException("OpenAI returned invalid JSON.");
    }
}