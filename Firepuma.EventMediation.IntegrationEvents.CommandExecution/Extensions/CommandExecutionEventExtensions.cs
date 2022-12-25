using Firepuma.CommandsAndQueries.Abstractions.Entities;
using Firepuma.EventMediation.IntegrationEvents.Constants;

namespace Firepuma.EventMediation.IntegrationEvents.CommandExecution.Extensions;

public static class CommandExecutionEventExtensions
{
    public static void SetBriefLock(this ICommandExecutionEvent commandExecution)
    {
        var soonUnixSeconds = DateTimeOffset.UtcNow.AddMinutes(2).ToUnixTimeSeconds();
        commandExecution.ExtraValues[IntegrationEventExtraValuesKeys.IntegrationEventLockUntilUnixSeconds.ToString()] = soonUnixSeconds;
    }
}