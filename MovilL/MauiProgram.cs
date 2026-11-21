using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MovilL.DbContext;
using MovilL.Services;
using MovilL.views;

namespace MovilL
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

            builder.Services.AddSingleton<IDatabasePathService, DatabasePathService>();
            builder.Services.AddDbContextFactory<MovilDbContext>((serviceProvider, options) =>
            {
                var pathService = serviceProvider.GetRequiredService<IDatabasePathService>();
                options.UseSqlite($"Data Source={pathService.GetDatabasePath()}");
            });
            builder.Services.AddSingleton<IMovilDatabaseService, MovilDatabaseService>();

            builder.Services.AddTransient<views.NuevoMovimientoPage>();
            builder.Services.AddTransient<views.ResumenPage>();
            builder.Services.AddTransient<views.LoginPage>();
            builder.Services.AddTransient<views.Inicio>();
            builder.Services.AddTransient<views.Perfil>();
            builder.Services.AddTransient<views.Configuracion>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            var app = builder.Build();
            app.Services.GetRequiredService<IMovilDatabaseService>()
                .EnsureCreatedAsync()
                .GetAwaiter()
                .GetResult();

            return app;
        }
    }
}
