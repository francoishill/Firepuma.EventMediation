namespace Firepuma.EventMediation.IntegrationEvents.CommandExecution.Abstractions;

public interface IIntegrationEventProducingCommandResponse
{
    string IntegrationEventId { get; set; }
}