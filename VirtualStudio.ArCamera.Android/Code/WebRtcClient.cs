using Android.Content;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Opengl;
using Android.OS;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xam.WebRtc.Android;
using Xamarin.Essentials;

namespace VirtualStudio.ArCamera
{
    public interface IWebRtcObserver
    {
        void OnGenerateCandidate(IceCandidate iceCandidate);
        void OnIceConnectionStateChanged(PeerConnection.IceConnectionState iceConnectionState);
        void OnOpenDataChannel();
        void OnReceiveData(byte[] data);
        void OnReceiveMessage(string message);
        void OnConnectWebRtc();
        void OnDisconnectWebRtc();
    }

    public class WebRtcClient : Java.Lang.Object, PeerConnection.IObserver, DataChannel.IObserver
    {
        private readonly Context _context;

        private readonly IWebRtcObserver _observer;

        private readonly SurfaceViewRenderer _remoteView;
        private readonly SurfaceViewRenderer _localView;

        private readonly List<PeerConnection.IceServer> _iceServers;

        private IEglBase _eglBase;
        private PeerConnectionFactory _peerConnectionFactory;

        private VideoTrack _localVideoTrack;
        private AudioTrack _localAudioTrack;


        private int incomingConnectionId;
        private int outgoingConnectionId;
        private readonly object _outgoingConnectionLock = new object();
        private readonly object _incomingConnectionLock = new object();
        private PeerConnection _incomingPeerConnection;
        private PeerConnection _outgoingPeerConnection;
        private DataChannel _outgoingDataChannel;
        private DataChannel _incomingDataChannel;
        private int framebufferId;
        private int textureId;


        private (PeerConnection peer, DataChannel data) _incomingConnection
        {
            get
            {
                if (_incomingPeerConnection == null)
                {
                    lock (_incomingConnectionLock)
                    {
                        if (_incomingPeerConnection == null)
                        {
                            _incomingPeerConnection = SetupIncomingPeerConnection();
                        }
                    }
                }
                return (_incomingPeerConnection, _incomingDataChannel);
            }
        }

        private (PeerConnection peer, DataChannel data) _outgoingConnection
        {
            get
            {
                if (_outgoingPeerConnection == null)
                {
                    lock (_outgoingConnectionLock)
                    {
                        if (_outgoingPeerConnection == null)
                        {
                            _outgoingPeerConnection = SetupOutgoingPeerConnection();
                        }
                    }
                }

                return (_outgoingPeerConnection, _outgoingDataChannel);
            }
        }

        private bool _isConnected;

        public WebRtcClient(
            Context context,
            SurfaceViewRenderer remoteView,
            SurfaceViewRenderer localView,
            IWebRtcObserver observer,
            IImageSource imageSource)
        {
            _context = context;
            _remoteView = remoteView;
            _localView = localView;

            _observer = observer;

            _iceServers = new List<PeerConnection.IceServer>(1)
            {
                PeerConnection.IceServer
                .InvokeBuilder("stun:stun.l.google.com:19302")
                .CreateIceServer()
            };

            var options = PeerConnectionFactory.InitializationOptions
                .InvokeBuilder(_context)
                .CreateInitializationOptions();

            PeerConnectionFactory.Initialize(options);

            _eglBase = EglBase.Create();
            _peerConnectionFactory = PeerConnectionFactory.InvokeBuilder()
                .SetVideoDecoderFactory(new DefaultVideoDecoderFactory(_eglBase.EglBaseContext))
                .SetVideoEncoderFactory(new DefaultVideoEncoderFactory(_eglBase.EglBaseContext, true, true))
                .SetOptions(new PeerConnectionFactory.Options())
                .CreatePeerConnectionFactory();

            MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (_localView != null)
                    InitView(_localView);
                InitView(_remoteView);
            });

            //surfaceTexture.UpdateTexImage();

            var localVideoSource = _peerConnectionFactory.CreateVideoSource(false);
            string threadName = Thread.CurrentThread().Name;

            var surfaceTextureHelper = SurfaceTextureHelper.Create(threadName, _eglBase.EglBaseContext);

            var videoCapturer = new ImageCapturer(surfaceTextureHelper, localVideoSource.CapturerObserver, imageSource);

            videoCapturer.StartCapture(640, 480, 30);

            _localVideoTrack = _peerConnectionFactory.CreateVideoTrack("video0", localVideoSource);
            if (_localView != null)
                _localVideoTrack.AddSink(_localView);

            var localAudioSource = _peerConnectionFactory.CreateAudioSource(new MediaConstraints());
            _localAudioTrack = _peerConnectionFactory.CreateAudioTrack("audio0", localAudioSource);
        }


        public void Connect(int connectionId, Action<SessionDescription, string> completionHandler)
        {
            outgoingConnectionId = connectionId;
            _outgoingDataChannel = SetupOutgoingDataChannel();

            var mediaConstraints = new MediaConstraints();
            _outgoingConnection.peer.CreateOffer(SdpObserver.OnCreateSuccess((sdp) =>
            {
                _outgoingConnection.peer.SetLocalDescription(
                    SdpObserver.OnSet(() =>
                    {
                        completionHandler(sdp, string.Empty);
                    },
                    (string err) =>
                    {
                        completionHandler(null, err);
                    }),
                    sdp);

            }), mediaConstraints);
        }

        public void Disconnect(int connectionId)
        {
            if (connectionId == incomingConnectionId)
            {
                Disconnect(_incomingPeerConnection, _incomingDataChannel);
                _incomingPeerConnection = null;
                _incomingDataChannel = null;
                incomingConnectionId = 0;
            }
            else if (connectionId == outgoingConnectionId)
            {
                Disconnect(_outgoingPeerConnection, _outgoingDataChannel);
                _outgoingPeerConnection = null;
                _outgoingDataChannel = null;
                outgoingConnectionId = 0;
            }
        }

        private void Disconnect(PeerConnection peerConnection, DataChannel dataChannel)
        {
            if (peerConnection != null)
            {
                // https://bugs.chromium.org/p/webrtc/issues/detail?id=6924
                Task.Run(() =>
                {
                    lock (peerConnection)
                    {
                        if (peerConnection != null)
                        {
                            dataChannel?.Close();
                            peerConnection?.Close();

                            dataChannel?.Dispose();
                            peerConnection?.Dispose();
                        }
                    }
                });
            }
        }

        public void ReceiveOffer(int connectionId, SessionDescription offerSdp, Action<SessionDescription, string> completionHandler)
        {
            incomingConnectionId = connectionId;

            _incomingConnection.peer.SetRemoteDescription(
                SdpObserver.OnSet(() =>
                {
                    var mediaConstraints = new MediaConstraints();
                    _incomingConnection.peer.CreateAnswer(
                        SdpObserver.OnCreate((answerSdp) =>
                        {
                            _incomingConnection.peer.SetLocalDescription(SdpObserver.OnSet(() =>
                            {
                                completionHandler(answerSdp, string.Empty);
                            },
                            (err) =>
                            {
                                completionHandler(null, err);
                            }),
                            answerSdp);
                        },
                        (err) =>
                        {
                            completionHandler(null, err);
                        }),
                        mediaConstraints);
                },
                (err) =>
                {
                    completionHandler(null, err);
                }),
                offerSdp);
        }

        public void ReceiveAnswer(int connectionId, SessionDescription answerSdp, Action<SessionDescription, string> completionHandler)
        {
            if (connectionId != outgoingConnectionId)
                throw new System.Exception();

            _outgoingConnection.peer.SetRemoteDescription(
                SdpObserver.OnSet(() =>
                {
                    completionHandler(answerSdp, string.Empty);
                },
                (err) =>
                {
                    completionHandler(null, err);
                }),
                answerSdp);
        }

        public bool SendMessage(string message)
        {
            if (_outgoingConnection.data != null && _outgoingConnection.data.InvokeState() == DataChannel.State.Open)
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                return SendBytes(bytes);
            }

            return false;
        }

        public bool SendMessage(byte[] message)
        {
            if (_outgoingConnection.data != null && _outgoingConnection.data.InvokeState() == DataChannel.State.Open)
            {
                return SendBytes(message);
            }

            return false;
        }

        private bool SendBytes(byte[] bytes)
        {
            var buffer = new DataChannel.Buffer(Java.Nio.ByteBuffer.Wrap(bytes), true);
            var result = _outgoingConnection.data.Send(buffer);
            return result;
        }

        public void ReceiveCandidate(int connectionId, IceCandidate candidate)
        {
            if (connectionId == incomingConnectionId)
                _incomingConnection.peer.AddIceCandidate(candidate);
            else if (connectionId == outgoingConnectionId)
                _outgoingConnection.peer.AddIceCandidate(candidate);
        }

        private void InitView(SurfaceViewRenderer view)
        {
            view.SetMirror(false);
            view.SetEnableHardwareScaler(true);
            view.Init(_eglBase.EglBaseContext, null);
        }

        private PeerConnection SetupOutgoingPeerConnection()
        {
            var rtcConfig = new PeerConnection.RTCConfiguration(_iceServers);

            var pc = _peerConnectionFactory.CreatePeerConnection(rtcConfig, this);

            pc.AddTrack(_localVideoTrack, new[] { "stream0" });
            pc.AddTrack(_localAudioTrack, new[] { "stream0" });

            return pc;
        }


        private PeerConnection SetupIncomingPeerConnection()
        {
            var rtcConfig = new PeerConnection.RTCConfiguration(_iceServers);

            var pc = _peerConnectionFactory.CreatePeerConnection(rtcConfig, this);

            return pc;
        }

        private DataChannel SetupOutgoingDataChannel()
        {
            var init = new DataChannel.Init()
            {
                Id = 1
            };

            var dc = _outgoingConnection.peer.CreateDataChannel("dataChannel", init);
            dc.RegisterObserver(this);

            return dc;
        }

        #region PeerConnectionObserver
        public void OnAddStream(MediaStream p0)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(OnAddStream)}");

            var videoTracks = p0?.VideoTracks?.OfType<VideoTrack>();
            videoTracks?.FirstOrDefault()?.AddSink(_remoteView);

            var audioTracks = p0?.AudioTracks?.OfType<AudioTrack>();
            audioTracks?.FirstOrDefault()?.SetEnabled(true);
            audioTracks?.FirstOrDefault()?.SetVolume(10);
        }

        public void OnAddTrack(RtpReceiver p0, MediaStream[] p1)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(OnAddTrack)}");
        }

        public void OnDataChannel(DataChannel p0)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(OnDataChannel)}");

            MainThread.BeginInvokeOnMainThread(() => _observer?.OnOpenDataChannel());

            _incomingDataChannel = p0;
            _incomingDataChannel.RegisterObserver(this);
        }

        public void OnIceCandidate(IceCandidate p0)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(OnIceCandidate)}");

            MainThread.BeginInvokeOnMainThread(() => _observer?.OnGenerateCandidate(p0));
        }

        public void OnIceCandidatesRemoved(IceCandidate[] p0)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(OnIceCandidatesRemoved)}");
        }

        public void OnIceConnectionChange(PeerConnection.IceConnectionState p0)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(OnIceConnectionChange)}");

            if (p0 == PeerConnection.IceConnectionState.Connected ||
                p0 == PeerConnection.IceConnectionState.Completed)
            {
                if (!_isConnected)
                {
                    _isConnected = true;
                    MainThread.BeginInvokeOnMainThread(() => _observer?.OnConnectWebRtc());
                }
            }
            //else if (_isConnected)
            //{
            //    _isConnected = false;
            //    Disconnect(_incomingPeerConnection, _incomingDataChannel);
            //    Disconnect(_outgoingPeerConnection, _outgoingDataChannel);
            //    MainThread.BeginInvokeOnMainThread(() => _observer?.OnDisconnectWebRtc());
            //}

            MainThread.BeginInvokeOnMainThread(() => _observer?.OnIceConnectionStateChanged(p0));
        }

        public void OnIceConnectionReceivingChange(bool p0)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(OnIceConnectionReceivingChange)}");
        }

        public void OnIceGatheringChange(PeerConnection.IceGatheringState p0)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(OnIceGatheringChange)}");
        }

        public void OnRemoveStream(MediaStream p0)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(OnRemoveStream)}");
        }

        public void OnRenegotiationNeeded()
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(OnRenegotiationNeeded)}");
        }

        public void OnSignalingChange(PeerConnection.SignalingState p0)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(OnSignalingChange)}");
        }
        #endregion

        #region DataChannelObserver
        public void OnBufferedAmountChange(long p0)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(OnBufferedAmountChange)} {_outgoingConnection.data.BufferedAmount()}");
        }

        public void OnMessage(DataChannel.Buffer p0)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(OnMessage)}");

            var bytes = new byte[p0.Data.Remaining()];
            p0.Data.Get(bytes);

            if (p0.Binary)
            {
                MainThread.BeginInvokeOnMainThread(() => _observer?.OnReceiveData(bytes));
            }
            else
            {
                var msg = Encoding.UTF8.GetString(bytes);
                MainThread.BeginInvokeOnMainThread(() => _observer?.OnReceiveMessage(msg));
            }
        }

        public void OnStateChange()
        {
            var state = _outgoingConnection.data?.InvokeState();
            System.Diagnostics.Debug.WriteLine($"{nameof(OnStateChange)} {state?.ToString()}");
        }
        #endregion
    }
}
