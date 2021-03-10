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
    public class RemovePlaceholderFromRepositoryTest
    {
        VirtualStudio virtualStudio;
        PlaceholderStudioComponent placeholder;

        [TestInitialize]
        public void Init()
        {
            virtualStudio = new VirtualStudio();
            placeholder = new PlaceholderStudioComponent();
            virtualStudio.ComponentRepository.AddPlaceholder(placeholder);
        }

        [TestMethod]
        public async Task Removes_Placeholder_from_ComponentRepository()
        {
            await new RemovePlaceholderFromRepositoryCommand(placeholder.Id).Process(virtualStudio);

            Assert.IsTrue(virtualStudio.ComponentRepository.Placeholders.Count == 0);
        }

        [TestMethod]
        public async Task Sets_Error_to_NotFound_when_ID_does_not_exist()
        {
            var removeCommand = new RemovePlaceholderFromRepositoryCommand(999);
            await removeCommand.Process(virtualStudio);

            Assert.IsTrue(removeCommand.Error.Type == ErrorType.NotFound);
        }
    }
}
