using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using VirtualStudio.Core.Abstractions;
using System.Reflection;
using System.Linq;
using VirtualStudio.Core.Arrangement;
using VirtualStudio.Shared.Abstractions;
using VirtualStudio.Shared.DTOs;
using VirtualStudio.Shared;

namespace VirtualStudio.Core.Test
{
    [TestClass]
    public class VirtualStudioEventProcessorTest
    {
        const string virtualStudioName = "New virtual studio";
        VirtualStudio virtualStudio;
        Mock<IVirtualStudioUpdateListener> operationHandlerMock;
        VirtualStudioEventProcessor virtualStudioEventProcessor;
        PlaceholderStudioComponent placeholderInRepository;
        IStudioComponent placeholderInComponents;
        Mock<IStudioComponent> client1Mock;
        int client1Id;
        Mock<IStudioComponent> client2Mock;
        int client2Id;


        [TestInitialize]
        public void TestInit()
        {
            var virtualStudio = new VirtualStudioWithArrangement();

            // Create a placeholder and add it to ComponentRepository and add a copy to the VirtualStudio's components.
            placeholderInRepository = new PlaceholderStudioComponent();
            placeholderInRepository.SetName("Placeholder 1");
            placeholderInRepository.AddInput("Input 1", Shared.DataKind.Audio, "WebRTC");
            placeholderInRepository.AddOutput("Output 1", Shared.DataKind.Audio, "WebRTC");
            virtualStudio.ComponentRepository.AddPlaceholder(placeholderInRepository);
            placeholderInComponents = virtualStudio.AddComponent(placeholderInRepository);

            // Create a component mock and add it to ComponentRepository AND the VirtualStudio.
            client1Mock = new Mock<IStudioComponent>();
            client1Mock.Setup(m => m.SetId(It.IsAny<int>())).Callback<int>((value) => client1Id = value);
            client1Mock.SetupGet(m => m.Id).Returns(() => client1Id);
            virtualStudio.ComponentRepository.AddClient(client1Mock.Object);
            virtualStudio.AddComponent(client1Mock.Object);

            // Create a component mock and add it only to ComponentRepository.
            client2Mock = new Mock<IStudioComponent>();
            client2Mock.Setup(m => m.SetId(It.IsAny<int>())).Callback<int>((value) => client2Id = value);
            client2Mock.SetupGet(m => m.Id).Returns(() => client2Id);
            virtualStudio.ComponentRepository.AddClient(client2Mock.Object);

            this.virtualStudio = virtualStudio;

            operationHandlerMock = new Mock<IVirtualStudioUpdateListener>();
            virtualStudioEventProcessor = new VirtualStudioEventProcessor(virtualStudio, virtualStudioName, operationHandlerMock.Object);
        }

        [DataTestMethod]
        [DataRow(0, 0)]
        [DataRow(-1, -1)]
        [DataRow(1, 1)]
        [DataRow(25, -48)]
        public void Calls_AddPlaceholderNode_when_a_placeholder_gets_added(float posX, float posY)
        {
            var componentToBeAdded = placeholderInRepository;
            operationHandlerMock.Setup(m => m.AddPlaceholderNode(virtualStudioName, It.IsAny<StudioComponentDto>(), posX, posY)).Verifiable();

            (virtualStudio as VirtualStudioWithArrangement).AddComponent(componentToBeAdded, new Position2D(posX, posY));

            operationHandlerMock.Verify();
        }

        [DataTestMethod]
        [DataRow(0, 0)]
        [DataRow(-1, -1)]
        [DataRow(1, 1)]
        [DataRow(25, -48)]
        public void Calls_AddComponentNode_when_a_client_gets_added(float posX, float posY)
        {
            var componentToBeAdded = client2Mock.Object;
            operationHandlerMock.Setup(m => m.AddComponentNode(virtualStudioName, componentToBeAdded.Id, posX, posY)).Verifiable();

            (virtualStudio as VirtualStudioWithArrangement).AddComponent(componentToBeAdded, new Position2D(posX, posY));

            operationHandlerMock.Verify();
        }

        [TestMethod]
        public void Calls_RemoveComponent_when_a_placeholder_gets_removed()
        {
            var componentToBeRemoved = placeholderInComponents;
            operationHandlerMock.Setup(m => m.RemoveComponent(virtualStudioName, componentToBeRemoved.Id)).Verifiable();

            virtualStudio.RemoveComponent(componentToBeRemoved);

            operationHandlerMock.Verify();
        }

        [TestMethod]
        public void Calls_RemoveComponent_when_a_client_gets_removed()
        {
            var componentToBeRemoved = client1Mock.Object;
            operationHandlerMock.Setup(m => m.RemoveComponent(virtualStudioName, componentToBeRemoved.Id)).Verifiable();

            virtualStudio.RemoveComponent(componentToBeRemoved);

            operationHandlerMock.Verify();
        }

        [TestMethod]
        public void Calls_AddPlaceholderToRepository_when_a_placeholder_gets_added_to_ComponentRepository()
        {
            var placeholderToBeAdded = new PlaceholderStudioComponent();
            operationHandlerMock.Setup(m => m.AddPlaceholderToRepository(virtualStudioName, It.IsAny<StudioComponentDto>())).Verifiable();

            virtualStudio.ComponentRepository.AddPlaceholder(placeholderToBeAdded);

            operationHandlerMock.Verify();
        }

        [TestMethod]
        public void Calls_RemovePlaceholderFromRepository_when_a_placeholder_gets_removed_from_ComponentRepository()
        {
            var placeholderToBeRemoved = placeholderInRepository;
            operationHandlerMock.Setup(m => m.RemovePlaceholderFromRepository(virtualStudioName, placeholderToBeRemoved.Id)).Verifiable();

            virtualStudio.ComponentRepository.RemovePlaceholder(placeholderToBeRemoved);

            operationHandlerMock.Verify();
        }

        [TestMethod]
        public void Calls_AddClientToRepository_when_a_client_gets_added_to_ComponentRepository()
        {
            var clientMock = new Mock<IStudioComponent>();
            int clientId = 0;
            clientMock.Setup(m => m.SetId(It.IsAny<int>())).Callback<int>((value) => clientId = value);
            clientMock.SetupGet(m => m.Id).Returns(() => clientId);
            operationHandlerMock.Setup(m => m.AddClientToRepository(virtualStudioName, It.IsAny<StudioComponentDto>())).Verifiable();

            virtualStudio.ComponentRepository.AddClient(clientMock.Object);

            operationHandlerMock.Verify();
        }

        [TestMethod]
        public void Calls_RemoveClientFromRepository_when_a_client_gets_removed_from_ComponentRepository()
        {
            operationHandlerMock.Setup(m => m.RemoveClientFromRepository(virtualStudioName, client2Mock.Object.Id)).Verifiable();

            virtualStudio.ComponentRepository.RemoveClient(client2Mock.Object);

            operationHandlerMock.Verify();
        }

        [TestMethod]
        public void Calls_CreateConnection_when_a_connection_gets_added()
        {
            var input = placeholderInComponents.Inputs[0];
            var output = placeholderInComponents.Outputs[0];
            operationHandlerMock.Setup(m => m.CreateConnection(virtualStudioName, It.IsAny<StudioConnectionDto>())).Verifiable();

            virtualStudio.CreateConnection(output, input);

            operationHandlerMock.Verify();
        }

        [TestMethod]
        public void Calls_RemoveConnection_when_a_connection_gets_removed()
        {
            var input = placeholderInComponents.Inputs[0];
            var output = placeholderInComponents.Outputs[0];
            operationHandlerMock.Setup(m => m.RemoveConnection(virtualStudioName, 1)).Verifiable();
            var connection = virtualStudio.CreateConnection(output, input);

            virtualStudio.RemoveConnection(connection);

            operationHandlerMock.Verify();
        }

        [DataTestMethod]
        [DataRow(ConnectionState.Connected)]
        [DataRow(ConnectionState.Destroyed)]
        [DataRow(ConnectionState.Disconnecting)]
        [DataRow(ConnectionState.Connecting)]
        [DataRow(ConnectionState.Unknown)]
        public void Calls_ChangeConnectionState_when_a_connection_state_gets_changed(ConnectionState connectionState)
        {
            var componentMock = new Mock<IStudioComponent>();
            var input = new StudioComponentInput(1, "input", DataKind.Audio, "WebRTC", componentMock.Object);
            var output = new StudioComponentOutput(2, "output", DataKind.Audio, "WebRTC", componentMock.Object);
            componentMock.Setup(m => m.Inputs).Returns(new List<StudioComponentInput> { input });
            componentMock.Setup(m => m.Outputs).Returns(new List<StudioComponentOutput> { output });
            virtualStudio.ComponentRepository.AddClient(componentMock.Object);
            virtualStudio.AddComponent(componentMock.Object);
            var connection = virtualStudio.CreateConnection(output, input);
            operationHandlerMock.Setup(m => m.ChangeConnectionState(virtualStudioName, 1, connectionState)).Verifiable();

            componentMock.Raise(m => m.InputConnectionStateUpdated += null, null, (input, connection, connectionState));
            componentMock.Raise(m => m.OutputConnectionStateUpdated += null, null, (output, connection, connectionState));

            operationHandlerMock.Verify();
        }

        [TestMethod]
        public void Calls_ChangeComponentProperty_when_a_components_property_gets_changed()
        {
            var component = placeholderInRepository;
            string newName = "gojisjgfod";
            operationHandlerMock.Setup(m => m.ChangeComponentProperty(virtualStudioName, component.Id, nameof(component.Name), newName)).Verifiable();

            component.SetName(newName);

            operationHandlerMock.Verify();
        }

        [TestMethod]
        public void Calls_AddInputToComponent_when_an_input_gets_added_to_a_placeholder_in_components()
        {
            var component = placeholderInComponents;
            operationHandlerMock.Setup(m => m.AddInputToComponent(virtualStudioName, component.Id, It.IsAny<StudioComponentEndpointDto>())).Verifiable();

            (component as PlaceholderStudioComponent).AddInput("input", DataKind.Audio, "WebRTC");

            operationHandlerMock.Verify();
        }

        [TestMethod]
        public void Calls_AddInputToComponent_when_an_input_gets_added_to_a_placeholder_in_repository()
        {
            var component = placeholderInRepository;
            operationHandlerMock.Setup(m => m.AddInputToComponent(virtualStudioName, component.Id, It.IsAny<StudioComponentEndpointDto>())).Verifiable();

            (component as PlaceholderStudioComponent).AddInput("input", DataKind.Audio, "WebRTC");

            operationHandlerMock.Verify();
        }

        [TestMethod]
        public void Calls_AddInputToComponent_when_an_input_gets_added_to_a_component_in_components()
        {
            operationHandlerMock.Setup(m => m.AddInputToComponent(virtualStudioName, client1Mock.Object.Id, It.IsAny<StudioComponentEndpointDto>())).Verifiable();

            client1Mock.Raise(m => m.InputAdded += null, null, new StudioComponentInput(1, "input", DataKind.Audio, "WebRTC", client1Mock.Object));

            operationHandlerMock.Verify();
        }

        [TestMethod]
        public void Calls_AddInputToComponent_when_an_input_gets_added_to_a_component_in_repository()
        {
            operationHandlerMock.Setup(m => m.AddInputToComponent(virtualStudioName, client2Mock.Object.Id, It.IsAny<StudioComponentEndpointDto>())).Verifiable();

            client2Mock.Raise(m => m.InputAdded += null, null, new StudioComponentInput(1, "input", DataKind.Audio, "WebRTC", client2Mock.Object));

            operationHandlerMock.Verify();
        }

        [TestMethod]
        public void Calls_AddOutputToComponent_when_an_output_gets_added_to_a_placeholder_in_components()
        {
            var component = placeholderInComponents;
            operationHandlerMock.Setup(m => m.AddOutputToComponent(virtualStudioName, component.Id, It.IsAny<StudioComponentEndpointDto>())).Verifiable();

            (component as PlaceholderStudioComponent).AddOutput("output", DataKind.Audio, "WebRTC");

            operationHandlerMock.Verify();
        }

        [TestMethod]
        public void Calls_AddOutputToComponent_when_an_output_gets_added_to_a_placeholder_in_repository()
        {
            var component = placeholderInRepository;
            operationHandlerMock.Setup(m => m.AddInputToComponent(virtualStudioName, component.Id, It.IsAny<StudioComponentEndpointDto>())).Verifiable();

            (component as PlaceholderStudioComponent).AddInput("input", DataKind.Audio, "WebRTC");

            operationHandlerMock.Verify();
        }

        [TestMethod]
        public void Calls_AddOutputToComponent_when_an_output_gets_added_to_a_component_in_components()
        {
            operationHandlerMock.Setup(m => m.AddOutputToComponent(virtualStudioName, client1Mock.Object.Id, It.IsAny<StudioComponentEndpointDto>())).Verifiable();

            client1Mock.Raise(m => m.OutputAdded += null, null, new StudioComponentOutput(1, "output", DataKind.Audio, "WebRTC", client1Mock.Object));

            operationHandlerMock.Verify();
        }

        [TestMethod]
        public void Calls_AddOutputToComponent_when_an_output_gets_added_to_a_component_in_repository()
        {
            operationHandlerMock.Setup(m => m.AddOutputToComponent(virtualStudioName, client2Mock.Object.Id, It.IsAny<StudioComponentEndpointDto>())).Verifiable();

            client2Mock.Raise(m => m.OutputAdded += null, null, new StudioComponentOutput(1, "output", DataKind.Audio, "WebRTC", client2Mock.Object));

            operationHandlerMock.Verify();
        }

        [TestMethod]
        public void Calls_RemoveInputFromComponent_when_an_input_gets_removed_from_a_placeholder()
        {
            operationHandlerMock.Setup(m => m.RemoveInputFromComponent(virtualStudioName, placeholderInComponents.Id, placeholderInComponents.Inputs[0].Id)).Verifiable();

            (placeholderInComponents as PlaceholderStudioComponent).RemoveInput(placeholderInComponents.Inputs[0]);

            operationHandlerMock.Verify();
        }

        [TestMethod]
        public void Calls_RemoveOutputFromComponent_when_an_output_gets_removed_from_a_placeholder()
        {
            operationHandlerMock.Setup(m => m.RemoveOutputFromComponent(virtualStudioName, placeholderInComponents.Id, placeholderInComponents.Outputs[0].Id)).Verifiable();

            (placeholderInComponents as PlaceholderStudioComponent).RemoveOutput(placeholderInComponents.Outputs[0]);

            operationHandlerMock.Verify();
        }

        [DataTestMethod]
        [DataRow(0, 0)]
        [DataRow(-1, -1)]
        [DataRow(1, 1)]
        [DataRow(25, -48)]
        public void Calls_MoveComponentNode_when_a_ComponentNodes_position_gets_changed(float posX, float posY)
        {
            var componentNode = (virtualStudio as VirtualStudioWithArrangement).ComponentNodes.First();
            operationHandlerMock.Setup(m => m.MoveComponentNode(virtualStudioName, componentNode.Id, posX, posY)).Verifiable();

            componentNode.Position = new Position2D(posX, posY);

            operationHandlerMock.Verify();
        }

        [DataTestMethod]
        [DataRow(0, 0)]
        [DataRow(-1, -1)]
        [DataRow(1, 1)]
        [DataRow(25, -48)]
        public void Calls_MoveComponentNode_when_a_ComponentNodes_position_gets_changed_via_the_VirtualStudio(float posX, float posY)
        {
            var componentNode = (virtualStudio as VirtualStudioWithArrangement).ComponentNodes.First();
            operationHandlerMock.Setup(m => m.MoveComponentNode(virtualStudioName, componentNode.Id, posX, posY)).Verifiable();

            (virtualStudio as VirtualStudioWithArrangement).MoveComponentNode(componentNode, new Position2D(posX, posY));

            operationHandlerMock.Verify();
        }
    }
}
