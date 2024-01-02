using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MemoryGame.SynchronousDataService
{
    public abstract class ApiBase
    {
        protected readonly HttpClient client;
        //protected string serviceAddress;
        protected readonly JsonSerializerOptions jsonSerializerOptions;

        protected ApiBase()
        {
#if DEBUG
        client = new HttpClient(new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        });
#else
        client = new HttpClient();
#endif
        
            jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
        }
    }
}
