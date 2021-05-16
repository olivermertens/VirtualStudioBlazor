using SIPSorcery.Net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtualStudio.Shared;
using VirtualStudio.Shared.Connection;
using VirtualStudio.Shared.DTOs;
using WebRtcReceiver.Core;
using VirtualStudio.Shared.Abstractions;
using VirtualStudio.Shared.DTOs.WebRtc;
using System.IO;

namespace WebRtcReceiver.App
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            using (FileStream fileStream = File.Create("videodata.264"))
            {
                var virtualStudioClient = new VirtualStudioClient(fileStream);
                await virtualStudioClient.ConnectAsync();

                Console.ReadLine();
            }
        }
    }

    class VirtualStudioClient : IWebRtcClient, IReceivingPeerListener
    {
        private SignalRVirtualStudioConnection _connection;
        private readonly ReceivingPeer _peer;
        private readonly Stream _outputStream;

        public VirtualStudioClient(Stream outputStream)
        {
            _outputStream = outputStream ?? throw new ArgumentNullException(nameof(outputStream));
            _peer = new ReceivingPeer(this, true);
        }

        public async Task ConnectAsync()
        {
            _connection = await SignalRVirtualStudioConnection.CreateAsync("localhost", 5001, true);
            _connection.AddListener(this);

            await _connection.JoinVirtualStudio("vs", new StudioComponentDto
            {
                Name = "Receiver",
                IsPlaceholder = false,
                Inputs = new List<StudioComponentEndpointDto>
                {
                    new StudioComponentEndpointDto(){IOType = EndpointIOType.Input, ConnectionType = "WebRTC", DataKind = DataKind.Video, Id = 1, Name = "VideoInput"}
                }
            });
        }

        #region IReceivingPeerListener
        public void OnConnectionStateChanged(ReceivingPeer peer, RTCPeerConnectionState connectionState)
        {
            switch (connectionState)
            {
                case RTCPeerConnectionState.closed:
                    // TODO
                    break;
            }
        }

        public void OnFrameReceived(ReceivingPeer peer, byte[] frame, uint timestamp)
        {
            _outputStream.Write(frame);
        }

        public void OnIceCandidate(ReceivingPeer peer, RTCIceCandidate iceCandidate)
        {
            _connection.SendIceCandidate(new IceCandidateArgs
            {
                ConnectionId = peer.ConnectionId,
                CandidateDto = new RtcIceCandidateDto
                {
                    Candidate = iceCandidate.candidate,
                    SdpMid = iceCandidate.sdpMid,
                    SdpMLineIndex = iceCandidate.sdpMLineIndex,
                    UsernameFragment = iceCandidate.usernameFragment
                }
            });
        }
        #endregion

        #region IWebRtcClient
        public Task RequestSdpOffer(SdpOfferRequestArgs args)
        {
            throw new NotImplementedException();
        }

        public async Task RequestSdpAnswer(SdpAnswerRequestArgs args)
        {
            var answer = _peer.GetSdpAnswer(args);
            await _connection.SendSdpAnswer(new SdpAnswerResponseArgs
            {
                ConnectionId = _peer.ConnectionId,
                SdpAnswer = answer.sdp,
                UseInsertableStreams = false
            });
        }

        public Task AddIceCandidate(IceCandidateArgs args)
        {
            _peer.AddIceCandidate(args);
            return Task.CompletedTask;
        }

        public Task Connect(ConnectWebRtcCommandArgs args)
        {
            throw new NotImplementedException();
        }

        public Task Disconnect(DisconnectWebRtcCommandArgs args)
        {
            _peer.Disconnect(args);
            return Task.CompletedTask;
        }
        #endregion
    }
}
