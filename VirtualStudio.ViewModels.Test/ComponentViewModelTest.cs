using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.ViewModels.Test
{
    [TestClass]
    public class ComponentViewModelTest
    {
        [TestMethod]
        public void Creates_the_ViewModel_from_dto()
        {
            var dto = new StudioComponentDto()
            {
                Id = 355,
                Name = "5gjdgj",
                IsPlaceholder = false,
                Inputs = new List<StudioComponentEndpointDto>
                {
                    new StudioComponentEndpointDto(){IOType = Shared.EndpointIOType.Input, ConnectionType = "ghd", DataKind = Shared.DataKind.Video, Id = 1, Name = "Input 1"}
                },
                Outputs = new List<StudioComponentEndpointDto>
                {
                    new StudioComponentEndpointDto(){IOType = Shared.EndpointIOType.Output, ConnectionType = "hgffds", DataKind = Shared.DataKind.Other, Id = 1, Name = "Output 1"}
                }             
            };

            var vm = new ComponentViewModel(dto);

            Assert.AreEqual(dto.Id, vm.Id);
            Assert.AreEqual(dto.Name, vm.Name);
            Assert.AreEqual(dto.Inputs.Count, vm.Inputs.Count);
            Assert.AreEqual(dto.Outputs.Count, vm.Outputs.Count);
            Assert.AreEqual(dto.Inputs.First().Id, vm.Inputs[0].Id);
            Assert.AreEqual(dto.Outputs.First().Id, vm.Outputs[0].Id);
            Assert.AreEqual(vm, vm.Inputs[0].ComponentViewModel);
            Assert.AreEqual(vm, vm.Outputs[0].ComponentViewModel);

        }
    }
}
