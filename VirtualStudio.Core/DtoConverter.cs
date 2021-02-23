using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Core.Arrangement;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.Core
{
    public static class DtoConverter
    {
        public static StudioComponentDto ToDto(this IStudioComponent component)
        {
            return new StudioComponentDto
            {
                IsPlaceholder = component is PlaceholderStudioComponent,
                Id = component.Id,
                Name = component.Name,
                Inputs = component.Inputs?.ToDto(),
                Outputs = component.Outputs?.ToDto()
            };
        }

        public static IEnumerable<StudioComponentDto> ToDto(this IEnumerable<IStudioComponent> studioComponents)
        {
            foreach (var component in studioComponents)
                yield return component.ToDto();
        }

        public static StudioComponentEndpointDto ToDto(this IStudioComponentEndpoint studioComponentEndpoint)
        {
            return new StudioComponentEndpointDto
            {
                Name = studioComponentEndpoint.Name,
                DataKind = studioComponentEndpoint.DataKind,
                IOType = studioComponentEndpoint.IOType,
                ConnectionType = studioComponentEndpoint.ConnectionType
            };
        }

        public static ICollection<StudioComponentEndpointDto> ToDto(this IEnumerable<IStudioComponentEndpoint> studioComponentEndpoints)
        {
            var collection = new List<StudioComponentEndpointDto>();
            foreach(var endpoint in studioComponentEndpoints)
            {
                collection.Add(endpoint.ToDto());
            }
            return collection;
        }

        public static StudioConnectionDto ToDto(this IStudioConnection studioConnection)
        {
            return new StudioConnectionDto
            {
                Id = studioConnection.Id,
                State = studioConnection.State,
                InputComponentId = studioConnection.Input?.Component.Id ?? -1,
                InputId = studioConnection.Input?.Id ?? -1,
                OutputComponentId = studioConnection.Output?.Component.Id ?? -1,
                OutputId = studioConnection.Output?.Id ?? -1
            };
        }

        public static IEnumerable<StudioConnectionDto> ToDto(this IEnumerable<IStudioConnection> studioConnections)
        {
            foreach (var connection in studioConnections)
                yield return connection.ToDto();
        }

        public static ComponentNodeDto ToDto(this ComponentNode componentNode)
        {
            return new ComponentNodeDto
            {
                Component = componentNode.Component.ToDto(),
                X = componentNode.Position.X,
                Y = componentNode.Position.Y
            };
        }

        public static IEnumerable<ComponentNodeDto> ToDto(this IEnumerable<ComponentNode> componentNodes)
        {
            foreach(var node in componentNodes)
                yield return node.ToDto();
        }

        public static StudioComponentRepositoryDto ToDto(this StudioComponentRepository studioComponentRepository)
        {
            return new StudioComponentRepositoryDto
            {
                Clients = studioComponentRepository.Clients.ToDto(),
                Placeholders = studioComponentRepository.Placeholders.ToDto()
            };
        }

        public static VirtualStudioWithArrangementDto ToDto(this VirtualStudioWithArrangement virtualStudioWithArrangement)
        {
            return new VirtualStudioWithArrangementDto
            {
                ComponentNodes = virtualStudioWithArrangement.ComponentNodes.ToDto(),
                Connections = virtualStudioWithArrangement.Connections.ToDto(),
                ComponentRepository = virtualStudioWithArrangement.ComponentRepository.ToDto()
            };
        }
    }
}
