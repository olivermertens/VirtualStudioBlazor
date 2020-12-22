using System;
using VirtualStudio.Shared;
using System.Linq;
using System.Collections.Generic;

namespace VirtualStudio.Server
{
    public class StudioEndpoint : StudioObject
    {
        public string Name { get; private set; }
        public DataKind DataKind { get; private set; }
        public DataDescription Description { get; private set; }
        public StudioEndpointCollection ContainingCollection { get; internal set; }
        public List<StudioLink> Links { get; } = new List<StudioLink>();

        public bool TrySetName(string name)
        {
            if (name != Name)
            {
                if (ContainingCollection != null &&
                    ContainingCollection.ContainsItemWithName(name))
                {
                    return false;
                }
                Name = name;
            }
            return true;
        }

        public EndpointDescription ToEndpointDescription()
        => StudioEndpoint.ToEndpointDescription(this);

        public static StudioEndpoint CreateVideoEndpoint(string name)
            => new StudioEndpoint() { Name = name, DataKind = DataKind.Video, Description = DataDescription.Video };

        public static StudioEndpoint CreateAudioEndpoint(string name)
            => new StudioEndpoint() { Name = name, DataKind = DataKind.Audio, Description = DataDescription.Audio };

        public static StudioEndpoint CreateDataEndpoint(string name, DataDescription dataDescription)
            => new StudioEndpoint() { Name = name, DataKind = DataKind.Data, Description = dataDescription };

        public static StudioEndpoint FromEndpointDescription(EndpointDescription endpointDescription)
            => new StudioEndpoint()
            {
                Name = endpointDescription.Name,
                DataKind = endpointDescription.DataKind,
                Description = endpointDescription.Description
            };

        public static EndpointDescription ToEndpointDescription(StudioEndpoint studioEndpoint)
            => new EndpointDescription()
            {
                Name = studioEndpoint.Name,
                DataKind = studioEndpoint.DataKind,
                Description = studioEndpoint.Description
            };
    }
}