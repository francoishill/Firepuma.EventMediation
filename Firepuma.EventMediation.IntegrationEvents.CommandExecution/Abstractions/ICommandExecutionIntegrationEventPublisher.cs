using Firepuma.CommandsAndQueries.Abstractions.Entities;

namespace Firepuma.EventMediation.IntegrationEvents.CommandExecution.Abstractions;

internal interface ICommandExecutionIntegrationEventPublisher
{
    Task PublishEventAsync(
        ICommandExecutionEvent executionEvent,
        bool ignoreExistingLock,
        CancellationToken cancellationToken);
}