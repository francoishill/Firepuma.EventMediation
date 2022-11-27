using Firepuma.EventMediation.Abstractions.IntegrationEvents;
using Microsoft.AspNetCore.Mvc;
using Sample.EventMediation.InMemory.IntegrationEvents;

namespace Sample.EventMediation.InMemory.Controllers;

[ApiController]
[Route("[controller]")]
public class EventPublisherController : ControllerBase
{
    private readonly IIntegrationEventMediator _eventMediator;

    public EventPublisherController(IIntegrationEventMediator eventMediator)
    {
        _eventMediator = eventMediator;
    }

    [HttpPost]
    public async Task<IActionResult> PublishEvent(CancellationToken cancellationToken)
    {
        var newEvent = new SampleIntegrationEvent
        {
            Title = $"This is just a random title generated on {DateTime.UtcNow:O}",
        };

        await Task.Delay(TimeSpan.FromMilliseconds(350), cancellationToken); // simulate a bit of latency, to create a time gap between event CreatedOn and received time

        await _eventMediator.HandleAsync(newEvent, cancellationToken);

        return Ok(new
        {
            QueuedIntegrationEventId = newEvent.EventId,
        });
    }
}