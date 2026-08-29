using System.Net.Http.Json;
using System.Text.Json;
using JobAlertFilter.Models;
using JobAlertFilter.Options;
using JobAlertFilter.Services.Providers.Abstractions;
using Microsoft.Extensions.Options;

namespace JobAlertFilter.Services.Providers;

public class OllamaService(IOptions<AIProviderOptions> opts)
    : AiServiceBase(opts), IAiService
{
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