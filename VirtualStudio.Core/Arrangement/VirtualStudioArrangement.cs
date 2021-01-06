using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core.Arrangement
{
    public class VirtualStudioArrangement : VirtualStudio
    {
        public event EventHandler<ComponentNode> ComponentNodeMoved;

        private List<ComponentNode> _componentNodes;
        public IReadOnlyCollection<ComponentNode> ComponentNodes { get; }

        public VirtualStudioArrangement(IStudioConnectionFactory connectionFactory = null, ILogger logger = null)
            : base(connectionFactory, logger)
        {
            _componentNodes = new List<ComponentNode>();
            ComponentNodes = _componentNodes.AsReadOnly();
        }

        public override void AddComponent(StudioComponent component)
        {
            AddComponent(component, Position2D.Zero);
        }

        public void AddComponent(StudioComponent component, Position2D targetPosition)
        {
            if (!_components.Contains(component))
            {
                _componentNodes.Add(new ComponentNode(component) { Position = targetPosition });
                base.AddComponent(component);
            }
        }

        public override void RemoveComponent(StudioComponent component)
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
