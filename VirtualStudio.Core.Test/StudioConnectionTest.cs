using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Shared;

namespace VirtualStudio.Core.Test
{
    [TestClass]
    public class StudioConnectionTest
    {
        static StudioConnectionFactory studioConnectionFactory;

        StudioComponentInput input;
        StudioComponentOutput output;
        Mock<IStudioComponent> inputComponentMock;
        Mock<IStudioComponent> outputComponentMock;


        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            studioConnectionFactory = new StudioConnectionFactory();
        }

        [TestInitialize]
        public void TestInit()
        {
            inputComponentMock = new Mock<IStudioComponent>();
            input = new StudioComponentInput(1, "input", DataKind.Video, "ConnectionType", inputComponentMock.Object);
            inputComponentMock.Setup(c => c.Inputs[0]).Returns(input);

            outputComponentMock = new Mock<IStudioComponent>();
            output = new StudioComponentOutput(1, "output", DataKind.Video, "ConnectionType", outputComponentMock.Object);
            outputComponentMock.Setup(c => c.Outputs[0]).Returns(output);
        }


        [TestMethod]
        public void Initializes_a_StudioConnection_with_State_disconnected()
        {
            var connection = studioConnectionFactory.CreateStudioConnection(output, input);

            Assert.IsTrue(connection.State == ConnectionState.Disconnected);
        }

        [TestMethod]
        public void Sets_ConnectionState_to_Connecting_when_the_input_component_updates_the_state_to_Connecting()
        {
            var connection = studioConnectionFactory.CreateStudioConnection(output, input);

            inputComponentMock.Raise(c => c.InputConnectionStateUpdated += null, inputComponentMock.Object, (input, connection.Id, ConnectionState.Connecting));

            Assert.IsTrue(connection.State == ConnectionState.Connecting);
        }

        [TestMethod]
        public void Sets_ConnectionState_to_Connecting_when_the_output_component_updates_the_state_to_Connecting()
        {
            var connection = studioConnectionFactory.CreateStudioConnection(output, input);

            outputComponentMock.Raise(c => c.OutputConnectionStateUpdated += null, outputComponentMock.Object, (output, connection.Id, ConnectionState.Connecting));

            Assert.IsTrue(connection.State == ConnectionState.Connecting);
        }

        [DataTestMethod]
        [DataRow(ConnectionState.Disconnected, ConnectionState.Disconnected, ConnectionState.Disconnected)]
        [DataRow(ConnectionState.Connected, ConnectionState.Connected, ConnectionState.Connected)]
        [DataRow(ConnectionState.Connected, ConnectionState.Connecting, ConnectionState.Connecting)]
        [DataRow(ConnectionState.Connected, ConnectionState.Disconnecting, ConnectionState.Disconnecting)]
        [DataRow(ConnectionState.Connecting, ConnectionState.Connected, ConnectionState.Connecting)]
        [DataRow(ConnectionState.Connecting, ConnectionState.Connecting, ConnectionState.Connecting)]
        [DataRow(ConnectionState.Connecting, ConnectionState.Disconnected, ConnectionState.Connecting)]
        [DataRow(ConnectionState.Disconnected, ConnectionState.Disconnected, ConnectionState.Disconnected)]
        [DataRow(ConnectionState.Disconnected, ConnectionState.Connecting, ConnectionState.Connecting)]
        [DataRow(ConnectionState.Disconnected, ConnectionState.Disconnecting, ConnectionState.Disconnecting)]
        [DataRow(ConnectionState.Disconnected, ConnectionState.Destroyed, ConnectionState.Disconnected)]
        [DataRow(ConnectionState.Disconnecting, ConnectionState.Disconnected, ConnectionState.Disconnecting)]
        [DataRow(ConnectionState.Disconnecting, ConnectionState.Disconnecting, ConnectionState.Disconnecting)]
        [DataRow(ConnectionState.Disconnecting, ConnectionState.Connected, ConnectionState.Disconnecting)]
        [DataRow(ConnectionState.Disconnecting, ConnectionState.Destroyed, ConnectionState.Disconnecting)]
        [DataRow(ConnectionState.Destroyed, ConnectionState.Destroyed, ConnectionState.Destroyed)]
        [DataRow(ConnectionState.Destroyed, ConnectionState.Disconnected, ConnectionState.Disconnected)]
        [DataRow(ConnectionState.Destroyed, ConnectionState.Disconnecting, ConnectionState.Disconnecting)]
        [DataRow(ConnectionState.Connected, ConnectionState.Disconnected, ConnectionState.Unknown)]
        public void Changes_the_ConnectionState_according_to_the_endpoints_ConnectionState_updates(ConnectionState inputState, ConnectionState outputState, ConnectionState resultingState)
        {
            var connection = studioConnectionFactory.CreateStudioConnection(output, input);

            inputComponentMock.Raise(c => c.InputConnectionStateUpdated += null, inputComponentMock.Object, (input, connection.Id, inputState));
            outputComponentMock.Raise(c => c.OutputConnectionStateUpdated += null, outputComponentMock.Object, (output, connection.Id, outputState));

            Assert.IsTrue(connection.State == resultingState);
        }

        [DataTestMethod]
        [DataRow(ConnectionState.Connected, ConnectionState.Connected, true)]
        [DataRow(ConnectionState.Connecting, ConnectionState.Disconnected, false)]
        [DataRow(ConnectionState.Destroyed, ConnectionState.Destroyed, true)]
        [DataRow(ConnectionState.Disconnected, ConnectionState.Disconnected, false)] // Initial TargetState is Disconnected.
        [DataRow(ConnectionState.Disconnecting, ConnectionState.Disconnected, false)]
        [DataRow(ConnectionState.Unknown, ConnectionState.Disconnected, false)]
        public void Sets_all_valid_target_states(ConnectionState targetState, ConnectionState resultingTargetState, bool doesInvokeHandleMethod)
        {
            var connection = studioConnectionFactory.CreateStudioConnection(output, input);

            connection.SetTargetState(targetState);
            if (doesInvokeHandleMethod)
                inputComponentMock.Verify(c => c.HandleConnectionTargetStateChanged(input, connection, targetState), Times.Once);
            else
                inputComponentMock.Verify(c => c.HandleConnectionTargetStateChanged(input, connection, targetState), Times.Never);
            Assert.AreEqual(resultingTargetState, connection.TargetState);
        }

        [DataTestMethod]
        [DataRow(ConnectionState.Connected)]
        [DataRow(ConnectionState.Connecting)]
        [DataRow(ConnectionState.Disconnected)]
        [DataRow(ConnectionState.Disconnecting)]
        [DataRow(ConnectionState.Unknown)]
        public void Does_not_change_the_target_state_after_it_was_once_set_to_Destroyed(ConnectionState state)
        {
            var connection = studioConnectionFactory.CreateStudioConnection(output, input);
            connection.SetTargetState(ConnectionState.Destroyed);

            connection.SetTargetState(state);

            Assert.IsTrue(connection.TargetState == ConnectionState.Destroyed);
        }
    }
}
