using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Moq;

namespace VirtualStudio.Core.Test
{
    [TestClass]
    public class StudioComponentRepositoryTest
    {
        [TestMethod]
        public void Adds_a_PlaceholderStudioComponent()
        {
            var studioComponentRepository = new StudioComponentRepository();
            var placeholder = new PlaceholderStudioComponent();

            bool success = studioComponentRepository.AddPlaceholder(placeholder);

            Assert.IsTrue(success);
            Assert.IsTrue(placeholder.Id == 1);
        }

        [TestMethod]
        public void Finds_a_StudioComponent_by_ID()
        {
            var studioComponentRepository = new StudioComponentRepository();
            var placeholder = new PlaceholderStudioComponent();
            studioComponentRepository.AddPlaceholder(placeholder);

            var foundComponent = studioComponentRepository.Find(c => c.Id == placeholder.Id);
            Assert.IsNotNull(foundComponent);
            Assert.AreEqual(placeholder, foundComponent);
        }

        [TestMethod]
        public void Throws_exception_when_adding_a_component_with_an_existing_ID()
        {    
            var studioComponentRepository = new StudioComponentRepository();
            var placeholder = new PlaceholderStudioComponent();
            var placeholder2 = new Mock<PlaceholderStudioComponent>();
            placeholder2.SetupGet(p => p.Id).Returns(1);

            studioComponentRepository.AddPlaceholder(placeholder);

            Assert.ThrowsException<ArgumentException>(() =>
            {
                studioComponentRepository.AddPlaceholder(placeholder2.Object);
            });
            placeholder2.VerifyGet(p => p.Id);
        }
    }
}
