using JotSpot.Api.Endpoints;

namespace JotSpot.Api.Tests;

public class JotsEndpointsTests : IntegrationTest
{
    [Fact]
    public async Task GetJots_ReturnsOk_EmptyList()
    {
        var msg = await SutClient.GetAsync("jots");

        msg.StatusCode.Should().Be(HttpStatusCode.OK);
        msg.Content.Should().NotBeNull();
        
        var jots = await msg.Content.ReadFromJsonAsync<Jots.Response[]>();
        jots.Should().BeEmpty();
    }

    [Fact]
    public async Task PostJot_ReturnsCreated()
    {
        var request = new Jots.Request("test title", "test text");

        var (msg, created) = await CreateJot(request);
        
        msg.StatusCode.Should().Be(HttpStatusCode.Created);
        msg.Content.Should().NotBeNull();
        
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created!.Title.Should().Be(request.Title);
        created!.Text.Should().Be(request.Text);
        
        msg.Headers.Location.Should().NotBeNull();
        msg.Headers.Location!.Should().BeOfType<Uri>();
        msg.Headers.Location!.OriginalString.Should().Contain(created.Id.ToString());
    }
    
    [Fact]
    public async Task GetJot_ReturnsAvailableJot()
    {
        var request = new Jots.Request("test title", "test text");
        var (_, created) = await CreateJot(request);

        var msg = await SutClient.GetAsync($"jots/{created.Id}");
        msg.StatusCode.Should().Be(HttpStatusCode.OK);
        msg.Content.Should().NotBeNull();

        var jot = await msg.Content.ReadFromJsonAsync<Jots.Response>();
        jot.Should().NotBeNull();
        jot!.Id.Should().NotBeEmpty();
        jot!.Title.Should().Be(request.Title);
        jot!.Text.Should().Be(request.Text);
    }
    
    [Fact]
    public async Task GetJot_ReturnsNotFound()
    {
        var msg = await SutClient.GetAsync($"jots/{Guid.Empty}");
        msg.StatusCode.Should().Be(HttpStatusCode.NotFound);
        msg.Content.Should().NotBeNull();
        
        var body = await msg.Content.ReadAsStringAsync();
        body.Should().BeEmpty();
    }
    
    [Fact]
    public async Task DeleteJot_DeletesAvailableJot()
    {
        var request = new Jots.Request("test title", "test text");
        var (_, created) = await CreateJot(request);

        var getMsg = await SutClient.GetAsync($"jots/{created.Id}");
        getMsg.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var delMsg = await SutClient.DeleteAsync($"jots/{created.Id}");
        delMsg.StatusCode.Should().Be(HttpStatusCode.NoContent);
        delMsg.Content.Should().NotBeNull();
        var body = await delMsg.Content.ReadAsStringAsync();
        body.Should().BeEmpty();
        
        var chkMsg = await SutClient.GetAsync($"jots/{created.Id}");
        chkMsg.StatusCode.Should().Be(HttpStatusCode.NotFound);
        chkMsg.Content.Should().NotBeNull();
    }
    
    [Fact]
    public async Task DeleteJot_ReturnsNotFound()
    {
        var msg = await SutClient.DeleteAsync($"jots/{Guid.Empty}");
        msg.StatusCode.Should().Be(HttpStatusCode.NotFound);
        msg.Content.Should().NotBeNull();
        
        var body = await msg.Content.ReadAsStringAsync();
        body.Should().BeEmpty();
    }
    
    [Fact]
    public async Task PutJot_UpdatesAvailableJot()
    {
        var request = new Jots.Request("initial title", "initial text");
        var (_, created) = await CreateJot(request);

        var getMsg = await SutClient.GetAsync($"jots/{created.Id}");
        getMsg.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var update = new Jots.Request("updated title", "updated text");
        var putMsg = await SutClient.PutAsync($"jots/{created.Id}", JsonContent.Create(update));
        putMsg.StatusCode.Should().Be(HttpStatusCode.OK);
        putMsg.Content.Should().NotBeNull();

        var updated = await putMsg.Content.ReadFromJsonAsync<Jots.Response>();
        updated.Should().NotBeNull();
        updated!.Id.Should().NotBeEmpty();
        updated!.Title.Should().Be(update.Title);
        updated!.Text.Should().Be(update.Text);
        
        var chkMsg = await SutClient.GetAsync($"jots/{updated.Id}");
        chkMsg.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var retrieved = await chkMsg.Content.ReadFromJsonAsync<Jots.Response>();
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(updated.Id);
        retrieved!.Title.Should().Be(updated.Title);
        retrieved!.Text.Should().Be(updated.Text);
    }
    
    [Fact]
    public async Task PutJot_ReturnsNotFound()
    {
        var request = new Jots.Request("initial title", "initial text");
        
        var msg = await SutClient.PutAsync($"jots/{Guid.Empty}", JsonContent.Create(request));
        msg.StatusCode.Should().Be(HttpStatusCode.NotFound);
        msg.Content.Should().NotBeNull();
        
        var body = await msg.Content.ReadAsStringAsync();
        body.Should().BeEmpty();
    }

    private async Task<(HttpResponseMessage, Jots.Response)> CreateJot(Jots.Request request)
    {
        var message = await SutClient.PostAsJsonAsync("jots", request);
        
        var created = await message.Content.ReadFromJsonAsync<Jots.Response>();

        return (message, created!);
    }
}













