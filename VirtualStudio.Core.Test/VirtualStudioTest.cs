using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core.Test
{
    [TestClass]
    public class VirtualStudioTest
    {
        static StudioConnectionFactory studioConnectionFactory;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            studioConnectionFactory = new StudioConnectionFactory();
            studioConnectionFactory.RegisterStudioConnectionType(new PlaceholderStudioConnectionFactory());
        }

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
        public void Throws_exception_if_input_or_output_for_a_connection_does_not_exist_in_components()
        {
            var virtualStudio = new VirtualStudio();
            var input = new StudioComponentInput("input", DataKind.Audio, PlaceholderStudioConnectionFactory.Name);
            var output = new StudioComponentOutput("output", DataKind.Audio, PlaceholderStudioConnectionFactory.Name);

            Assert.ThrowsException<System.InvalidOperationException>(() =>
            {
                StudioConnection connection = virtualStudio.CreateConnection(output, input);
            });
        }

        [TestMethod]
        public void Adds_connection_between_endpoints_that_exist_in_components()
        {
            var virtualStudio = new VirtualStudio(studioConnectionFactory);
            var component = new PlaceholderStudioComponent();
            var input = new StudioComponentInput("input", DataKind.Audio, PlaceholderStudioConnectionFactory.Name);
            component.AddInput(input);
            var output = new StudioComponentOutput("output", DataKind.Audio, PlaceholderStudioConnectionFactory.Name);
            component.AddOutput(output);
            virtualStudio.AddComponent(component);

            Assert.IsTrue(virtualStudio.CanCreateConnection(output, input));

            StudioConnection connection = virtualStudio.CreateConnection(output, input); 

            Assert.IsNotNull(connection);
            Assert.IsTrue(virtualStudio.Connections.Count == 1);
        }

        [TestMethod]
        public void Removes_connection()
        {
            var virtualStudio = new VirtualStudio(studioConnectionFactory);
            var component = new PlaceholderStudioComponent();
            var input = new StudioComponentInput("input", DataKind.Audio, PlaceholderStudioConnectionFactory.Name);
            component.AddInput(input);
            var output = new StudioComponentOutput("output", DataKind.Audio, PlaceholderStudioConnectionFactory.Name);
            component.AddOutput(output);
            virtualStudio.AddComponent(component);
            StudioConnection connection = virtualStudio.CreateConnection(output, input);

            virtualStudio.RemoveConnection(connection);

            Assert.IsTrue(virtualStudio.Connections.Count == 0);
        }

        [TestMethod]
        public void Removes_connection_when_a_related_component_gets_removed()
        {
            var virtualStudio = new VirtualStudio(studioConnectionFactory);
            var component = new PlaceholderStudioComponent();
            var input = new StudioComponentInput("input", DataKind.Audio, PlaceholderStudioConnectionFactory.Name);
            component.AddInput(input);
            var output = new StudioComponentOutput("output", DataKind.Audio, PlaceholderStudioConnectionFactory.Name);
            component.AddOutput(output);
            virtualStudio.AddComponent(component);
            StudioConnection connection = virtualStudio.CreateConnection(output, input);

            virtualStudio.RemoveComponent(component);

            Assert.IsTrue(virtualStudio.Connections.Count == 0);
        }

        [TestMethod]
        public void Removes_connection_when_a_related_endpoint_gets_removed_from_the_component()
        {
            var virtualStudio = new VirtualStudio(studioConnectionFactory);
            var component = new PlaceholderStudioComponent();
            var input = new StudioComponentInput("input", DataKind.Audio, PlaceholderStudioConnectionFactory.Name);
            component.AddInput(input);
            var output = new StudioComponentOutput("output", DataKind.Audio, PlaceholderStudioConnectionFactory.Name);
            component.AddOutput(output);
            virtualStudio.AddComponent(component);
            StudioConnection connection = virtualStudio.CreateConnection(output, input);

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
