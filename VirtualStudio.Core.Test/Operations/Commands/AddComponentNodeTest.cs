using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Core.Arrangement;
using VirtualStudio.Core.Operations;
using VirtualStudio.Shared;

namespace VirtualStudio.Core.Test.Operations
{
    [TestClass]
    public class AddComponentNodeTest
    {
        VirtualStudioWithArrangement virtualStudio;

        [TestInitialize]
        public async Task Init()
        {
            virtualStudio = new VirtualStudioWithArrangement();
            var placeholderComponent = new PlaceholderStudioComponent();
            placeholderComponent.SetName("New Placeholder");

            var addPlaceholderToRepositoryCommand = new AddPlaceholderToRepositoryCommand(placeholderComponent.ToDto());
            await addPlaceholderToRepositoryCommand.Process(virtualStudio);
        }

        [DataTestMethod]
        [DataRow(0, 0)]
        [DataRow(-1, -1)]
        [DataRow(-1, 1)]
        [DataRow(1, -1)]
        [DataRow(1, 1)]
        [DataRow(34546, 23546)]
        [DataRow(34546, -23546)]
        public async Task Adds_a_cloned_PlaceholderComponent_from_ComponentRepository(float x, float y)
        {
            var placeholder = virtualStudio.ComponentRepository.Placeholders.First();

            var addComponentCommand = new AddComponentNodeCommand(placeholder.Id, x, y);
            await addComponentCommand.Process(virtualStudio);

            Assert.IsTrue(virtualStudio.Components.Count == 1);
            var addedPlaceholder = virtualStudio.ComponentNodes.First();
            Assert.AreNotEqual(placeholder.Id, addedPlaceholder.Id);
            Assert.AreEqual(x, addedPlaceholder.Position.X);
            Assert.AreEqual(y, addedPlaceholder.Position.Y);
        }

        [TestMethod]
        public async Task Does_not_add_a_PlaceholderComponent_with_a_non_existing_Id_and_sets_Error_to_NotFound()
        {
            var addComponentCommand = new AddComponentNodeCommand(9999, 0, 0);
            await addComponentCommand.Process(virtualStudio);

            Assert.IsNotNull(addComponentCommand.Error);
            Assert.IsTrue(addComponentCommand.Error.Type == ErrorType.NotFound);
            Assert.IsTrue(virtualStudio.Components.Count == 0);
        }

        [DataTestMethod]
        [DataRow(0, 0)]
        [DataRow(-1, -1)]
        [DataRow(-1, 1)]
        [DataRow(1, -1)]
        [DataRow(1, 1)]
        [DataRow(34546, 23546)]
        [DataRow(34546, -23546)]
        public async Task Adds_a_ClientComponent_from_ComponentRepository(float x, float y)
        {
            var clientComponentMock = new Mock<IStudioComponent>();
            int clientComponentMockId = 2;
            clientComponentMock.SetupGet(c => c.Id).Returns(clientComponentMockId);
            virtualStudio.ComponentRepository.AddClient(clientComponentMock.Object);

            var addComponentCommand = new AddComponentNodeCommand(clientComponentMockId, x, y);
            await addComponentCommand.Process(virtualStudio);

            Assert.IsTrue(virtualStudio.Components.Count == 1);
            var addedComponentNode = virtualStudio.ComponentNodes.First();
            Assert.AreEqual(clientComponentMockId, addedComponentNode.Id);
            Assert.AreEqual(x, addedComponentNode.Position.X);
            Assert.AreEqual(y, addedComponentNode.Position.Y);
        }

        [TestMethod]
        public async Task Adds_a_ClientComponent_after_it_got_removed()
        {
            var clientComponentMock = new Mock<IStudioComponent>();
            int clientComponentMockId = 2;
            clientComponentMock.SetupGet(c => c.Id).Returns(clientComponentMockId);
            virtualStudio.ComponentRepository.AddClient(clientComponentMock.Object);
            await new AddComponentNodeCommand(clientComponentMockId, 0, 0).Process(virtualStudio);
            await new RemoveComponentCommand(clientComponentMockId).Process(virtualStudio);

            await new AddComponentNodeCommand(clientComponentMockId, 0, 0).Process(virtualStudio);

            Assert.IsTrue(virtualStudio.Components.Count == 1);
            Assert.AreEqual(clientComponentMockId, virtualStudio.Components.First().Id);
        }

        [TestMethod]
        public async Task Does_not_add_a_ClientComponent_that_already_exists_in_Components_and_sets_Error_to_InvalidOperation()
        {
            var clientComponentMock = new Mock<IStudioComponent>();
            int clientComponentMockId = 2;
            clientComponentMock.SetupGet(c => c.Id).Returns(clientComponentMockId);
            virtualStudio.ComponentRepository.AddClient(clientComponentMock.Object);
            await new AddComponentNodeCommand(clientComponentMockId, 0, 0).Process(virtualStudio);

            var addComponentCommand = new AddComponentNodeCommand(clientComponentMockId, 0, 0);
            await addComponentCommand.Process(virtualStudio);

            Assert.IsTrue(virtualStudio.Components.Count == 1);
            Assert.IsTrue(addComponentCommand.Error.Type == ErrorType.InvalidOperation);
        }
    }
}
