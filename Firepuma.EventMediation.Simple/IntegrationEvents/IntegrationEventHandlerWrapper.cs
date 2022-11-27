using Firepuma.EventMediation.Abstractions.IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;

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
        var handler = serviceProvider.GetService<IIntegrationEventHandler<TEvent>>();

        if (handler == null)
        {
            throw new InvalidOperationException($"No integration event handler registered to handle event type '{typeof(TEvent).FullName}'. Please register with " +
                                                $"dependency injection by adding IIntegrationEventHandler<YOUR_EVENT_CLASS>, or by " +
                                                $"calling the {nameof(ServiceCollectionExtensions.AddIntegrationEventMediation)} extension method");
        }

        await handler.HandleAsync(integrationEvent, cancellationToken);
    }
}