using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Hardware.Camera2;
using Android.Media;
using Android.OS;
using Android.Renderscripts;
using Android.Util;
using Android.Views;
using AndroidX.Core.Content;
using FAA_Project.Data;
using Java.Lang;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.PlatformConfiguration;

using static Android.OS.Build;
using Environment = Android.OS.Environment;

namespace FAA_Project;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public HandlerThread backgroundThread;
    public Handler backgroundHandler;

    public static MainActivity ActivityCurrent { get; set; }
    public MainActivity()
    {
        ActivityCurrent = this;
    }



    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        if (ContextCompat.CheckSelfPermission(this,Manifest.Permission.Camera) != Permission.Granted
                ||
                (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != Permission.Granted)
                ||
                (ContextCompat.CheckSelfPermission(this, Manifest.Permission.RecordAudio) != Permission.Granted)
        )
        {
            RequestPermissions(new string[] { Manifest.Permission.Camera, Manifest.Permission.WriteExternalStorage, Manifest.Permission.RecordAudio }, 1);
        }
        
    }
    protected override void OnPause()
    {
        if (Data.CameraService.IsBound)
            FAA_Project.Data.CameraService.Camera_service.CloseCamera();
        StopBackgroundThread();
        base.OnPause();
    }

    protected override void OnResume()
    {
        base.OnResume();
        StartBackgroundThread();
        if(Data.CameraService.IsBound)
            FAA_Project.Data.CameraService.Camera_service.OpenCamera();
    }
  
    
    private void StartBackgroundThread()
    {
        backgroundThread = new HandlerThread("CameraBackground");
        backgroundThread.Start();
        backgroundHandler = new Handler(backgroundThread.Looper);
    }

    private void StopBackgroundThread()
    {
        backgroundThread.QuitSafely();
        try
        {
            backgroundThread.Join();
            backgroundThread = null;
            backgroundHandler = null;
        }
        catch (InterruptedException e)
        {
            e.PrintStackTrace();
        }
    }


}
