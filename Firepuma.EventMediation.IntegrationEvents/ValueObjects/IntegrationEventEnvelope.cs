namespace Firepuma.EventMediation.IntegrationEvents.ValueObjects;

public class IntegrationEventEnvelope
{
    public string EventId { get; init; } = null!;
    public string EventType { get; init; } = null!;
    public string EventPayload { get; init; } = null!;
}