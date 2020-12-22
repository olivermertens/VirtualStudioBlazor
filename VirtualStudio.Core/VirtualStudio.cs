using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core
{
    public class VirtualStudio
    {
        private List<StudioComponentBase> _components;
        public IReadOnlyCollection<StudioComponentBase> Components { get; }
        private List<IStudioConnection> _connections;
        public IReadOnlyCollection<IStudioConnection> Connections { get; }

        private IStudioConnectionFactory connectionFactory;
        private ILogger logger;

        public VirtualStudio(IStudioConnectionFactory connectionFactory, ILogger logger = null)
        {
            this.logger = logger ?? NullLogger.Instance;
            this.connectionFactory = connectionFactory;
            _components = new List<StudioComponentBase>();
            Components = _components.AsReadOnly();
            _connections = new List<IStudioConnection>();
            Connections = _connections.AsReadOnly();
        }

        public void AddComponent(StudioComponentBase component)
        {
            if (_components.Contains(component))
            {
                return;
            }
            _components.Add(component);
            component.EndpointRemoved += Component_EndpointRemoved;
        }

        public void RemoveComponent(StudioComponentBase component)
        {
            if (_components.Remove(component))
            {
                component.EndpointRemoved -= Component_EndpointRemoved;
                var foundConnections = new List<IStudioConnection>();
                foreach (var input in component.Inputs)
                {
                    _connections.RemoveAll(c => c.Input == input);
                }
                foreach (var output in component.Outputs)
                {
                    _connections.RemoveAll(c => c.Output == output);
                }
            }
        }

        public IStudioConnection CreateConnection(IStudioOutput output, IStudioInput input)
        {
            if (IsInputInComponents(input) && IsOutputInComponents(output))
            {
                var connection = connectionFactory.CreateConnection(output, input);
                _connections.Add(connection);
                return connection;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void RemoveConnection(IStudioConnection connection)
        {
            _connections.Remove(connection);
        }

        private void Component_EndpointRemoved(object sender, IStudioEndpoint endPoint)
        {
            if (endPoint is IStudioInput)
            {
                _connections.RemoveAll(c => c.Input == endPoint);
            }
            if (endPoint is IStudioOutput)
            {
                _connections.RemoveAll(c => c.Output == endPoint);
            }
        }

        private bool IsInputInComponents(IStudioInput input)
        {
            foreach (var component in _components)
            {
                if (component.Inputs.FirstOrDefault(c => c == input) != null)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsOutputInComponents(IStudioOutput output)
        {
            foreach (var component in _components)
            {
                if (component.Outputs.FirstOrDefault(c => c == output) != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
