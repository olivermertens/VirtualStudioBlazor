using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualStudio.Core.Arrangement;

namespace VirtualStudio.Core.Test
{
    [TestClass]
    public class VirtualStudioArrangementTest
    {
        [TestMethod]
        public void Adds_component_to_Components_and_ComponentNodes()
        {
            var component = new PlaceholderStudioComponent();
            var arrangement = new VirtualStudioWithArrangement();
            arrangement.ComponentRepository.AddPlaceholder(component);

            arrangement.AddComponent(component);

            Assert.IsTrue(arrangement.Components.Count == arrangement.ComponentNodes.Count);
            Assert.IsTrue(arrangement.Components.Count == 1);
        }

        [TestMethod]
        public void Does_not_add_a_component_that_does_not_exist_in_ComponentRepository()
        {
            var component = new PlaceholderStudioComponent();
            var arrangement = new VirtualStudioWithArrangement();

            var addedComponent = arrangement.AddComponent(component);

            Assert.IsNull(addedComponent);
            Assert.IsTrue(arrangement.Components.Count == 0);
        }

        [TestMethod]
        public void Removes_component_from_Components_and_ComponentNodes()
        {
            var component = new PlaceholderStudioComponent();
            var arrangement = new VirtualStudioWithArrangement();
            arrangement.ComponentRepository.AddPlaceholder(component);
            var addedComponent = arrangement.AddComponent(component);

            arrangement.RemoveComponent(addedComponent);

            Assert.IsTrue(arrangement.Components.Count == arrangement.ComponentNodes.Count);
            Assert.IsTrue(arrangement.Components.Count == 0);
        }

        [TestMethod]
        public void Changes_position_of_ComponentNode()
        {
            var component = new PlaceholderStudioComponent();
            var arrangement = new VirtualStudioWithArrangement();
            arrangement.ComponentRepository.AddPlaceholder(component);
            arrangement.AddComponent(component);
            var componentNode = arrangement.ComponentNodes.First();
            var targetPosition = new Position2D(12, 34);

            arrangement.MoveComponentNode(componentNode, targetPosition);

            Assert.AreEqual(targetPosition, arrangement.ComponentNodes.First().Position);
        }

        [TestMethod]
        public void Adds_component_at_provided_position()
        {
            var component = new PlaceholderStudioComponent();
            var targetPosition = new Position2D(12, 34);
            var arrangement = new VirtualStudioWithArrangement();
            arrangement.ComponentRepository.AddPlaceholder(component);

            arrangement.AddComponent(component, targetPosition);

            Assert.IsTrue(arrangement.ComponentNodes.Count == 1);
            Assert.AreEqual(targetPosition, arrangement.ComponentNodes.First().Position);
        }

        [TestMethod]
        public void Does_not_add_a_ComponentNode_or_Component_if_the_contained_component_already_exists()
        {
            var component = new PlaceholderStudioComponent();
            var targetPosition = new Position2D(12, 34);
            var arrangement = new VirtualStudioWithArrangement();
            arrangement.ComponentRepository.AddPlaceholder(component);
            var existingComponent = arrangement.AddComponent(component, targetPosition).Component;

            arrangement.AddComponent(existingComponent, targetPosition);

            Assert.IsTrue(arrangement.ComponentNodes.Count == arrangement.Components.Count);
            Assert.IsTrue(arrangement.ComponentNodes.Count == 1);
        }
    }
}
