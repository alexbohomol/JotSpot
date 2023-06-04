namespace JotSpot.Api.Endpoints;

public static class Jots
{
    private static readonly List<Jot> JotStore = new();
    
    public static void AddJotsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/jots", () => JotStore.ToArray());
        
        app.MapPost("/jots", (Request request) =>
        {
            var jot = new Jot
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Text = request.Text
            };
            
            JotStore.Add(jot);

            return Results.Created($"/jots/{jot.Id}", new Response(jot.Id, jot.Title, jot.Text));
        });

        app.MapGet("/jots/{id}", (Guid id) =>
        {
            var jot = JotStore.FirstOrDefault(x => x.Id == id);
            
            return jot is null 
                ? Results.NotFound() 
                : Results.Ok(Response.FromDomain(jot));
        });

        app.MapDelete("/jots/{id}", (Guid id) =>
        {
            var jot = JotStore.FirstOrDefault(x => x.Id == id);
            if (jot is null)
            {
                return Results.NotFound();
            }

            var removed = JotStore.Remove(jot);

            return removed ? Results.NoContent() : Results.NotFound();
        });

        app.MapPut("/jots/{id}", (Guid id, Request request) =>
        {
            var jot = JotStore.FirstOrDefault(x => x.Id == id);
            if (jot is null)
            {
                return Results.NotFound();
            }

            jot.Title = request.Title;
            jot.Text = request.Text;

            return Results.Ok(Response.FromDomain(jot));
        });
    }

    public class Jot
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
    }

    public record Response(Guid Id, string Title, string Text)
    {
        public static Response FromDomain(Jot jot)
        {
            return new Response(jot.Id, jot.Title, jot.Text);
        }
    }

    public record Request(string Title, string Text);
}