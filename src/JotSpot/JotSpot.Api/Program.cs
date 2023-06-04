using JotSpot.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");
app.AddRootEndpoints();
app.AddJotsEndpoints();

app.Run();

public partial class Program { }