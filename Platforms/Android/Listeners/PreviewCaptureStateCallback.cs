using System;
using Android.Hardware.Camera2;
using Android.Widget;
using FAA_Project;
using FAA_Project.Data;

namespace FAA_Project
{
	public class PreviewCaptureStateCallback: CameraCaptureSession.StateCallback
	{
        Camera2VideoFragment fragment;
        public PreviewCaptureStateCallback(Camera2VideoFragment frag)
        {
            fragment = frag;
        }
        public override void OnConfigured(CameraCaptureSession session)
        {
            fragment.previewSession = session;
            fragment.updatePreview();

        }

        public override void OnConfigureFailed(CameraCaptureSession session)
        {
            if (null != MainActivity.ActivityCurrent)
                Toast.MakeText(MainActivity.ActivityCurrent, "Failed", ToastLength.Short).Show();
        }
    }
}

