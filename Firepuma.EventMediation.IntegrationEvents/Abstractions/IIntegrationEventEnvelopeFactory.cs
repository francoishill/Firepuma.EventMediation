using Firepuma.EventMediation.IntegrationEvents.ValueObjects;

namespace Firepuma.EventMediation.IntegrationEvents.Abstractions;

public interface IIntegrationEventEnvelopeFactory
{
    IntegrationEventEnvelope CreateEnvelope(
        string eventId,
        string eventType,
        string eventPayload);

    IntegrationEventEnvelope CreateEnvelopeFromObject<TEvent>(
        TEvent integrationEventPayload);
}