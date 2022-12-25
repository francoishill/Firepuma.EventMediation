using Firepuma.CommandsAndQueries.Abstractions.Commands;
using Firepuma.EventMediation.IntegrationEvents.ValueObjects;
using MediatR;

namespace Firepuma.EventMediation.IntegrationEvents.CommandExecution.Abstractions;

public class CreateCommandsFromIntegrationEventRequest<TEvent> : IRequest<IEnumerable<ICommandRequest>>
{
    public string EventSourceId { get; init; }
    public IntegrationEventEnvelope EventEnvelope { get; init; }
    public TEvent EventPayload { get; init; }

    public CreateCommandsFromIntegrationEventRequest(string eventSourceId, IntegrationEventEnvelope eventEnvelope, TEvent eventPayload)
    {
        EventSourceId = eventSourceId;
        EventEnvelope = eventEnvelope;
        EventPayload = eventPayload;
    }
}