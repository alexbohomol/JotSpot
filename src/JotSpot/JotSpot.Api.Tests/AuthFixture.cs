using Microsoft.AspNetCore.Mvc.Testing;

namespace JotSpot.Api.Tests;

public class AuthFixture : IAsyncLifetime
{
    private string? _token401;
    private string? _token200;
    
    public void ResetAuthentication(HttpClient client) => client
        .DefaultRequestHeaders
        .Remove("Authorization");

    public void Authorise(HttpClient client) => client
        .DefaultRequestHeaders
        .Add("Authorization", $"Bearer {_token200}");

    public void Unauthorise(HttpClient client) => client
        .DefaultRequestHeaders
        .Add("Authorization", $"Bearer {_token401}");

    #region IAsyncLifetime

    public async Task InitializeAsync()
    {
        await using var identityApiFactory = new WebApplicationFactory<Identity.Api.IApiMarker>();
        using var identityApiClient = identityApiFactory.CreateClient();
        
        var resp401 = await identityApiClient.PostAsJsonAsync(
            "api/auth/token", 
            new TokenRequest("blah-blah", "blah-blah"));
        
        _token401 = await resp401.Content.ReadAsStringAsync();
        
        var resp200 = await identityApiClient.PostAsJsonAsync(
            "api/auth/token", 
            new TokenRequest("alex", "1234567"));
        
        _token200 = await resp200.Content.ReadAsStringAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    #endregion
}