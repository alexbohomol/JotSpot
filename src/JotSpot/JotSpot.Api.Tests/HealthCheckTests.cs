namespace JotSpot.Api.Tests;

public class HealthCheckTests : IntegrationTest
{
    [Fact]
    public async Task GetRoot_Returns_HelloWorld()
    {
        // Act
        var msg = await SutClient.GetAsync("health");

        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.OK);
        msg.Content.Should().NotBeNull();
        var body = await msg.Content.ReadAsStringAsync();
        body.Should().Be("Healthy");
    }
}