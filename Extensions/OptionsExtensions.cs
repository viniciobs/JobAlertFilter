using JobAlertFilter.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobAlertFilter.Extensions;

public static class OptionsExtensions
{
    public static IServiceCollection AddAndValidateOptions<TOptions>(this IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
        where TOptions : class, IOptionValidator
    {
        services.AddOptions<TOptions>()
            .Bind(configuration.GetSection(sectionName))
            .Validate(x => x != null && x.IsValid(), $"{sectionName} is invalid. Fill all the required fields in appsettings.json.")
            .ValidateOnStart();

        return services;
    }
}