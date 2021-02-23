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
    public class AddOutputToPlaceholderTest
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
        public async Task Adds_an_Output_to_a_PlaceholderComponent_in_ComponentRepository()
        {
            var placeholder = virtualStudio.ComponentRepository.Placeholders.First();
            var newOutput = new StudioComponentEndpointDto { Id = 0, Name = "New Output", DataKind = DataKind.Audio, ConnectionType = "UDP" };

            var addOutputToPlaceholderCommand = new AddOutputToPlaceholderCommand(placeholder.Id, newOutput);
            await addOutputToPlaceholderCommand.Process(virtualStudio);

            Assert.IsTrue(placeholder.Outputs.Count == 1);
            Assert.AreEqual(newOutput.Name, placeholder.Outputs[0].Name);
            Assert.AreEqual(EndpointIOType.Output, placeholder.Outputs[0].IOType);
            Assert.AreEqual(newOutput.DataKind, placeholder.Outputs[0].DataKind);
            Assert.AreEqual(newOutput.ConnectionType, placeholder.Outputs[0].ConnectionType);
        }

        [TestMethod]
        public async Task Adds_an_Output_to_a_PlaceholderComponent_in_Components_of_VirtualStudio()
        {
            var placeholder = virtualStudio.ComponentRepository.Placeholders.First();
            var addedPlaceholder = virtualStudio.AddComponent(placeholder);
            var newOutput = new StudioComponentEndpointDto { Id = 0, Name = "New Output", DataKind = DataKind.Audio, ConnectionType = "UDP" };

            var addOutputToPlaceholderCommand = new AddOutputToPlaceholderCommand(addedPlaceholder.Id, newOutput);
            await addOutputToPlaceholderCommand.Process(virtualStudio);

            Assert.IsTrue(addedPlaceholder.Outputs.Count == 1);
            Assert.IsTrue(placeholder.Id != addedPlaceholder.Id);
            Assert.AreEqual(newOutput.Name, addedPlaceholder.Outputs[0].Name);
            Assert.AreEqual(EndpointIOType.Output, addedPlaceholder.Outputs[0].IOType);
            Assert.AreEqual(newOutput.DataKind, addedPlaceholder.Outputs[0].DataKind);
            Assert.AreEqual(newOutput.ConnectionType, addedPlaceholder.Outputs[0].ConnectionType);
        }
    }
}
