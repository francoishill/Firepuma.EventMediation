using System.Diagnostics.CodeAnalysis;

namespace Firepuma.EventMediation.IntegrationEvents.Abstractions;

public interface IIntegrationEventTypeProvider
{
    bool TryGetIntegrationEventType<TMessage>(
        TMessage messagePayload,
        [NotNullWhen(true)] out string? eventType);
}