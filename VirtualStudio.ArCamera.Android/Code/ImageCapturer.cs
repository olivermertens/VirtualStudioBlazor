using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.Opengl;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Interop;
using Java.Nio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xam.WebRtc.Android;

namespace VirtualStudio.ArCamera
{
    public interface IImageSource
    {
        event EventHandler<Image> ImageAvailable;
        event EventHandler<VideoFrame.II420Buffer> VideoFrameAvailable;
        event EventHandler<int> Texture2dAvailable;

        void Start();
        void Stop();
    }

    public class ImageCapturer : Java.Lang.Object, IVideoCapturer
    {
        public bool IsScreencast => throw new NotImplementedException();
        public JniManagedPeerStates JniManagedPeerState => throw new NotImplementedException();

        private readonly SurfaceTextureHelper textureHelper;
        private readonly ICapturerObserver capturerObserver;
        private readonly IImageSource imageSource;

        private TextureBufferImpl buffer;
        private DateTime startTime;
        private int textureId;

        private Bitmap testBitmap;
        private int width;
        private int height;
        private YuvConverter yuvConverter;

        public ImageCapturer(SurfaceTextureHelper textureHelper, ICapturerObserver capturerObserver, IImageSource imageSource)
        {
            this.textureHelper = textureHelper ?? throw new ArgumentNullException(nameof(textureHelper));
            this.capturerObserver = capturerObserver ?? throw new ArgumentNullException(nameof(capturerObserver));
            this.imageSource = imageSource ?? throw new ArgumentNullException(nameof(imageSource));
            this.yuvConverter = new YuvConverter();
        }

        public void StartCapture(int width, int height, int fps)
        {
            this.width = width;
            this.height = height;

            textureHelper.SetTextureSize(width, height);

            int[] textures = new int[1];
            GLES20.GlGenTextures(1, textures, 0);
            textureId = textures[0];
            GLES20.GlBindTexture(GLES20.GlTexture2d, textureId);
            GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMinFilter, GLES20.GlNearest);
            GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMagFilter, GLES20.GlNearest);

            //var matrix = new Android.Graphics.Matrix();
            //matrix.PreTranslate(0.5f, 0.5f);
            //matrix.PreScale(-1f, -1f);
            //matrix.PreTranslate(-0.5f, -0.5f);

            //YuvConverter yuvConverter = new YuvConverter();

            //buffer = new TextureBufferImpl(
            //    width,
            //    height,
            //    VideoFrame.TextureBufferType.Oes,
            //    1,
            //    matrix,
            //    textureHelper.Handler,
            //    yuvConverter,
            //    null);

            //capturerObserver.OnCapturerStarted(true);
            startTime = DateTime.Now;

            //imageSource.ImageAvailable += ImageSource_ImageAvailable;
            imageSource.VideoFrameAvailable += ImageSource_VideoFrameAvailable;
            //imageSource.Texture2dAvailable += ImageSource_Texture2dAvailable;
            imageSource.Start();
        }

        private void ImageSource_VideoFrameAvailable(object sender, VideoFrame.II420Buffer buffer)
        {
            long timestampNs = (DateTime.Now - startTime).Ticks * 100;
            var videoFrame = new VideoFrame(buffer, 0, timestampNs);
            capturerObserver.OnFrameCaptured(videoFrame);
            videoFrame.Release();
        }

        private void ImageSource_ImageAvailable(object sender, Image image)
        {
            try
            {
                var buffer = new TextureBufferImpl(width, height, VideoFrame.TextureBufferType.Oes, BackgroundRenderer.TextureId, new Android.Graphics.Matrix(), textureHelper.Handler, new YuvConverter(), null);
                long timestampNs = (DateTime.Now - startTime).Ticks * 100;
                var i420buffer = yuvConverter.Convert(buffer);
                var videoFrame = new VideoFrame(i420buffer, 0, timestampNs);

                //var frameBuffer = new ImageI420Buffer(image);
                //var videoFrame = new VideoFrame(frameBuffer, 0, timestampNs);
                capturerObserver.OnFrameCaptured(videoFrame);
                videoFrame.Release();
                image?.Close();
            }
            catch (Exception ex)
            {
            }
        }

        public void StopCapture()
        {
            imageSource.Stop();
            imageSource.ImageAvailable -= ImageSource_ImageAvailable;
            capturerObserver.OnCapturerStopped();
        }

        public void ChangeCaptureFormat(int p0, int p1, int p2)
        {

        }

        public void Disposed()
        {

        }

        public void DisposeUnlessReferenced()
        {

        }

        public void Finalized()
        {

        }

        public void Initialize(SurfaceTextureHelper p0, Context p1, ICapturerObserver p2)
        {

        }

        public void SetJniIdentityHashCode(int value)
        {

        }

        public void SetJniManagedPeerState(JniManagedPeerStates value)
        {

        }

        public void SetPeerReference(JniObjectReference reference)
        {

        }
    }

    class ImageI420Buffer : Java.Lang.Object, VideoFrame.II420Buffer
    {
        public int Height => javaI420Buffer.Height;
        public int Width => javaI420Buffer.Width;
        public ByteBuffer DataY => javaI420Buffer.DataY;
        public ByteBuffer DataU => javaI420Buffer.DataU;
        public ByteBuffer DataV => javaI420Buffer.DataV;
        public int StrideY => javaI420Buffer.StrideY;
        public int StrideU => javaI420Buffer.StrideU;
        public int StrideV => javaI420Buffer.StrideV;

        private readonly Image image;
        JavaI420Buffer javaI420Buffer;

        public ImageI420Buffer(Image image)
        {
            if (image.Format != ImageFormatType.Yuv420888)
                throw new ArgumentException("Wrong image format", nameof(image));

            this.image = image;
            var height = image.Height;
            var width = image.Width;
            var imagePlanes = image.GetPlanes();
            var dataY = imagePlanes[0].Buffer;
            var dataU = imagePlanes[1].Buffer;
            var dataV = imagePlanes[2].Buffer;
            var strideY = imagePlanes[0].RowStride;
            var strideU = imagePlanes[1].RowStride;
            var strideV = imagePlanes[2].RowStride;

            javaI420Buffer = JavaI420Buffer.Allocate(width, height);
            //byte[] dataUArray = new byte[dataU.Remaining()];
            //byte[] dataVArray = new byte[dataV.Remaining()];
            //dataU.Get(dataUArray);
            //dataV.Get(dataVArray);
            javaI420Buffer.DataY.Put(dataY);
            //var planeLength = javaI420Buffer.DataV.Remaining();
            //for (int i = 0; i < planeLength; i++)
            //{
            //    javaI420Buffer.DataV.Put(dataV.Get(i * 2));
            //    javaI420Buffer.DataU.Put(dataU.Get(i * 2));
            //}

            //javaI420Buffer.Release();
            //javaI420Buffer = JavaI420Buffer.Wrap(width, height, dataY, strideY, dataU, strideU, dataV, strideV, null);
        }

        public VideoFrame.IBuffer CropAndScale(int cropX, int cropY, int cropWidth, int cropHeight, int scaleWidth, int scaleHeight)
        {
            return javaI420Buffer.CropAndScale(cropX, cropY, cropWidth, cropHeight, scaleWidth, scaleHeight);
        }

        public void Release()
        {
            image.Close();
            javaI420Buffer.Release();
        }

        public void Retain()
        {
            javaI420Buffer.Retain();
        }

        public VideoFrame.II420Buffer ToI420()
        {
            return this;
        }
    }
}