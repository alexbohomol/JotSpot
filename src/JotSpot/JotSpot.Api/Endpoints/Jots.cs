using JotSpot.Api.Models;

namespace JotSpot.Api.Endpoints;

public static class Jots
{
    public static void AddJotsEndpoints(this IEndpointRouteBuilder app)
    {
        var jots = app.MapGroup("/jots");
        
        jots.MapGet("", GetAllJots);
        jots.MapPost("", CreateJot);
        jots.MapGet("{id}", GetJot);
        jots.MapDelete("{id}", DeleteJot);
        jots.MapPut("{id}", UpdateJot);

        jots.RequireAuthorization();
    }

    private static IResult GetAllJots(IRepository repository)
    {
        var jots = repository.GetAll().Select(JotResponse.FromDomain).ToArray();
        return Results.Ok(jots);
    }

    private static IResult UpdateJot(Guid id, JotRequest request, IRepository repository)
    {
        var jot = repository.GetById(id);
        if (jot is null)
        {
            return Results.NotFound();
        }

        jot.Title = request.Title;
        jot.Text = request.Text;

        return Results.Ok(JotResponse.FromDomain(jot));
    }

    private static IResult CreateJot(JotRequest request, IRepository repository)
    {
        var jot = new Jot { Id = Guid.NewGuid(), Title = request.Title, Text = request.Text };

        repository.Add(jot);

        return Results.Created($"/jots/{jot.Id}", new JotResponse(jot.Id, jot.Title, jot.Text));
    }

    private static IResult GetJot(Guid id, IRepository repository)
    {
        var jot = repository.GetById(id);

        return jot is null
            ? Results.NotFound()
            : Results.Ok(JotResponse.FromDomain(jot));
    }

    private static IResult DeleteJot(Guid id, IRepository repository)
    {
        return repository.Delete(id)
            ? Results.NoContent()
            : Results.NotFound();
    }
}