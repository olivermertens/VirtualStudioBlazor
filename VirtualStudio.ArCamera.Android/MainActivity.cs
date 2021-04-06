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
using VirtualStudio.ArCamera.Android.Code;
using VirtualStudio.Shared;
using VirtualStudio.Shared.Abstractions;
using VirtualStudio.Shared.Connection;
using VirtualStudio.Shared.DTOs;
using VirtualStudio.Shared.DTOs.WebRtc;
using Xam.WebRtc.Android;

namespace VirtualStudio.ArCamera.Android
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, IWebRtcObserver, IWebRtcClient
    {
        SurfaceViewRenderer localVideoSurface;
        SurfaceViewRenderer remoteVideoSurface;
        SignalRVirtualStudioConnection virtualStudioConnection;

        private readonly string virtualStudioName = "vs";
        private WebRtcClient webRtcClient;
        private int connectionId;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            CheckCameraAccess();
        }

        private void CheckCameraAccess()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == (int)Permission.Granted)
            {
                Init();
            }
            else
            {
                ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.Camera }, 1);
            }
        }

        private async void Init()
        {
            localVideoSurface = FindViewById<SurfaceViewRenderer>(Resource.Id.local_video_surface);
            remoteVideoSurface = FindViewById<SurfaceViewRenderer>(Resource.Id.remote_video_surface);
            webRtcClient = new WebRtcClient(this, remoteVideoSurface, localVideoSurface, this);
            await ConnectToServer();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == 1 && grantResults[0] == Permission.Granted)
                Init();
            else
                CheckCameraAccess();
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        //protected override void OnStop()
        //{
        //    bool success = virtualStudioConnection.LeaveVirtualStudio(virtualStudioName).Result;
        //    if (success)
        //        Console.WriteLine($"Left '{virtualStudioName}' successfully.");
        //    else
        //        Console.WriteLine($"Leaving '{virtualStudioName}' failed.");

        //    _ = virtualStudioConnection.DisposeAsync();
        //    base.OnStop();
        //}

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
    }
}