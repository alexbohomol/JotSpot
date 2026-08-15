namespace JotSpot.Api.Tests;

public class HealthCheckTests : JotSpotApiTest
{
    [Fact]
    public async Task GetHealth_ReturnsOk_Healthy()
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
