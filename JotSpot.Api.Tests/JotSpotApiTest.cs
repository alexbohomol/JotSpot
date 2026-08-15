namespace JotSpot.Api.Tests;

using Microsoft.AspNetCore.Hosting;

public class JotSpotApiTest : IClassFixture<JotSpotApiApplicationFactory>
{
    protected HttpClient SutClient { get; }

    protected JotSpotApiTest()
    {
        var factory = new JotSpotApiApplicationFactory()
            .WithWebHostBuilder(InitWebHostBuilder);

        SutClient = factory.CreateClient();
    }

    protected virtual void InitWebHostBuilder(IWebHostBuilder builder) { }
}
