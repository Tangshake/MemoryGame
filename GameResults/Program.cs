using GameResults.DatabasAccess;
using GameResults.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(cfg =>
{
    cfg.SwaggerDoc("v1", new()
    {
        Title = "MemoryGame Results API",
        Description = "MemoryGame ASP.NET Core MinimalAPI",
        Version = "v1",
        Contact = new OpenApiContact
        {
            Name = "Householdspirits",
            Email = "contact@householdspirits.com",
            Url = new Uri("https://householdspirits.com")
        },

        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
    // Generate the xml docs
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    cfg.IncludeXmlComments(xmlPath);
});

builder.Services.AddSingleton<IGameResultRepository, GameResultRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(cfg => cfg.SwaggerEndpoint("swagger/v1/swagger.json", "v1"));

app.MapPost("/api/result", async (IGameResultRepository gameResultRepository, [FromBody] UserResultRequest userResultRequest) =>
{
    if (userResultRequest is null)
        return Results.BadRequest();

    var result = await gameResultRepository.AddUserResult(userResultRequest);

    return result == 0 ? Results.BadRequest() : Results.Ok();

}).WithOpenApi(op => new(op)
{
    Summary = "Adds user result",
    Description = "Adds users game result to the database",
})
    .WithName("AddUserResult")
    .Produces<int>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest);


app.MapGet("/api/result/{count:int}", async (IGameResultRepository gameResultRepository, int count) =>
{
    if (gameResultRepository is null)
        return Results.BadRequest();

    var result = await gameResultRepository.GetTopResults(count);

    return Results.Ok(result);

}).WithOpenApi(op => new(op)
{
    Summary = "Get user results",
    Description = "Get top players from the database",
})
    .WithName("GetTopResults")
    .Produces<int>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest);


app.Run();
