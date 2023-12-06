﻿using Android.App;
using Android.Runtime;

namespace MemoryGame
{
    #if !DEBUG
    [Application]
    #else
    [Application(UsesCleartextTraffic = true)]
    #endif
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
