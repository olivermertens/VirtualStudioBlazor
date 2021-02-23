using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Operations;
using VirtualStudio.Shared;

namespace VirtualStudio.Core.Test.Operations
{
    [TestClass]
    public class RemoveOutputFromPlaceholderComponentTest
    {
        VirtualStudio virtualStudio;
        PlaceholderStudioComponent placeholderInRepository;
        PlaceholderStudioComponent placeholderInComponents;

        [TestInitialize]
        public void Init()
        {
            virtualStudio = new VirtualStudio();
            placeholderInRepository = new PlaceholderStudioComponent();
            placeholderInRepository.SetName("New Placeholder");
            placeholderInRepository.AddOutput("Output 1", DataKind.Other, "hgdfhgoid");
            virtualStudio.ComponentRepository.AddPlaceholder(placeholderInRepository);
            placeholderInComponents = virtualStudio.AddComponent(placeholderInRepository) as PlaceholderStudioComponent;
        }

        [TestMethod]
        public async Task Removes_the_input_from_PlaceholderComponent_in_ComponentRepository()
        {
            var removeOutputCommand = new RemoveOutputFromPlaceholderCommand(placeholderInRepository.Id, placeholderInRepository.Outputs[0].Id);
            await removeOutputCommand.Process(virtualStudio);

            Assert.IsNull(removeOutputCommand.Error);
            Assert.IsTrue(placeholderInRepository.Outputs.Count == 0);
        }

        [TestMethod]
        public async Task Removes_the_input_from_PlaceholderComponent_in_Components()
        {
            var removeOutputCommand = new RemoveOutputFromPlaceholderCommand(placeholderInComponents.Id, placeholderInComponents.Outputs[0].Id);
            await removeOutputCommand.Process(virtualStudio);

            Assert.IsNull(removeOutputCommand.Error);
            Assert.IsTrue(placeholderInComponents.Outputs.Count == 0);
        }

        [TestMethod]
        public async Task Sets_Error_to_NotFound_when_the_Components_ID_does_not_exist()
        {
            var removeOutputCommand = new RemoveOutputFromPlaceholderCommand(999, 1);
            await removeOutputCommand.Process(virtualStudio);

            Assert.IsTrue(removeOutputCommand.Error.Type == ErrorType.NotFound);
        }

        [TestMethod]
        public async Task Sets_Error_to_NotFound_when_the_Outputs_ID_does_not_exist()
        {
            var removeOutputCommand = new RemoveOutputFromPlaceholderCommand(placeholderInComponents.Id, 999);
            await removeOutputCommand.Process(virtualStudio);

            Assert.IsTrue(removeOutputCommand.Error.Type == ErrorType.NotFound);
        }
    }
}
