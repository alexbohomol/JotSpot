namespace JotSpot.Api.Endpoints;

public static class Root
{
    public static void AddRootEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => "Hello World!");
    }
}