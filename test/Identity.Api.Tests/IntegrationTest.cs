using Microsoft.AspNetCore.Mvc.Testing;

namespace Identity.Api.Tests;

public class IntegrationTest : IClassFixture<WebApplicationFactory<IApiMarker>>
{
    protected HttpClient SutClient { get; }

    protected IntegrationTest()
    {
        var factory = new WebApplicationFactory<IApiMarker>();
        SutClient = factory.CreateClient();
    }
}
