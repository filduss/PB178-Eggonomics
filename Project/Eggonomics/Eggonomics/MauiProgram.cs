using Eggonomics.Data;
using Eggonomics.Interfaces;
using Eggonomics.Services;
using Eggonomics.ViewModels;
using Eggonomics.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Eggonomics
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Configuration.AddUserSecrets<App>();
#endif

            var connectionString = builder.Configuration["ConnectionString"]
                ?? throw new InvalidOperationException("ConnectionString not found in user secrets.");

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString), ServiceLifetime.Transient);

            builder.Services.AddSingleton<DatabaseInitializer>();

            var supabaseUrl = builder.Configuration["SupabaseUrl"]
                ?? throw new InvalidOperationException("SupabaseUrl not found in user secrets.");
            var supabaseKey = builder.Configuration["SupabaseKey"]
                ?? throw new InvalidOperationException("SupabaseKey not found in user secrets.");
            builder.Services.AddSingleton(new Supabase.Client(supabaseUrl, supabaseKey));
            builder.Services.AddTransient<IAuthService, AuthService>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<FridgesViewModel>();
            builder.Services.AddTransient<FridgesPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
                Task.Run(() => initializer.InitializeAsync()).GetAwaiter().GetResult();
            }

            return app;
        }
    }
}
