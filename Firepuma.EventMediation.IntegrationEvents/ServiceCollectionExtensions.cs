using Firepuma.EventMediation.IntegrationEvents.Abstractions;
using Firepuma.EventMediation.IntegrationEvents.Factories;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Firepuma.EventMediation.IntegrationEvents;

public static class ServiceCollectionExtensions
{
    public static void AddIntegrationEventPublishing<
        TTypeProvider,
        TEventPublisher>(
        this IServiceCollection services)
        where TTypeProvider : class, IIntegrationEventTypeProvider
        where TEventPublisher : class, IIntegrationEventPublisher
    {
        services.AddTransient<IIntegrationEventTypeProvider, TTypeProvider>();
        services.AddTransient<IIntegrationEventEnvelopeFactory, IntegrationEventEnvelopeFactory>();
        services.AddTransient<IIntegrationEventPublisher, TEventPublisher>();
    }

    public static void AddIntegrationEventReceiving<
        TEventDeserializer,
        TIntegrationEventHandler>(
        this IServiceCollection services)
        where TEventDeserializer : class, IIntegrationEventDeserializer
        where TIntegrationEventHandler : class, IIntegrationEventHandler
    {
        services.AddTransient<IIntegrationEventDeserializer, TEventDeserializer>();
        services.AddTransient<IIntegrationEventHandler, TIntegrationEventHandler>();
    }
}