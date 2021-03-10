using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Operations;
using VirtualStudio.Shared;

namespace VirtualStudio.Core.Test.Operations
{
    [TestClass]
    public class RemoveComponentTest
    {
        VirtualStudio virtualStudio;

        [TestInitialize]
        public void Init()
        {
            virtualStudio = new VirtualStudio();
            var placeholder = new PlaceholderStudioComponent();
            virtualStudio.ComponentRepository.AddPlaceholder(placeholder);
            virtualStudio.AddComponent(placeholder);
        }

        [TestMethod]
        public async Task Removes_a_Component()
        {
            var component = virtualStudio.Components.First();

            await new RemoveComponentCommand(component.Id).Process(virtualStudio);

            Assert.IsTrue(virtualStudio.Components.Count == 0);
        }

        [TestMethod]
        public async Task Sets_Error_to_NotFound_when_ID_does_not_exist()
        {
            var removeCommand = new RemoveComponentCommand(999);
            await removeCommand.Process(virtualStudio);

            Assert.IsTrue(removeCommand.Error.Type == ErrorType.NotFound);
            Assert.IsTrue(virtualStudio.Components.Count == 1);
        }
    }
}
