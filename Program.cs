using JobAlertFilter.Configuration;
using JobAlertFilter.Services;
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
    .AddOptions<AppConfiguration>()
    .Bind(builder.Configuration.GetSection("AppConfiguration"))
    .Validate(x => x.IsValid(), "AppConfiguration is invalid. Fill all the required fields in appsettings.json.")
    .ValidateOnStart();

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<AppConfiguration>>().Value);

builder.Services.AddTransient<EmailScanner>();

using var host = builder.Build();

var cancellationToken = host.Services
    .GetRequiredService<IHostApplicationLifetime>()
    .ApplicationStopping;

var scanner = host.Services.GetRequiredService<EmailScanner>();

try
{
    await scanner.RunAsync(cancellationToken);
}
catch (Exception ex)
{
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while running the email scanner.");
}
