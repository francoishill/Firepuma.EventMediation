// ReSharper disable StringLiteralTypo

namespace Firepuma.EventMediation.IntegrationEvents.Factories;

public static class IntegrationEventIdFactory
{
    public static string GenerateIntegrationEventId(DateTime? dateTime = null)
    {
        var datePart = (dateTime ?? DateTime.UtcNow).ToString("yyyyMMdd-HHmmss-fffffff");
        return $"{datePart}-{Guid.NewGuid().ToString()}";
    }
}