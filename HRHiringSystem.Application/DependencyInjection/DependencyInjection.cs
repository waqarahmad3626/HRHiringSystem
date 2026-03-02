using HRHiringSystem.Application.Validators;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Linq;

namespace HRHiringSystem.Application.DependencyInjection
{
    public static class ValidationServiceRegistration
    {
        public static IServiceCollection AddApplicationValidation(this IServiceCollection services)
        {
            var assembly = typeof(UserValidator).Assembly;

            var candidateTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface);

            foreach (var implType in candidateTypes)
            {
                var validatorInterfaces = implType.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>))
                    .ToArray();

                if (validatorInterfaces.Length == 0)
                    continue;

                foreach (var serviceType in validatorInterfaces)
                {
                    // Register interface -> implementation as transient
                    services.Add(ServiceDescriptor.Transient(serviceType, implType));
                }
            }

            return services;
        }
    }
}
