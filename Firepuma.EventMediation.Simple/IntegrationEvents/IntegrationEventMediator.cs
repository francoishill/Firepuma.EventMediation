using System.Collections.Concurrent;
using Firepuma.EventMediation.Abstractions.IntegrationEvents;

namespace Firepuma.EventMediation.Simple.IntegrationEvents;

internal class IntegrationEventMediator : IIntegrationEventMediator
{
    private readonly IServiceProvider _serviceProvider;

    private static readonly ConcurrentDictionary<Type, IntegrationEventHandlerBase> _handlers = new();

    public IntegrationEventMediator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task HandleAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken)
    {
        if (integrationEvent == null)
        {
            throw new ArgumentNullException(nameof(integrationEvent));
        }

        var type = integrationEvent.GetType();

        var handler = _handlers.GetOrAdd(type,
            static t => (IntegrationEventHandlerBase)(Activator.CreateInstance(typeof(IntegrationEventHandlerWrapper<>).MakeGenericType(t))
                                                      ?? throw new InvalidOperationException($"Could not create wrapper type for {t}")));

        return handler.HandleAsync(integrationEvent, _serviceProvider, cancellationToken);
    }
}