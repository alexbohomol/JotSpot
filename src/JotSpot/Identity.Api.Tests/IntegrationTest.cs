using Microsoft.AspNetCore.Mvc.Testing;

namespace Identity.Api.Tests;

public class IntegrationTest : IClassFixture<WebApplicationFactory<IApiMarker>>
{
    protected readonly HttpClient SutClient;

    protected IntegrationTest()
    {
        var factory = new WebApplicationFactory<IApiMarker>();
        SutClient = factory.CreateClient();
    }
}
