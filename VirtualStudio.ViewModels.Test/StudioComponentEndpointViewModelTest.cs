using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.ViewModels.Test
{
    [TestClass]
    public class StudioComponentEndpointViewModelTest
    {
        [TestMethod]
        public void Creates_the_ViewModel_from_DTO()
        {
            StudioComponentEndpointDto dto = new StudioComponentEndpointDto()
            {
                ConnectionType = "sdfdsgdfgd",
                DataKind = Shared.DataKind.Video,
                Id = 25,
                IOType = Shared.EndpointIOType.Output,
                Name = "rgjepaesfeü"
            };

            var vm = new StudioComponentEndpointViewModel(null, dto);

            Assert.AreEqual(dto.Name, vm.Name);
            Assert.AreEqual(dto.IOType, vm.IOType);
            Assert.AreEqual(dto.Id, vm.Id);
            Assert.AreEqual(dto.DataKind, vm.DataKind);
            Assert.AreEqual(dto.ConnectionType, vm.ConnectionType);
        }

        [TestMethod]
        public void Fires_event_on_Name_change()
        {
            var vm = new StudioComponentEndpointViewModel(null, new StudioComponentEndpointDto());
            bool eventFired = false;
            System.ComponentModel.PropertyChangedEventArgs args = null;
            vm.PropertyChanged += (s, e) =>
            {
                args = e;
                eventFired = true;
            };

            vm.Name = "ajgkdls";

            Assert.IsTrue(eventFired);
            Assert.AreEqual(nameof(vm.Name), args.PropertyName);
        }
    }
}
