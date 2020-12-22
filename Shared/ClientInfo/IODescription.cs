using System.Collections.Generic;

namespace VirtualStudio.Shared
{
    public class IODescription
    {
        public List<EndpointDescription> Inputs { get; set; }
        public List<EndpointDescription> Outputs { get; set; }

        public IODescription()
        {
            Inputs = new List<EndpointDescription>();
            Outputs = new List<EndpointDescription>();
        }
    }
}