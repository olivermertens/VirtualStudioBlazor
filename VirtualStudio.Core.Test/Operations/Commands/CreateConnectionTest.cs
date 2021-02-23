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
    public class CreateConnectionTest
    {
        VirtualStudio virtualStudio;
        IStudioComponent componentWithUdpAudioInput;
        IStudioComponent componentWithUdpAudioInputInRepository;
        IStudioComponent componentWithWebRtcAudioInput;
        IStudioComponent componentWithWebRtcVideoInput;
        IStudioComponent componentWithUdpAudioOutput;
        IStudioComponent componentWithWebRtcAudioOutput;
        IStudioComponent componentWithWebRtcVideoOutput;
        
        [TestInitialize]
        public void Init()
        {
            virtualStudio = new VirtualStudio();

            var placeholder = new PlaceholderStudioComponent();
            placeholder.AddInput("Input 1", DataKind.Audio, "UDP");
            virtualStudio.ComponentRepository.AddPlaceholder(placeholder);
            componentWithUdpAudioInput = virtualStudio.AddComponent(placeholder);
            componentWithUdpAudioInputInRepository = placeholder;

            placeholder = new PlaceholderStudioComponent();
            placeholder.AddInput("Input 1", DataKind.Audio, "WebRtc");
            virtualStudio.ComponentRepository.AddPlaceholder(placeholder);
            componentWithWebRtcAudioInput = virtualStudio.AddComponent(placeholder);

            placeholder = new PlaceholderStudioComponent();
            placeholder.AddInput("Input 1", DataKind.Video, "WebRtc");
            virtualStudio.ComponentRepository.AddPlaceholder(placeholder);
            componentWithWebRtcVideoInput = virtualStudio.AddComponent(placeholder);

            placeholder = new PlaceholderStudioComponent();
            placeholder.AddOutput("Output 1", DataKind.Audio, "UDP");
            virtualStudio.ComponentRepository.AddPlaceholder(placeholder);
            componentWithUdpAudioOutput = virtualStudio.AddComponent(placeholder);

            placeholder = new PlaceholderStudioComponent();
            placeholder.AddOutput("Output 1", DataKind.Audio, "WebRtc");
            virtualStudio.ComponentRepository.AddPlaceholder(placeholder);
            componentWithWebRtcAudioOutput = virtualStudio.AddComponent(placeholder);

            placeholder = new PlaceholderStudioComponent();
            placeholder.AddOutput("Output 1", DataKind.Video, "WebRtc");
            virtualStudio.ComponentRepository.AddPlaceholder(placeholder);
            componentWithWebRtcVideoOutput = virtualStudio.AddComponent(placeholder);


        }

        [TestMethod]
        public async Task Create_connections_between_compatible_input_and_output()
        {
            var createConnectionCommand = new CreateConnectionCommand(componentWithUdpAudioOutput.Id, 1, componentWithUdpAudioInput.Id, 1);
            await createConnectionCommand.Process(virtualStudio);
            Assert.IsNull(createConnectionCommand.Error);

            createConnectionCommand = new CreateConnectionCommand(componentWithWebRtcAudioOutput.Id, 1, componentWithWebRtcAudioInput.Id, 1);
            await createConnectionCommand.Process(virtualStudio);
            Assert.IsNull(createConnectionCommand.Error);

            createConnectionCommand = new CreateConnectionCommand(componentWithWebRtcVideoOutput.Id, 1, componentWithWebRtcVideoInput.Id, 1);
            await createConnectionCommand.Process(virtualStudio);
            Assert.IsNull(createConnectionCommand.Error);

            Assert.IsTrue(virtualStudio.Connections.Count == 3);
        }

        [TestMethod]
        public async Task Does_not_create_a_connection_if_DataKinds_do_not_match_and_sets_Error_to_InvalidOperation()
        {
            var createConnectionCommand = new CreateConnectionCommand(componentWithWebRtcAudioOutput.Id, 1, componentWithWebRtcVideoInput.Id, 1);
            await createConnectionCommand.Process(virtualStudio);

            Assert.IsTrue(createConnectionCommand.Error.Type == ErrorType.InvalidOperation);
            Assert.IsTrue(virtualStudio.Connections.Count == 0);
        }

        [TestMethod]
        public async Task Does_not_create_a_connection_if_ConnectionTypes_do_not_match_and_sets_Error_to_InvalidOperation()
        {
            var createConnectionCommand = new CreateConnectionCommand(componentWithWebRtcAudioOutput.Id, 1, componentWithUdpAudioInput.Id, 1);
            await createConnectionCommand.Process(virtualStudio);

            Assert.IsTrue(createConnectionCommand.Error.Type == ErrorType.InvalidOperation);
            Assert.IsTrue(virtualStudio.Connections.Count == 0);
        }

        [TestMethod]
        public async Task Does_not_create_a_connection_between_two_outputs()
        {
            var createConnectionCommand = new CreateConnectionCommand(componentWithWebRtcAudioOutput.Id, 1, componentWithWebRtcAudioOutput.Id, 1);
            await createConnectionCommand.Process(virtualStudio);

            Assert.IsTrue(createConnectionCommand.Error.Type == ErrorType.NotFound);
            Assert.IsTrue(virtualStudio.Connections.Count == 0);
        }

        [TestMethod]
        public async Task Does_not_create_a_connection_between_two_inputs()
        {
            var createConnectionCommand = new CreateConnectionCommand(componentWithWebRtcAudioInput.Id, 1, componentWithWebRtcAudioInput.Id, 1);
            await createConnectionCommand.Process(virtualStudio);

            Assert.IsTrue(createConnectionCommand.Error.Type == ErrorType.NotFound);
            Assert.IsTrue(virtualStudio.Connections.Count == 0);
        }

        [TestMethod]
        public async Task Does_not_create_a_connection_when_the_output_input_is_already_connected()
        {
            var createConnectionCommand = new CreateConnectionCommand(componentWithWebRtcAudioOutput.Id, 1, componentWithWebRtcAudioInput.Id, 1);
            await createConnectionCommand.Process(virtualStudio);

            createConnectionCommand = new CreateConnectionCommand(componentWithWebRtcAudioOutput.Id, 1, componentWithWebRtcAudioInput.Id, 1);
            await createConnectionCommand.Process(virtualStudio);

            Assert.IsTrue(createConnectionCommand.Error.Type == ErrorType.InvalidOperation);
            Assert.IsTrue(virtualStudio.Connections.Count == 1);
        }

        [TestMethod]
        public async Task Does_not_create_a_connection_with_components_that_only_exist_in_the_ComponentRepository()
        {
            var createConnectionCommand = new CreateConnectionCommand(componentWithUdpAudioOutput.Id, 1, componentWithUdpAudioInputInRepository.Id, 1);
            var success = await createConnectionCommand.Process(virtualStudio);

            Assert.IsFalse(success);
            Assert.IsNotNull(createConnectionCommand.Error);
            Assert.AreEqual(ErrorType.NotFound, createConnectionCommand.Error.Type);
        }
    }
}
