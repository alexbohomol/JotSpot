using Microsoft.AspNetCore.Mvc.Testing;

namespace JotSpot.Api.Tests;

public class IntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly HttpClient SutClient;

    protected IntegrationTest()
    {
        var factory = new WebApplicationFactory<Program>();
        SutClient = factory.CreateClient();
    }
}