using FlightDay.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.AddServiceDefaults();

builder.Services.AddDbContext<FlightDayDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("flightdaydb")));
    
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapDefaultEndpoints();