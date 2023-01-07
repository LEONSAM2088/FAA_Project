using System;
using Android.Hardware.Camera2;
using Android.Widget;
using FAA_Project;
using FAA_Project.Data;

namespace FAA_Project
{
	public class MyCameraStateCallback : CameraDevice.StateCallback
	{
        Camera2VideoFragment fragment;
        public MyCameraStateCallback(Camera2VideoFragment frag)
        {
            fragment = frag;
        }
        public override void OnOpened(CameraDevice camera)
        {
            fragment.cameraDevice = camera;
            fragment.startPreview();
            fragment.cameraOpenCloseLock.Release();
           
            fragment.configureTransform(640, 480);
        }

        public override void OnDisconnected(CameraDevice camera)
        {
            fragment.cameraOpenCloseLock.Release();
            camera.Close();
            fragment.cameraDevice = null;
        }

        public override void OnError(CameraDevice camera, CameraError error)
        {
            fragment.cameraOpenCloseLock.Release();
            camera.Close();
            fragment.cameraDevice = null;
            
        }

    }
}

