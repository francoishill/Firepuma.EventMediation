using Firepuma.CommandsAndQueries.Abstractions.Entities;
using Firepuma.CommandsAndQueries.Abstractions.Services;
using Firepuma.EventMediation.IntegrationEvents.Abstractions;
using Firepuma.EventMediation.IntegrationEvents.CommandExecution.Abstractions;
using Firepuma.EventMediation.IntegrationEvents.CommandExecution.Extensions;
using Firepuma.EventMediation.IntegrationEvents.Constants;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable ArgumentsStyleNamedExpression

namespace Firepuma.EventMediation.IntegrationEvents.CommandExecution.Services;

internal class CommandExecutionIntegrationEventPublisher : ICommandExecutionIntegrationEventPublisher
{
    private readonly ILogger<CommandExecutionIntegrationEventPublisher> _logger;
    private readonly ICommandExecutionStorage _commandExecutionStorage;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly IIntegrationEventEnvelopeFactory _envelopeFactory;

    public CommandExecutionIntegrationEventPublisher(
        ILogger<CommandExecutionIntegrationEventPublisher> logger,
        ICommandExecutionStorage commandExecutionStorage,
        IIntegrationEventPublisher integrationEventPublisher,
        IIntegrationEventEnvelopeFactory envelopeFactory)
    {
        _logger = logger;
        _commandExecutionStorage = commandExecutionStorage;
        _integrationEventPublisher = integrationEventPublisher;
        _envelopeFactory = envelopeFactory;
    }

    public async Task PublishEventAsync(
        ICommandExecutionEvent executionEvent,
        bool ignoreExistingLock,
        CancellationToken cancellationToken)
    {
        if (executionEvent.Successful != true)
        {
            _logger.LogError(
                "This method should not be called for unsuccessful execution events (command execution document id {DocumentId}, command id {CommandId})",
                executionEvent.Id, executionEvent.CommandId);
            return;
        }

        if (!executionEvent.ExtraValues.TryGetValue(IntegrationEventExtraValuesKeys.IntegrationEventPayloadJson.ToString(), out var eventPayloadJsonObj)
            || eventPayloadJsonObj?.ToString() == null)
        {
            _logger.LogWarning(
                "Unable to extract {PayloadField} from command execution document id {DocumentId}, command id {CommandId}",
                IntegrationEventExtraValuesKeys.IntegrationEventPayloadJson, executionEvent.Id, executionEvent.CommandId);
            return;
        }

        if (!executionEvent.ExtraValues.TryGetValue(IntegrationEventExtraValuesKeys.IntegrationEventPayloadType.ToString(), out var eventPayloadType)
            || eventPayloadType?.ToString() == null)
        {
            _logger.LogWarning(
                "Unable to extract {PayloadTypeField} from command execution document id {DocumentId}, command id {CommandId}",
                IntegrationEventExtraValuesKeys.IntegrationEventPayloadType, executionEvent.Id, executionEvent.CommandId);
            return;
        }

        if (!ignoreExistingLock
            && executionEvent.ExtraValues.TryGetValue(IntegrationEventExtraValuesKeys.IntegrationEventLockUntilUnixSeconds.ToString(), out var previousLockUnixSecondsObj))
        {
            DateTimeOffset? lockExpiryDate = null;
            bool? isExpired = null;
            if (previousLockUnixSecondsObj is long previousLockUnixSeconds)
            {
                lockExpiryDate = DateTimeOffset.FromUnixTimeSeconds(previousLockUnixSeconds);
                isExpired = previousLockUnixSeconds < DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
            else
            {
                _logger.LogWarning(
                    "Unable to determine whether command execution lock expired, " +
                    "command document id {DocumentId}, command id {CommandId}, previous lock expiry {LockUnixSeconds} unix seconds",
                    executionEvent.Id, executionEvent.CommandId, previousLockUnixSecondsObj);
            }

            if (isExpired == false)
            {
                _logger.LogError(
                    "Command execution lock is not yet expired so aborting this operation, " +
                    "command document id {DocumentId}, command id {CommandId}, previous lock expiry {LockUnixSeconds} unix seconds ({LockExpiryDate})",
                    executionEvent.Id, executionEvent.CommandId, previousLockUnixSecondsObj, lockExpiryDate?.ToString("O"));
                return;
            }

            _logger.LogWarning(
                "Command execution lock expired and not removed, but will published again now, it was probably started by another thread/process that got aborted midway, " +
                "command document id {DocumentId}, command id {CommandId}, previous lock expiry {LockUnixSeconds} unix seconds ({LockExpiryDate})",
                executionEvent.Id, executionEvent.CommandId, previousLockUnixSecondsObj, lockExpiryDate?.ToString("O"));
        }

        try
        {
            // lock for a short while, to ensure we don't have duplicate processing
            executionEvent.SetBriefLock();
            await _commandExecutionStorage.UpsertItemAsync(executionEvent, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to obtain lock for publishing integration event for command execution document id {DocumentId}, command id {CommandId}",
                executionEvent.Id, executionEvent.CommandId);

            return;
        }

        try
        {
            var eventPayloadJson = eventPayloadJsonObj.ToString()!;
            var eventPayload = JsonConvert.DeserializeObject<JObject>(eventPayloadJson);
            if (eventPayload == null)
            {
                SetPublishResult(
                    executionEvent,
                    DateTime.UtcNow,
                    false,
                    new PublishError
                    {
                        Message = "Unable to deserialize event payload as JObject, its result was null",
                    });
            }
            else
            {
                var integrationEventId = eventPayload[nameof(IIntegrationEventProducingCommandResponse.IntegrationEventId)]?.Value<string>();
                if (!string.IsNullOrWhiteSpace(integrationEventId))
                {
                    var envelope = _envelopeFactory.CreateEnvelope(
                        eventId: integrationEventId,
                        eventType: eventPayloadType.ToString()!,
                        eventPayload: eventPayloadJson);
                    await _integrationEventPublisher.SendAsync(envelope, cancellationToken);

                    SetPublishResult(executionEvent, DateTime.UtcNow, true, null);
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unable to publish integration event, error message: {Error}",
                exception.Message);

            SetPublishResult(
                executionEvent,
                DateTime.UtcNow,
                false,
                new PublishError
                {
                    Message = exception.Message,
                    StackTrace = exception.StackTrace,
                });
        }

        executionEvent.ExtraValues.Remove(IntegrationEventExtraValuesKeys.IntegrationEventLockUntilUnixSeconds.ToString());
        await _commandExecutionStorage.UpsertItemAsync(executionEvent, cancellationToken);
    }

    private void SetPublishResult(
        ICommandExecutionEvent commandExecution,
        DateTime dateTime,
        bool isSuccessful,
        PublishError? error)
    {
        commandExecution.ExtraValues[IntegrationEventExtraValuesKeys.IntegrationEventPublishResultTime.ToString()] = dateTime;
        commandExecution.ExtraValues[IntegrationEventExtraValuesKeys.IntegrationEventPublishResultSuccess.ToString()] = isSuccessful;

        if (error != null)
        {
            _logger.LogError(
                "Error trying to publish integration event: {Error}, stack trace: {StackTrace}",
                error.Message, error.StackTrace);

            commandExecution.ExtraValues[IntegrationEventExtraValuesKeys.IntegrationEventPublishResultError.ToString()] = JsonConvert.SerializeObject(error, GetPublishResultSerializerSettings());
        }
    }

    private class PublishError
    {
        public required string Message { get; init; }
        public string? StackTrace { get; init; }
    }

    private static JsonSerializerSettings GetPublishResultSerializerSettings()
    {
        var jsonSerializerSettings = new JsonSerializerSettings();
        jsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        return jsonSerializerSettings;
    }
}