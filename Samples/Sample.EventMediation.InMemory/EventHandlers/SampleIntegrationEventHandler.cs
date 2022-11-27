using System.Text.Json;
using Firepuma.EventMediation.Abstractions.IntegrationEvents;
using Sample.EventMediation.InMemory.IntegrationEvents;

namespace Sample.EventMediation.InMemory.EventHandlers;

public class SampleIntegrationEventHandler : IIntegrationEventHandler<SampleIntegrationEvent>
{
    private readonly ILogger<SampleIntegrationEventHandler> _logger;

    public SampleIntegrationEventHandler(ILogger<SampleIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(SampleIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handler received an integration event with EventId: {EventId}, CreatedOn: {CreatedOn}, Data: {Data}, Time now: {Now}",
            integrationEvent.EventId, integrationEvent.CreatedOn.ToString("O"), JsonSerializer.Serialize(integrationEvent), DateTime.UtcNow.ToString("O"));

        await Task.CompletedTask;
    }
}