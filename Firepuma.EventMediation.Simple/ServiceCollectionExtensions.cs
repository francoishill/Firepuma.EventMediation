using System.Reflection;
using Firepuma.EventMediation.Abstractions.IntegrationEvents;
using Firepuma.EventMediation.Simple.IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Firepuma.EventMediation.Simple;

[Obsolete("Firepuma.EventMediation.Simple library is obsolete, please use Firepuma.EventMediation.IntegrationEvents and Firepuma.EventMediation.IntegrationEvents.CommandExecution instead")]
public static class ServiceCollectionExtensions
{
    [Obsolete("Firepuma.EventMediation.Simple library is obsolete, please use Firepuma.EventMediation.IntegrationEvents and Firepuma.EventMediation.IntegrationEvents.CommandExecution instead")]
    public static void AddIntegrationEventMediation(this IServiceCollection services, Assembly[] assembliesWithHandlers)
    {
        services.AddTransient<IIntegrationEventMediator, IntegrationEventMediator>();
        services.AddIntegrationEventHandlersFromAssemblies(assembliesWithHandlers);
    }

    private static void AddIntegrationEventHandlersFromAssemblies(this IServiceCollection services, Assembly[] assemblies)
    {
        var handlerType = typeof(IIntegrationEventHandler<>);
        foreach (var assembly in assemblies)
        {
            assembly.GetTypesAssignableTo(handlerType).ForEach(type =>
            {
                foreach (var implementedInterface in type.ImplementedInterfaces)
                {
                    services.AddTransient(implementedInterface, type);
                }
            });
        }
    }

    private static List<TypeInfo> GetTypesAssignableTo(this Assembly assembly, Type compareType)
    {
        var typeInfoList = assembly.DefinedTypes.Where(x => x.IsClass
                                                            && !x.IsAbstract
                                                            && x != compareType
                                                            && x.GetInterfaces()
                                                                .Any(i => i.IsGenericType
                                                                          && i.GetGenericTypeDefinition() == compareType)).ToList();

        return typeInfoList;
    }
}