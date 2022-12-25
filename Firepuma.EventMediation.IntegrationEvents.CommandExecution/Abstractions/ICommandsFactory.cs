using Firepuma.CommandsAndQueries.Abstractions.Commands;
using MediatR;

// ReSharper disable UnusedType.Global

namespace Firepuma.EventMediation.IntegrationEvents.CommandExecution.Abstractions;

public interface ICommandsFactory<TEvent> : IRequestHandler<CreateCommandsFromIntegrationEventRequest<TEvent>, IEnumerable<ICommandRequest>>
{
}