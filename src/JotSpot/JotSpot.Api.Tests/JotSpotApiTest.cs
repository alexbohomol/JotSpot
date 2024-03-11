namespace JotSpot.Api.Tests;

public class JotSpotApiTest : IClassFixture<JotSpotApiApplicationFactory>
{
    protected readonly HttpClient SutClient;

    protected JotSpotApiTest()
    {
        var factory = new JotSpotApiApplicationFactory()
            .WithWebHostBuilder(InitWebHostBuilder);
        
        SutClient = factory.CreateClient();
    }

    protected virtual void InitWebHostBuilder(IWebHostBuilder builder) { }
}