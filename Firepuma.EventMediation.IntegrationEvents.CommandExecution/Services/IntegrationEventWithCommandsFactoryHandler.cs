using System.Diagnostics;
using Firepuma.CommandsAndQueries.Abstractions.Commands;
using Firepuma.EventMediation.IntegrationEvents.Abstractions;
using Firepuma.EventMediation.IntegrationEvents.CommandExecution.Abstractions;
using Firepuma.EventMediation.IntegrationEvents.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedType.Global
// ReSharper disable LogMessageIsSentenceProblem
// ReSharper disable RedundantExplicitArrayCreation

namespace Firepuma.EventMediation.IntegrationEvents.CommandExecution.Services;

public class IntegrationEventWithCommandsFactoryHandler : IIntegrationEventHandler
{
    private readonly ILogger<IntegrationEventWithCommandsFactoryHandler> _logger;
    private readonly IIntegrationEventDeserializer _integrationEventDeserializer;
    private readonly IMediator _mediator;

    public IntegrationEventWithCommandsFactoryHandler(
        ILogger<IntegrationEventWithCommandsFactoryHandler> logger,
        IIntegrationEventDeserializer integrationEventDeserializer,
        IMediator mediator)
    {
        _logger = logger;
        _integrationEventDeserializer = integrationEventDeserializer;
        _mediator = mediator;
    }

    public async Task<bool> TryHandleEvent(
        string eventSourceId,
        IntegrationEventEnvelope integrationEventEnvelope,
        CancellationToken cancellationToken)
    {
        if (!_integrationEventDeserializer.TryDeserializeIntegrationEvent(integrationEventEnvelope, out var eventPayload))
        {
            _logger.LogError(
                "Unable to deserialize integration event with type {Type} and id {Id}",
                integrationEventEnvelope.EventType, integrationEventEnvelope.EventId);
            return false;
        }

        var createCommandsRequest = CreateCommandsRequest(eventSourceId, integrationEventEnvelope, eventPayload);

        var commandCreationStopwatch = Stopwatch.StartNew();
        // this mediator Send will be handled by the appropriate ICommandsFactory<> implementation / handler
        var commands = (await _mediator.Send(createCommandsRequest, cancellationToken)).ToArray();
        commandCreationStopwatch.Stop();

        if (commands.Length == 0)
        {
            _logger.LogInformation(
                "No commands were produced for integration event type {EventType} id {IntegrationEventId}, creation " +
                "attempt took {DurationMs} ms. The reason for no commands could be because it returns 0 commands in specific " +
                "conditions, see previous logs messages for potential reason.",
                eventPayload.GetType().FullName, integrationEventEnvelope.EventId, commandCreationStopwatch.ElapsedMilliseconds);
            return true;
        }

        _logger.LogDebug(
            "Creation (not yet execution) of {Count} commands took {DurationMs} ms",
            commands.Length, commandCreationStopwatch.ElapsedMilliseconds);

        var successCount = 0;
        var errorCount = 0;

        var executionStopwatch = Stopwatch.StartNew();
        await Task.WhenAll(commands.Select(
            async command =>
            {
                try
                {
                    await _mediator.Send(command, cancellationToken);
                    Interlocked.Increment(ref successCount);
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "Failed to execute command {Type} with id {CommandId}, from integration event type {EventType} and id {IntegrationEventId}",
                        command.GetType().FullName, command.CommandId, eventPayload.GetType().FullName, integrationEventEnvelope.EventId);

                    Interlocked.Increment(ref errorCount);
                }
            }));
        executionStopwatch.Stop();

        if (successCount > 0)
        {
            _logger.LogInformation(
                "Successfully executed {Count}/{Total} commands in {Milliseconds} ms caused by integration event type {EventType} and id {IntegrationEventId}",
                successCount, commands.Length, executionStopwatch.ElapsedMilliseconds, eventPayload.GetType().FullName, integrationEventEnvelope.EventId);
        }

        return errorCount == 0;
    }

    private static IRequest<IEnumerable<ICommandRequest>> CreateCommandsRequest(
        string eventSourceId,
        IntegrationEventEnvelope eventEnvelope,
        object eventPayload)
    {
        var type = eventPayload.GetType();
        var createCommandsRequest = (IRequest<IEnumerable<ICommandRequest>>)
            (Activator.CreateInstance(
                 typeof(CreateCommandsFromIntegrationEventRequest<>).MakeGenericType(type),
                 args: new object[] { eventSourceId, eventEnvelope, eventPayload })
             ?? throw new InvalidOperationException($"Could not create wrapper type for {type}"));
        return createCommandsRequest;
    }
}