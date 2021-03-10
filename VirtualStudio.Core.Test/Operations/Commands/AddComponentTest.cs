using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Core.Operations;
using VirtualStudio.Shared;

namespace VirtualStudio.Core.Test.Operations
{
    [TestClass]
    public class AddComponentTest
    {
        VirtualStudio virtualStudio;

        [TestInitialize]
        public async Task Init()
        {
            virtualStudio = new VirtualStudio();
            var placeholderComponent = new PlaceholderStudioComponent();
            placeholderComponent.SetName("New Placeholder");

            var addPlaceholderToRepositoryCommand = new AddPlaceholderToRepositoryCommand(placeholderComponent.ToDto());
            await addPlaceholderToRepositoryCommand.Process(virtualStudio);
        }

        [TestMethod]
        public async Task Adds_a_cloned_PlaceholderComponent_from_ComponentRepository()
        {
            var placeholder = virtualStudio.ComponentRepository.Placeholders.First();

            var addComponentCommand = new AddComponentCommand(placeholder.Id);
            await addComponentCommand.Process(virtualStudio);

            Assert.IsTrue(virtualStudio.Components.Count == 1);
            var addedPlaceholder = virtualStudio.Components.First();
            Assert.AreNotEqual(placeholder.Id, addedPlaceholder.Id);
        }

        [TestMethod]
        public async Task Does_not_add_a_PlaceholderComponent_with_a_non_existing_Id_and_sets_Error_to_NotFound()
        {
            var addComponentCommand = new AddComponentCommand(9999);
            await addComponentCommand.Process(virtualStudio);

            Assert.IsNotNull(addComponentCommand.Error);
            Assert.IsTrue(addComponentCommand.Error.Type == ErrorType.NotFound);
            Assert.IsTrue(virtualStudio.Components.Count == 0);
        }

        [TestMethod]
        public async Task Adds_a_ClientComponent_from_ComponentRepository()
        {
            var clientComponentMock = new Mock<IStudioComponent>();
            int clientComponentMockId = 2;
            clientComponentMock.SetupGet(c => c.Id).Returns(clientComponentMockId);
            virtualStudio.ComponentRepository.AddClient(clientComponentMock.Object);

            var addComponentCommand = new AddComponentCommand(clientComponentMockId);
            await addComponentCommand.Process(virtualStudio);

            Assert.IsTrue(virtualStudio.Components.Count == 1);
            Assert.AreEqual(clientComponentMockId, virtualStudio.Components.First().Id);
        }

        [TestMethod]
        public async Task Adds_a_ClientComponent_after_it_got_removed()
        {
            var clientComponentMock = new Mock<IStudioComponent>();
            int clientComponentMockId = 2;
            clientComponentMock.SetupGet(c => c.Id).Returns(clientComponentMockId);
            virtualStudio.ComponentRepository.AddClient(clientComponentMock.Object);
            await new AddComponentCommand(clientComponentMockId).Process(virtualStudio);
            await new RemoveComponentCommand(clientComponentMockId).Process(virtualStudio);

            await new AddComponentCommand(clientComponentMockId).Process(virtualStudio);

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
            await new AddComponentCommand(clientComponentMockId).Process(virtualStudio);

            var addComponentCommand = new AddComponentCommand(clientComponentMockId);
            await addComponentCommand.Process(virtualStudio);

            Assert.IsTrue(virtualStudio.Components.Count == 1);
            Assert.IsTrue(addComponentCommand.Error.Type == ErrorType.InvalidOperation);
        }
    }
}
