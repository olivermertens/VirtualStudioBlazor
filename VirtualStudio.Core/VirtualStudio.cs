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
        public virtual event EventHandler<StudioComponent> ComponentAdded;
        public virtual event EventHandler<StudioComponent> ComponentRemoved;
        public virtual event EventHandler<StudioConnection> ConnectionAdded;
        public virtual event EventHandler<StudioConnection> ConnectionRemoved;

        protected List<StudioComponent> _components;
        public IReadOnlyCollection<StudioComponent> Components { get; }
        protected List<StudioConnection> _connections;
        public IReadOnlyCollection<StudioConnection> Connections { get; }
        public StudioComponentRepository ComponentRepository { get; }

        private IStudioConnectionFactory connectionFactory;
        protected ILogger logger;


        public VirtualStudio() : this(null, null) { }

        public VirtualStudio(IStudioConnectionFactory connectionFactory = null, ILogger logger = null)
        {
            this.logger = logger ?? NullLogger.Instance;
            this.connectionFactory = connectionFactory ?? new StudioConnectionFactory();
            _components = new List<StudioComponent>();
            Components = _components.AsReadOnly();
            _connections = new List<StudioConnection>();
            Connections = _connections.AsReadOnly();
            ComponentRepository = new StudioComponentRepository();
        }

        public virtual void AddComponent(StudioComponent component)
        {
            if (_components.Contains(component))
            {
                return;
            }
            _components.Add(component);
            component.InputRemoved += Component_EndpointRemoved;
            component.OutputRemoved += Component_EndpointRemoved;
            ComponentAdded?.Invoke(this, component);
        }

        public virtual void RemoveComponent(StudioComponent component)
        {
            if (_components.Remove(component))
            {
                component.InputRemoved -= Component_EndpointRemoved;
                component.OutputRemoved -= Component_EndpointRemoved;
                var foundConnections = new List<StudioConnection>();
                if (component.Inputs != null)
                {
                    foreach (var input in component.Inputs)
                    {
                        _connections.RemoveAll(c => c.Input == input);
                    }
                }
                if (component.Outputs != null)
                {
                    foreach (var output in component.Outputs)
                    {
                        _connections.RemoveAll(c => c.Output == output);
                    }
                }
                ComponentRemoved?.Invoke(this, component);
            }
        }

        public bool CanCreateConnection(StudioComponentOutput output, StudioComponentInput input)
        {
            if (IsInputInComponents(input) && IsOutputInComponents(output))
            {
                var connection = connectionFactory.CreateStudioConnection(output, input);
                if (connection != null)
                {
                    return true;
                }
            }
            return false;
        }

        public StudioConnection CreateConnection(StudioComponentOutput output, StudioComponentInput input)
        {
            if (IsInputInComponents(input) && IsOutputInComponents(output))
            {
                var connection = connectionFactory.CreateStudioConnection(output, input);
                if (connection != null)
                {
                    _connections.Add(connection);
                    ConnectionAdded?.Invoke(this, connection);
                    return connection;
                }
            }
            throw new InvalidOperationException();
        }

        public void RemoveConnection(StudioConnection connection)
        {
            if (_connections.Remove(connection))
            {
                ConnectionRemoved?.Invoke(this, connection);
            }
        }

        private void Component_EndpointRemoved(object sender, IStudioComponentEndpoint endPoint)
        {
            if (endPoint is StudioComponentInput)
            {
                foreach (var con in _connections.FindAll(c => c.Input == endPoint))
                {
                    RemoveConnection(con);
                }
            }
            if (endPoint is StudioComponentOutput)
            {
                foreach (var con in _connections.FindAll(c => c.Output == endPoint))
                {
                    RemoveConnection(con);
                }
            }
        }

        private bool IsInputInComponents(StudioComponentInput input)
        {
            foreach (var component in _components)
            {
                if (component.Inputs?.FirstOrDefault(c => c == input) != null)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsOutputInComponents(StudioComponentOutput output)
        {
            foreach (var component in _components)
            {
                if (component.Outputs?.FirstOrDefault(c => c == output) != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
