using Firepuma.EventMediation.IntegrationEvents.Abstractions;
using Firepuma.EventMediation.IntegrationEvents.ValueObjects;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Firepuma.EventMediation.IntegrationEvents.Factories;

internal class IntegrationEventEnvelopeFactory : IIntegrationEventEnvelopeFactory
{
    private readonly ILogger<IntegrationEventEnvelopeFactory> _logger;
    private readonly IIntegrationEventTypeProvider _integrationEventTypeProvider;

    public IntegrationEventEnvelopeFactory(
        ILogger<IntegrationEventEnvelopeFactory> logger,
        IIntegrationEventTypeProvider integrationEventTypeProvider)
    {
        _logger = logger;
        _integrationEventTypeProvider = integrationEventTypeProvider;
    }

    public IntegrationEventEnvelope CreateEnvelope(
        string eventId,
        string eventType,
        string eventPayload)
    {
        return new IntegrationEventEnvelope
        {
            EventId = eventId,
            EventType = eventType,
            EventPayload = eventPayload,
        };
    }

    public IntegrationEventEnvelope CreateEnvelopeFromObject<TEvent>(TEvent integrationEventPayload)
    {
        if (!_integrationEventTypeProvider.TryGetIntegrationEventType(integrationEventPayload, out var integrationEventType))
        {
            var payloadType = integrationEventPayload?.GetType().FullName;
            _logger.LogError(
                "Unable to get integration event for type {Type}",
                payloadType);

            throw new Exception($"Unable to get integration event for type {payloadType}");
        }

        var integrationEventId = IntegrationEventIdFactory.GenerateIntegrationEventId();

        var integrationEventPayloadJson = JsonConvert.SerializeObject(
            integrationEventPayload,
            GetIntegrationEventPayloadSerializerSettings());

        return new IntegrationEventEnvelope
        {
            EventId = integrationEventId,
            EventType = integrationEventType,
            EventPayload = integrationEventPayloadJson,
        };
    }

    private static JsonSerializerSettings GetIntegrationEventPayloadSerializerSettings()
    {
        var jsonSerializerSettings = new JsonSerializerSettings();
        jsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        return jsonSerializerSettings;
    }
}