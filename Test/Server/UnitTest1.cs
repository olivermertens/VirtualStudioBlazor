using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualStudio.Shared;

namespace VirtualStudio.Server.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var studioClient = new StudioClient();
            studioClient.TransmissionMethods.Add(TransmissionMethodDescription.WebRtc);
            studioClient.IODescription.Outputs.Add(EndpointDescription.CreateVideoEndpoint("Video"));
            studioClient.IODescription.Outputs.Add(EndpointDescription.CreateAudioEndpoint("Audio"));
            studioClient.IODescription.Inputs.Add(EndpointDescription.CreateVideoEndpoint("Feedback"));

            studioClient.ConnectionId = "1234";

            
        }
    }
}
