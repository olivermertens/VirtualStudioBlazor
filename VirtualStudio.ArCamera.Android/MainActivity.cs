using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VirtualStudio.Shared;
using VirtualStudio.Shared.Abstractions;
using VirtualStudio.Shared.Connection;
using VirtualStudio.Shared.DTOs;
using VirtualStudio.Shared.DTOs.WebRtc;
using Xam.WebRtc.Android;
using Google.AR.Core;
using Google.AR.Core.Exceptions;
using Android.Widget;
using Android.Opengl;
using Javax.Microedition.Khronos.Opengles;
using Android.Util;
using Android.Views;
using System.Collections.Concurrent;
using Google.Android.Material.Snackbar;
using Android.Graphics;
using static Android.Views.View;
using Java.Interop;
using Android.Hardware.Camera2;
using Android.Media;
using System.Linq;
using Java.Lang;

namespace VirtualStudio.ArCamera
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : AppCompatActivity, IWebRtcObserver, IWebRtcClient, GLSurfaceView.IRenderer, IOnTouchListener, IImageSource
    {
        const string TAG = "VIRTUALSTUDIO_ARCAMERA";
        SurfaceViewRenderer localVideoSurface;
        SurfaceViewRenderer remoteVideoSurface;
        SignalRVirtualStudioConnection virtualStudioConnection;
        private readonly string virtualStudioName = "vs";
        private WebRtcClient webRtcClient;
        private int connectionId;

        private Android.Util.Size targetResolution = new Android.Util.Size(1920, 1080);

        Snackbar loadingMessageSnackbar = null;
        GestureDetector gestureDetector;
        DisplayRotationHelper displayRotationHelper;
        private Session arSession;

        private GLSurfaceView glSurfaceView;

        private BackgroundRenderer backgroundRenderer = new BackgroundRenderer();
        private ObjectRenderer virtualObject = new ObjectRenderer();
        private ObjectRenderer virtualObjectShadow = new ObjectRenderer();
        private PlaneRenderer planeRenderer = new PlaneRenderer();
        private PointCloudRenderer pointCloudRenderer = new PointCloudRenderer();

        private CameraDevice cameraDevice;

        static float[] anchorMatrix = new float[16];
        ConcurrentQueue<MotionEvent> queuedSingleTaps = new ConcurrentQueue<MotionEvent>();
        List<Anchor> anchors = new List<Anchor>();

        public event EventHandler<Image> ImageAvailable;
        public event EventHandler<VideoFrame.II420Buffer> VideoFrameAvailable;
        public event EventHandler<int> Texture2dAvailable;

        private EGLConfig androidEglConfig;

        private bool doCaptureCameraFrame = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            StartArSession();
            SetupGlSurfaceView();
            localVideoSurface = FindViewById<SurfaceViewRenderer>(Resource.Id.local_video_surface);
            remoteVideoSurface = FindViewById<SurfaceViewRenderer>(Resource.Id.remote_video_surface);
        }

        private void SetupGlSurfaceView()
        {
            glSurfaceView = FindViewById<GLSurfaceView>(Resource.Id.surfaceview);
            glSurfaceView.PreserveEGLContextOnPause = true;
            glSurfaceView.SetEGLContextClientVersion(2);
            glSurfaceView.SetEGLConfigChooser(8, 8, 8, 8, 16, 0);
            glSurfaceView.SetOnTouchListener(this);
            glSurfaceView.SetRenderer(this);
        }

        private void StartArSession()
        {
            displayRotationHelper = new DisplayRotationHelper(this);
            string message = null;
            Java.Lang.Exception exception = null;
            arSession = null;
            try
            {
                arSession = new Session(/*context=*/this);

            }
            catch (UnavailableArcoreNotInstalledException e)
            {
                message = "Please install ARCore";
                exception = e;
            }
            catch (UnavailableApkTooOldException e)
            {
                message = "Please update ARCore";
                exception = e;
            }
            catch (UnavailableSdkTooOldException e)
            {
                message = "Please update this app";
                exception = e;
            }
            catch (CameraAccessException e)
            {
                message = "CameraAccessException";
                exception = e;
            }
            catch (Java.Lang.Exception e)
            {
                exception = e;
                message = "This device does not support AR";
            }

            if (message != null)
            {
                Toast.MakeText(this, message, ToastLength.Long).Show();
                return;
            }

            var config = new Google.AR.Core.Config(arSession);
            //var cameraConfigs = arSession.GetSupportedCameraConfigs(new CameraConfigFilter(arSession));
            //arSession.CameraConfig = cameraConfigs.First(c => c.ImageSize.Height == 1080);
            if (!arSession.IsSupported(config))
            {
                Toast.MakeText(this, "This device does not support AR", ToastLength.Long).Show();
                Finish();
                return;
            }

            arSession.Configure(config);

            gestureDetector = new GestureDetector(this, new SimpleTapGestureDetector
            {
                SingleTapUpHandler = (args) =>
                {
                    OnSingleTap(args);
                    return true;
                },
                DownHandler = _ => true
            });
        }

        protected override void OnResume()
        {
            base.OnResume();

            // ARCore requires camera permissions to operate. If we did not yet obtain runtime
            // permission on Android M and above, now is a good time to ask the user for it.
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == Android.Content.PM.Permission.Granted)
            {
                if (arSession != null)
                {
                    ShowLoadingMessage();
                    // Note that order matters - see the note in onPause(), the reverse applies here.
                    arSession.Resume();
                }

                displayRotationHelper.OnResume();
                glSurfaceView.OnResume();

                if (webRtcClient is null)
                {

                    webRtcClient = new WebRtcClient(this, remoteVideoSurface, localVideoSurface, this, this);
                }

                _ = ConnectToServer();
            }
            else
            {
                ActivityCompat.RequestPermissions(this, new string[] { Android.Manifest.Permission.Camera }, 1);
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.Camera) == Android.Content.PM.Permission.Granted)
            {

                // Note that the order matters - GLSurfaceView is paused first so that it does not try
                // to query the session. If Session is paused before GLSurfaceView, GLSurfaceView may
                // still call mSession.update() and get a SessionPausedException.
                glSurfaceView.OnPause();
                displayRotationHelper.OnPause();
                if (arSession != null)
                    arSession.Pause();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.Camera) != Android.Content.PM.Permission.Granted)
            {
                Toast.MakeText(this, "Camera permission is needed to run this application", ToastLength.Long).Show();
                Finish();
            }
        }

        private void Init()
        {

        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);

            if (hasFocus)
            {
                // Standard Android full-screen functionality.
                //Window.DecorView.SystemUiVisibility = Android.Views.SystemUiFlags.LayoutStable
                //| Android.Views.SystemUiFlags.LayoutHideNavigation
                //| Android.Views.SystemUiFlags.LayoutFullscreen
                //| Android.Views.SystemUiFlags.HideNavigation
                //| Android.Views.SystemUiFlags.Fullscreen
                //| Android.Views.SystemUiFlags.ImmersiveSticky;

                Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            }
        }

        async Task ConnectToServer()
        {
            virtualStudioConnection = await SignalRVirtualStudioConnection.CreateAsync("192.168.2.109", 5001, true);
            virtualStudioConnection.AddListener(this);

            StudioComponentDto studioComponentDto = new StudioComponentDto
            {
                Name = "AR Camera",
                IsPlaceholder = false,
                Inputs = new List<StudioComponentEndpointDto>
                {
                    new StudioComponentEndpointDto(){IOType = EndpointIOType.Input, ConnectionType = "WebRTC", DataKind = DataKind.Video, Id = 2, Name = "Feedback"}
                },
                Outputs = new List<StudioComponentEndpointDto>
                {
                    new StudioComponentEndpointDto(){IOType = EndpointIOType.Output, ConnectionType = "WebRTC", DataKind = DataKind.VideoAudio, Id = 1, Name = "Video"}
                }
            };

            bool success = await virtualStudioConnection.JoinVirtualStudio(virtualStudioName, studioComponentDto);

            if (success)
                Console.WriteLine($"Joined '{virtualStudioName}' successfully.");
            else
                Console.WriteLine($"Joining '{virtualStudioName}' failed.");
        }

        #region IWebRtcClient
        public Task RequestSdpOffer(SdpOfferRequestArgs args)
        {
            connectionId = args.ConnectionId;

            webRtcClient.Connect((sdpOffer, error) =>
            {
                virtualStudioConnection.SendSdpOffer(new SdpOfferResponseArgs
                {
                    ConnectionId = args.ConnectionId,
                    SdpOffer = sdpOffer.Description,
                    SupportsInsertableStreams = false
                });
            });
            return Task.CompletedTask;
        }

        public Task RequestSdpAnswer(SdpAnswerRequestArgs args)
        {
            connectionId = args.ConnectionId;

            webRtcClient.ReceiveOffer(new SessionDescription(SessionDescription.SdpType.Offer, args.SdpOffer), (sdpAnswer, error) =>
            {
                virtualStudioConnection.SendSdpAnswer(new SdpAnswerResponseArgs
                {
                    ConnectionId = args.ConnectionId,
                    SdpAnswer = sdpAnswer.Description,
                    UseInsertableStreams = false
                });
            });
            return Task.CompletedTask;
        }

        public Task AddIceCandidate(IceCandidateArgs args)
        {
            webRtcClient.ReceiveCandidate(new IceCandidate(args.CandidateDto.SdpMid, args.CandidateDto.SdpMLineIndex, args.CandidateDto.Candidate));
            return Task.CompletedTask;
        }

        public Task Connect(ConnectWebRtcCommandArgs args)
        {
            webRtcClient.ReceiveAnswer(new SessionDescription(SessionDescription.SdpType.Answer, args.SdpAnswer), (sdp, error) => { });
            return Task.CompletedTask;
        }

        public Task Disconnect(DisconnectWebRtcCommandArgs args)
        {
            webRtcClient.Disconnect();
            return Task.CompletedTask;
        }

        #endregion

        #region IWebRtcObserver
        public void OnGenerateCandidate(IceCandidate iceCandidate)
        {
            Console.WriteLine($"{nameof(OnGenerateCandidate)}: {iceCandidate.Sdp}");
            virtualStudioConnection.SendIceCandidate(new IceCandidateArgs
            {
                ConnectionId = connectionId,
                CandidateDto = new RtcIceCandidateDto
                {
                    Candidate = iceCandidate.Sdp,
                    SdpMid = iceCandidate.SdpMid,
                    SdpMLineIndex = iceCandidate.SdpMLineIndex
                }
            });
        }

        public void OnIceConnectionStateChanged(PeerConnection.IceConnectionState iceConnectionState)
        {
            Console.WriteLine($"{nameof(OnIceConnectionStateChanged)}: {iceConnectionState.Name()}");
        }

        public void OnOpenDataChannel()
        {
            Console.WriteLine(nameof(OnOpenDataChannel));
        }

        public void OnReceiveData(byte[] data)
        {
            Console.WriteLine($"{nameof(OnReceiveMessage)}: byte[{data.Length}]");
        }

        public void OnReceiveMessage(string message)
        {
            Console.WriteLine($"{nameof(OnReceiveMessage)}: {message}");
        }

        public void OnConnectWebRtc()
        {
            Console.WriteLine(nameof(OnConnectWebRtc));
        }

        public void OnDisconnectWebRtc()
        {
            Console.WriteLine(nameof(OnDisconnectWebRtc));
        }

        #endregion

        #region Renderer
        YuvConverter yuvConverter = new YuvConverter();
        int renderTextureId;
        int fboId;
        Android.Util.Size textureSize;

        public void OnSurfaceCreated(IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
        {
            GLES20.GlClearColor(0.9f, 0.1f, 0.1f, 1.0f);
            // GLES20.GlViewport(0, 0, glSurfaceView.Width, glSurfaceView.Height);

            textureSize = arSession.CameraConfig.TextureSize;
            arSession.SetDisplayGeometry(1, targetResolution.Width, targetResolution.Height);

            int[] glObjs = new int[1];
            GLES20.GlGenFramebuffers(1, glObjs, 0);
            fboId = glObjs[0];
            GLES20.GlBindFramebuffer(GLES20.GlFramebuffer, fboId);
            GLES20.GlViewport(0, 0, targetResolution.Width, targetResolution.Height);
            GLES20.GlGenTextures(1, glObjs, 0);
            renderTextureId = glObjs[0]; ;
            GLES20.GlBindTexture(GLES20.GlTexture2d, renderTextureId);
            GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureWrapS, GLES20.GlClampToEdge);
            GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureWrapT, GLES20.GlClampToEdge);
            GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMinFilter, GLES20.GlNearest);
            GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMagFilter, GLES20.GlNearest);
            GLES20.GlTexImage2D(GLES20.GlTexture2d, 0, GLES20.GlRgba, targetResolution.Width, targetResolution.Height, 0, GLES20.GlRgba, GLES20.GlUnsignedByte, null);

            GLES20.GlBindTexture(GLES20.GlTexture2d, 0);
            GLES20.GlFramebufferTexture2D(GLES20.GlFramebuffer, GLES20.GlColorAttachment0, GLES20.GlTexture2d, renderTextureId, 0);
            GLES20.GlBindFramebuffer(GLES20.GlFramebuffer, 0);

            GlUtil.CheckNoGLES2Error("Create render texture.");

            // Create the texture and pass it to ARCore session to be filled during update().
            backgroundRenderer.CreateOnGlThread(/*context=*/this);
            if (arSession != null)
                arSession.SetCameraTextureName(BackgroundRenderer.TextureId);


            // Prepare the other rendering objects.
            try
            {
                virtualObject.CreateOnGlThread(/*context=*/this, "andy.obj", "andy.png");
                virtualObject.setMaterialProperties(0.0f, 3.5f, 1.0f, 6.0f);

                virtualObjectShadow.CreateOnGlThread(/*context=*/this,
                        "andy_shadow.obj", "andy_shadow.png");
                virtualObjectShadow.SetBlendMode(ObjectRenderer.BlendMode.Shadow);
                virtualObjectShadow.setMaterialProperties(1.0f, 0.0f, 0.0f, 1.0f);
            }
            catch (Java.IO.IOException e)
            {
                Log.Error(TAG, "Failed to read obj file");
            }

            try
            {
                planeRenderer.CreateOnGlThread(/*context=*/this, "trigrid.png");
            }
            catch (Java.IO.IOException e)
            {
                Log.Error(TAG, "Failed to read plane texture");
            }
            pointCloudRenderer.CreateOnGlThread(/*context=*/this);
        }

        public void OnSurfaceChanged(IGL10 gl, int width, int height)
        {
            displayRotationHelper.OnSurfaceChanged(width, height);
            GLES20.GlViewport(0, 0, width, height);
        }

        public void OnDrawFrame(IGL10 gl)
        {
            // Clear screen to notify driver it should not load any pixels from previous frame.
            GLES20.GlBindFramebuffer(GLES20.GlFramebuffer, 0);
            GLES20.GlViewport(0, 0, glSurfaceView.Width, glSurfaceView.Height);
            GLES20.GlClear(GLES20.GlColorBufferBit | GLES20.GlDepthBufferBit);

            if (arSession == null)
                return;

            // Notify ARCore session that the view size changed so that the perspective matrix and the video background
            // can be properly adjusted
            // displayRotationHelper.UpdateSessionIfNeeded(arSession);

            try
            {
                // Obtain the current frame from ARSession. When the configuration is set to
                // UpdateMode.BLOCKING (it is by default), this will throttle the rendering to the
                // camera framerate.
                Frame frame = arSession.Update();
                Google.AR.Core.Camera camera = frame.Camera;


                // Draw background.
                GLES20.GlViewport(0, 0, glSurfaceView.Width, glSurfaceView.Height);
                backgroundRenderer.Draw(frame);

                GLES20.GlBindFramebuffer(GLES20.GlFramebuffer, fboId);
                GLES20.GlViewport(0, 0, targetResolution.Width, targetResolution.Height);
                GLES20.GlClear(GLES20.GlColorBufferBit | GLES20.GlDepthBufferBit);
                backgroundRenderer.Draw(frame);
                GLES20.GlBindFramebuffer(GLES20.GlFramebuffer, 0);
                GLES20.GlViewport(0, 0, glSurfaceView.Width, glSurfaceView.Height);
                GlUtil.CheckNoGLES2Error("Switch framebuffers.");

                // Handle taps. Handling only one tap per frame, as taps are usually low frequency
                // compared to frame rate.
                MotionEvent tap = null;
                queuedSingleTaps.TryDequeue(out tap);

                if (tap != null && camera.TrackingState == TrackingState.Tracking)
                {
                    foreach (var hit in frame.HitTest(tap))
                    {
                        var trackable = hit.Trackable;

                        // Check if any plane was hit, and if it was hit inside the plane polygon.
                        if (trackable is Plane && ((Plane)trackable).IsPoseInPolygon(hit.HitPose))
                        {
                            // Cap the number of objects created. This avoids overloading both the
                            // rendering system and ARCore.
                            if (anchors.Count >= 16)
                            {
                                anchors[0].Detach();
                                anchors.RemoveAt(0);
                            }
                            // Adding an Anchor tells ARCore that it should track this position in
                            // space.  This anchor is created on the Plane to place the 3d model
                            // in the correct position relative to both the world and to the plane
                            anchors.Add(hit.CreateAnchor());

                            // Hits are sorted by depth. Consider only closest hit on a plane.
                            break;
                        }
                    }
                }

                // If not tracking, don't draw 3d objects.
                if (camera.TrackingState == TrackingState.Paused)
                    return;

                // Get projection matrix.
                float[] projmtx = new float[16];
                camera.GetProjectionMatrix(projmtx, 0, 0.1f, 100.0f);

                // Get camera matrix and draw.
                float[] viewmtx = new float[16];
                camera.GetViewMatrix(viewmtx, 0);

                // Compute lighting from average intensity of the image.
                var lightIntensity = frame.LightEstimate.PixelIntensity;

                // Visualize tracked points.
                var pointCloud = frame.AcquirePointCloud();
                pointCloudRenderer.Update(pointCloud);

                // App is repsonsible for releasing point cloud resources after using it
                pointCloud.Release();

                var planes = new List<Plane>();
                foreach (var p in arSession.GetAllTrackables(Java.Lang.Class.FromType(typeof(Plane))))
                {
                    var plane = (Plane)p;
                    planes.Add(plane);
                }

                // Check if we detected at least one plane. If so, hide the loading message.
                if (loadingMessageSnackbar != null)
                {
                    foreach (var plane in planes)
                    {
                        if (plane.GetType() == Plane.Type.HorizontalUpwardFacing
                                && plane.TrackingState == TrackingState.Tracking)
                        {
                            HideLoadingMessage();
                            break;
                        }
                    }
                }

                Draw(frame, camera, projmtx, viewmtx, lightIntensity, planes);

                
                GLES20.GlBindFramebuffer(GLES20.GlFramebuffer, fboId);
                GLES20.GlViewport(0, 0, targetResolution.Width, targetResolution.Height);
                // Restore the depth state for further drawing.
                GLES20.GlDepthMask(true);
                GLES20.GlEnable(GLES20.GlDepthTest);
                // Draw(frame, camera, projmtx, viewmtx, lightIntensity, planes);
                DrawModels(projmtx, viewmtx, lightIntensity);


                if (doCaptureCameraFrame)
                {
                    var textureBuffer = new TextureBufferImpl(targetResolution.Width, targetResolution.Height, VideoFrame.TextureBufferType.Rgb, renderTextureId, new Android.Graphics.Matrix(), null, null, null);
                    var i420Buffer = yuvConverter.Convert(textureBuffer);
                    VideoFrameAvailable?.Invoke(this, i420Buffer);
                }

            }
            catch (System.Exception ex)
            {
                // Avoid crashing the application due to unhandled exceptions.
                Log.Error(TAG, "Exception on the OpenGL thread", ex);
            }
        }

        private void Draw(Frame frame, Google.AR.Core.Camera camera, float[] projmtx, float[] viewmtx, float lightIntensity, List<Plane> planes)
        {
            // Draw pointcloud.
            pointCloudRenderer.Draw(camera.DisplayOrientedPose, viewmtx, projmtx);

            // Visualize planes.
            planeRenderer.DrawPlanes(planes, camera.DisplayOrientedPose, projmtx);

            // Visualize anchors created by touch.
            DrawModels(projmtx, viewmtx, lightIntensity);
        }

        private void DrawModels(float[] projmtx, float[] viewmtx, float lightIntensity)
        {
            // Visualize anchors created by touch.
            float scaleFactor = 1.0f;
            foreach (var anchor in anchors)
            {
                if (anchor.TrackingState != TrackingState.Tracking)
                    continue;

                // Get the current combined pose of an Anchor and Plane in world space. The Anchor
                // and Plane poses are updated during calls to session.update() as ARCore refines
                // its estimate of the world.
                anchor.Pose.ToMatrix(anchorMatrix, 0);

                // Update and draw the model and its shadow.
                virtualObject.updateModelMatrix(anchorMatrix, scaleFactor);
                virtualObjectShadow.updateModelMatrix(anchorMatrix, scaleFactor);
                virtualObject.Draw(viewmtx, projmtx, lightIntensity);
                virtualObjectShadow.Draw(viewmtx, projmtx, lightIntensity);
            }
        }

        private void ShowLoadingMessage()
        {
            this.RunOnUiThread(() =>
            {
                loadingMessageSnackbar = Snackbar.Make(FindViewById(Android.Resource.Id.Content),
                        "Searching for surfaces...", Snackbar.LengthIndefinite);
                loadingMessageSnackbar.View.SetBackgroundColor(Color.DarkGray);
                loadingMessageSnackbar.Show();
            });
        }

        private void HideLoadingMessage()
        {
            this.RunOnUiThread(() =>
            {
                loadingMessageSnackbar?.Dismiss();
                loadingMessageSnackbar = null;
            });

        }

        public bool OnTouch(View v, MotionEvent e)
        {
            return gestureDetector.OnTouchEvent(e);
        }

        private void OnSingleTap(MotionEvent e)
        {
            // Queue tap if there is space. Tap is lost if queue is full.
            if (queuedSingleTaps.Count < 16)
                queuedSingleTaps.Enqueue(e);
        }

        void IImageSource.Start()
        {
            doCaptureCameraFrame = true;
        }

        void IImageSource.Stop()
        {
            doCaptureCameraFrame = false;
        }
        #endregion
    }

    class SimpleTapGestureDetector : GestureDetector.SimpleOnGestureListener
    {
        public Func<MotionEvent, bool> SingleTapUpHandler { get; set; }

        public override bool OnSingleTapUp(MotionEvent e)
        {
            return SingleTapUpHandler?.Invoke(e) ?? false;
        }

        public Func<MotionEvent, bool> DownHandler { get; set; }

        public override bool OnDown(MotionEvent e)
        {
            return DownHandler?.Invoke(e) ?? false;
        }
    }

    public class PixelCopyFinishedListener : Java.Lang.Object, PixelCopy.IOnPixelCopyFinishedListener
    {
        private Action<int> handler;

        public PixelCopyFinishedListener(Action<int> pixelCopyFinishedHandler)
        {
            handler = pixelCopyFinishedHandler;
        }

        public void OnPixelCopyFinished(int copyResult)
        {
            handler(copyResult);
        }

        public JniManagedPeerStates JniManagedPeerState => throw new NotImplementedException();
        public void Disposed() { }
        public void DisposeUnlessReferenced() { }
        public void Finalized() { }
        public void SetJniIdentityHashCode(int value) { }
        public void SetJniManagedPeerState(JniManagedPeerStates value) { }
        public void SetPeerReference(JniObjectReference reference) { }
    }
}