using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Core.DTOs
{
    public class StudioComponentEndpointDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public EndpointIOType IOType { get; set; }

        public DataKind DataKind { get; set; }

        public string ConnectionType { get; set; }
    }
}
