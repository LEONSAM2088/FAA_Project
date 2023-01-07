using System;
using Android.Hardware.Camera2;
using Android.Widget;
using FAA_Project;
using FAA_Project.Data;

namespace Camera2VideoSample
{
	public class PreviewCaptureStateCallback: CameraCaptureSession.StateCallback
	{
        CameraService fragment;
		public PreviewCaptureStateCallback(CameraService frag)
		{
			fragment = frag;
		}
		public override void OnConfigured (CameraCaptureSession session)
		{
			
			

		}

		public override void OnConfigureFailed (CameraCaptureSession session)
		{
			
			if (null != MainActivity.ActivityCurrent) 
				Toast.MakeText (MainActivity.ActivityCurrent, "Failed", ToastLength.Short).Show ();
		}
	}
}

