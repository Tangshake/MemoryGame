using MemoryGame.Model.Player;
using MemoryGame.Services;
using MemoryGame.SynchronousDataService.Highscore;
using MemoryGame.SynchronousDataService.Login;
using MemoryGame.SynchronousDataService.Register;
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
            builder.Services.AddSingleton<IVerifyTokenApiClient, VerifyTokenApiClient>();
            builder.Services.AddSingleton<IHighscoreService, HighscoreService>();
            builder.Services.AddSingleton<ILoginService, LoginService>();
            builder.Services.AddSingleton<IRegisterService, RegisterService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
