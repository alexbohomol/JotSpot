using static JotSpot.Api.Handlers.JotHandlers;

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
    }
}