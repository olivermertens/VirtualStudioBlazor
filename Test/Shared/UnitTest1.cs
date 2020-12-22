using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VirtualStudio.Shared.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
           
        }

        [TestMethod]
        public void SerializeAndDeserializeClientInfo()
        {
            var studioClient = new StudioClientBase();
            studioClient.Identifier = "TestClient";
            studioClient.NetworkInfo = new NetworkInfo() { LocalEndpoint = new ConnectionPoint("127.0.0.1", 1111), IceCandidates = new List<ConnectionPoint>() { new ConnectionPoint("192.168.0.1", 2356) } };
            studioClient.IODescription = new IODescription();
            studioClient.TransmissionMethods.Add(TransmissionMethodDescription.WebRtc);
            studioClient.IODescription.Inputs.Add(EndpointDescription.CreateVideoEndpoint("VideoIn"));
            studioClient.IODescription.Outputs.Add(EndpointDescription.CreateVideoEndpoint("VideoOut"));
            studioClient.IODescription.Outputs.Add(EndpointDescription.CreateDataEndpoint("SomeData", new DataDescription("SomeDataFormat")));

            string json = System.Text.Json.JsonSerializer.Serialize(studioClient);
            var clientInfo2 = System.Text.Json.JsonSerializer.Deserialize<StudioClientBase>(json);

            Assert.AreEqual(studioClient.Identifier, clientInfo2.Identifier);
            Assert.AreEqual(studioClient.NetworkInfo.LocalEndpoint, clientInfo2.NetworkInfo.LocalEndpoint);
            CollectionAssert.AreEqual(studioClient.NetworkInfo.IceCandidates, clientInfo2.NetworkInfo.IceCandidates);
            CollectionAssert.AreEqual(studioClient.TransmissionMethods, clientInfo2.TransmissionMethods);
            CollectionAssert.AreEqual(studioClient.IODescription.Inputs, clientInfo2.IODescription.Inputs);
            CollectionAssert.AreEqual(studioClient.IODescription.Outputs, clientInfo2.IODescription.Outputs);
        }
    }
}
