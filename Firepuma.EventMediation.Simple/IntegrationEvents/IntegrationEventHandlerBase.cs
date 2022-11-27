namespace Firepuma.EventMediation.Simple.IntegrationEvents;

internal abstract class IntegrationEventHandlerBase
{
    public abstract Task HandleAsync(
        object integrationEvent,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}