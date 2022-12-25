using Firepuma.EventMediation.IntegrationEvents.ValueObjects;

namespace Firepuma.EventMediation.IntegrationEvents.Abstractions;

public interface IIntegrationEventPublisher
{
    Task SendAsync(IntegrationEventEnvelope eventEnvelope, CancellationToken cancellationToken);
}