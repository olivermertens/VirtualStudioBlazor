using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Shared;

namespace VirtualStudio.Core.Abstractions
{
    public interface IStudioComponent
    {
        event EventHandler<(StudioComponentInput input, int connectionId, ConnectionState state)> InputConnectionStateUpdated;
        event EventHandler<(StudioComponentOutput output, int connectionId, ConnectionState state)> OutputConnectionStateUpdated;
        event EventHandler<StudioComponentInput> InputAdded;
        event EventHandler<StudioComponentInput> InputRemoved;
        event EventHandler<StudioComponentOutput> OutputAdded;
        event EventHandler<StudioComponentOutput> OutputRemoved;
        event EventHandler<string> PropertyChanged;

        int Id { get; }
        string Name { get; }
        IList<StudioComponentOutput> Outputs { get; }
        IList<StudioComponentInput> Inputs { get; }
        bool SetId(int id);
        bool SetName(string name);
        void HandleConnectionTargetStateChanged(StudioComponentInput input, IStudioConnection studioConnection, ConnectionState state);
        T GetConnectionHandler<T>() where T : IConnectionHandler;
    }
}
