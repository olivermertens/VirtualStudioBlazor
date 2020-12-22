using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtualStudio.Shared;

namespace VirtualStudio.Server
{
    public class StudioComponent : StudioObject
    {
        #region static
        public static StudioComponent FromStudioClient(StudioClient studioClient)
        {
            var studioComponent = new StudioComponent() { StudioClient = studioClient };
            foreach (var input in studioClient.IODescription.Inputs)
            {
                studioComponent.Inputs.Add(StudioEndpoint.FromEndpointDescription(input));
            }
            foreach (var output in studioClient.IODescription.Outputs)
            {
                studioComponent.Outputs.Add(StudioEndpoint.FromEndpointDescription(output));
            }
            return studioComponent;
        }

        private static TransmissionMethodDescription? SelectTransmissionMethod(StudioComponent from, StudioComponent to, DataKind dataKind)
        {
            foreach (var method in from.StudioClient.TransmissionMethods)
            {
                var methodList = to.StudioClient.TransmissionMethods.FindAll(tm => tm.Name == method.Name);
                if (methodList.Count > 0)
                {
                    foreach (var m in methodList)
                    {
                        if (m.SupportedDataKinds.HasFlag(dataKind))
                        {
                            return m;
                        }
                    }
                }
            }
            return null;
        }
        #endregion


        public event EventHandler StudioClientAssigned;
        public event EventHandler StudioClientRemoved;

        public StudioEndpointCollection Inputs { get; }
        public StudioEndpointCollection Outputs { get; }

        private StudioClient studioClient;
        public StudioClient StudioClient
        {
            get => studioClient;
            private set
            {
                if(value != studioClient)
                {
                    if(value is null)
                    {
                        studioClient.LinkStateChanged -= StudioClient_LinkStateChanged;
                        studioClient.ConnectionStateChanged -= StudioClient_ConnectionStateChanged;
                    }
                    else
                    {
                        studioClient.LinkStateChanged += StudioClient_LinkStateChanged;
                        studioClient.ConnectionStateChanged += StudioClient_ConnectionStateChanged;
                    }
                }
            }
        }

        private void StudioClient_ConnectionStateChanged(object sender, System.EventArgs e)
        {
            if (sender is StudioClient studioClient)
            {
                switch (studioClient.ConnectionState)
                {
                    case ClientConnectionState.Connected:
                        throw new System.NotImplementedException();
                    case ClientConnectionState.Disconnected:
                        SetAllLinkStates(LinkState.Unknown);
                        break;
                }
            }
        }

        private void SetAllLinkStates(LinkState state)
        {
            foreach (var input in Inputs)
            {
                foreach (var link in input.Links)
                {
                    link.ChangeStateOfFrom(state);
                }
            }
            foreach (var output in Outputs)
            {
                foreach (var link in output.Links)
                {
                    link.ChangeStateOfTo(state);
                }
            }
        }

        public StudioComponent()
        {
            Inputs = new StudioEndpointCollection(this);
            Outputs = new StudioEndpointCollection(this);
        }

        public void AddInput(string name, DataKind dataKind, DataDescription? dataDescription = null)
        {
            AddStudioEndpoint(name, dataKind, dataDescription, Inputs, StudioClient?.IODescription.Inputs);
        }

        public void AddOutput(string name, DataKind dataKind, DataDescription? dataDescription = null)
        {
            AddStudioEndpoint(name, dataKind, dataDescription, Outputs, StudioClient?.IODescription.Outputs);
        }

        public async Task RemoveStudioClient()
        {
            if (StudioClient == null)
            {
                return;
            }

            await DisconnectAllLinks();
            StudioClient = null;
            StudioClientRemoved?.Invoke(this, null);
        }

        public bool TryConnectLink(StudioLink link)
        {
            if (link.From == null || link.To == null || link.From.DataKind != link.To.DataKind || link.State != (LinkState.Disconnected, LinkState.Disconnected))
            {
                return false;
            }

            if (link.From.ContainingCollection.StudioComponent != this)
            {
                return false;
            }

            var toComponent = link.To.ContainingCollection.StudioComponent;

            var transmissionMethod = SelectTransmissionMethod(this, toComponent, link.From.DataKind);
            if (!transmissionMethod.HasValue)
            {
                return false;
            }

            switch (transmissionMethod.Value.Type)
            {
                case TransmissionMethodType.WebRtc:
                    return StudioClient.InitWebRtcConnection(this.Id, link.From.ToEndpointDescription());
                default:
                    return false;

            }
        }

        public async Task DisconnectAllLinks()
        {
            if (StudioClient != null && StudioClient.Hub.Clients.Client(StudioClient.ConnectionId) != null)
            {
                foreach (var input in Inputs)
                    foreach (var link in input.Links)
                        await StudioClient.DisconnectLink(link.Id);

                foreach (var output in Outputs)
                    foreach (var link in output.Links)
                        await StudioClient.DisconnectLink(link.Id);
            }
        }

        public bool CanAssignStudioClient(StudioClient studioClient)
        {
            if (StudioClient != null)
            {
                return false;
            }
            foreach (var input in Inputs)
            {
                if (!studioClient.IODescription.Inputs.Exists(i =>
                   (i.Name, i.DataKind, i.Description.Identifier) == (input.Name, input.DataKind, input.Description.Identifier)))
                {
                    return false;
                }
            }
            return true;
        }

        public bool TryAssignStudioClient(StudioClient studioClient)
        {
            if (CanAssignStudioClient(studioClient))
            {
                StudioClient = studioClient;
                StudioClientAssigned?.Invoke(this, null);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void StudioClient_LinkStateChanged(object sender, LinkStateUpdate e)
        {
            if(TryGetLink(e.LinkId, out StudioLink link, out bool isInput))
            {
                if(isInput)
                {
                    link.ChangeStateOfTo(e.State);
                }
                else
                {
                    link.ChangeStateOfFrom(e.State);
                }
            }
        }

        private bool TryGetLink(int linkId, out StudioLink studioLink, out bool isInput)
        {
            isInput = Inputs.TryGetLink(linkId, out studioLink);
            if(isInput || Outputs.TryGetLink(linkId, out studioLink))
            {
                return true;
            }
            return false;
        }

        private void AddStudioEndpoint(string name, DataKind dataKind, DataDescription? dataDescription,
                            StudioEndpointCollection targetCollection, List<EndpointDescription> studioClientCollection)
        {
            if (targetCollection.ContainsItemWithName(name))
            {
                throw new System.ArgumentException($"Endpoint with name {name} already exists.", nameof(name));
            }
            if (studioClientCollection != null)
            {
                var index = studioClientCollection.FindIndex(i => i.Name == name && i.DataKind == dataKind);
                if (index < 0)
                {
                    throw new System.ArgumentException($"Arguments do not match an endpoint of {nameof(StudioClient)}.");
                }
                targetCollection.Add(StudioEndpoint.FromEndpointDescription(studioClientCollection[index]));
            }
            else
            {
                if (dataKind == DataKind.Data && !dataDescription.HasValue)
                {
                    throw new System.ArgumentException($"A {nameof(dataDescription)} must be provided for DataKind Data", nameof(dataDescription));
                }
                StudioEndpoint studioEndpoint;
                switch (dataKind)
                {
                    case DataKind.Video:
                        studioEndpoint = StudioEndpoint.CreateVideoEndpoint(name);
                        break;
                    case DataKind.Audio:
                        studioEndpoint = StudioEndpoint.CreateAudioEndpoint(name);
                        break;
                    case DataKind.Data:
                        studioEndpoint = StudioEndpoint.CreateDataEndpoint(name, dataDescription.Value);
                        break;
                    default:
                        return;
                }
                targetCollection.Add(studioEndpoint);
            }
        }
    }
}