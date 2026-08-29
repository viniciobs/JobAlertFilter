using JobAlertFilter.Extensions;
using JobAlertFilter.Options;
using JobAlertFilter.Services;
using JobAlertFilter.Services.Providers;
using JobAlertFilter.Services.Providers.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("Configuration/appsettings.json", optional: false)
    .AddJsonFile($"Configuration/appsettings.local.json", optional: true);

builder.Services
    .AddAndValidateOptions<AppOptions>(builder.Configuration, "AppConfiguration")
    .AddAndValidateOptions<ProfileOptions>(builder.Configuration, "Profile")
    .AddAndValidateOptions<AIProviderOptions>(builder.Configuration, "AIProvider");

builder.Services
    .AddSingleton<EmailScanner>()
    .AddSingleton<FileContentLoader>()
    .AddSingleton<OllamaService>()
    .AddSingleton<OpenAIService>()
    .AddSingleton<JobAnalyzer>()
    .AddSingleton<ResultWriter>();

builder.Services.AddSingleton<IAiService>(sp =>
{
    var opts = sp.GetRequiredService<IOptions<AIProviderOptions>>().Value;
    return opts.Provider.ToLowerInvariant() switch
    {
        "ollama" => sp.GetRequiredService<OllamaService>(),
        "groq" => sp.GetRequiredService<OpenAIService>(),
        _ => throw new NotSupportedException($"AI provider '{opts.Provider}' is not supported.")
    };
});

using var host = builder.Build();

var scanner = host.Services.GetRequiredService<EmailScanner>();
var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

try
{
    await scanner.RunAsync(lifetime.ApplicationStopping);
}
catch (Exception ex)
{
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while running the email scanner.");
}
finally
{
    lifetime.StopApplication();
}
