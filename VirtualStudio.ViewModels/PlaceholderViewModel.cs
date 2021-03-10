using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.ViewModels
{
    public sealed class PlaceholderViewModel : ComponentViewModel
    {
        public PlaceholderViewModel(StudioComponentDto studioComponentDto)
            : base(studioComponentDto)
        {
            if (!studioComponentDto.IsPlaceholder)
                throw new ArgumentException("Provided DTO does not describe a placeholder.");
        }
    }
}
