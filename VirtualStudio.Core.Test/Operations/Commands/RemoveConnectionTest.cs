using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Core.Operations;
using VirtualStudio.Shared;

namespace VirtualStudio.Core.Test.Operations
{
    [TestClass]
    public class RemoveConnectionTest
    {
        VirtualStudio virtualStudio;
        IStudioComponent componentWithUdpAudioInput;
        IStudioComponent componentWithUdpAudioOutput;
        IStudioConnection connection;

        [TestInitialize]
        public void Init()
        {
            virtualStudio = new VirtualStudio();

            var placeholder = new PlaceholderStudioComponent();
            placeholder.AddInput("Input 1", DataKind.Audio, "UDP");
            virtualStudio.ComponentRepository.AddPlaceholder(placeholder);
            componentWithUdpAudioInput = virtualStudio.AddComponent(placeholder);

            placeholder = new PlaceholderStudioComponent();
            placeholder.AddOutput("Output 1", DataKind.Audio, "UDP");
            virtualStudio.ComponentRepository.AddPlaceholder(placeholder);
            componentWithUdpAudioOutput = virtualStudio.AddComponent(placeholder);

            connection = virtualStudio.CreateConnection(componentWithUdpAudioOutput.Outputs[0], componentWithUdpAudioInput.Inputs[0]);
        }

        [TestMethod]
        public async Task Removes_a_connection()
        {
            var removeConnectionCommand = new RemoveConnectionCommand(connection.Id);
            await removeConnectionCommand.Process(virtualStudio);

            Assert.IsTrue(virtualStudio.Connections.Count == 0);
        }

        [TestMethod]
        public async Task Sets_Error_to_NotFound_when_the_ID_does_not_exist()
        {
            var removeConnectionCommand = new RemoveConnectionCommand(999);
            await removeConnectionCommand.Process(virtualStudio);

            Assert.IsTrue(removeConnectionCommand.Error.Type == ErrorType.NotFound);
        }
    }
}
