using System.Text.Json;
using System.Text.Json.Serialization;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Firepuma.EventMediation.IntegrationEvents.ValueObjects;

public class IntegrationEventEnvelope
{
    public required string EventId { get; init; }
    public required string EventType { get; init; }
    public required string EventPayload { get; init; }

    public TIntegrationEvent? DeserializePayload<TIntegrationEvent>()
    {
        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
        };
        return JsonSerializer.Deserialize<TIntegrationEvent?>(EventPayload, deserializeOptions);
    }
}