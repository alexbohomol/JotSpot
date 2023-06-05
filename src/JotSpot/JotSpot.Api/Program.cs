using JotSpot.Api.Endpoints;
using JotSpot.Api.Handlers;
using JotSpot.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<IRepository, Repository>();

var app = builder.Build();

app.MapHealthChecks("/health");
app.AddRootEndpoints();
app.AddJotsEndpoints();

app.Run();