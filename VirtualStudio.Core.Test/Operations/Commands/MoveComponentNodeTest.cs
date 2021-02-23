using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Arrangement;
using VirtualStudio.Core.Operations;

namespace VirtualStudio.Core.Test.Operations
{
    [TestClass]
    public class MoveComponentNodeTest
    {
        VirtualStudioWithArrangement virtualStudioWithArrangement;
        ComponentNode componentNode;

        [TestInitialize]
        public void Init()
        {
            virtualStudioWithArrangement = new VirtualStudioWithArrangement();
            var placeholder = new PlaceholderStudioComponent();
            virtualStudioWithArrangement.ComponentRepository.AddPlaceholder(placeholder);
            componentNode = virtualStudioWithArrangement.AddComponent(placeholder, new Position2D(0, 0));
        }

        [DataTestMethod]
        [DataRow(1, 2)]
        [DataRow(-50, 26)]
        public async Task Moves_componentNode_to_provided_position(int x, int y)
        {
            var position = new Position2D(x, y);
            await new MoveComponentNodeCommand(componentNode.Id, position).Process(virtualStudioWithArrangement);

            Assert.AreEqual(position, componentNode.Position);
        }

        [TestMethod]
        public async Task Sets_Error_to_NotFound_when_the_ID_does_not_exist()
        {
            var moveCommand = new MoveComponentNodeCommand(999, new Position2D());
            await moveCommand.Process(virtualStudioWithArrangement);

            Assert.IsTrue(moveCommand.Error.Type == ErrorType.NotFound);
        }
    }
}
