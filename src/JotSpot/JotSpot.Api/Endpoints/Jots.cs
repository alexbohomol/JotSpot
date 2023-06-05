namespace JotSpot.Api.Endpoints;

public static class Jots
{
    public static void AddJotsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/jots", (IRepository repository) => repository.GetAll());
        
        app.MapPost("/jots", (Request request, IRepository repository) =>
        {
            var jot = new Jot
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Text = request.Text
            };
            
            repository.Add(jot);

            return Results.Created($"/jots/{jot.Id}", new Response(jot.Id, jot.Title, jot.Text));
        });

        app.MapGet("/jots/{id}", (Guid id, IRepository repository) =>
        {
            var jot = repository.GetById(id);
            
            return jot is null 
                ? Results.NotFound() 
                : Results.Ok(Response.FromDomain(jot));
        });

        app.MapDelete("/jots/{id}", (Guid id, IRepository repository) => 
            repository.Delete(id) 
                ? Results.NoContent() 
                : Results.NotFound());

        app.MapPut("/jots/{id}", (Guid id, Request request, IRepository repository) =>
        {
            var jot = repository.GetById(id);
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
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
    
    public interface IRepository
    {
        Jot[] GetAll();
        void Add(Jot jot);
        Jot? GetById(Guid id);
        bool Delete(Guid id);
    }

    // TODO: TBD - implement later when introducing real storage
    public class Repository : IRepository
    {
        public Jot[] GetAll()
        {
            throw new NotImplementedException();
        }

        public void Add(Jot jot)
        {
            throw new NotImplementedException();
        }

        public Jot? GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        public bool Delete(Guid id)
        {
            throw new NotImplementedException();
        }
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