using HRHiringSystem.Application.Handlers;
using HRHiringSystem.Application.Handlers.Abstract;
using HRHiringSystem.Application.Helpers;
using HRHiringSystem.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace HRHiringSystem.Application.DependencyInjection;

public static class HandlerServiceRegistration
{
    public static IServiceCollection AddApplicationHandlers(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IUserHandler, UserHandler>();
        services.AddScoped<IRoleHandler, RoleHandler>();
        services.AddScoped<IJobHandler, JobHandler>();
        services.AddScoped<IJobApplicationHandler, JobApplicationHandler>();
        services.AddScoped<IAuthHandler, AuthHandler>();
        services.AddScoped<ICvHandler, CvHandler>();
        services.AddScoped<IEvaluationReportHandler, EvaluationReportHandler>();

        return services;
    }
}
