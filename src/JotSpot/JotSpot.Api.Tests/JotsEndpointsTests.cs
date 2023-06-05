using JotSpot.Api.Endpoints;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace JotSpot.Api.Tests;

public class JotsEndpointsTests : IClassFixture<WebApplicationFactory<IApiMarker>>
{
    private readonly HttpClient _sutClient;
    
    public JotsEndpointsTests()
    {
        var factory = new WebApplicationFactory<IApiMarker>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<Jots.IRepository, RepositoryMock>();
                });
            });
        
        _sutClient = factory.CreateClient();
    }
    
    [Fact]
    public async Task GetJots_ReturnsOk_EmptyList()
    {
        // Act
        var msg = await _sutClient.GetAsync("jots");

        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.OK);
        msg.Content.Should().NotBeNull();
        
        var jots = await msg.Content.ReadFromJsonAsync<Jots.Response[]>();
        jots.Should().BeEmpty();
    }

    [Fact]
    public async Task GetJots_ReturnsOk_ListOfJots()
    {
        // Arrange
        var (_, jot1) = await CreateJotAsync(new Jots.Request("title #1", "text #1"));
        var (_, jot2) = await CreateJotAsync(new Jots.Request("title #2", "text #2"));
        var (_, jot3) = await CreateJotAsync(new Jots.Request("title #3", "text #3"));
        
        // Act
        var msg = await _sutClient.GetAsync("jots");

        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.OK);
        msg.Content.Should().NotBeNull();
        
        var jots = await msg.Content.ReadFromJsonAsync<Jots.Response[]>();
        jots.Should().HaveCount(3);
        jots.Should().Contain(new[] { jot1, jot2, jot3 });
    }

    [Fact]
    public async Task PostJot_ReturnsCreated()
    {
        // Arrange
        var request = new Jots.Request("test title", "test text");

        // Act
        var (msg, created) = await CreateJotAsync(request);
        
        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.Created);
        msg.Content.Should().NotBeNull();
        
        msg.Headers.Location.Should().NotBeNull();
        msg.Headers.Location.Should().BeOfType<Uri>();
        msg.Headers.Location!.OriginalString.Should().Contain(created.Id.ToString());
        
        created.Should().NotBeNull();
        created.Id.Should().NotBeEmpty();
        created.Should().BeEquivalentTo(request);
    }
    
    [Fact]
    public async Task GetJot_ReturnsAvailableJot()
    {
        // Arrange
        var request = new Jots.Request("test title", "test text");
        var (_, created) = await CreateJotAsync(request);

        // Act
        var (msg, jot) = await GetJotByIdAsync(created.Id);
        
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
        // Act
        var (msg, jot) = await GetJotByIdAsync(Guid.Empty);
        
        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.NotFound);
        msg.Content.Should().NotBeNull();
        jot.Should().BeNull();
    }
    
    [Fact]
    public async Task DeleteJot_DeletesAvailableJot()
    {
        // Arrange
        var request = new Jots.Request("test title", "test text");
        var (_, created) = await CreateJotAsync(request);

        var (getMsg, _) = await GetJotByIdAsync(created.Id);
        getMsg.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Act
        var delMsg = await _sutClient.DeleteAsync($"jots/{created.Id}");
        
        // Assert
        delMsg.StatusCode.Should().Be(HttpStatusCode.NoContent);
        delMsg.Content.Should().NotBeNull();
        var body = await delMsg.Content.ReadAsStringAsync();
        body.Should().BeEmpty();
        
        var chkMsg = await _sutClient.GetAsync($"jots/{created.Id}");
        chkMsg.StatusCode.Should().Be(HttpStatusCode.NotFound);
        chkMsg.Content.Should().NotBeNull();
    }
    
    [Fact]
    public async Task DeleteJot_ReturnsNotFound()
    {
        // Act
        var msg = await _sutClient.DeleteAsync($"jots/{Guid.Empty}");
        
        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.NotFound);
        msg.Content.Should().NotBeNull();
        var body = await msg.Content.ReadAsStringAsync();
        body.Should().BeEmpty();
    }
    
    [Fact]
    public async Task PutJot_UpdatesAvailableJot()
    {
        // Arrange
        var initial = new Jots.Request("initial title", "initial text");
        
        var (pstMsg, created) = await CreateJotAsync(initial);
        pstMsg.StatusCode.Should().Be(HttpStatusCode.Created);

        var (getMsg, _) = await GetJotByIdAsync(created.Id);
        getMsg.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var update = new Jots.Request("updated title", "updated text");
        
        // Act
        var putMsg = await _sutClient.PutAsync($"jots/{created.Id}", JsonContent.Create(update));
        
        // Assert
        putMsg.StatusCode.Should().Be(HttpStatusCode.OK);
        putMsg.Content.Should().NotBeNull();
        
        var updated = await putMsg.Content.ReadFromJsonAsync<Jots.Response>();
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
        var request = new Jots.Request("initial title", "initial text");
        
        // Act
        var msg = await _sutClient.PutAsync($"jots/{Guid.Empty}", JsonContent.Create(request));
        
        // Assert
        msg.StatusCode.Should().Be(HttpStatusCode.NotFound);
        msg.Content.Should().NotBeNull();
        var body = await msg.Content.ReadAsStringAsync();
        body.Should().BeEmpty();
    }

    private async Task<(HttpResponseMessage, Jots.Response)> CreateJotAsync(Jots.Request request)
    {
        var message = await _sutClient.PostAsJsonAsync("jots", request);
        
        var created = await message.Content.ReadFromJsonAsync<Jots.Response>();

        return (message, created!);
    }

    private async Task<(HttpResponseMessage, Jots.Response)> GetJotByIdAsync(Guid id)
    {
        var message = await _sutClient.GetAsync($"jots/{id}");

        var jot = message.IsSuccessStatusCode 
            ? await message.Content.ReadFromJsonAsync<Jots.Response>()
            : null;
        
        return (message, jot!);
    }
    
    private class RepositoryMock : Jots.IRepository
    {
        private readonly List<Jots.Jot> _store = new();

        public Jots.Jot[] GetAll() => _store.ToArray();

        public void Add(Jots.Jot jot) => _store.Add(jot);

        public Jots.Jot? GetById(Guid id) => _store.FirstOrDefault(x => x.Id == id);

        public bool Delete(Guid id)
        {
            var jot = GetById(id);
            
            return jot is not null && _store.Remove(jot);
        }
    }
}