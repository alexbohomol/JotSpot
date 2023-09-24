using Bogus;
using JotSpot.Api.Handlers;
using JotSpot.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using JotSpotApi = JotSpot.Api.IApiMarker;
using IdentityApi = Identity.Api.IApiMarker;

namespace JotSpot.Api.Tests;

public class JotsEndpointsTests : 
    IClassFixture<WebApplicationFactory<JotSpotApi>>,
    IClassFixture<WebApplicationFactory<IdentityApi>>
{
    private const string JotsApiUrl = "jots";
    private readonly HttpClient _sutClient;
    private readonly HttpClient _idClient;

    public JotsEndpointsTests()
    {
        var jotSpotApiFactory = new WebApplicationFactory<IApiMarker>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IRepository, RepositoryMock>();
                });
            });
        
        _sutClient = jotSpotApiFactory.CreateClient();

        var identityApiFactory = new WebApplicationFactory<Identity.Api.IApiMarker>();
        _idClient = identityApiFactory.CreateClient();
    }
    
    [Fact]
    public async Task GetJots_ReturnsUnauthorized_Unauthenticated()
    {
        // Arrange
        // we do not authenticate here
        
        // Act
        var msg = await _sutClient.GetAsync(JotsApiUrl);

        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        msg.Content.Should().NotBeNull();
        //TODO: AssertUnauthorizedProblemDetails
    }
    
    [Fact]
    public async Task GetJots_ReturnsUnauthorized_InvalidCredentials()
    {
        // Arrange
        await RequestAuthorizationFor(TokenRequests.InvalidCredentials);

        // Act
        var msg = await _sutClient.GetAsync(JotsApiUrl);

        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        msg.Content.Should().NotBeNull();
        //TODO: AssertUnauthorizedProblemDetails
    }
    
    [Fact]
    public async Task GetJots_ReturnsOk_EmptyList()
    {
        // Arrange
        await RequestAuthorizationFor(TokenRequests.ValidCredentials);

        // Act
        var msg = await _sutClient.GetAsync(JotsApiUrl);

        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.OK);
        msg.Content.Should().NotBeNull();
        
        var jots = await msg.Content.ReadFromJsonAsync<JotResponse[]>();
        jots.Should().BeEmpty();
    }

    [Fact]
    public async Task GetJots_ReturnsOk_ListOfJots()
    {
        // Arrange
        await RequestAuthorizationFor(TokenRequests.ValidCredentials);
        var (_, jot1) = await CreateJotAsync(JotRequests.New);
        var (_, jot2) = await CreateJotAsync(JotRequests.New);
        var (_, jot3) = await CreateJotAsync(JotRequests.New);
        
        // Act
        var msg = await _sutClient.GetAsync(JotsApiUrl);

        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.OK);
        msg.Content.Should().NotBeNull();
        
        var jots = await msg.Content.ReadFromJsonAsync<JotResponse?[]>();
        jots.Should().HaveCount(3);
        jots.Should().Contain(new[] { jot1, jot2, jot3 });
    }

    [Fact]
    public async Task PostJot_ReturnsUnauthorized_Unauthenticated()
    {
        // Arrange
        // we do not authenticate here

        // Act
        var (msg, _) = await CreateJotAsync(JotRequests.New);
        
        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        msg.Content.Should().NotBeNull();
        //TODO: AssertUnauthorizedProblemDetails
    }

    [Fact]
    public async Task PostJot_ReturnsUnauthorized_InvalidCredentials()
    {
        // Arrange
        await RequestAuthorizationFor(TokenRequests.InvalidCredentials);

        // Act
        var (msg, _) = await CreateJotAsync(JotRequests.New);
        
        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        msg.Content.Should().NotBeNull();
        //TODO: AssertUnauthorizedProblemDetails
    }

    [Fact]
    public async Task PostJot_ReturnsCreated()
    {
        // Arrange
        await RequestAuthorizationFor(TokenRequests.ValidCredentials);
        var request = JotRequests.New;

        // Act
        var (msg, created) = await CreateJotAsync(request);
        
        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.Created);
        msg.Content.Should().NotBeNull();
        
        msg.Headers.Location.Should().NotBeNull();
        msg.Headers.Location.Should().BeOfType<Uri>();
        msg.Headers.Location!.OriginalString.Should().Contain(created!.Id.ToString());
        
        created.Should().NotBeNull();
        created.Id.Should().NotBeEmpty();
        created.Should().BeEquivalentTo(request);
    }
    
    [Fact]
    public async Task GetJot_ReturnsUnauthorized_Unauthenticated()
    {
        // Arrange
        // we do not authenticate here

        // Act
        var (msg, _) = await GetJotByIdAsync(Guid.Empty);
        
        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        msg.Content.Should().NotBeNull();
        //TODO: AssertUnauthorizedProblemDetails
    }
    
    [Fact]
    public async Task GetJot_ReturnsUnauthorized_InvalidCredentials()
    {
        // Arrange
        await RequestAuthorizationFor(TokenRequests.InvalidCredentials);

        // Act
        var (msg, _) = await GetJotByIdAsync(Guid.Empty);
        
        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        msg.Content.Should().NotBeNull();
        //TODO: AssertUnauthorizedProblemDetails
    }
    
    [Fact]
    public async Task GetJot_ReturnsAvailableJot()
    {
        // Arrange
        await RequestAuthorizationFor(TokenRequests.ValidCredentials);
        
        var request = JotRequests.New;
        var (_, created) = await CreateJotAsync(request);

        // Act
        var (msg, jot) = await GetJotByIdAsync(created!.Id);
        
        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.OK);
        msg.Content.Should().NotBeNull();
        
        jot.Should().NotBeNull();
        jot.Should().BeEquivalentTo(request);
        jot.Id.Should().Be(created.Id);
    }
    
    [Fact]
    public async Task GetJot_ReturnsNotFound()
    {
        // Arrange
        await RequestAuthorizationFor(TokenRequests.ValidCredentials);
        
        // Act
        var (msg, jot) = await GetJotByIdAsync(Guid.Empty);
        
        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.NotFound);
        msg.Content.Should().NotBeNull();
        jot.Should().BeNull();
    }
    
    [Fact]
    public async Task DeleteJot_ReturnsUnauthorized_Unauthenticated()
    {
        // Arrange
        // we do not authenticate here
        
        // Act
        var msg = await _sutClient.DeleteAsync($"{JotsApiUrl}/{Guid.Empty}");
        
        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        msg.Content.Should().NotBeNull();
        //TODO: AssertUnauthorizedProblemDetails
    }
    
    [Fact]
    public async Task DeleteJot_ReturnsUnauthorized_InvalidCredentials()
    {
        // Arrange
        await RequestAuthorizationFor(TokenRequests.InvalidCredentials);
        
        // Act
        var msg = await _sutClient.DeleteAsync($"{JotsApiUrl}/{Guid.Empty}");
        
        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        msg.Content.Should().NotBeNull();
        //TODO: AssertUnauthorizedProblemDetails
    }
    
    [Fact]
    public async Task DeleteJot_DeletesAvailableJot()
    {
        // Arrange
        await RequestAuthorizationFor(TokenRequests.ValidCredentials);

        var (pstMsg, created) = await CreateJotAsync(JotRequests.New);
        pstMsg.StatusCode.Should().Be(HttpStatusCode.Created);

        var (getMsg, _) = await GetJotByIdAsync(created!.Id);
        getMsg.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Act
        var delMsg = await _sutClient.DeleteAsync($"{JotsApiUrl}/{created.Id}");
        
        // Assert
        delMsg.StatusCode.Should().Be(HttpStatusCode.NoContent);
        delMsg.Content.Should().NotBeNull();
        var body = await delMsg.Content.ReadAsStringAsync();
        body.Should().BeEmpty();
        
        var chkMsg = await _sutClient.GetAsync($"{JotsApiUrl}/{created.Id}");
        chkMsg.StatusCode.Should().Be(HttpStatusCode.NotFound);
        chkMsg.Content.Should().NotBeNull();
    }
    
    [Fact]
    public async Task DeleteJot_ReturnsNotFound()
    {
        // Arrange
        await RequestAuthorizationFor(TokenRequests.ValidCredentials);

        // Act
        var msg = await _sutClient.DeleteAsync($"{JotsApiUrl}/{Guid.Empty}");
        
        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.NotFound);
        msg.Content.Should().NotBeNull();
        var body = await msg.Content.ReadAsStringAsync();
        body.Should().BeEmpty();
    }
    
    [Fact]
    public async Task PutJot_ReturnsUnauthorized_Unauthenticated()
    {
        // Arrange
        // we do not authenticate here
        
        // Act
        var msg = await _sutClient.PutAsync(
            $"{JotsApiUrl}/{Guid.Empty}", 
            JsonContent.Create(JotRequests.New));
        
        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        msg.Content.Should().NotBeNull();
        //TODO: AssertUnauthorizedProblemDetails
    }
    
    [Fact]
    public async Task PutJot_ReturnsUnauthorized_InvalidCredentials()
    {
        // Arrange
        await RequestAuthorizationFor(TokenRequests.InvalidCredentials);
        
        // Act
        var msg = await _sutClient.PutAsync(
            $"{JotsApiUrl}/{Guid.Empty}", 
            JsonContent.Create(JotRequests.New));
        
        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        msg.Content.Should().NotBeNull();
        //TODO: AssertUnauthorizedProblemDetails
    }
    
    [Fact]
    public async Task PutJot_UpdatesAvailableJot()
    {
        // Arrange
        await RequestAuthorizationFor(TokenRequests.ValidCredentials);

        var (pstMsg, created) = await CreateJotAsync(JotRequests.New);
        pstMsg.StatusCode.Should().Be(HttpStatusCode.Created);

        var (getMsg, _) = await GetJotByIdAsync(created!.Id);
        getMsg.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var update = JotRequests.New;
        
        // Act
        var putMsg = await _sutClient.PutAsync($"{JotsApiUrl}/{created.Id}", JsonContent.Create(update));
        
        // Assert
        putMsg.StatusCode.Should().Be(HttpStatusCode.OK);
        putMsg.Content.Should().NotBeNull();
        
        var updated = await putMsg.Content.ReadFromJsonAsync<JotResponse>();
        updated.Should().NotBeNull();
        updated.Should().BeEquivalentTo(update);
        updated!.Id.Should().NotBeEmpty();

        var (chkMsg, retrieved) = await GetJotByIdAsync(updated.Id);
        chkMsg.StatusCode.Should().Be(HttpStatusCode.OK);
        retrieved.Should().NotBeNull();
        retrieved.Should().BeEquivalentTo(updated);
    }
    
    [Fact]
    public async Task PutJot_ReturnsNotFound()
    {
        // Assert
        await RequestAuthorizationFor(TokenRequests.ValidCredentials);
        
        var request = JotRequests.New;
        
        // Act
        var msg = await _sutClient.PutAsync($"{JotsApiUrl}/{Guid.Empty}", JsonContent.Create(request));
        
        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.NotFound);
        msg.Content.Should().NotBeNull();
        var body = await msg.Content.ReadAsStringAsync();
        body.Should().BeEmpty();
    }

    private async Task<(HttpResponseMessage, JotResponse?)> CreateJotAsync(JotRequest request)
    {
        var message = await _sutClient.PostAsJsonAsync(JotsApiUrl, request);
        
        var created = message.IsSuccessStatusCode
            ? await message.Content.ReadFromJsonAsync<JotResponse>()
            : null;

        return (message, created);
    }

    private async Task<(HttpResponseMessage, JotResponse)> GetJotByIdAsync(Guid id)
    {
        var message = await _sutClient.GetAsync($"{JotsApiUrl}/{id}");

        var jot = message.IsSuccessStatusCode 
            ? await message.Content.ReadFromJsonAsync<JotResponse>()
            : null;
        
        return (message, jot!);
    }

    private async Task RequestAuthorizationFor(TokenRequest tokenRequest)
    {
        var tokenMsg = await _idClient.PostAsJsonAsync("api/auth/token", tokenRequest);
        if (tokenMsg.IsSuccessStatusCode)
        {
            var tokenString = await tokenMsg.Content.ReadAsStringAsync();
            _sutClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenString}");
        }
    }
    
    private class RepositoryMock : IRepository
    {
        private readonly List<Jot> _store = new();

        public Jot[] GetAll() => _store.ToArray();

        public void Add(Jot jot) => _store.Add(jot);

        public Jot? GetById(Guid id) => _store.FirstOrDefault(x => x.Id == id);

        public bool Delete(Guid id)
        {
            var jot = GetById(id);
            
            return jot is not null && _store.Remove(jot);
        }
    }
}

internal static class TokenRequests
{
    public static TokenRequest ValidCredentials => new("alex@jotspot.com", "1234567");
    public static TokenRequest InvalidCredentials => new("blah-blah", "blah-blah");
}

internal static class JotRequests
{
    private static readonly Faker<JotRequest> Faker = new Faker<JotRequest>()
        .CustomInstantiator(f => 
            new JotRequest(
                f.Random.Words(5),
                f.Random.Words(10)));
    
    public static JotRequest New => Faker.Generate();
}