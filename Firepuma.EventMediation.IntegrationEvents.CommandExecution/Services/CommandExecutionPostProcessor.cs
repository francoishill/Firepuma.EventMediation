using Firepuma.CommandsAndQueries.Abstractions.Commands;
using Firepuma.CommandsAndQueries.Abstractions.Entities;
using Firepuma.EventMediation.IntegrationEvents.CommandExecution.Abstractions;
using Firepuma.EventMediation.IntegrationEvents.Constants;

namespace Firepuma.EventMediation.IntegrationEvents.CommandExecution.Services;

internal class CommandExecutionPostProcessor : ICommandExecutionPostProcessor
{
    private readonly ICommandExecutionIntegrationEventPublisher _integrationEventPublisher;

    public CommandExecutionPostProcessor(
        ICommandExecutionIntegrationEventPublisher integrationEventPublisher)
    {
        _integrationEventPublisher = integrationEventPublisher;
    }

    public async Task ProcessAsync<TResponse>(
        ICommandExecutionEvent executionEvent,
        ICommandRequest command,
        bool successful,
        TResponse? response,
        Exception? error,
        CancellationToken cancellationToken)
    {
        if (!successful
            || !executionEvent.ExtraValues.ContainsKey(IntegrationEventExtraValuesKeys.IntegrationEventPayloadJson.ToString()))
        {
            return;
        }

        // we set the lock for ourself as part of the decorator, so ignore it here since another
        // background process should not have started processing it
        const bool ignoreExistingLock = true;

        await _integrationEventPublisher.PublishEventAsync(executionEvent, ignoreExistingLock, cancellationToken);
    }
}