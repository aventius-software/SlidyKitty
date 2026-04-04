using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace SlidyKitty.Code.Extensions;

internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all non-abstract classes that implement or inherit from the specified base type 
    /// as themselves in the service collection with the given service lifetime.
    /// </summary>
    /// <remarks>
    /// Each discovered implementation is registered as itself, not as the base type or interface. This 
    /// method is useful for scenarios where concrete types need to be resolved directly from the service 
    /// provider. Only non-abstract, public classes assignable to the specified base type are registered.
    /// </remarks>
    /// <typeparam name="TBase">The base type or interface whose implementations will be registered.</typeparam>
    /// <param name="services">The service collection to which the implementations will be added.</param>
    /// <param name="lifetime">The lifetime with which to register each implementation. Specifies how the service will be instantiated and
    /// reused.</param>
    /// <param name="assembly">The assembly to scan for implementations of the specified base type.</param>
    /// <returns>The same service collection instance, to allow for method chaining.</returns>
    public static IServiceCollection AddAllImplementationsAsSelf<TBase>(this IServiceCollection services, ServiceLifetime lifetime, Assembly assembly)
    {
        var baseType = typeof(TBase);
        var types = assembly
            .GetTypes()
            .Where(t => baseType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

        foreach (var type in types)
        {
            var descriptor = new ServiceDescriptor(type, type, lifetime);
            services.Add(descriptor);
        }

        return services;
    }
}
