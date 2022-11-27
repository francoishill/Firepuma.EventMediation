namespace Firepuma.EventMediation.Abstractions.IntegrationEvents;

public interface IIntegrationEventMediator
{
    Task HandleAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken);
}