using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Shared.DTOs
{
    public class VirtualStudioWithArrangementDto
    {
        public StudioComponentRepositoryDto ComponentRepository { get; set; }
        public IEnumerable<ComponentNodeDto> ComponentNodes { get; set; }
        public IEnumerable<StudioConnectionDto> Connections { get; set; }
    }
}
