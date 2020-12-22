using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VirtualStudio.Shared
{
    public struct EndpointDescription
    {
        public string Name { get; set; }
        public DataKind DataKind { get; set; }
        public DataDescription Description { get; set; }

        public static EndpointDescription CreateVideoEndpoint(string name)
            => new EndpointDescription() {Name = name, DataKind = DataKind.Video, Description = DataDescription.Video };

        public static EndpointDescription CreateAudioEndpoint(string name)
            => new EndpointDescription() {Name = name, DataKind = DataKind.Audio, Description = DataDescription.Audio };

        public static EndpointDescription CreateDataEndpoint(string name, DataDescription dataDescription)
            => new EndpointDescription() {Name = name, DataKind = DataKind.Data, Description = dataDescription };

    }
}