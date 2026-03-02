using HRHiringSystem.Application.Interfaces;
using HRHiringSystem.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using HRHiringSystem.Infrastructure.Services;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

namespace HRHiringSystem.Infrastructure.DependencyInjection;

public static class RepositoryServiceRegistration
{
    public static IServiceCollection AddInfrastructureRepositories(this IServiceCollection services)
    {
        // Generic repository
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Specific repositories
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<ICandidateRepository, CandidateRepository>();
        services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();

        // Infrastructure services
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }

    public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDB") 
            ?? "mongodb://localhost:27017";

        // Configure GUID serialization globally (required for MongoDB.Driver v2.19+)
        // Use try-catch because serializers can only be registered once
        try
        {
            MongoDB.Bson.Serialization.BsonSerializer.RegisterSerializer(
                new MongoDB.Bson.Serialization.Serializers.GuidSerializer(MongoDB.Bson.GuidRepresentation.Standard));
        }
        catch (MongoDB.Bson.BsonSerializationException)
        {
            // Serializer already registered, ignore
        }

        // Register MongoDB client as singleton (thread-safe, connection pooling)
        services.AddSingleton<IMongoClient>(sp => new MongoClient(connectionString));

        // Register evaluation report repository
        services.AddScoped<IEvaluationReportRepository, EvaluationReportRepository>();

        return services;
    }

    public static IServiceCollection AddAIAgentClient(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = configuration["AIAgent:BaseUrl"] ?? "http://localhost:8001";

        services.AddHttpClient<IAIAgentService, AIAgentService>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromMinutes(5); // AI evaluation may take time
        });

        return services;
    }

    public static IServiceCollection AddAzureFunctionClient(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = configuration["AzureFunction:BaseUrl"] ?? "http://localhost:7071";

        services.AddHttpClient<IAzureFunctionService, AzureFunctionService>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30); // Just triggering, shouldn't take long
        });

        return services;
    }
}
