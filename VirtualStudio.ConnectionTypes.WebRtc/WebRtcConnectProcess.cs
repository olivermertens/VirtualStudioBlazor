using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using VirtualStudio.Core;

namespace VirtualStudio.ConnectionTypes.WebRtc
{
    public class WebRtcConnectProcess : IDisposable
    {
        public IStudioConnection Connection { get; }
        public string SdpOffer { get; private set; }
        public string SdpAnswer { get; private set; }
        public ManualResetEvent SdpOfferAvailableEvent { get; } = new ManualResetEvent(false);
        public ManualResetEvent SdpAnswerAvailableEvent { get; } = new ManualResetEvent(false);
        public bool SenderSupportsInsertableStreams { get; set; }
        public bool UseInsertableStreams { get; set; }

        public WebRtcConnectProcess(IStudioConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public void SetSdpOffer(string sdpOffer)
        {
            if (SdpOffer != null)
                throw new InvalidOperationException("A SDP offer was already set.");
            SdpOffer = sdpOffer;
            SdpOfferAvailableEvent.Set();
        }

        public void SetSdpAnswer(string sdpAnswer)
        {
            if(SdpAnswer != null)
                throw new InvalidOperationException("A SDP answer was already set.");
            SdpAnswer = sdpAnswer;
            SdpAnswerAvailableEvent.Set();
        }

        public void Dispose()
        {
            SdpOfferAvailableEvent.Dispose();
            SdpAnswerAvailableEvent.Dispose();
        }
    }
}
