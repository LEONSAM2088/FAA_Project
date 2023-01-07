using System;
using Android.Hardware.Camera2;
using Android.Widget;
using FAA_Project;
using FAA_Project.Data;

namespace Camera2VideoSample
{
	public class MyCameraStateCallback : CameraDevice.StateCallback
	{
		CameraService fragment;
		public MyCameraStateCallback(CameraService frag)
		{
			fragment = frag;
		}
		public override void OnOpened (CameraDevice camera)
		{
			fragment.cameraDevice = camera;
			fragment.StartPreview ();
			fragment.cameraOpenCloseLock.Release ();
		}

		public override void OnDisconnected (CameraDevice camera)
		{
			fragment.cameraOpenCloseLock.Release ();
			camera.Close ();
			fragment.cameraDevice = null;
		}

		public override void OnError (CameraDevice camera, CameraError error)
		{
			fragment.cameraOpenCloseLock.Release ();
			camera.Close ();
			fragment.cameraDevice = null;
			if (null != MainActivity.ActivityCurrent)
                MainActivity.ActivityCurrent.Finish();
        }


	}
}

