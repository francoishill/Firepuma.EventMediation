using Firepuma.CommandsAndQueries.Abstractions.Commands;
using Firepuma.EventMediation.IntegrationEvents.CommandExecution.Abstractions;
using Firepuma.EventMediation.IntegrationEvents.CommandExecution.Services;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Firepuma.EventMediation.IntegrationEvents.CommandExecution;

public static class ServiceCollectionExtensions
{
    public static void AddIntegrationEventsForCommandExecution(
        this IServiceCollection services)
    {
        services.AddTransient<ICommandExecutionDecorator, CommandExecutionIntegrationEventDecorator>();
        services.AddTransient<ICommandExecutionPostProcessor, CommandExecutionPostProcessor>();
        services.AddTransient<ICommandExecutionIntegrationEventPublisher, CommandExecutionIntegrationEventPublisher>();
    }
}