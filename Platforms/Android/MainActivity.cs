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

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait,ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
   

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
        if (Camera2VideoFragment.IsBound)
        {
           
           Camera2VideoFragment.Camera_service.CloseCamera();
            
        }
            
        base.OnPause();
    }
    
    protected override void OnResume()
    {
        base.OnResume();

        if (Camera2VideoFragment.IsBound)
        {

            
            Camera2VideoFragment.Camera_service.OpenCamera(640, 480);
        }
    }


    public void StartBackgroundThread()
    {
        Camera2VideoFragment.Camera_service.backgroundThread = new HandlerThread("CameraBackground");
        Camera2VideoFragment.Camera_service.backgroundThread.Start();
        Camera2VideoFragment.Camera_service.backgroundHandler = new Handler(Camera2VideoFragment.Camera_service.backgroundThread.Looper);

    }

    public void StopBackgroundThread()
    {
        Camera2VideoFragment.Camera_service.backgroundThread.QuitSafely();
        try
        {
            Camera2VideoFragment.Camera_service.backgroundThread.Join();
            Camera2VideoFragment.Camera_service.backgroundThread = null;
            Camera2VideoFragment.Camera_service.backgroundHandler = null;
        }
        catch (InterruptedException e)
        {
            e.PrintStackTrace();
        }
    }


}
