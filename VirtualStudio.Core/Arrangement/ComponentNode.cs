using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core.Arrangement
{
    public class ComponentNode : Node
    {
        public int Id => Component.Id;
        public StudioComponent Component { get; }

        public ComponentNode(StudioComponent component)
        {
            Component = component ?? throw new ArgumentNullException(nameof(component));
        }
    }
}
