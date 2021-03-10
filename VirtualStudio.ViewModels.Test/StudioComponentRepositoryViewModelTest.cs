using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.ViewModels.Test
{
    [TestClass]
    public class StudioComponentRepositoryViewModelTest
    {
        [TestMethod]
        public void Creates_the_ViewModel_from_dto()
        {
            var dto = new StudioComponentRepositoryDto()
            {
                Clients = new List<StudioComponentDto> { new StudioComponentDto()},
                Placeholders = new List<StudioComponentDto> { new StudioComponentDto { IsPlaceholder = true} } 
            };

            var vm = new StudioComponentRepositoryViewModel(dto);

            Assert.AreEqual(dto.Clients.Count(), vm.Clients.Count);
            Assert.AreEqual(dto.Placeholders.Count(), vm.Placeholders.Count);
        }
    }
}
