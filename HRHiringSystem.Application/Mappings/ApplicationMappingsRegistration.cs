using Microsoft.Extensions.DependencyInjection;
using AutoMapper;

namespace HRHiringSystem.Application.Mappings;

public static class ApplicationMappingsRegistration
{
    public static IServiceCollection AddApplicationMappings(this IServiceCollection services)
    {
        // Register all mapping profiles in this assembly and configure naming-prefix conventions
        // so properties like `UserName` map to `Name` automatically.
        var prefixes = new[] { "User", "Job", "Candidate", "JobApplication", "ApplicationResult", "InterviewSchedule", "Role" };

        services.AddAutoMapper(cfg =>
        {
            cfg.RecognizeDestinationPrefixes(prefixes);
            cfg.RecognizePrefixes(prefixes);
        }, typeof(ApplicationMappingsRegistration).Assembly);

        return services;
    }
}
