/*using System;
using Android.Views;
using Android.Graphics;
using FAA_Project.Data;

namespace Camera2VideoSample
{
	public class MySurfaceTextureListener: Java.Lang.Object,TextureView.ISurfaceTextureListener
	{
        CameraService fragment;
		public MySurfaceTextureListener(CameraService frag)
		{
			fragment = frag;
		}

		public void OnSurfaceTextureAvailable(SurfaceTexture surface_texture,int width, int height)
		{
			fragment.OpenCamera (width,height);
		}

		public void OnSurfaceTextureSizeChanged(SurfaceTexture surface_texture, int width, int height)
		{
			fragment.ConfigureTransform (width, height);
		}

		public bool OnSurfaceTextureDestroyed(SurfaceTexture surface_texture)
		{
			return true;
		}

		public void OnSurfaceTextureUpdated(SurfaceTexture surface_texture)
		{
		}

	}
}

*/