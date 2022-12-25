using System.Diagnostics.CodeAnalysis;
using Firepuma.EventMediation.IntegrationEvents.ValueObjects;

namespace Firepuma.EventMediation.IntegrationEvents.Abstractions;

public interface IIntegrationEventDeserializer
{
    bool TryDeserializeIntegrationEvent(
        IntegrationEventEnvelope envelope,
        [NotNullWhen(true)] out object? eventPayload);
}