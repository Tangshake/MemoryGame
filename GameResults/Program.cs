using GameResults.Bearer;
using GameResults.DatabasAccess;
using GameResults.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<JwtBearerSettings>()
    .BindConfiguration("JwtBearer")
    .ValidateDataAnnotations();

builder.Services.AddSingleton<IGameResultRepository, GameResultRepository>();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddScheme<JwtBearerOptions, JwtBearerHandler>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    JwtRepository jwt = new JwtRepository(builder.Configuration);
    var result = jwt.GetJwtSecretAsync().Result;

    var jwtBearerSettings = builder.Configuration.GetSection("JwtBearer")
        .Get<JwtBearerSettings>() ?? throw new NullReferenceException();

    options.Events = new JwtBearerEvents
    {
        //For not web-based clients (like C# client) token is always sent via Authorization bearer header;
        //For web-based clients (javascript) token is sent via query string.
        OnMessageReceived = context =>
        {
            // Read the token out of the query string
            var accessToken = context.Request.Query["access_token"];

            // If the request is for our hub...
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/game-hub")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };

    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidIssuer = jwtBearerSettings.Issuer,
        ValidAudience = jwtBearerSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(result.jwt_key
            )),
        ClockSkew = TimeSpan.Zero,
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true
    };
});

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



var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(cfg => cfg.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"));

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseAuthentication();


app.MapPost("/api/result", [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
async (IGameResultRepository gameResultRepository, [FromBody] UserResultRequest userResultRequest) =>
{
    
    if (userResultRequest is null)
        return Results.BadRequest();

    var result = await gameResultRepository.AddUserResultAsync(userResultRequest);

    return result == 0 ? Results.BadRequest() : Results.Ok();

})
    .WithOpenApi(op => new(op)
    {
        Summary = "Adds user result",
        Description = "Adds users game result to the database",
    })
    .WithName("AddUserResult")
    .Produces<int>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest);


app.MapGet("/api/result/{count:int}", [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
async (IGameResultRepository gameResultRepository, [FromRoute] int count) =>
{
    if (gameResultRepository is null || count <= 0)
        return Results.BadRequest();

    var result = await gameResultRepository.GetTopResultsAsync(count);

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
