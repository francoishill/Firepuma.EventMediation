namespace Firepuma.EventMediation.Abstractions.IntegrationEvents;

public interface IIntegrationEventHandler<in TEvent>
{
    Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken);
}