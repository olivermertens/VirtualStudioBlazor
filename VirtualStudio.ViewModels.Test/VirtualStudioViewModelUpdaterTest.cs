using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualStudio.Shared;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.ViewModels.Test
{
    [TestClass]
    public class VirtualStudioViewModelUpdaterTest
    {
        VirtualStudioViewModel viewModel;
        string virtualStudioName = "";

        [TestInitialize]
        public void TestInit()
        {
            viewModel = CreateTestViewModel();
        }

        [TestMethod]
        public void Adds_a_client_to_repository()
        {
            var clientDto = new StudioComponentDto
            {
                Id = 10,
                Name = "Client 10",
                IsPlaceholder = false,
                Outputs = new List<StudioComponentEndpointDto>
                {
                    new StudioComponentEndpointDto
                    {
                        IOType = Shared.EndpointIOType.Output,
                        Id = 1
                    }
                },
                Inputs = new List<StudioComponentEndpointDto>
                { 
                    new StudioComponentEndpointDto
                    {
                        IOType = Shared.EndpointIOType.Input,
                        Id = 2
                    },
                    new StudioComponentEndpointDto
                    {
                        IOType = Shared.EndpointIOType.Input,
                        Id = 3
                    }
                }
            };

            var updater = new VirtualStudioViewModelUpdater(viewModel);
            updater.AddClientToRepository(virtualStudioName, clientDto);

            var addedClient = viewModel.ComponentRepository.Clients.First(c => c.Id == 10);
            Assert.AreEqual(clientDto.Name, addedClient.Name);
            Assert.IsFalse(addedClient is PlaceholderViewModel);
            Assert.AreEqual(clientDto.Outputs.Count, addedClient.Outputs.Count);
            Assert.AreEqual(clientDto.Inputs.Count, addedClient.Inputs.Count);
            Assert.AreEqual(clientDto.Inputs.First().IOType, addedClient.Inputs[0].IOType);
            Assert.AreEqual(clientDto.Outputs.First().IOType, addedClient.Outputs[0].IOType);
            CollectionAssert.AreEqual(clientDto.Outputs.Select(o => o.Id).ToArray(), addedClient.Outputs.Select(o => o.Id).ToArray());
            CollectionAssert.AreEqual(clientDto.Inputs.Select(o => o.Id).ToArray(), addedClient.Inputs.Select(o => o.Id).ToArray());
        }

        [TestMethod]
        public void Adds_a_placeholder_to_repository()
        {
            var placeholderDto = new StudioComponentDto
            {
                Id = 10,
                Name = "Placeholder 10",
                IsPlaceholder = true,
                Outputs = new List<StudioComponentEndpointDto>
                {
                    new StudioComponentEndpointDto
                    {
                        IOType = Shared.EndpointIOType.Output,
                        Id = 1
                    }
                },
                Inputs = new List<StudioComponentEndpointDto>
                {
                    new StudioComponentEndpointDto
                    {
                        IOType = Shared.EndpointIOType.Input,
                        Id = 2
                    },
                    new StudioComponentEndpointDto
                    {
                        IOType = Shared.EndpointIOType.Input,
                        Id = 3
                    }
                }
            };

            var updater = new VirtualStudioViewModelUpdater(viewModel);
            updater.AddPlaceholderToRepository(virtualStudioName, placeholderDto);

            var addedClient = viewModel.ComponentRepository.Placeholders.First(c => c.Id == 10);
            Assert.AreEqual(placeholderDto.Name, addedClient.Name);
            Assert.IsTrue(addedClient is PlaceholderViewModel);
            Assert.AreEqual(placeholderDto.Outputs.Count, addedClient.Outputs.Count);
            Assert.AreEqual(placeholderDto.Inputs.Count, addedClient.Inputs.Count);
            Assert.AreEqual(placeholderDto.Inputs.First().IOType, addedClient.Inputs[0].IOType);
            Assert.AreEqual(placeholderDto.Outputs.First().IOType, addedClient.Outputs[0].IOType);
            CollectionAssert.AreEqual(placeholderDto.Outputs.Select(o => o.Id).ToArray(), addedClient.Outputs.Select(o => o.Id).ToArray());
            CollectionAssert.AreEqual(placeholderDto.Inputs.Select(o => o.Id).ToArray(), addedClient.Inputs.Select(o => o.Id).ToArray());
        }

        [TestMethod]
        public void Adds_a_ComponentNode_to_ComponentNodes()
        {
            var updater = new VirtualStudioViewModelUpdater(viewModel);
            var componentFromRepository = viewModel.ComponentRepository.Clients[0];

            updater.AddComponentNode(virtualStudioName, componentFromRepository.Id, 123, 456);

            var addedComponentNode = viewModel.ComponentNodes.First(c => c.Component.Id == componentFromRepository.Id);

            Assert.AreEqual(123, addedComponentNode.PositionX);
            Assert.AreEqual(456, addedComponentNode.PositionY);
            Assert.AreEqual(componentFromRepository, addedComponentNode.Component);
        }

        [TestMethod]
        public void Adds_a_Placeholder_to_ComponentNodes()
        {
            var updater = new VirtualStudioViewModelUpdater(viewModel);
            var placeholderDto = new StudioComponentDto
            {
                Id = 10,
                IsPlaceholder = true,
            };

            updater.AddPlaceholderNode(virtualStudioName, placeholderDto, 123, 456);

            var addedComponentNode = viewModel.ComponentNodes.First(c => c.Component.Id == 10);

            Assert.AreEqual(123, addedComponentNode.PositionX);
            Assert.AreEqual(456, addedComponentNode.PositionY);
        }

        [TestMethod]
        public void Adds_inputs_to_components()
        {
            var updater = new VirtualStudioViewModelUpdater(viewModel);
            var inputDto = new StudioComponentEndpointDto
            {
                Id = 25,
                IOType = Shared.EndpointIOType.Input,
                Name = "Ein Input",
                ConnectionType = "WebRTC",
                DataKind = Shared.DataKind.Video
            };

            var placeholderInRepository = viewModel.ComponentRepository.Placeholders[0];
            updater.AddInputToComponent(virtualStudioName, placeholderInRepository.Id, inputDto);
            Check(placeholderInRepository.Inputs.First(i => i.Id == 25));

            var clientInRepository = viewModel.ComponentRepository.Clients[0];
            updater.AddInputToComponent(virtualStudioName, clientInRepository.Id, inputDto);
            Check(clientInRepository.Inputs.First(i => i.Id == 25));

            var placeholderInNodes = viewModel.ComponentNodes.First(c => c.Component is PlaceholderViewModel).Component;
            updater.AddInputToComponent(virtualStudioName, placeholderInNodes.Id, inputDto);
            Check(placeholderInNodes.Inputs.First(i => i.Id == 25));

            var clientInNodes = viewModel.ComponentNodes.First(c => c.Component is PlaceholderViewModel == false).Component;
            updater.AddInputToComponent(virtualStudioName, clientInNodes.Id, inputDto);
            Check(clientInNodes.Inputs.First(i => i.Id == 25));

            void Check(StudioComponentEndpointViewModel input)
            {
                Assert.IsTrue(input.IOType == Shared.EndpointIOType.Input);
                Assert.AreEqual(inputDto.Name, input.Name);
                Assert.AreEqual(inputDto.ConnectionType, input.ConnectionType);
                Assert.AreEqual(inputDto.DataKind, input.DataKind);
                Assert.IsNotNull(input.ComponentViewModel);

            }
        }

        [TestMethod]
        public void Adds_outputs_to_components()
        {
            var updater = new VirtualStudioViewModelUpdater(viewModel);
            var outputDto = new StudioComponentEndpointDto
            {
                Id = 25,
                IOType = Shared.EndpointIOType.Output,
                Name = "Ein Output",
                ConnectionType = "WebRTC",
                DataKind = Shared.DataKind.Video
            };

            var placeholderInRepository = viewModel.ComponentRepository.Placeholders[0];
            updater.AddOutputToComponent(virtualStudioName, placeholderInRepository.Id, outputDto);
            Check(placeholderInRepository.Outputs.First(i => i.Id == 25));

            var clientInRepository = viewModel.ComponentRepository.Clients[0];
            updater.AddOutputToComponent(virtualStudioName, clientInRepository.Id, outputDto);
            Check(clientInRepository.Outputs.First(i => i.Id == 25));

            var placeholderInNodes = viewModel.ComponentNodes.First(c => c.Component is PlaceholderViewModel).Component;
            updater.AddOutputToComponent(virtualStudioName, placeholderInNodes.Id, outputDto);
            Check(placeholderInNodes.Outputs.First(i => i.Id == 25));

            var clientInNodes = viewModel.ComponentNodes.First(c => c.Component is PlaceholderViewModel == false).Component;
            updater.AddOutputToComponent(virtualStudioName, clientInNodes.Id, outputDto);
            Check(clientInNodes.Outputs.First(i => i.Id == 25));

            void Check(StudioComponentEndpointViewModel output)
            {
                Assert.IsTrue(output.IOType == Shared.EndpointIOType.Output);
                Assert.AreEqual(outputDto.Name, output.Name);
                Assert.AreEqual(outputDto.ConnectionType, output.ConnectionType);
                Assert.AreEqual(outputDto.DataKind, output.DataKind);
                Assert.IsNotNull(output.ComponentViewModel);
            }
        }

        [TestMethod]
        public void Changes_component_names()
        {
            var updater = new VirtualStudioViewModelUpdater(viewModel);

            var placeholderInRepository = viewModel.ComponentRepository.Placeholders[0];
            updater.ChangeComponentProperty(virtualStudioName, placeholderInRepository.Id, "Name", "werrtegf");
            Assert.AreEqual("werrtegf", placeholderInRepository.Name);

            var clientInRepository = viewModel.ComponentRepository.Clients[0];
            updater.ChangeComponentProperty(virtualStudioName, clientInRepository.Id, "Name", "hfdkpodosf");
            Assert.AreEqual("hfdkpodosf", clientInRepository.Name);

            var placeholderInNodes = viewModel.ComponentNodes.First(c => c.Component is PlaceholderViewModel).Component;
            updater.ChangeComponentProperty(virtualStudioName, placeholderInNodes.Id, "Name", "gjoisdkr");
            Assert.AreEqual("gjoisdkr", placeholderInNodes.Name);

            var clientInNodes = viewModel.ComponentNodes.First(c => c.Component is PlaceholderViewModel == false).Component;
            updater.ChangeComponentProperty(virtualStudioName, clientInNodes.Id, "Name", "holpöskgjj");
            Assert.AreEqual("holpöskgjj", clientInNodes.Name);
        }

        [DataTestMethod]
        [DataRow(ConnectionState.Connected)]
        [DataRow(ConnectionState.Connecting)]
        [DataRow(ConnectionState.Destroyed)]
        [DataRow(ConnectionState.Disconnected)]
        [DataRow(ConnectionState.Disconnecting)]
        [DataRow(ConnectionState.Unknown)]
        public void Changes_connection_state(ConnectionState connectionState)
        {
            var updater = new VirtualStudioViewModelUpdater(viewModel);
            var connection = viewModel.Connections[0];

            updater.ChangeConnectionState(virtualStudioName, connection.Id, connectionState);

            Assert.AreEqual(connectionState, connection.State);
        }

        [DataTestMethod]
        [DataRow(0, 0)]
        [DataRow(-1, -1)]
        [DataRow(1, 1)]
        [DataRow(-1, 1)]
        [DataRow(1, -1)]
        [DataRow(214345, -534645)]
        [DataRow(214345, 54434)]
        public void Moves_component_node(float x, float y)
        {
            var updater = new VirtualStudioViewModelUpdater(viewModel);
            var componentNode = viewModel.ComponentNodes[0];

            updater.MoveComponentNode(virtualStudioName, componentNode.Component.Id, x, y);
            Assert.AreEqual(x, componentNode.PositionX);
            Assert.AreEqual(y, componentNode.PositionY);
        }

        [TestMethod]
        public void Removes_client_from_repository()
        {
            var updater = new VirtualStudioViewModelUpdater(viewModel);
            var client = viewModel.ComponentRepository.Clients[0];
            updater.RemoveClientFromRepository(virtualStudioName, client.Id);
            Assert.IsFalse(viewModel.ComponentRepository.Clients.Contains(client));
        }

        [TestMethod]
        public void Removes_placeholder_from_repository()
        {
            var updater = new VirtualStudioViewModelUpdater(viewModel);
            var placeholder = viewModel.ComponentRepository.Placeholders[0];
            updater.RemovePlaceholderFromRepository(virtualStudioName, placeholder.Id);
            Assert.IsFalse(viewModel.ComponentRepository.Placeholders.Contains(placeholder));
        }

        [TestMethod]
        public void Removes_ComponentNode()
        {
            var updater = new VirtualStudioViewModelUpdater(viewModel);
            var componentNode = viewModel.ComponentNodes[0];
            updater.RemoveComponent(virtualStudioName, componentNode.Component.Id);
            Assert.IsFalse(viewModel.ComponentNodes.Contains(componentNode));
        }

        [TestMethod]
        public void Removes_output_from_components()
        {
            var updater = new VirtualStudioViewModelUpdater(viewModel);
            var componentNode = viewModel.ComponentNodes[1];
            Assert.IsTrue(componentNode.Component.Outputs.Count == 1);
            updater.RemoveOutputFromComponent(virtualStudioName, componentNode.Component.Id, componentNode.Component.Outputs[0].Id);
            Assert.IsTrue(componentNode.Component.Outputs.Count == 0);

            var clientInRepo = viewModel.ComponentRepository.Clients[1];
            updater.AddOutputToComponent(virtualStudioName, clientInRepo.Id, new StudioComponentEndpointDto { IOType = EndpointIOType.Input, Id = 234 });
            Assert.IsTrue(clientInRepo.Outputs.Count == 1);
            updater.RemoveOutputFromComponent(virtualStudioName, clientInRepo.Id, clientInRepo.Outputs[0].Id);
            Assert.IsTrue(clientInRepo.Outputs.Count == 0);

            var placeholderInRepo = viewModel.ComponentRepository.Placeholders[1];
            Assert.IsTrue(placeholderInRepo.Outputs.Count == 1);
            updater.RemoveOutputFromComponent(virtualStudioName, placeholderInRepo.Id, placeholderInRepo.Outputs[0].Id);
            Assert.IsTrue(placeholderInRepo.Outputs.Count == 0);
        }

        [TestMethod]
        public void Removes_input_from_components()
        {
            var updater = new VirtualStudioViewModelUpdater(viewModel);
            var componentNode = viewModel.ComponentNodes[0];
            Assert.IsTrue(componentNode.Component.Inputs.Count == 1);
            updater.RemoveInputFromComponent(virtualStudioName, componentNode.Component.Id, componentNode.Component.Inputs[0].Id);
            Assert.IsTrue(componentNode.Component.Inputs.Count == 0);

            var clientInRepo = viewModel.ComponentRepository.Clients[0];
            Assert.IsTrue(clientInRepo.Inputs.Count == 1);
            updater.RemoveInputFromComponent(virtualStudioName, clientInRepo.Id, clientInRepo.Inputs[0].Id);
            Assert.IsTrue(clientInRepo.Inputs.Count == 0);

            var placeholderInRepo = viewModel.ComponentRepository.Placeholders[0];
            Assert.IsTrue(placeholderInRepo.Inputs.Count == 1);
            updater.RemoveInputFromComponent(virtualStudioName, placeholderInRepo.Id, placeholderInRepo.Inputs[0].Id);
            Assert.IsTrue(placeholderInRepo.Inputs.Count == 0);
        }

        [TestMethod]
        public void Removes_connection()
        {
            var updater = new VirtualStudioViewModelUpdater(viewModel);
            var connection = viewModel.Connections[0];
            updater.RemoveConnection(virtualStudioName, connection.Id);
            Assert.AreEqual(0, viewModel.Connections.Count);
        }

        [DataTestMethod]
        [DataRow(ConnectionState.Connected)]
        [DataRow(ConnectionState.Connecting)]
        [DataRow(ConnectionState.Destroyed)]
        [DataRow(ConnectionState.Disconnected)]
        [DataRow(ConnectionState.Disconnecting)]
        [DataRow(ConnectionState.Unknown)]
        public void Adds_connection(ConnectionState connectionState)
        {
            var updater = new VirtualStudioViewModelUpdater(viewModel);
            var connection = viewModel.Connections[0];
            var input = connection.Input;
            var output = connection.Output;
            updater.RemoveConnection(virtualStudioName, connection.Id);
            Assert.AreEqual(0, viewModel.Connections.Count);

            updater.CreateConnection(virtualStudioName, new StudioConnectionDto
            {
                Id = 1,
                InputComponentId = input.ComponentViewModel.Id,
                OutputComponentId = output.ComponentViewModel.Id,
                InputId = input.Id,
                OutputId = output.Id,
                State = connectionState
            });

            Assert.AreEqual(1, viewModel.Connections.Count);
            Assert.AreEqual(connectionState, viewModel.Connections[0].State);
        }


        private VirtualStudioViewModel CreateTestViewModel()
        {
            var dto = new VirtualStudioWithArrangementDto()
            {
                ComponentRepository = new StudioComponentRepositoryDto
                {
                    Clients = new List<StudioComponentDto>
                    {
                        new StudioComponentDto
                        {
                            Id = 4,
                            IsPlaceholder = false,
                            Inputs = new List<StudioComponentEndpointDto>
                            {
                                new StudioComponentEndpointDto
                                {
                                    IOType = Shared.EndpointIOType.Input,
                                    Id = 1
                                }
                            }
                        },
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
                        },
                        new StudioComponentDto
                        {
                            Id = 8,
                            IsPlaceholder = true,
                            Outputs = new List<StudioComponentEndpointDto>
                            {
                                new StudioComponentEndpointDto
                                {
                                    IOType = Shared.EndpointIOType.Output,
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
                        },
                    },
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
                    }
                },
                Connections = new List<StudioConnectionDto>
                {
                    new StudioConnectionDto {Id = 1, InputComponentId = 7, InputId = 1, OutputComponentId = 5, OutputId = 1, State = Shared.ConnectionState.Connected}
                }
            };

            return new VirtualStudioViewModel(dto);
        }
    }
}
