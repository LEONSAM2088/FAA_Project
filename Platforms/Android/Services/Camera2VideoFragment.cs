
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
using static Android.Views.TextureView;
using View = Android.Views.View;
using Semaphore = Java.Util.Concurrent.Semaphore;
using Size = Android.Util.Size;
using Resource = Microsoft.Maui.Controls.Resource;
using IOException = Java.IO.IOException;
using File = Java.IO.File;
using RectF = Android.Graphics.RectF;
using FAA_Project.Data;
using Button = Android.Widget.Button;
using System.Runtime.CompilerServices;
using Environment = Android.OS.Environment;
using FAA_Project.Platforms.Android.Services;

namespace FAA_Project
{
    public class Camera2VideoFragment: ICameraService
    {
        private const string TAG = "Camera2VideoFragment";
        private SparseIntArray ORIENTATIONS = new SparseIntArray();

      

        

        public CameraDevice cameraDevice;
        public CameraCaptureSession previewSession;
        public MediaRecorder mediaRecorder;

        private bool isRecordingVideo;
        public Semaphore cameraOpenCloseLock = new Semaphore(1);

        // Called when the CameraDevice changes state
        private MyCameraStateCallback stateListener;

        public static Camera2VideoFragment Camera_service { get; set; }
        public static bool IsBound { get; set; }

        // Handles several lifecycle events of a TextureView
        private MySurfaceTextureListener surfaceTextureListener;

        public CaptureRequest.Builder builder;
        private CaptureRequest.Builder previewBuilder;

        private Size videoSize;
        private Size previewSize;
        private readonly RestService _restService;

        public HandlerThread backgroundThread;
        public Handler backgroundHandler;


        public Camera2VideoFragment(RestService restService)
        {
            _restService = restService;
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation0, 90);
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation90, 0);
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation180, 270);
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation270, 180);
            surfaceTextureListener = new MySurfaceTextureListener(this);
            stateListener = new MyCameraStateCallback(this);
            Camera_service = this;
            IsBound = true;
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

        private Size ChooseOptimalSize(Size[] choices, int width, int height, Size aspectRatio)
        {
            var bigEnough = new List<Size>();
            int w = aspectRatio.Width;
            int h = aspectRatio.Height;
            foreach (Size option in choices)
            {
                if (option.Height == option.Width * h / w &&
                    option.Width >= width && option.Height >= height)
                    bigEnough.Add(option);
            }

            if (bigEnough.Count > 0)
                return (Size)Collections.Min(bigEnough, new CompareSizesByArea());
            else
            {
                Log.Error(TAG, "Couldn't find any suitable preview size");
                return choices[0];
            }
        }
        

        

       

        public async void OnClick()
        {
          
            if (isRecordingVideo)
            {
               await stopRecordingVideo();
            }
            else
            {
                StartRecordingVideo();
            }
                     
                

        }
        
        //Tries to open a CameraDevice
        public void OpenCamera(int width, int height)
        {
            
            if (null == MainActivity.ActivityCurrent  || MainActivity.ActivityCurrent.IsFinishing)
                return;

            MainActivity.ActivityCurrent.StartBackgroundThread();

            CameraManager manager = (CameraManager)MainActivity.ActivityCurrent.GetSystemService(Context.CameraService);
            try
            {
                if (!cameraOpenCloseLock.TryAcquire(2500, TimeUnit.Milliseconds))
                    throw new RuntimeException("Time out waiting to lock camera opening.");
                string cameraId = manager.GetCameraIdList()[1];
                CameraCharacteristics characteristics = manager.GetCameraCharacteristics(cameraId);
                StreamConfigurationMap map = (StreamConfigurationMap)characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap);
                videoSize = ChooseVideoSize(map.GetOutputSizes(Class.FromType(typeof(MediaRecorder))));
                previewSize = ChooseOptimalSize(map.GetOutputSizes(Class.FromType(typeof(MediaRecorder))), width, height, videoSize);
                int orientation = (int)MainActivity.ActivityCurrent.WindowManager.DefaultDisplay.Rotation;
                
                configureTransform(width, height);
                mediaRecorder = new MediaRecorder(MainActivity.ActivityCurrent);
                manager.OpenCamera(cameraId, stateListener, null);

            }
            catch (CameraAccessException)
            {
                Toast.MakeText(MainActivity.ActivityCurrent, "Cannot access the camera.", ToastLength.Short).Show();
                MainActivity.ActivityCurrent.Finish();
            }
            catch (NullPointerException)
            {
                
            }
            catch (InterruptedException)
            {
                throw new RuntimeException("Interrupted while trying to lock camera opening.");
            }
        }

        //Start the camera preview
        public void startPreview()
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

                var recorderSurface = mediaRecorder.Surface;
                surfaces.Add(recorderSurface);
                previewBuilder.AddTarget(recorderSurface);

                cameraDevice.CreateCaptureSession(surfaces, new PreviewCaptureStateCallback(this), backgroundHandler);

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

        public void CloseCamera()
        {
            MainActivity.ActivityCurrent.StartBackgroundThread();
            try
            {
                cameraOpenCloseLock.Acquire();
                if (null != cameraDevice)
                {
                    cameraDevice.Close();
                    cameraDevice = null;
                }
                if (null != mediaRecorder)
                {
                    mediaRecorder.Release();
                    mediaRecorder = null;
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

        //Update the preview
        public void updatePreview()
        {
            if (null == cameraDevice)
                return;

            try
            {
                setUpCaptureRequestBuilder(previewBuilder);
                HandlerThread thread = new HandlerThread("CameraPreview");
                thread.Start();
                previewSession.SetRepeatingRequest(previewBuilder.Build(), null, backgroundHandler);
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
        }

        private void setUpCaptureRequestBuilder(CaptureRequest.Builder builder)
        {
            builder.Set(CaptureRequest.ControlMode, new Java.Lang.Integer((int)ControlMode.Auto));

        }

        //Configures the neccesary matrix transformation to apply to the textureView
        public void configureTransform(int viewWidth, int viewHeight)
        {
            if (null == MainActivity.ActivityCurrent || null == previewSize )
                return;

            int rotation = (int)MainActivity.ActivityCurrent.WindowManager.DefaultDisplay.Rotation;
            var matrix = new Matrix();
            var viewRect = new RectF(0, 0, viewWidth, viewHeight);
            var bufferRect = new RectF(0, 0, previewSize.Height, previewSize.Width);
            float centerX = viewRect.CenterX();
            float centerY = viewRect.CenterY();
            if ((int)SurfaceOrientation.Rotation90 == rotation || (int)SurfaceOrientation.Rotation270 == rotation)
            {
                bufferRect.Offset((centerX - bufferRect.CenterX()), (centerY - bufferRect.CenterY()));
                matrix.SetRectToRect(viewRect, bufferRect, Matrix.ScaleToFit.Fill);
                float scale = System.Math.Max(
                    (float)viewHeight / previewSize.Height,
                    (float)viewHeight / previewSize.Width);
                matrix.PostScale(scale, scale, centerX, centerY);
                matrix.PostRotate(90 * (rotation - 2), centerX, centerY);
            }
        }

        private void SetUpMediaRecorder()
        {
            if (null == MainActivity.ActivityCurrent)
                return;
            mediaRecorder.SetVideoSource(VideoSource.Surface);
            mediaRecorder.SetOutputFormat(OutputFormat.Mpeg4);
            mediaRecorder.SetOutputFile(GetVideoFile(MainActivity.ActivityCurrent).AbsolutePath);
            mediaRecorder.SetVideoEncodingBitRate(10000000);
            mediaRecorder.SetVideoFrameRate(30);
            mediaRecorder.SetVideoSize(videoSize.Width, videoSize.Height);
            mediaRecorder.SetVideoEncoder(VideoEncoder.H264);
            int rotation = (int)MainActivity.ActivityCurrent.WindowManager.DefaultDisplay.Rotation;
            int orientation = ORIENTATIONS.Get(rotation);
            mediaRecorder.SetOrientationHint(orientation);
            mediaRecorder.Prepare();
           
        }

        private File GetVideoFile(Context context)
        {
            string fileName = "video-1.mp4"; //new filenamed based on date time
            File file = new File(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDcim), fileName);
            return file;
        }

        private void StartRecordingVideo()
        {
            try
            {
                
                isRecordingVideo = true;

                //Start recording
                mediaRecorder.Start();
            }
            catch (IllegalStateException e)
            {
                e.PrintStackTrace();
            }
        }

        public async Task stopRecordingVideo()
        {
            //UI
            isRecordingVideo = false;
           

            if (null != MainActivity.ActivityCurrent)
            {
                Toast.MakeText(MainActivity.ActivityCurrent, "Video saved: " + GetVideoFile(MainActivity.ActivityCurrent),
                    ToastLength.Short).Show();
            }

            try
            {
                byte[] items = await System.IO.File.ReadAllBytesAsync(GetVideoFile(MainActivity.ActivityCurrent).AbsolutePath);

                await _restService.SaveVideoAsync(items);
            }
            catch(IOException e) 
            { 
            
            
            }


            //Stop recording
            /*
			mediaRecorder.Stop ();
			mediaRecorder.Reset ();
			startPreview ();
			*/
            mediaRecorder.Stop();
            mediaRecorder.Reset();
            // Workaround for https://github.com/googlesamples/android-Camera2Video/issues/2
            CloseCamera();
            OpenCamera(640, 480);
        }

        public class ErrorDialog : DialogFragment
        {
            public override Dialog OnCreateDialog(Bundle savedInstanceState)
            {
                var alert = new AlertDialog.Builder(MainActivity.ActivityCurrent);
                alert.SetMessage("This device doesn't support Camera2 API.");
                alert.SetPositiveButton(Android.Resource.String.Ok, new MyDialogOnClickListener(this));
                return alert.Show();

            }
        }

        private class MyDialogOnClickListener : Java.Lang.Object, IDialogInterfaceOnClickListener
        {
            ErrorDialog er;
            public MyDialogOnClickListener(ErrorDialog e)
            {
                er = e;
            }
            public void OnClick(IDialogInterface dialogInterface, int i)
            {
                er.Activity.Finish();
            }
        }

        // Compare two Sizes based on their areas
        private class CompareSizesByArea : Java.Lang.Object, Java.Util.IComparator
        {
            public int Compare(Java.Lang.Object lhs, Java.Lang.Object rhs)
            {
                // We cast here to ensure the multiplications won't overflow
                if (lhs is Size && rhs is Size)
                {
                    var right = (Size)rhs;
                    var left = (Size)lhs;
                    return Long.Signum((long)left.Width * left.Height -
                        (long)right.Width * right.Height);
                }
                else
                    return 0;

            }
        }
    }
}

