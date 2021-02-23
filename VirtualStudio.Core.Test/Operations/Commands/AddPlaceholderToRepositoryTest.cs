using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public class AddPlaceholderToRepositoryTest
    {
        private VirtualStudio virtualStudio;

        [TestInitialize]
        public void Init()
        {
            virtualStudio = new VirtualStudio();
        }

        [TestMethod]
        public async Task Adds_a_PlaceholderComponent_to_the_ComponentRepository()
        {
            var placeholderComponent = new PlaceholderStudioComponent();
            placeholderComponent.SetName("New Placeholder");
            placeholderComponent.AddInput("Input 1", DataKind.Video, "WebRtc");
            placeholderComponent.AddOutput("Output 1", DataKind.Video, "WebRtc");

            var addPlaceholderToRepositoryCommand = new AddPlaceholderToRepositoryCommand(placeholderComponent.ToDto());
            await addPlaceholderToRepositoryCommand.Process(virtualStudio);

            Assert.IsTrue(virtualStudio.ComponentRepository.Placeholders.Count == 1);
            var addedPlaceholder = virtualStudio.ComponentRepository.Placeholders.First();
            Assert.AreEqual(placeholderComponent.Name, addedPlaceholder.Name);
            Assert.AreEqual(placeholderComponent.Inputs.Count, addedPlaceholder.Inputs.Count);
            Assert.AreEqual(placeholderComponent.Outputs.Count, addedPlaceholder.Outputs.Count);
            Assert.AreEqual(placeholderComponent.Inputs[0].Id, addedPlaceholder.Inputs[0].Id);
            Assert.AreEqual(placeholderComponent.Inputs[0].Name, addedPlaceholder.Inputs[0].Name);
            Assert.AreEqual(placeholderComponent.Inputs[0].IOType, addedPlaceholder.Inputs[0].IOType);
            Assert.AreEqual(placeholderComponent.Inputs[0].DataKind, addedPlaceholder.Inputs[0].DataKind);
            Assert.AreEqual(placeholderComponent.Inputs[0].ConnectionType, addedPlaceholder.Inputs[0].ConnectionType);
            Assert.AreEqual(placeholderComponent.Outputs[0].Id, addedPlaceholder.Outputs[0].Id);
            Assert.AreEqual(placeholderComponent.Outputs[0].Name, addedPlaceholder.Outputs[0].Name);
            Assert.AreEqual(placeholderComponent.Outputs[0].IOType, addedPlaceholder.Outputs[0].IOType);
            Assert.AreEqual(placeholderComponent.Outputs[0].DataKind, addedPlaceholder.Outputs[0].DataKind);
            Assert.AreEqual(placeholderComponent.Outputs[0].ConnectionType, addedPlaceholder.Outputs[0].ConnectionType);
        }

        [TestMethod]
        public void Throws_NullReferenceException_when_passed_argument_is_null()
        {
            var addPlaceholderToRepositoryCommand = new AddPlaceholderToRepositoryCommand(null);
            Assert.ThrowsExceptionAsync<NullReferenceException>(async() => await addPlaceholderToRepositoryCommand.Process(virtualStudio));
        }
    }
}
