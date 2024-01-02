using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGame.Endpoints
{
    public static class Configuration
    {
        public static string LoginEndpoint
        {
            get
            {
#if ANDROID
                return "https://10.0.2.2:7294/api/login";
#endif
                return "https://localhost:7294/api/login";
            }
        }

        public static string HighscoreEndpoint
        {
            get
            {
#if ANDROID
                return "https://10.0.2.2:7036/api/result";
#endif
                return "https://localhost:7036/api/result";
            }
        }

        public static string JwtRefreshEndpoint
        {
            get
            {
#if ANDROID
                return "https://10.0.2.2:7294/api/refresh/jwt";
#endif
                return "https://localhost:7294/api/refresh/jwt";
            }
        }

        public static string SignlRHub
        {
            get
            {
                var baseUrl = DeviceInfo.Platform == DevicePlatform.Android ?
                        "https://10.0.2.2:5106" : "https://localhost:5106";

                return baseUrl;
            }
        }
    }
}
