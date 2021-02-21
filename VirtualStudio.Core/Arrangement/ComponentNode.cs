using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core.Arrangement
{
    public class ComponentNode : Node
    {
        public int Id => Component.Id;
        public IStudioComponent Component { get; }

        public ComponentNode(IStudioComponent component)
        {
            Component = component ?? throw new ArgumentNullException(nameof(component));
        }
    }
}
