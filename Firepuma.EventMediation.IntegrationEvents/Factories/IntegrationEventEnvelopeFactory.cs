using Firepuma.EventMediation.IntegrationEvents.ValueObjects;
using Newtonsoft.Json;

namespace Firepuma.EventMediation.IntegrationEvents.Factories;

// ReSharper disable once UnusedType.Global
public static class IntegrationEventEnvelopeFactory
{
    // ReSharper disable once UnusedMember.Global
    public static IntegrationEventEnvelope CreateEnvelope<TEvent>(
        string integrationEventType,
        TEvent integrationEventPayload)
    {
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