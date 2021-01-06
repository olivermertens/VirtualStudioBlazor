using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core.Test
{
    [TestClass]
    public class StudioConnectionFactoryTest
    {
        [TestMethod]
        public void Registers_the_PlaceholderStudioConnection_Type()
        {
            var factory = new StudioConnectionFactory();

            bool success = factory.RegisterStudioConnectionType(new PlaceholderStudioConnectionFactory());

            Assert.IsTrue(success);
        }

        [TestMethod]
        public void Does_not_allow_to_add_a_StudioConnectionType_twice()
        {
            var factory = new StudioConnectionFactory();

            bool success = factory.RegisterStudioConnectionType(new PlaceholderStudioConnectionFactory());
            success = factory.RegisterStudioConnectionType(new PlaceholderStudioConnectionFactory());

            Assert.IsFalse(success);
        }

        [TestMethod]
        public void Creates_a_DefaultStudioConnection()
        {
            var factory = new StudioConnectionFactory();
            var defaultOutput = new StudioComponentOutput("output", DataKind.Audio, PlaceholderStudioConnectionFactory.Name);
            var defaultInput = new StudioComponentInput("input", DataKind.Audio, PlaceholderStudioConnectionFactory.Name);
            factory.RegisterStudioConnectionType(new PlaceholderStudioConnectionFactory());

            Assert.IsTrue(factory.CanCreateStudioConnection(defaultOutput, defaultInput));

            StudioConnection connection = factory.CreateStudioConnection(defaultOutput, defaultInput);

            Assert.IsNotNull(connection);
        }

        [TestMethod]
        public void Returns_null_if_there_is_no_matching_connection_type_defined()
        {
            var factory = new StudioConnectionFactory();
            var defaultOutput = new StudioComponentOutput("output", DataKind.Audio, "Egal");
            var defaultInput = new StudioComponentInput("input", DataKind.Audio, "Egal");

            Assert.IsFalse(factory.CanCreateStudioConnection(defaultOutput, defaultInput));

            Assert.IsNull(factory.CreateStudioConnection(defaultOutput, defaultInput));
        }
    }
}
