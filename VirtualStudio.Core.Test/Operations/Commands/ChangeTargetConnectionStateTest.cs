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
    public class ChangeTargetConnectionStateTest
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

        [DataTestMethod]
        [DataRow(ConnectionState.Disconnected, ConnectionState.Connected)]
        [DataRow(ConnectionState.Connected, ConnectionState.Disconnected)]
        [DataRow(ConnectionState.Connected, ConnectionState.Destroyed)]
        [DataRow(ConnectionState.Disconnected, ConnectionState.Destroyed)]
        public async Task Changes_TargetConnectionState(ConnectionState fromState, ConnectionState toState)
        {
            connection.SetTargetState(fromState);
            var changeTargetStateCommand = new ChangeTargetConnectionStateCommand(connection.Id, toState);
            await changeTargetStateCommand.Process(virtualStudio);

            Assert.AreEqual(toState, connection.TargetState);
        }

        [DataTestMethod]
        [DataRow(ConnectionState.Disconnected, ConnectionState.Disconnecting)]
        [DataRow(ConnectionState.Disconnected, ConnectionState.Connecting)]
        [DataRow(ConnectionState.Disconnected, ConnectionState.Unknown)]
        [DataRow(ConnectionState.Connected, ConnectionState.Disconnecting)]
        [DataRow(ConnectionState.Connected, ConnectionState.Connecting)]
        [DataRow(ConnectionState.Connected, ConnectionState.Unknown)]
        [DataRow(ConnectionState.Destroyed, ConnectionState.Connected)]
        [DataRow(ConnectionState.Destroyed, ConnectionState.Connecting)]
        [DataRow(ConnectionState.Destroyed, ConnectionState.Disconnecting)]
        [DataRow(ConnectionState.Destroyed, ConnectionState.Disconnected)]
        [DataRow(ConnectionState.Destroyed, ConnectionState.Unknown)]
        public async Task Does_not_change_TargetConnectionState(ConnectionState fromState, ConnectionState toState)
        {
            connection.SetTargetState(fromState);
            var changeTargetStateCommand = new ChangeTargetConnectionStateCommand(connection.Id, toState);
            await changeTargetStateCommand.Process(virtualStudio);

            Assert.IsTrue(changeTargetStateCommand.Error.Type == ErrorType.InvalidArgument);
            Assert.AreNotEqual(toState, connection.TargetState);
            Assert.AreEqual(fromState, connection.TargetState);
        }
    }
}
