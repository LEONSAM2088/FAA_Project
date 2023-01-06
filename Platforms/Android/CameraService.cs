using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Media;

using Java.Lang;
using Java.Util;
using Java.IO;
using Java.Util.Concurrent;
using Size = Android.Util.Size;
using View = Android.Views.View;
using Button = Android.Widget.Button;
using Semaphore = Java.Util.Concurrent.Semaphore;
using IOException = Java.IO.IOException;
using File = Java.IO.File;
using Resource = Microsoft.Maui.Controls.Resource;
using AndroidX.Fragment.App;
using Environment = Android.OS.Environment;

namespace FAA_Project.Data
{
    [Service]
    public class CameraService : Service, ICameraService
    {


        public string[] cameraIds;
        private MediaRecorder mMediaRecorder;
        private Java.IO.File mCurrentFile;
        private CameraManager _cameraManager;
        private string _cameraId;
        private HandlerThread backgroundThread;
        private Handler backgroundHandler;

        public CameraService()
        {
            Intent startService = new Intent(MainActivity.ActivityCurrent, typeof(CameraService));
            startService.SetAction("START_SERVICE");
            MainActivity.ActivityCurrent.StartService(startService);
        }
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            
            return base.OnStartCommand(intent, flags, startId);
        }
        public override void OnCreate()
        {
            base.OnCreate();

            _cameraManager = (CameraManager)GetSystemService(CameraService);
            cameraIds = _cameraManager.GetCameraIdList();

            SetUpMediaRecorder();
        }
     
        public void OpenCamera()
        {
            
        }

        public void OnOpened(CameraDevice camera)
        {
          
        }

        public void StartPreview()
        {
         
        }

        public void OnConfigured(CameraCaptureSession session)
        {
          
        }

        public void SetPreview(SurfaceView preview)
        {
       
        }

        private void OnSurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
           
        }

        public  void CloseCamera()
        {
           
        }
        
        private void SetUpMediaRecorder()
        {



            mMediaRecorder = new MediaRecorder(this);


            mMediaRecorder.SetVideoSource(VideoSource.Surface);
            mMediaRecorder.SetOutputFormat(OutputFormat.Mpeg4);
            mCurrentFile = new Java.IO.File(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDcim), "test.mp4");
            mMediaRecorder.SetOutputFile(mCurrentFile.AbsolutePath);

            var encoderProfiles = CamcorderProfile.GetAll(cameraIds[0], (int)CamcorderQuality.Q480p);
            var profile = encoderProfiles.VideoProfiles.FirstOrDefault();

            mMediaRecorder.SetVideoFrameRate(profile.FrameRate);
            mMediaRecorder.SetVideoSize(profile.Width, profile.Height);
            mMediaRecorder.SetVideoEncodingBitRate(profile.Bitrate);
            mMediaRecorder.SetVideoEncoder(VideoEncoder.H264);

            try
            {
                mMediaRecorder.Prepare();
            }
            catch (Java.Lang.Exception e)
            {

            }


        }
        private void StartBackgroundThread()
        {
            backgroundThread = new HandlerThread("CameraBackground");
            backgroundThread.Start();
            backgroundHandler = new Handler(backgroundThread.Looper);
        }







        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
    }
}
