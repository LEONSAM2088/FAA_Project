using Android.App;
using Android.Hardware.Camera2;
using FAA_Project.Data;

namespace Camera2Basic.Listeners
{
    public class CameraStateListener : CameraDevice.StateCallback
    {
        private readonly CameraService owner;

        public CameraStateListener(CameraService owner)
        {
            if (owner == null)
                throw new System.ArgumentNullException("owner");
            this.owner = owner;
        }

        public override void OnOpened(CameraDevice cameraDevice)
        {
            
        }

        public override void OnDisconnected(CameraDevice cameraDevice)
        {
            
        }

        public override void OnError(CameraDevice cameraDevice, CameraError error)
        {
            
        }
    }
}