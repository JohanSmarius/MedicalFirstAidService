using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Staff.API;
using Event.API;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddStaffModule();
builder.AddEventModule();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapEventEndpoints();
app.MapStaffEndpoints();

app.Run();
