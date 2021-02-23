using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Shared.DTOs
{
    public class StudioComponentDto
    {
        public bool IsPlaceholder { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<StudioComponentEndpointDto> Inputs { get; set; }
        public ICollection<StudioComponentEndpointDto> Outputs { get; set; }
    }
}
