using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualStudio.Shared.Abstractions;
using VirtualStudio.Shared.DTOs.WebRtc;

namespace WebRtcReceiver.Core
{
    public interface IReceivingPeerListener
    {
        void OnFrameReceived(ReceivingPeer peer, Memory<byte> frame, uint rtpTimestamp, uint timestamp);
        void OnConnectionStateChanged(ReceivingPeer peer, RTCPeerConnectionState connectionState);
        void OnIceCandidate(ReceivingPeer peer, RTCIceCandidate iceCandidate);
    }

    public class ReceivingPeer
    {
        private const int H264_SUGGESTED_FORMAT_ID = 100;

        public int ConnectionId { get; private set; }
        public bool ExtractTimestampFromFrame { get; set; }

        private RTCPeerConnection _peerConnection;

        private readonly bool _noAudio;
        private readonly IReceivingPeerListener _listener;

        public ReceivingPeer(IReceivingPeerListener listener, bool noAudio)
        {
            _listener = listener ?? throw new ArgumentNullException(nameof(listener));
            _noAudio = noAudio;
        }

        public RTCSessionDescriptionInit GetSdpAnswer(SdpAnswerRequestArgs args)
        {
            ConnectionId = args.ConnectionId;

            RTCConfiguration config = new RTCConfiguration
            {
                X_UseRtpFeedbackProfile = true
            };
            var pc = new RTCPeerConnection(config);

            // Add local receive only tracks. This ensures that the SDP answer includes only the codecs we support.
            if (!_noAudio)
            {
                MediaStreamTrack audioTrack = new MediaStreamTrack(SDPMediaTypesEnum.audio, false,
                    new List<SDPAudioVideoMediaFormat> { new SDPAudioVideoMediaFormat(SDPWellKnownMediaFormatsEnum.PCMU) }, MediaStreamStatusEnum.RecvOnly);
                pc.addTrack(audioTrack);
            }
            // MediaStreamTrack videoTrack = new MediaStreamTrack(new VideoFormat(96, "VP8", 90000, "x-google-max-bitrate=5000000"), MediaStreamStatusEnum.RecvOnly);

            // pc.OnVideoFrameReceived += _videoSink.GotVideoFrame;
            // pc.OnVideoFormatsNegotiated += (formats) => _videoSink.SetVideoSinkFormat(formats.First());

            pc.OnVideoFrameReceived += (endpoint, rtpTimestampMs, frame, format) =>
            {
                int frameLength = frame.Length;
                string rtpTimestamp = new DateTime(rtpTimestampMs * 100).ToString("dd.MM.yyyy HH:mm:ss,ffff");
                string timestamp = "none";
                uint timestampMs = 0;
                if(ExtractTimestampFromFrame)
                {
                    frameLength -= 4;
                    timestampMs = (uint)(frame[frameLength] << 24 | frame[frameLength + 1] << 16 | frame[frameLength + 2] << 8 | frame[frameLength + 3]);
                    timestamp = new DateTime(timestampMs * 10000, DateTimeKind.Utc).ToString("dd.MM.yyyy HH:mm:ss,ffff");
                }
                Console.WriteLine($"On frame received: byte[{frame.Length}], rtpTs: {rtpTimestamp} extractedTs: {timestamp})");
                _listener.OnFrameReceived(this, new Memory<byte>(frame, 0, frameLength), rtpTimestampMs, timestampMs);
            };

            pc.onicecandidate += (iceCandidate) =>
            {
                Console.WriteLine("On ice candidate");
                _listener.OnIceCandidate(this, iceCandidate);
            };

            pc.onconnectionstatechange += (state) =>
            {
                Console.WriteLine($"Peer connection state change to {state}.");

                if (state == RTCPeerConnectionState.failed)
                {
                    pc.Close("ice disconnection");
                }
                _listener.OnConnectionStateChanged(this, state);
            };

            pc.OnSendReport += (media, sr) => Console.WriteLine($"RTCP Send for {media}\n{sr.GetDebugSummary()}");
            pc.oniceconnectionstatechange += (state) => Console.WriteLine($"ICE connection state change to {state}.");

            var sdpOffer = SDP.ParseSDPDescription(args.SdpOffer);
            //sdpOffer.Media.FirstOrDefault()?.
            var videoMedia = sdpOffer.Media.FirstOrDefault(m => m.Media == SDPMediaTypesEnum.video);
            var h264VideoFormat = videoMedia.MediaFormats.Values.First(m => m.Rtpmap == "H264/90000").ToVideoFormat();
            var videoTrack = new MediaStreamTrack(h264VideoFormat, MediaStreamStatusEnum.RecvOnly);
            pc.addTrack(videoTrack);

            //var track = new MediaStreamTrack(SDPMediaTypesEnum.video, false, new List<SDPAudioVideoMediaFormat> { new SDPAudioVideoMediaFormat(SDPMediaTypesEnum.video, 102, "H264", 90000) });
            //pc.addTrack(track);

            var setRemoteDescriptionResult = pc.setRemoteDescription(new RTCSessionDescriptionInit
            {
                type = RTCSdpType.offer,
                sdp = args.SdpOffer
            });
            //MediaStreamTrack videoTrack = new MediaStreamTrack(new VideoFormat(VideoCodecsEnum.H264, 102), MediaStreamStatusEnum.RecvOnly);
            //pc.addTrack(videoTrack);

            var answer = pc.createAnswer();
            var setLocalDescriptionResult = pc.setLocalDescription(answer);

            Console.WriteLine("SDP Offer:\n" + args.SdpOffer);
            Console.WriteLine("SDP Answer 2:\n" + answer.sdp);

            _peerConnection = pc;          
            return answer;
        }

        public void AddIceCandidate(IceCandidateArgs args)
        {
            Console.WriteLine("Add ice candidate");
            if (args.ConnectionId != ConnectionId)
                throw new Exception();

            _peerConnection.addIceCandidate(new RTCIceCandidateInit
            {
                candidate = args.CandidateDto.Candidate,
                sdpMid = args.CandidateDto.SdpMid,
                sdpMLineIndex = (ushort)args.CandidateDto.SdpMLineIndex,
                usernameFragment = args.CandidateDto.UsernameFragment
            });
        }

        public void Disconnect(DisconnectWebRtcCommandArgs args)
        {
            if (args.ConnectionId != ConnectionId)
                throw new Exception();

            _peerConnection.Dispose();
        }
    }
}
