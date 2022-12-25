using Firepuma.EventMediation.IntegrationEvents.ValueObjects;

namespace Firepuma.EventMediation.IntegrationEvents.Abstractions;

public interface IIntegrationEventHandler
{
    Task<bool> TryHandleEvent(
        string eventSourceId,
        IntegrationEventEnvelope integrationEventEnvelope,
        CancellationToken cancellationToken);
}