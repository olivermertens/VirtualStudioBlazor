using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.ViewModels.Test
{
    [TestClass]
    public class VirtualStudioViewModelTest
    {
        [TestMethod]
        public void Creates_the_ViewModel_from_dto()
        {
            var dto = new VirtualStudioWithArrangementDto()
            {
                ComponentRepository = new StudioComponentRepositoryDto
                {
                    Clients = new List<StudioComponentDto> 
                    {
                        new StudioComponentDto
                        {
                            Id = 5,
                            IsPlaceholder = false,
                            Outputs = new List<StudioComponentEndpointDto>
                            {
                                new StudioComponentEndpointDto
                                {
                                    IOType = Shared.EndpointIOType.Output,
                                    Id = 1
                                }
                            }
                        }
                    },
                    Placeholders = new List<StudioComponentDto>
                    {
                        new StudioComponentDto
                        {
                            Id = 6,
                            IsPlaceholder = true,
                            Inputs = new List<StudioComponentEndpointDto>
                            { 
                                new StudioComponentEndpointDto
                                {
                                    IOType = Shared.EndpointIOType.Input,
                                    Id = 1
                                }
                            }
                        }
                    }
                },
                ComponentNodes = new List<ComponentNodeDto>
                {
                    new ComponentNodeDto 
                    {
                        Component = new StudioComponentDto
                        {
                            Id = 5,
                            IsPlaceholder = false,
                            Outputs = new List<StudioComponentEndpointDto>
                            {
                                new StudioComponentEndpointDto
                                {
                                    IOType = Shared.EndpointIOType.Output,
                                    Id = 1
                                }
                            }
                        }
                    },
                    new ComponentNodeDto 
                    {
                        Component = new StudioComponentDto
                        {
                            Id = 7,
                            IsPlaceholder = true,
                            Inputs = new List<StudioComponentEndpointDto>
                            {
                                new StudioComponentEndpointDto
                                {
                                    IOType = Shared.EndpointIOType.Input,
                                    Id = 1
                                }
                            }
                        }
                    },
                },
                Connections = new List<StudioConnectionDto>
                { 
                    new StudioConnectionDto {Id = 1, InputComponentId = 7, InputId = 1, OutputComponentId = 5, OutputId = 1, State = Shared.ConnectionState.Connected}
                }
            };

            var vm = new VirtualStudioViewModel(dto);

            Assert.AreEqual(vm.ComponentNodes[0].Component.Outputs[0], vm.Connections[0].Output);
            Assert.AreEqual(vm.ComponentRepository.Clients[0], vm.ComponentNodes[0].Component);
            Assert.AreNotEqual(vm.ComponentRepository.Placeholders[0], vm.ComponentNodes[1].Component);
        }
    }
}
