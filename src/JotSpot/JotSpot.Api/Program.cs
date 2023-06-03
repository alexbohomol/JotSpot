﻿using JotSpot.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.AddRootEndpoints();
app.AddJotsEndpoints();

app.Run();

public partial class Program { }