namespace Sample.EventMediation.InMemory.IntegrationEvents;

public class SampleIntegrationEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string Title { get; set; } = null!;
}