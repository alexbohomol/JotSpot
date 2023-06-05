using JotSpot.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<Jots.IRepository, Jots.Repository>();

var app = builder.Build();

app.MapHealthChecks("/health");
app.AddRootEndpoints();
app.AddJotsEndpoints();

app.Run();