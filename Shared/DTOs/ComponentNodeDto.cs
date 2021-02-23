using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Shared.DTOs
{
    public class ComponentNodeDto
    {
        public float X { get; set; }
        public float Y { get; set; }
        public StudioComponentDto Component { get; set; }
    }
}
