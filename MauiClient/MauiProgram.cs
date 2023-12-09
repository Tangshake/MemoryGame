using MemoryGame.Model.Player;
using MemoryGame.Services;
using Microsoft.Extensions.Logging;

namespace MemoryGame
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddScoped<PlayerData>();
            builder.Services.AddSingleton<ILoginApiClient, LoginApiClient>();
            builder.Services.AddSingleton<IRegisterApiClient, RegisterApiClient>();
            builder.Services.AddSingleton<IVerifyTokenApiClient, VerifyTokenApiClient>();
            builder.Services.AddSingleton<IGameResultRepository, GameResultRepository>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
