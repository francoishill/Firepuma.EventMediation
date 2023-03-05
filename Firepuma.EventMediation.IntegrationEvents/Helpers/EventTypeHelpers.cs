using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Firepuma.EventMediation.IntegrationEvents.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Firepuma.EventMediation.IntegrationEvents.Helpers;

// ReSharper disable once UnusedType.Global
public static class EventTypeHelpers
{
    // ReSharper disable once UnusedMember.Global
    public static bool TryGetEventTypeFromAttribute<TAttribute>(
        object? messagePayload,
        Func<TAttribute, string> extractEventTypeFromAttribute,
        [NotNullWhen(true)] out string? eventType)
        where TAttribute : Attribute
    {
        var attributeValue = ((TAttribute?)messagePayload?
            .GetType()
            .GetCustomAttribute(typeof(TAttribute)));

        var integrationEventTypeFromAttribute = attributeValue != null ? extractEventTypeFromAttribute(attributeValue) : null;

        if (!string.IsNullOrWhiteSpace(integrationEventTypeFromAttribute))
        {
            eventType = integrationEventTypeFromAttribute;
            return true;
        }

        eventType = null;
        return false;
    }

    // ReSharper disable once UnusedMember.Global
    public static bool TryDeserializeIntegrationEventWithAttribute<TAttribute>(
        this IntegrationEventEnvelope envelope,
        ILogger logger,
        IEnumerable<Type> types,
        Func<TAttribute, string> extractEventTypeFromAttribute,
        [NotNullWhen(true)] out object? eventPayload)
        where TAttribute : Attribute
    {
        var stopwatch = Stopwatch.StartNew();
        var typesWithMyAttribute = GetTypesWithAttribute<TAttribute>(types)
            .Where(x => extractEventTypeFromAttribute(x.Attribute) == envelope.EventType)
            .ToList();

        stopwatch.Stop();
        logger.LogDebug("DURATION of finding attributes was {Milliseconds} ms", stopwatch.ElapsedMilliseconds);

        if (typesWithMyAttribute.Count > 1)
        {
            eventPayload = null;
            logger.LogError(
                "Unable to deserialize integration event because found more than 1 type matching '{Type}' attribute",
                envelope.EventType);
            return false;
        }

        var singleMatchingType = typesWithMyAttribute.SingleOrDefault();
        if (singleMatchingType != null)
        {
            var deserializePayloadMethod = typeof(IntegrationEventEnvelope).GetMethod(nameof(IntegrationEventEnvelope.DeserializePayload));
            var genericDeserializeMethod = deserializePayloadMethod?.MakeGenericMethod(singleMatchingType.Type);
            var deserializedObject = genericDeserializeMethod?.Invoke(envelope, null);

            if (deserializedObject != null)
            {
                eventPayload = deserializedObject;
                return true;
            }
        }

        eventPayload = null;
        return false;
    }

    // ReSharper disable once UnusedMember.Global
    public static IEnumerable<string> GetDuplicateIntegrationEventTypeAttributes<TAttribute>(
        IEnumerable<Type> types,
        Func<TAttribute, string> getIntegrationEventTypeFromAttribute)
        where TAttribute : Attribute
    {
        var typesWithAttribute = GetTypesWithAttribute<TAttribute>(types).ToList();
        return typesWithAttribute
            .GroupBy(x => getIntegrationEventTypeFromAttribute(x.Attribute))
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);
    }

    private static IEnumerable<TypeAndAttribute<TAttribute>> GetTypesWithAttribute<TAttribute>(
        IEnumerable<Type> types)
        where TAttribute : Attribute
    {
        return types
            .Select(type => new TypeAndAttribute<TAttribute>
            {
                Type = type,
                Attribute = type
                    .GetCustomAttributes(typeof(TAttribute), false)
                    .Cast<TAttribute>()
                    .SingleOrDefault()!,
            })
            .Where(x => x.Attribute != null!);
    }

    private class TypeAndAttribute<TAttribute>
        where TAttribute : Attribute
    {
        public required Type Type { get; init; }
        public required TAttribute Attribute { get; init; }
    }
}