using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Core.Test.DummyClasses;

namespace VirtualStudio.Core.Test
{
    [TestClass]
    public class VirtualStudioTest
    {
        static IStudioConnectionFactory connectionFactory = new DummyStudioConnectionFactory();

        [TestMethod]
        public void Adds_component()
        {
            var virtualStudio = new VirtualStudio(connectionFactory);
            var component = new DummyStudioComponent();

            virtualStudio.AddComponent(component);

            Assert.IsTrue(virtualStudio.Components.Count == 1);
        }

        [TestMethod]
        public void Removes_component()
        {
            var virtualStudio = new VirtualStudio(connectionFactory);
            var component = new DummyStudioComponent();
            virtualStudio.AddComponent(component);

            virtualStudio.RemoveComponent(component);

            Assert.IsTrue(virtualStudio.Components.Count == 0);
        }

        [TestMethod]
        public void Does_not_add_already_existing_component()
        {
            var virtualStudio = new VirtualStudio(connectionFactory);
            var component = new DummyStudioComponent();
            virtualStudio.AddComponent(component);

            virtualStudio.AddComponent(component);

            Assert.IsTrue(virtualStudio.Components.Count == 1);
        }

        [TestMethod]
        public void Throws_exception_if_input_or_output_for_a_connection_does_not_exist_in_components()
        {
            var virtualStudio = new VirtualStudio(connectionFactory);
            var input = new DummyEndpoint();
            var output = new DummyEndpoint();

            Assert.ThrowsException<System.InvalidOperationException>(() =>
            {
                IStudioConnection connection = virtualStudio.CreateConnection(output, input);
            });
        }

        [TestMethod]
        public void Adds_connection_between_endpoints_that_exist_in_components()
        {
            var virtualStudio = new VirtualStudio(connectionFactory);

            IStudioConnection connection = AddConnection(virtualStudio);

            Assert.IsNotNull(connection);
            Assert.IsTrue(virtualStudio.Connections.Count == 1);
        }

        private IStudioConnection AddConnection(VirtualStudio virtualStudio)
        {
            var component = new DummyStudioComponent();
            var input = new DummyEndpoint();
            component.AddInput(input);
            var output = new DummyEndpoint();
            component.AddOutput(output);
            virtualStudio.AddComponent(component);
            return virtualStudio.CreateConnection(output, input);
        }

        [TestMethod]
        public void Removes_connection()
        {
            var virtualStudio = new VirtualStudio(connectionFactory);
            IStudioConnection connection = AddConnection(virtualStudio);

            virtualStudio.RemoveConnection(connection);

            Assert.IsTrue(virtualStudio.Connections.Count == 0);
        }

        [TestMethod]
        public void Removes_connection_when_a_related_component_gets_removed()
        {
            var virtualStudio = new VirtualStudio(connectionFactory);
            var component = new DummyStudioComponent();
            var input = new DummyEndpoint();
            component.AddInput(input);
            var output = new DummyEndpoint();
            component.AddOutput(output);
            virtualStudio.AddComponent(component);
            IStudioConnection connection = virtualStudio.CreateConnection(output, input);

            virtualStudio.RemoveComponent(component);

            Assert.IsTrue(virtualStudio.Connections.Count == 0);
        }

        [TestMethod]
        public void Removes_connection_when_a_related_endpoint_gets_removed_from_the_component()
        {
            var virtualStudio = new VirtualStudio(connectionFactory);
            var component = new DummyStudioComponent();
            var input = new DummyEndpoint();
            component.AddInput(input);
            var output = new DummyEndpoint();
            component.AddOutput(output);
            virtualStudio.AddComponent(component);
            IStudioConnection connection = virtualStudio.CreateConnection(output, input);

            component.RemoveOutput(output);

            Assert.IsTrue(virtualStudio.Connections.Count == 0);
        }
    }
}
