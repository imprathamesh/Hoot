namespace Hoot.Extensions;

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

public static class ServiceCollectionExtensions
{
    public static void AddServicesFromNamespace(this IServiceCollection services, Assembly assembly, string namespacePrefix)
    {
        var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Namespace != null && t.Namespace.StartsWith(namespacePrefix)) // Filter by Namespace
            .Select(t => new
            {
                Implementation = t,
                Interface = t.GetInterfaces().FirstOrDefault(i => i.Name == "I" + t.Name) // Match Interface by Naming Convention
            })
            .Where(t => t.Interface != null); // Ensure it has an interface

        foreach (var type in types)
        {
            services.AddScoped(type.Interface, type.Implementation);
        }
    }
}
//builder.Services.AddServicesFromNamespace(Assembly.GetExecutingAssembly(), "Hoot.Services");