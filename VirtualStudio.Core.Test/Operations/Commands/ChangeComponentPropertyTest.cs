using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Core.Operations;

namespace VirtualStudio.Core.Test.Operations
{
    [TestClass]
    public class ChangeComponentPropertyTest
    {
        VirtualStudio virtualStudio;
        IStudioComponent repositoryComponent;
        IStudioComponent component;

        [TestInitialize]
        public void Init()
        {
            virtualStudio = new VirtualStudio();
            var placeholderComponent = new PlaceholderStudioComponent();
            placeholderComponent.SetName("New Placeholder");
            virtualStudio.ComponentRepository.AddPlaceholder(placeholderComponent);
            component = virtualStudio.AddComponent(placeholderComponent);
            repositoryComponent = placeholderComponent;
        }

        [TestMethod]
        public async Task Changes_the_Name_of_a_Component()
        {
            string newName = "New Name";
            await new ChangeComponentPropertyCommand(component.Id, nameof(component.Name), newName).Process(virtualStudio);

            Assert.AreEqual(newName, component.Name);
        }

        [TestMethod]
        public async Task Changes_the_Name_of_a_Component_in_ComponentRepository()
        {
            string newName = "New Name";
            await new ChangeComponentPropertyCommand(repositoryComponent.Id, nameof(component.Name), newName).Process(virtualStudio);

            Assert.AreEqual(newName, repositoryComponent.Name);
        }
    }
}
