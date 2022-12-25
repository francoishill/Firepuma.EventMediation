using Firepuma.EventMediation.IntegrationEvents.Factories;

namespace Firepuma.EventMediation.Tests.IntegrationEvents.Factories;

public class IntegrationEventIdFactoryTests
{
    [Theory]
    [MemberData(nameof(GenerateIntegrationEventIdCasesMemberData))]
    public void GenerateIntegrationEventId_ExpectedBehavior(DateTime inputDate, string expectedPrefixOfEventId)
    {
        // Arrange

        // Act
        var eventId = IntegrationEventIdFactory.GenerateIntegrationEventId(inputDate);

        // Assert
        Assert.StartsWith(expectedPrefixOfEventId, eventId);
    }

    public static IEnumerable<object[]> GenerateIntegrationEventIdCasesMemberData => new[]
    {
        new object[] { new DateTime(1970, 1, 2, 3, 4, 5, 60), "19700102-030405-0600000-" },
        new object[] { new DateTime(2022, 2, 28, 23, 58, 59, 123), "20220228-235859-1230000-" },
    };
}