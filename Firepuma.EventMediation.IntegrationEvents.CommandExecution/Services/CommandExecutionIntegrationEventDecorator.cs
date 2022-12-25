using Firepuma.CommandsAndQueries.Abstractions.Commands;
using Firepuma.CommandsAndQueries.Abstractions.Entities;
using Firepuma.EventMediation.IntegrationEvents.Abstractions;
using Firepuma.EventMediation.IntegrationEvents.CommandExecution.Abstractions;
using Firepuma.EventMediation.IntegrationEvents.CommandExecution.Extensions;
using Firepuma.EventMediation.IntegrationEvents.Constants;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Firepuma.EventMediation.IntegrationEvents.CommandExecution.Services;

internal class CommandExecutionIntegrationEventDecorator : ICommandExecutionDecorator
{
    private readonly ILogger<CommandExecutionIntegrationEventDecorator> _logger;
    private readonly IIntegrationEventTypeProvider _integrationEventTypeProvider;

    public CommandExecutionIntegrationEventDecorator(
        ILogger<CommandExecutionIntegrationEventDecorator> logger,
        IIntegrationEventTypeProvider integrationEventTypeProvider)
    {
        _logger = logger;
        _integrationEventTypeProvider = integrationEventTypeProvider;
    }

    public void Decorate<TResponse>(
        ICommandExecutionEvent executionEvent,
        ICommandRequest command,
        bool successful,
        TResponse? response,
        Exception? error)
    {
        if (!successful)
        {
            _logger.LogInformation(
                "Command execution was not successful for command id {CommandId}, integration event result will be ignored, execution exception message: {Message}",
                command.CommandId, error?.Message);
            return;
        }

        if (response is not IIntegrationEventProducingCommandResponse integrationEventPayload)
        {
            _logger.LogDebug(
                "Command execution response is not of type {ExpectedType} (but is rather {ActualType}) for command id {CommandId}, integration event result will be ignored",
                nameof(IIntegrationEventProducingCommandResponse), response?.GetType().FullName, command.CommandId);
            return;
        }

        if (!_integrationEventTypeProvider.TryGetIntegrationEventType(integrationEventPayload, out var integrationEventType))
        {
            _logger.LogError(
                "Unable to get integration event for type {Type}, command type {CommandType}, command id {CommandId}",
                integrationEventPayload.GetType().FullName, command.GetType().FullName, command.CommandId);
            return;
        }

        // it is necessary to store as JSON otherwise will fail to deserialize the objects with mongo type discriminators
        var integrationEventPayloadJson = JsonConvert.SerializeObject(
            integrationEventPayload,
            GetIntegrationEventPayloadSerializerSettings());

        // lock for a short while now, it will get picked up by the CommandExecutionIntegrationEventPublisher
        executionEvent.SetBriefLock();
        executionEvent.ExtraValues[IntegrationEventExtraValuesKeys.IntegrationEventPayloadJson.ToString()] = integrationEventPayloadJson;
        executionEvent.ExtraValues[IntegrationEventExtraValuesKeys.IntegrationEventPayloadType.ToString()] = integrationEventType;
    }

    private static JsonSerializerSettings GetIntegrationEventPayloadSerializerSettings()
    {
        var jsonSerializerSettings = new JsonSerializerSettings();
        jsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        return jsonSerializerSettings;
    }
}