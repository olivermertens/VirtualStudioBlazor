using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Operations;
using VirtualStudio.Shared;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.Core.Test.Operations
{
    [TestClass]
    public class AddInputToPlaceholderTest
    {
        private VirtualStudio virtualStudio;

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
        public async Task Adds_an_Input_to_a_PlaceholderComponent_in_ComponentRepository()
        {
            var placeholder = virtualStudio.ComponentRepository.Placeholders.First();
            var newInput = new StudioComponentEndpointDto { Id = 0, Name = "New Input", DataKind = DataKind.Audio, ConnectionType = "UDP" };

            var addInputToPlaceholderCommand = new AddInputToPlaceholderCommand(placeholder.Id, newInput);
            await addInputToPlaceholderCommand.Process(virtualStudio);

            Assert.IsTrue(placeholder.Inputs.Count == 1);
            Assert.AreEqual(newInput.Name, placeholder.Inputs[0].Name);
            Assert.AreEqual(EndpointIOType.Input, placeholder.Inputs[0].IOType);
            Assert.AreEqual(newInput.DataKind, placeholder.Inputs[0].DataKind);
            Assert.AreEqual(newInput.ConnectionType, placeholder.Inputs[0].ConnectionType);
        }

        [TestMethod]
        public async Task Adds_an_Input_to_a_PlaceholderComponent_in_Components_of_VirtualStudio()
        {
            var placeholder = virtualStudio.ComponentRepository.Placeholders.First();
            var addedPlaceholder = virtualStudio.AddComponent(placeholder);
            var newInput = new StudioComponentEndpointDto { Id = 0, Name = "New Input", DataKind = DataKind.Audio, ConnectionType = "UDP" };

            var addInputToPlaceholderCommand = new AddInputToPlaceholderCommand(addedPlaceholder.Id, newInput);
            await addInputToPlaceholderCommand.Process(virtualStudio);

            Assert.IsTrue(addedPlaceholder.Inputs.Count == 1);
            Assert.IsTrue(placeholder.Id != addedPlaceholder.Id);
            Assert.AreEqual(newInput.Name, addedPlaceholder.Inputs[0].Name);
            Assert.AreEqual(EndpointIOType.Input, addedPlaceholder.Inputs[0].IOType);
            Assert.AreEqual(newInput.DataKind, addedPlaceholder.Inputs[0].DataKind);
            Assert.AreEqual(newInput.ConnectionType, addedPlaceholder.Inputs[0].ConnectionType);
        }
    }
}
