using Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddStartupServices(builder.Configuration);

var app = builder.Build();

app.ConfigureApplication();

app.Run();