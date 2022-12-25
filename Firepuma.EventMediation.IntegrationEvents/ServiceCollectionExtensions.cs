using Firepuma.EventMediation.IntegrationEvents.Abstractions;
using Firepuma.EventMediation.IntegrationEvents.Factories;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Firepuma.EventMediation.IntegrationEvents;

public static class ServiceCollectionExtensions
{
    public static void AddIntegrationEventsCore<
        TTypeProvider,
        TEventDeserializer,
        TIntegrationEventHandler>(
        this IServiceCollection services)
        where TTypeProvider : class, IIntegrationEventTypeProvider
        where TEventDeserializer : class, IIntegrationEventDeserializer
        where TIntegrationEventHandler : class, IIntegrationEventHandler
    {
        services.AddTransient<IIntegrationEventTypeProvider, TTypeProvider>();
        services.AddTransient<IIntegrationEventDeserializer, TEventDeserializer>();
        services.AddTransient<IIntegrationEventEnvelopeFactory, IntegrationEventEnvelopeFactory>();
        services.AddTransient<IIntegrationEventHandler, TIntegrationEventHandler>();
    }
}