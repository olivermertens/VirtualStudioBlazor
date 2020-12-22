using System;
using System.Threading.Tasks;
using VirtualStudio.Shared;

namespace VirtualStudio.Server
{
    public class StudioLink : StudioObject
    {
        public event EventHandler<(LinkState from, LinkState to)> StateChanged;
        public StudioEndpoint From { get; private set; }
        public StudioEndpoint To { get; private set; }
        public (LinkState from, LinkState to) State
        {
            get => state;
            private set { state = value; StateChanged?.Invoke(this, state); }
        }

        private (LinkState from, LinkState to) state;

        public StudioLink(StudioEndpoint from, StudioEndpoint to)
        {
            From = from;
            To = to;
            From.Links.Add(this);
            To.Links.Add(this);
        }

        public void RemoveEndpoints()
        {
            From.Links.Remove(this);
            To.Links.Remove(this);
            From = To = null;
        }

        public void ChangeStateOfFrom(LinkState state)
        {
            State = (state, State.to);
        }

        public void ChangeStateOfTo(LinkState state)
        {
            State = (State.from, state);
        }

        public bool TryInitConnection()
        {
            return From.ContainingCollection.StudioComponent?.TryConnectLink(this) ?? false;
        }
    }
}