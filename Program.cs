using JobAlertFilter.Extensions;
using JobAlertFilter.Options;
using JobAlertFilter.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("Configuration/appsettings.json", optional: false)
    .AddJsonFile($"Configuration/appsettings.local.json", optional: true);

builder.Services
    .AddAndValidateOptions<AppOptions>(builder.Configuration, "AppConfiguration")
    .AddAndValidateOptions<ProfileOptions>(builder.Configuration, "Profile")
    .AddAndValidateOptions<OllamaOptions>(builder.Configuration, "Ollama");

builder.Services
    .AddSingleton<EmailScanner>()
    .AddSingleton<PromptLoader>()
    .AddSingleton<OllamaService>()
    .AddSingleton<JobAnalyzer>();

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
