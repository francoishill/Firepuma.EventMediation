using Firepuma.EventMediation.Abstractions.IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

#pragma warning disable CS0618

namespace Firepuma.EventMediation.Simple.IntegrationEvents;

internal class IntegrationEventHandlerWrapper<TEvent> : IntegrationEventHandlerBase
{
    public override async Task HandleAsync(object integrationEvent, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        await HandleEventAsync((TEvent)integrationEvent, serviceProvider, cancellationToken);
    }

    private static async Task HandleEventAsync(
        TEvent integrationEvent,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<IntegrationEventHandlerWrapper<TEvent>>>();
        var handler = serviceProvider.GetService<IIntegrationEventHandler<TEvent>>();

        if (handler == null)
        {
            throw new InvalidOperationException($"No integration event handler registered to handle event type '{typeof(TEvent).FullName}'. Please register with " +
                                                $"dependency injection by adding IIntegrationEventHandler<YOUR_EVENT_CLASS>, or by " +
                                                $"calling the {nameof(ServiceCollectionExtensions.AddIntegrationEventMediation)} extension method");
        }

        logger.LogInformation("Event type '{EventType}' is being handled by handler type '{HandlerType}'", typeof(TEvent).FullName, handler.GetType().FullName);

        await handler.HandleAsync(integrationEvent, cancellationToken);
    }
}