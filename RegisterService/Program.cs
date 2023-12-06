using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using RegisterService.DatabaseAccess;
using RegisterService.DataTransferObject;
using RegisterService.FluentValidator;
using RegisterService.PasswordManagement;

var builder = WebApplication.CreateBuilder(args);

// For regular endpoints AddApiExplorer() method is called when this extensiom method is called
builder.Services.AddControllers();

// To register minimal API and generate OpenAPI documentation following extension method needs to be called
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(cfg =>
{
    cfg.SwaggerDoc("v1", new()
    {
        Title = "Register",
        Description = "Registeres new user and sends an confirmation email.",
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
});

builder.Services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IRegisterRepository, RegisterRepository>();
builder.Services.AddSingleton<IPasswordHash, PasswordHash>();

builder.Services.AddMailKit(config => config.UseMailKit(builder.Configuration.GetSection("Email").Get<MailKitOptions>()));

var app = builder.Build();

if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
