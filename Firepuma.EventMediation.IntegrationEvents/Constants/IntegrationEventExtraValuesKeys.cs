namespace Firepuma.EventMediation.IntegrationEvents.Constants;

[Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum IntegrationEventExtraValuesKeys
{
    IntegrationEventPayloadJson,
    IntegrationEventPayloadType,
    IntegrationEventLockUntilUnixSeconds,
    IntegrationEventPublishResultTime,
    IntegrationEventPublishResultSuccess,
    IntegrationEventPublishResultError,
}