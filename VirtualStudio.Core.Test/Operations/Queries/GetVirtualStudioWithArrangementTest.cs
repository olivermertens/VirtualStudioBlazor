using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Arrangement;
using VirtualStudio.Core.Operations;
using VirtualStudio.Shared;

namespace VirtualStudio.Core.Test.Operations
{
    [TestClass]
    public class GetVirtualStudioWithArrangementTest
    {
        VirtualStudioWithArrangement virtualStudio;

        [TestInitialize]
        public void Init()
        {
            virtualStudio = new VirtualStudioWithArrangement();
        }

        [TestMethod]
        public async Task Gets_empty_VirtualStudio_data()
        {
            var getVirtualStudioQuery = new GetVirtualStudioWithArrangementQuery();
            var virtualStudioDto = await getVirtualStudioQuery.Process(virtualStudio);

            Assert.IsNotNull(virtualStudioDto);
            Assert.IsNotNull(virtualStudioDto.ComponentNodes);
            Assert.IsNotNull(virtualStudioDto.ComponentRepository);
            Assert.IsNotNull(virtualStudioDto.Connections);
        }

        [TestMethod]
        public async Task Gets_VirtualStudio_data_with_placeholders()
        {
            var placeholder = new PlaceholderStudioComponent();
            placeholder.AddInput("Input 1", DataKind.Audio, "WebRtc");
            virtualStudio.ComponentRepository.AddPlaceholder(placeholder);
            var nodePosition = new Position2D(23, 45);
            virtualStudio.AddComponent(placeholder, nodePosition);

            var getVirtualStudioQuery = new GetVirtualStudioWithArrangementQuery();
            var virtualStudioDto = await getVirtualStudioQuery.Process(virtualStudio);

            Assert.IsTrue(virtualStudioDto.ComponentRepository.Placeholders.Count() == 1);
            var repoPlaceholder = virtualStudioDto.ComponentRepository.Placeholders.First();
            Assert.AreEqual(repoPlaceholder.Inputs.First().Name, placeholder.Inputs[0].Name);
            Assert.IsTrue(virtualStudioDto.ComponentNodes.Count() == 1);
            var componentNode = virtualStudioDto.ComponentNodes.First();
            Assert.AreEqual(nodePosition.X, componentNode.X);
            Assert.AreEqual(nodePosition.Y, componentNode.Y);
            Assert.AreEqual(componentNode.Component.Inputs.First().Name, placeholder.Inputs[0].Name);
            Assert.AreNotEqual(repoPlaceholder.Id, componentNode.Component.Id);
            Assert.IsTrue(virtualStudioDto.Connections.Count() == 0);
        }

        [TestMethod]
        public async Task Gets_VirtualStudio_data_with_a_connection()
        {
            var placeholder1 = new PlaceholderStudioComponent();
            placeholder1.AddInput("Input 1", DataKind.Audio, "WebRtc");
            var placeholder2 = new PlaceholderStudioComponent();
            placeholder2.AddOutput("Output 1", DataKind.Audio, "WebRtc");
            virtualStudio.ComponentRepository.AddPlaceholder(placeholder1);
            virtualStudio.ComponentRepository.AddPlaceholder(placeholder2);
            var nodePosition = new Position2D(23, 45);
            var componentNode1 = virtualStudio.AddComponent(placeholder1, nodePosition);
            var componentNode2 = virtualStudio.AddComponent(placeholder2, nodePosition);
            var connection = virtualStudio.CreateConnection(componentNode2.Component.Outputs[0], componentNode1.Component.Inputs[0]);

            var getVirtualStudioQuery = new GetVirtualStudioWithArrangementQuery();
            var virtualStudioDto = await getVirtualStudioQuery.Process(virtualStudio);

            Assert.IsTrue(virtualStudioDto.Connections.Count() == 1);
            var connectionDto = virtualStudioDto.Connections.First();
            Assert.AreEqual(connection.Id, connectionDto.Id);
            Assert.AreEqual(connection.Input.Id, connectionDto.InputId);
            Assert.AreEqual(connection.Input.Component.Id, connectionDto.InputComponentId);
            Assert.AreEqual(connection.Output.Id, connectionDto.OutputId);
            Assert.AreEqual(connection.Output.Component.Id, connectionDto.OutputComponentId);
            Assert.AreEqual(connection.State, connectionDto.State);
        }
    }
}
