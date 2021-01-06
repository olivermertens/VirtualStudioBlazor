using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core.DTOs
{
    internal static class DtoConverter
    {
        #region StudioComponent

        public static StudioComponentDto ToDto(this StudioComponent component)
        {
            return new StudioComponentDto
            {
                Id = component.Id,
                Name = component.Name,
                Inputs = component.Inputs?.ToDto(),
                Outputs = component.Outputs?.ToDto()
            };
        }

        #endregion

        #region StudioComponentEndpoint

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

        #endregion

        #region StudioConnection

        public static StudioConnectionDto ToDto(this StudioConnection studioConnection)
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

        #endregion
    }
}
