using JotSpot.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.AddRootEndpoints();

app.Run();

public partial class Program { }