using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGame.SignalR.Settings;

public class SignalRRetryPolicy : IRetryPolicy
{
    private readonly Random _random = new Random();

    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        if (retryContext.ElapsedTime < TimeSpan.FromSeconds(60))
        {
            var d = _random.NextDouble();
            Debug.WriteLine($"Reconnecting time: {d} Total time reconnecting: {retryContext.ElapsedTime}");
            return TimeSpan.FromSeconds( d * 10);
        }
        else
        {
            // If we've been reconnecting for more than 60 seconds so far, stop reconnecting.
            return null;
        }
    }
}

