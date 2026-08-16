namespace Jots.Api.Endpoints;

public static class RootEndpoints
{
    public static void AddEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => "Hello World!");
    }
}
