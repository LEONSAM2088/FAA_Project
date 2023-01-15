using Microsoft.Extensions.Logging;
using FAA_Project.Data;
#if ANDROID
using FAA_Project.Platforms.Android.Services;
#endif
namespace FAA_Project;

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

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton<WeatherForecastService>();


#if ANDROID
        builder.Services.AddSingleton<RestService>();
        builder.Services.AddSingleton<ICameraService, Camera2VideoFragment>();
        
#endif
        var app = builder.Build();
        return app;
	}
}
