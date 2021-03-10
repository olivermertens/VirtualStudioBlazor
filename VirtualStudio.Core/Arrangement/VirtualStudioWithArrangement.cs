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
        public event EventHandler<ComponentNode> ComponentNodeAdded;

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
                    componentNode.PositionChanged += ComponentNode_PositionChanged;
                    _componentNodes.Add(componentNode);
                    ComponentNodeAdded?.Invoke(this, componentNode);
                    return componentNode;
                }
            }
            return null;
        }

        public override void RemoveComponent(IStudioComponent component)
        {
            if (_components.Contains(component))
            {
                var node = _componentNodes.Single(c => c.Component == component);
                _componentNodes.Remove(node);
                node.PositionChanged -= ComponentNode_PositionChanged;
                base.RemoveComponent(component);
            }
        }

        public void MoveComponentNode(ComponentNode component, Position2D to)
        {
            if (_componentNodes.Contains(component))
            {
                component.Position = to;
            }
        }

        private void ComponentNode_PositionChanged(object sender, Position2D e)
        {
            if (sender is ComponentNode node)
                ComponentNodeMoved?.Invoke(this, node);
        }
    }
}
