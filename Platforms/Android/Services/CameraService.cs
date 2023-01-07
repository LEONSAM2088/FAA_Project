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
using Android.Hardware.Lights;
using Android.Renderscripts;
using static Android.Icu.Text.ListFormatter;
using Android.Nfc;
using Camera2VideoSample;

namespace FAA_Project.Data
{
    [Service]
    public class CameraService : Service, ICameraService
    {
        private const string TAG = "Camera2VideoFragment";
        public static CameraService Camera_service;
        private CaptureRequest.Builder previewBuilder;
        public static bool IsBound;
        public string[] cameraIds;
        private MediaRecorder mMediaRecorder;
        private Java.IO.File mCurrentFile;
        private CameraManager _cameraManager;
        private string _cameraId;
        private Size videoSize;
        private Size previewSize;
        private MyCameraStateCallback stateListener;
        public CameraDevice cameraDevice;
        private bool isRecordingVideo;
        public Semaphore cameraOpenCloseLock = new Semaphore(1);

        public CameraService()
        {
            Intent startService = new Intent(MainActivity.ActivityCurrent, typeof(CameraService));
            startService.SetAction("START_SERVICE");
            
            MainActivity.ActivityCurrent.StartService(startService);

        }
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            _cameraManager = (CameraManager)GetSystemService(CameraService);
            cameraIds = _cameraManager.GetCameraIdList();


            Camera_service = this;
            stateListener = new MyCameraStateCallback(this);
            IsBound = true;
            return base.OnStartCommand(intent, flags, startId);
        }
        
        
        public void OpenCamera()
        {
            if (null == MainActivity.ActivityCurrent || MainActivity.ActivityCurrent.IsFinishing)
                return;
            try
            {
                if (!cameraOpenCloseLock.TryAcquire(2500, TimeUnit.Milliseconds))
                    throw new RuntimeException("Time out waiting to lock camera opening.");
                string cameraId = _cameraManager.GetCameraIdList()[0];
                CameraCharacteristics characteristics = _cameraManager.GetCameraCharacteristics(cameraId);
                StreamConfigurationMap map = (StreamConfigurationMap)characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap);
                videoSize = ChooseVideoSize(map.GetOutputSizes(Class.FromType(typeof(MediaRecorder))));
                
                mMediaRecorder = new MediaRecorder(MainActivity.ActivityCurrent);
                
                _cameraManager.OpenCamera(cameraId, stateListener, MainActivity.ActivityCurrent.backgroundHandler);

            }
            catch (CameraAccessException)
            {
                Toast.MakeText(MainActivity.ActivityCurrent, "Cannot access the camera.", ToastLength.Short).Show();
                MainActivity.ActivityCurrent.Finish();
            }
            catch (NullPointerException)
            {
                //var dialog = new ErrorDialog();
                //dialog.Show(MainActivity.ActivityCurrent, "dialog");
            }
            catch (InterruptedException)
            {
                throw new RuntimeException("Interrupted while trying to lock camera opening.");
            }
        }
        public void CloseCamera()
        {
            try
            {
                cameraOpenCloseLock.Acquire();
                if (null != cameraDevice)
                {
                    cameraDevice.Close();
                    cameraDevice = null;
                }
                if (null != mMediaRecorder)
                {
                    mMediaRecorder.Release();
                    mMediaRecorder = null;
                }
            }
            catch (InterruptedException e)
            {
                throw new RuntimeException("Interrupted while trying to lock camera closing.");
            }
            finally
            {
                cameraOpenCloseLock.Release();
            }
        }

        public void OnOpened(CameraDevice camera)
        {
          
        }

        public void StartPreview()
        {
            if (null == cameraDevice || null == previewSize)
                return;

            try
            {
                SetUpMediaRecorder();
                SurfaceTexture texture = new(3);
                //Assert.IsNotNull(texture);
                texture.SetDefaultBufferSize(previewSize.Width, previewSize.Height);
                previewBuilder = cameraDevice.CreateCaptureRequest(CameraTemplate.Record);
                var surfaces = new List<Surface>();
                var previewSurface = new Surface(texture);
                surfaces.Add(previewSurface);
                previewBuilder.AddTarget(previewSurface);

                var recorderSurface = mMediaRecorder.Surface;
                surfaces.Add(recorderSurface);
                previewBuilder.AddTarget(recorderSurface);

                cameraDevice.CreateCaptureSession(surfaces, new PreviewCaptureStateCallback(this), MainActivity.ActivityCurrent.backgroundHandler);

            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
            catch (IOException e)
            {
                e.PrintStackTrace();
            }
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

        
        
        private void SetUpMediaRecorder()
        {

           

            mMediaRecorder = new MediaRecorder(MainActivity.ActivityCurrent);


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





        private Size ChooseVideoSize(Size[] choices)
        {
            foreach (Size size in choices)
            {
                if (size.Width == size.Height * 4 / 3 && size.Width <= 1000)
                    return size;
            }
            Log.Error(TAG, "Couldn't find any suitable video size");
            return choices[choices.Length - 1];
        }
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
    }
}
