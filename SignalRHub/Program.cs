using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SignalRHub.Hubs;
using SignalRHub.JwtBearer;
using SignalRHub.Repository.Jwt;
using System.Buffers.Text;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddSingleton<IJwtRepository, JwtRepository>();

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

builder.Services.AddOptions<JwtBearerSettings>()
    .BindConfiguration("JwtBearer")
    .ValidateDataAnnotations();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<SampleHub>("/game-hub");

app.Run();
