using LoginService.Authentication.Basic;
using LoginService.AuthTokens;
using LoginService.AuthTokens.Settings;
using LoginService.DatabaseAccess;
using LoginService.JWT;
using LoginService.JWT.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen( opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Memory Api", Version = "v1" });

    opt.AddSecurityDefinition(BasicAuthenticationDefaults.AutenticationScheme,
        new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = BasicAuthenticationDefaults.AutenticationScheme,
            In = ParameterLocation.Header,
            Description = "Basic authorization header"

        });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = BasicAuthenticationDefaults.AutenticationScheme
                }
            },
            new string[] { "Basic" }
        }
    });
});

builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<ITokenRepository, TokenRepository>();
builder.Services.AddSingleton<ITokenManager, TokenManager>();


builder.Services.AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(
        BasicAuthenticationDefaults.AutenticationScheme, null
    );

builder.Services.AddOptions<JwtBearerSettings>()
    .Bind(builder.Configuration.GetSection("JwtBearer"))
    .ValidateDataAnnotations();

builder.Services.AddOptions<RefreshTokenSettings>()
    .BindConfiguration("RefreshToken")
    .ValidateDataAnnotations();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
