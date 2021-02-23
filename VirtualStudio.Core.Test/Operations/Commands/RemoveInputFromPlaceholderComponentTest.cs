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
    public class RemoveInputFromPlaceholderComponentTest
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
            placeholderInRepository.AddInput("Input 1", DataKind.Other, "hgdfhgoid");
            virtualStudio.ComponentRepository.AddPlaceholder(placeholderInRepository);
            placeholderInComponents = virtualStudio.AddComponent(placeholderInRepository) as PlaceholderStudioComponent;
        }

        [TestMethod]
        public async Task Removes_the_input_from_PlaceholderComponent_in_ComponentRepository()
        {
            var removeInputCommand = new RemoveInputFromPlaceholderCommand(placeholderInRepository.Id, placeholderInRepository.Inputs[0].Id);
            await removeInputCommand.Process(virtualStudio);

            Assert.IsNull(removeInputCommand.Error);
            Assert.IsTrue(placeholderInRepository.Inputs.Count == 0);
        }

        [TestMethod]
        public async Task Removes_the_input_from_PlaceholderComponent_in_Components()
        {
            var removeInputCommand = new RemoveInputFromPlaceholderCommand(placeholderInComponents.Id, placeholderInComponents.Inputs[0].Id);
            await removeInputCommand.Process(virtualStudio);

            Assert.IsNull(removeInputCommand.Error);
            Assert.IsTrue(placeholderInComponents.Inputs.Count == 0);
        }

        [TestMethod]
        public async Task Sets_Error_to_NotFound_when_the_Components_ID_does_not_exist()
        {
            var removeInputCommand = new RemoveInputFromPlaceholderCommand(999, 1);
            await removeInputCommand.Process(virtualStudio);

            Assert.IsTrue(removeInputCommand.Error.Type == ErrorType.NotFound);
        }

        [TestMethod]
        public async Task Sets_Error_to_NotFound_when_the_Inputs_ID_does_not_exist()
        {
            var removeInputCommand = new RemoveInputFromPlaceholderCommand(placeholderInComponents.Id, 999);
            await removeInputCommand.Process(virtualStudio);

            Assert.IsTrue(removeInputCommand.Error.Type == ErrorType.NotFound);
        }
    }
}
