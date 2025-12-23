using TimeTracker.Api.Infrastructure.Persistence;
using TimeTracker.Api.Shared;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.ApplyMigrationsAsync();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapFeatureEndpoints();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
