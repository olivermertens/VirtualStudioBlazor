using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core.Arrangement
{
    public class VirtualStudioWithArrangement : VirtualStudio
    {
        public event EventHandler<ComponentNode> ComponentNodeMoved;

        private List<ComponentNode> _componentNodes;
        public IReadOnlyCollection<ComponentNode> ComponentNodes { get; }

        public VirtualStudioWithArrangement(ILogger logger = null)
            : base(logger)
        {
            _componentNodes = new List<ComponentNode>();
            ComponentNodes = _componentNodes.AsReadOnly();
        }

        public override IStudioComponent AddComponent(IStudioComponent component)
        {
            return AddComponent(component, Position2D.Zero)?.Component;
        }

        public ComponentNode AddComponent(IStudioComponent component, Position2D targetPosition)
        {
            if (!_components.Contains(component))
            {
                component = base.AddComponent(component);
                if (component != null)
                {
                    var componentNode = new ComponentNode(component) { Position = targetPosition };
                    _componentNodes.Add(componentNode);
                    return componentNode;
                }
            }
            return null;
        }

        public override void RemoveComponent(IStudioComponent component)
        {
            if (_components.Contains(component))
            {
                _componentNodes.Remove(_componentNodes.Single(c => c.Component == component));
                base.RemoveComponent(component);
            }
        }

        public void MoveComponentNode(ComponentNode component, Position2D to)
        {
            if (_componentNodes.Contains(component))
            {
                component.Position = to;
                ComponentNodeMoved?.Invoke(this, component);
            }
        }
    }
}
