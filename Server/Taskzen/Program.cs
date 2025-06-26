using System.Text.Json;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Taskzen.API.Middlewares;
using Taskzen.Data;
using Taskzen.Extensions;
using Taskzen.Helpers;
using Taskzen.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterInterfaces();

DotNetEnv.Env.Load();

var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var decryptedConnectionString = EncryptionHelper.Decrypt(defaultConnectionString);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(decryptedConnectionString));

// builder.Services.AddHangfire(config =>
//     config.UsePostgreSqlStorage(decryptedConnectionString));

// builder.Services.AddHangfireServer();

// Add services to the container.
builder.Services.AddControllers();

// Add JWT Authentication for Auth0
builder.Services.AddCorsOptions(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// To accomodate difference of naming convention in Angular & .NET
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

//Custom middleware for logging info
app.UseLogging();

//Custom middleware for Exception handling
app.UseExceptionHandling();

// app.UseCors("AllowFrontend");
app.UseAuthorization();

app.MapControllers();

app.Run();