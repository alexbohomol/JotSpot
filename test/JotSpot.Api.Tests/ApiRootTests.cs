namespace JotSpot.Api.Tests;

public class ApiRootTests : JotSpotApiTest
{
    [Fact]
    public async Task GetRoot_Returns_HelloWorld()
    {
        var msg = await SutClient.GetAsync("");

        msg.StatusCode.Should().Be(HttpStatusCode.OK);
        msg.Content.Should().NotBeNull();

        var body = await msg.Content.ReadAsStringAsync();
        body.Should().Be("Hello World!");
    }
}
