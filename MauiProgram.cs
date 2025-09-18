using Microsoft.Extensions.Logging;
using RentreyApp.Services;
using System.IO;
using System;
using Rentrey;

namespace RentreyApp
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
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "properties.db3");
            builder.Services.AddSingleton(s => new DatabaseService(dbPath));
            
            // Register your API service as a singleton
            builder.Services.AddSingleton<ProptrackPropertiesService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
