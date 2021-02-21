using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core.Test
{
    [TestClass]
    public class VirtualStudioTest
    {
        [TestMethod]
        public void Adds_component()
        {
            var virtualStudio = new VirtualStudio();
            var component = new PlaceholderStudioComponent();

            virtualStudio.AddComponent(component);

            Assert.IsTrue(virtualStudio.Components.Count == 1);
        }

        [TestMethod]
        public void Removes_component()
        {
            var virtualStudio = new VirtualStudio();
            var component = new PlaceholderStudioComponent();
            virtualStudio.AddComponent(component);

            virtualStudio.RemoveComponent(component);

            Assert.IsTrue(virtualStudio.Components.Count == 0);
        }

        [TestMethod]
        public void Does_not_add_already_existing_component()
        {
            var virtualStudio = new VirtualStudio();
            var component = new PlaceholderStudioComponent();
            virtualStudio.AddComponent(component);

            virtualStudio.AddComponent(component);

            Assert.IsTrue(virtualStudio.Components.Count == 1);
        }

        [TestMethod]
        public void Adds_connection_between_endpoints_that_exist_in_components()
        {
            var virtualStudio = new VirtualStudio();
            var component = new PlaceholderStudioComponent();
            var input = component.AddInput("input", DataKind.Audio, "ConnectionType");
            var output = component.AddOutput("output", DataKind.Audio, "ConnectionType");
            virtualStudio.AddComponent(component);

            Assert.IsTrue(virtualStudio.CanCreateConnection(output, input));

            IStudioConnection connection = virtualStudio.CreateConnection(output, input);

            Assert.IsNotNull(connection);
            Assert.IsTrue(virtualStudio.Connections.Count == 1);
        }

        [TestMethod]
        public void Removes_connection()
        {
            var virtualStudio = new VirtualStudio();
            var component = new PlaceholderStudioComponent();
            var input = component.AddInput("input", DataKind.Audio, "ConnectionType");
            var output = component.AddOutput("output", DataKind.Audio, "ConnectionType");
            virtualStudio.AddComponent(component);
            IStudioConnection connection = virtualStudio.CreateConnection(output, input);

            virtualStudio.RemoveConnection(connection);

            Assert.IsTrue(virtualStudio.Connections.Count == 0);
        }

        [TestMethod]
        public void Removes_connection_when_a_related_component_gets_removed()
        {
            var virtualStudio = new VirtualStudio();
            var component = new PlaceholderStudioComponent();
            var input = component.AddInput("input", DataKind.Audio, "ConnectionType");
            var output = component.AddOutput("output", DataKind.Audio, "ConnectionType");
            virtualStudio.AddComponent(component);
            IStudioConnection connection = virtualStudio.CreateConnection(output, input);

            virtualStudio.RemoveComponent(component);

            Assert.IsTrue(virtualStudio.Connections.Count == 0);
        }

        [TestMethod]
        public void Removes_connection_when_a_related_endpoint_gets_removed_from_the_component()
        {
            var virtualStudio = new VirtualStudio();
            var component = new PlaceholderStudioComponent();
            var input = component.AddInput("input", DataKind.Audio, "ConnectionType");
            var output = component.AddOutput("output", DataKind.Audio, "ConnectionType");
            virtualStudio.AddComponent(component);
            IStudioConnection connection = virtualStudio.CreateConnection(output, input);

            component.RemoveOutput(output);

            Assert.IsTrue(virtualStudio.Connections.Count == 0);
        }

        [TestMethod]
        public void Invokes_event_when_a_component_gets_added()
        {
            var virtualStudio = new VirtualStudio();
            bool wasEventInvoked = false;
            virtualStudio.ComponentAdded += (_, component) => wasEventInvoked = true;

            virtualStudio.AddComponent(new PlaceholderStudioComponent());

            Assert.IsTrue(wasEventInvoked);
        }

        [TestMethod]
        public void Fires_event_when_a_component_gets_removed()
        {
            var virtualStudio = new VirtualStudio();
            bool wasEventInvoked = false;
            var component = new PlaceholderStudioComponent();
            virtualStudio.AddComponent(component);
            virtualStudio.ComponentRemoved += (_, component) => wasEventInvoked = true;

            virtualStudio.RemoveComponent(component);

            Assert.IsTrue(wasEventInvoked);
        }
    }
}
