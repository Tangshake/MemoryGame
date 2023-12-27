using MemoryGame.Model.Player;
using MemoryGame.SynchronousDataService;
using MemoryGame.SynchronousDataService.Highscore;
using MemoryGame.SynchronousDataService.JwtRefresh;
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

            builder.Services.AddSingleton<IPlayerData, PlayerData>();
            builder.Services.AddSingleton<ILoginService, LoginService>();
            builder.Services.AddSingleton<IJwtRefreshService, JwtRefreshService>();
            builder.Services.AddSingleton<IRegisterService, RegisterService>();
            builder.Services.AddSingleton<IHighscoreService, HighscoreService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
