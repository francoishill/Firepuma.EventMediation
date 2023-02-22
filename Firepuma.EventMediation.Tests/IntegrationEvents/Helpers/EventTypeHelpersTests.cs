using Firepuma.EventMediation.IntegrationEvents.Helpers;
using Firepuma.EventMediation.IntegrationEvents.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Firepuma.EventMediation.Tests.IntegrationEvents.Helpers;

public class EventTypeHelpersTests
{
    [Fact]
    public void TryGetEventTypeFromAttribute_Successful_with_attribute()
    {
        // Arrange
        var messagePayload = new MockEventWithAttribute();

        string ExtractEventTypeFromAttribute(MockAttributeWithEventType attribute)
        {
            return attribute.EventType;
        }

        // Act
        var successful = EventTypeHelpers.TryGetEventTypeFromAttribute<MockAttributeWithEventType>(
            messagePayload,
            ExtractEventTypeFromAttribute,
            out var eventType);

        // Assert
        Assert.True(successful);
        Assert.Equal("My/Event/Number1", eventType);
    }

    [Fact]
    public void TryGetEventTypeFromAttribute_Fail_when_no_attribute()
    {
        // Arrange
        var messagePayload = new MockEventWithoutAttribute();

        string ExtractEventTypeFromAttribute(MockAttributeWithEventType attribute)
        {
            return attribute.EventType;
        }

        // Act
        var successful = EventTypeHelpers.TryGetEventTypeFromAttribute<MockAttributeWithEventType>(
            messagePayload,
            ExtractEventTypeFromAttribute,
            out var eventType);

        // Assert
        Assert.False(successful);
        Assert.Null(eventType);
    }

    [Fact]
    public void TryDeserializeIntegrationEventWithAttribute_Successful()
    {
        // Arrange
        var envelope = new IntegrationEventEnvelope
        {
            EventId = "",
            EventType = "My/Event/Number1",
            EventPayload = "{}",
        };

        string ExtractEventTypeFromAttribute(MockAttributeWithEventType attribute)
        {
            return attribute.EventType;
        }

        // Act
        var successful = envelope.TryDeserializeIntegrationEventWithAttribute<MockAttributeWithEventType>(
            Substitute.For<ILogger>(),
            AppDomain.CurrentDomain.GetAssemblies(),
            ExtractEventTypeFromAttribute,
            out var eventPayload);

        // Assert
        Assert.True(successful);
        var mockEventPayload = Assert
            .IsType<MockEventWithAttribute>(eventPayload);
        Assert.NotNull(mockEventPayload);
    }

    [Fact]
    public void TryDeserializeIntegrationEventWithAttribute_Fail_when_invalid_JSON_in_payload()
    {
        // Arrange
        var envelope = new IntegrationEventEnvelope
        {
            EventId = "",
            EventType = "My/Event/Number1",
            EventPayload = "",
        };

        string ExtractEventTypeFromAttribute(MockAttributeWithEventType attribute)
        {
            return attribute.EventType;
        }

        // Act
        // Assert
        Assert.ThrowsAny<Exception>(() =>
        {
            envelope.TryDeserializeIntegrationEventWithAttribute<MockAttributeWithEventType>(
                Substitute.For<ILogger>(),
                AppDomain.CurrentDomain.GetAssemblies(),
                ExtractEventTypeFromAttribute,
                out _);
        });
    }

    [Fact]
    public void TryDeserializeIntegrationEventWithAttribute_Fail_when_event_type_not_found()
    {
        // Arrange
        var envelope = new IntegrationEventEnvelope
        {
            EventId = "",
            EventType = "My/Event/Missing1",
            EventPayload = "",
        };

        string ExtractEventTypeFromAttribute(MockAttributeWithEventType attribute)
        {
            return attribute.EventType;
        }

        // Act
        var successful = envelope.TryDeserializeIntegrationEventWithAttribute<MockAttributeWithEventType>(
            Substitute.For<ILogger>(),
            AppDomain.CurrentDomain.GetAssemblies(),
            ExtractEventTypeFromAttribute,
            out var eventPayload);

        // Assert
        Assert.False(successful);
        Assert.Null(eventPayload);
    }

    [MockAttributeWithEventType("My/Event/Number1")]
    private class MockEventWithAttribute
    {
    }

    private class MockEventWithoutAttribute
    {
    }

    private class MockAttributeWithEventType : Attribute
    {
        public string EventType { get; init; }

        public MockAttributeWithEventType(string eventType)
        {
            EventType = eventType;
        }
    }
}