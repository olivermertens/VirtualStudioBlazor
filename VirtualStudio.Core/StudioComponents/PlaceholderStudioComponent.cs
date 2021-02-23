using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Shared;

namespace VirtualStudio.Core
{
    public class PlaceholderStudioComponent : IStudioComponent
    {
        public int Id { get; private set; }

        public string Name { get; private set; }


        private List<StudioComponentOutput> _outputs = new List<StudioComponentOutput>();
        public IList<StudioComponentOutput> Outputs => _outputs;

        private List<StudioComponentInput> _inputs = new List<StudioComponentInput>();
        public IList<StudioComponentInput> Inputs => _inputs;

        public event EventHandler<StudioComponentInput> InputAdded;
        public event EventHandler<StudioComponentInput> InputRemoved;
        public event EventHandler<StudioComponentOutput> OutputAdded;
        public event EventHandler<StudioComponentOutput> OutputRemoved;
        public event EventHandler<string> PropertyChanged;
        public event EventHandler<(StudioComponentInput input, IStudioConnection connection, ConnectionState state)> InputConnectionStateUpdated;
        public event EventHandler<(StudioComponentOutput output, IStudioConnection connection, ConnectionState state)> OutputConnectionStateUpdated;

        private IdGenerator idGenerator = new IdGenerator();

        public T GetConnectionHandler<T>() where T : IConnectionHandler
        {
            throw new NotImplementedException();
        }

        public bool SetId(int id)
        {
            if (id != Id)
            {
                Id = id;
                PropertyChanged?.Invoke(this, nameof(Id));
            }
            return true;
        }

        public bool SetName(string name)
        {
            if (Name != name)
            {
                Name = name;
                PropertyChanged?.Invoke(this, nameof(Name));
            }
            return true;
        }

        public Task<bool> StartConnectionAsync(IStudioConnection connection)
        {
            return Task.FromResult(false);
        }

        public Task<bool> StopConnectionAsync(IStudioConnection connection)
        {
            return Task.FromResult(true);
        }

        public StudioComponentInput AddInput(string name, DataKind dataKind, string connectionType)
        {
            var input = new StudioComponentInput(idGenerator.GetNewId(), name, dataKind, connectionType, this);
            _inputs.Add(input);
            InputAdded?.Invoke(this, input);
            return input;
        }

        public void RemoveInput(StudioComponentInput input)
        {
            if (_inputs.Remove(input))
            {
                InputRemoved?.Invoke(this, input);
            }
        }

        public StudioComponentOutput AddOutput(string name, DataKind dataKind, string connectionType)
        {
            var output = new StudioComponentOutput(idGenerator.GetNewId(), name, dataKind, connectionType, this);
            _outputs.Add(output);
            OutputAdded?.Invoke(this, output);
            return output;
        }

        public void RemoveOutput(StudioComponentOutput output)
        {
            if (_outputs.Remove(output))
            {
                OutputRemoved?.Invoke(this, output);
            }
        }

        public void HandleConnectionTargetStateChanged(StudioComponentInput input, IStudioConnection studioConnection, ConnectionState state)
        {
            if (!Inputs.Contains(input))
                throw new ArgumentException("The provided input does not belong to this component.");
            if (studioConnection.Input != input)
                throw new ArgumentException("The provided input does not belong to the provided connnection.");

            //Do nothing.
        }

        internal PlaceholderStudioComponent Clone()
        {
            var clone = new PlaceholderStudioComponent
            {
                Id = Id,
                Name = Name,
            };
            foreach(var input in Inputs)
                clone.Inputs.Add(new StudioComponentInput(input.Id, input.Name, input.DataKind, input.ConnectionType, clone));
            foreach (var output in Outputs)
                clone.Outputs.Add(new StudioComponentOutput(output.Id, output.Name, output.DataKind, output.ConnectionType, clone));

            return clone;
        }
    }
}
