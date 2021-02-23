using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Shared.DTOs
{
    public class StudioComponentRepositoryDto
    {
        public IEnumerable<StudioComponentDto> Placeholders { get; set; }
        public IEnumerable<StudioComponentDto> Clients { get; set; }
    }
}
