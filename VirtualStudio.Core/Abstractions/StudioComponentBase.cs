using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Core.Abstractions
{
    public abstract class StudioComponentBase
    {
        public event EventHandler<IStudioEndpoint> EndpointAdded;
        public event EventHandler<IStudioEndpoint> EndpointRemoved;

        protected List<IStudioInput> _inputs;
        public IReadOnlyCollection<IStudioInput> Inputs { get; }
        protected List<IStudioOutput> _outputs;
        public IReadOnlyCollection<IStudioOutput> Outputs { get; }

        public StudioComponentBase()
        {
            _inputs = new List<IStudioInput>();
            Inputs = _inputs.AsReadOnly();
            _outputs = new List<IStudioOutput>();
            Outputs = _outputs.AsReadOnly();
        }

        protected virtual void AddInput(IStudioInput input)
        {
            _inputs.Add(input);
            EndpointAdded?.Invoke(this, input);
        }

        protected virtual void AddOutput(IStudioOutput output)
        {
            _outputs.Add(output);
            EndpointAdded?.Invoke(this, output);
        }

        protected virtual void RemoveInput(IStudioInput input)
        {
            if (_inputs.Remove(input))
            {
                EndpointRemoved?.Invoke(this, input);
            }
        }

        protected virtual void RemoveOutput(IStudioOutput output)
        {
            if (_outputs.Remove(output))
            {
                EndpointRemoved?.Invoke(this, output);
            }
        }
    }
}
