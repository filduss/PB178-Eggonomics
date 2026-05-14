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
            builder.Services.AddTransient<IEggTrayService, EggTrayService>();
            builder.Services.AddTransient<IFridgeOperationService, FridgeOperationService>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<FridgesViewModel>();
            builder.Services.AddTransient<FridgesPage>();
            builder.Services.AddTransient<FridgeViewModel>();
            builder.Services.AddTransient<FridgePage>();
            builder.Services.AddTransient<FridgeOverviewViewModel>();
            builder.Services.AddTransient<FridgeOverviewPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var transparent = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);

            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("RemoveNativeBorder", (handler, view) =>
            {
                handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
                handler.PlatformView.Resources["TextControlBorderBrush"] = transparent;
                handler.PlatformView.Resources["TextControlBorderBrushPointerOver"] = transparent;
                handler.PlatformView.Resources["TextControlBorderBrushFocused"] = transparent;
                handler.PlatformView.Resources["TextControlBorderBrushDisabled"] = transparent;
                handler.PlatformView.Resources["TextControlBackground"] = transparent;
                handler.PlatformView.Resources["TextControlBackgroundPointerOver"] = transparent;
                handler.PlatformView.Resources["TextControlBackgroundFocused"] = transparent;
                handler.PlatformView.Resources["TextControlBackgroundDisabled"] = transparent;
            });

            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("RemoveNativeBorder", (handler, view) =>
            {
                handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
                handler.PlatformView.Resources["ComboBoxBorderBrush"] = transparent;
                handler.PlatformView.Resources["ComboBoxBorderBrushPointerOver"] = transparent;
                handler.PlatformView.Resources["ComboBoxBorderBrushFocused"] = transparent;
                handler.PlatformView.Resources["ComboBoxBorderBrushDisabled"] = transparent;
                handler.PlatformView.Resources["ComboBoxBackground"] = transparent;
                handler.PlatformView.Resources["ComboBoxBackgroundPointerOver"] = transparent;
                handler.PlatformView.Resources["ComboBoxBackgroundFocused"] = transparent;
            });

            Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("RemoveNativeBorder", (handler, view) =>
            {
                handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
                handler.PlatformView.Resources["DatePickerBorderBrush"] = transparent;
                handler.PlatformView.Resources["DatePickerBorderBrushPointerOver"] = transparent;
                handler.PlatformView.Resources["DatePickerBorderBrushFocused"] = transparent;
                handler.PlatformView.Resources["DatePickerBackground"] = transparent;
                handler.PlatformView.Resources["DatePickerBackgroundPointerOver"] = transparent;
            });

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
