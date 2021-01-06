using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualStudio.Core.Abstractions
{
    public abstract class StudioComponent
    {
        int idCounter = 1;
        protected int GetNewEndpointId() => idCounter++;

        public virtual event EventHandler<string> PropertyChanged;

        public virtual event EventHandler<StudioComponentInput> InputAdded;
        public virtual event EventHandler<StudioComponentInput> InputRemoved;
        public virtual event EventHandler<StudioComponentOutput> OutputAdded;
        public virtual event EventHandler<StudioComponentOutput> OutputRemoved;

        public virtual int Id { get; internal set; }
        public string Name { get; protected set; }

        private List<StudioComponentInput> _inputs;
        public virtual IReadOnlyCollection<StudioComponentInput> Inputs { get; }
        private List<StudioComponentOutput> _outputs;
        public virtual IReadOnlyCollection<StudioComponentOutput> Outputs { get; }

        public StudioComponent()
        {
            _inputs = new List<StudioComponentInput>();
            Inputs = _inputs.AsReadOnly();
            _outputs = new List<StudioComponentOutput>();
            Outputs = _outputs.AsReadOnly();
        }

        public void SetName(string name)
        {
            Name = name;
            InvokePropertyChanged(nameof(Name));
        }

        protected virtual void AddInput(StudioComponentInput input)
        {
            if(input.Id > 0)
            {
                if(IsEndpointIdInUse(input.Id))
                {
                    throw new ArgumentException("Cannot add an endpoint with an already existing ID.");
                }
            }
            else
            {
                input.Id = GetAvailableEndpointId();
            }
            _inputs.Add(input);
            input.Component = this;
            InputAdded?.Invoke(this, input);
        }

        protected virtual bool RemoveInput(StudioComponentInput input)
        {
            if (_inputs.Remove(input))
            {
                input.Component = null;
                InputRemoved?.Invoke(this, input);
                return true;
            }
            return false;
        }

        protected virtual void AddOutput(StudioComponentOutput output)
        {
            if (output.Id > 0)
            {
                if (IsEndpointIdInUse(output.Id))
                {
                    throw new ArgumentException("Cannot add an endpoint with an already existing ID.");
                }
            }
            else
            {
                output.Id = GetAvailableEndpointId();
            }
            _outputs.Add(output);
            output.Component = this;
            OutputAdded?.Invoke(this, output);
        }

        protected virtual bool RemoveOutput(StudioComponentOutput output)
        {
            if (_outputs.Remove(output))
            {
                output.Component = null;
                OutputRemoved?.Invoke(this, output);
                return true;
            }
            return false;
        }

        protected virtual void InvokePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, propertyName);


        private bool IsEndpointIdInUse(int id)
        {
            return _inputs.Exists(i => i.Id == id) || _outputs.Exists(i => i.Id == id);
        }

        private int GetAvailableEndpointId()
        {
            int id;
            do
            {
                id = GetNewEndpointId();
            } while (IsEndpointIdInUse(id));
            return id;
        }
    }
}
