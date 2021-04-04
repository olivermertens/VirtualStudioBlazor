using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.ConnectionTypes.WebRtc;
using VirtualStudio.Core;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Shared;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.StudioClient
{
    public class StudioClientComponent : IStudioComponent
    {
        public event EventHandler<(StudioComponentInput input, int connectionId, ConnectionState state)> InputConnectionStateUpdated;
        public event EventHandler<(StudioComponentOutput output, int connectionId, ConnectionState state)> OutputConnectionStateUpdated;
        public event EventHandler<StudioComponentInput> InputAdded;
        public event EventHandler<StudioComponentInput> InputRemoved;
        public event EventHandler<StudioComponentOutput> OutputAdded;
        public event EventHandler<StudioComponentOutput> OutputRemoved;
        public event EventHandler<string> PropertyChanged;

        public int Id { get; private set; }

        public string Name { get; private set; }

        public IList<StudioComponentOutput> Outputs { get; } = new List<StudioComponentOutput>();
        public IList<StudioComponentInput> Inputs { get; } = new List<StudioComponentInput>();

        private readonly IStudioClient studioClient;

        public StudioClientComponent(IStudioClient studioClient)
        {
            this.studioClient = studioClient ?? throw new ArgumentNullException(nameof(studioClient));
            AttachEventHandlers(this.studioClient);
        }

        public StudioClientComponent(IStudioClient studioClient, StudioComponentDto studioComponentDto) : this(studioClient)
        {
            Id = studioComponentDto.Id;
            Name = studioComponentDto.Name;
            if (studioComponentDto.Inputs != null)
                foreach (var input in studioComponentDto.Inputs)
                    Inputs.Add(new StudioComponentInput(input.Id, input.Name, input.DataKind, input.ConnectionType, this));
            if (studioComponentDto.Outputs != null)
                foreach (var output in studioComponentDto.Outputs)
                    Outputs.Add(new StudioComponentOutput(output.Id, output.Name, output.DataKind, output.ConnectionType, this));
        }

        public T GetConnectionHandler<T>() where T : IConnectionHandler
        {
            if (studioClient is T connectionHandler)
                return connectionHandler;

            return default(T);
        }

        public void HandleConnectionTargetStateChanged(StudioComponentInput input, IStudioConnection studioConnection, ConnectionState state)
        {
            if (state == studioConnection.State)
                return;

            if (state != ConnectionState.Connected && state != ConnectionState.Disconnected && state != ConnectionState.Destroyed)
                throw new ArgumentException(nameof(state));
            if (studioConnection.Input != input || studioConnection.Output is null)
                throw new ArgumentException(nameof(studioConnection));
            if (!Inputs.Contains(input))
                throw new ArgumentException(nameof(input));

            switch(input.ConnectionType)
            {
                case "WebRTC":
                    HandleWebRtcConnectionTargetStateChanged(input, studioConnection, state);
                    break;
            }
        }

        private async void HandleWebRtcConnectionTargetStateChanged(StudioComponentInput input, IStudioConnection studioConnection, ConnectionState state)
        {
            var inputConnectionHandler = studioClient as IWebRtcConnectionHandler;
            var outputConnectionHandler = studioConnection.Output.Component.GetConnectionHandler<IWebRtcConnectionHandler>();
            if (inputConnectionHandler is null || outputConnectionHandler is null)
                return;

            switch (state)
            {
                case ConnectionState.Connected:
                    inputConnectionHandler.IceCandidateReceived += (s, e) =>
                    {
                        if (e.connectionId == studioConnection.Id)
                            outputConnectionHandler.AddIceCandidate(studioConnection, e.candidateJson);
                    };
                    outputConnectionHandler.IceCandidateReceived += (s, e) =>
                    {
                        if (e.connectionId == studioConnection.Id)
                            inputConnectionHandler.AddIceCandidate(studioConnection, e.candidateJson);
                    };
                    (string sdpOffer, bool supportsInsertableStreams) = await outputConnectionHandler.GetSdpOffer(studioConnection);
                    System.Diagnostics.Debug.WriteLine("SDP Offer: " + sdpOffer);
                    (string sdpAnswer, bool useInsertableStreams) = await inputConnectionHandler.GetSdpAnswer(studioConnection, sdpOffer, supportsInsertableStreams);
                    System.Diagnostics.Debug.WriteLine("SDP Answer: " + sdpAnswer);
                    await outputConnectionHandler.Connect(studioConnection, sdpAnswer, useInsertableStreams);
                    break;
                case ConnectionState.Disconnected:
                case ConnectionState.Destroyed:
                    var outputDisconnectTask = outputConnectionHandler.Disconnect(studioConnection);
                    await inputConnectionHandler.Disconnect(studioConnection);
                    await outputDisconnectTask;
                    break;
            }
        }



        public bool SetId(int id)
        {
            Id = id;
            return true;
        }

        public bool SetName(string name)
        {
            Name = name;
            return true;
        }

        private void AttachEventHandlers(IStudioClient studioClient)
        {
            studioClient.InputConnectionStateUpdated += StudioClient_InputConnectionStateUpdated;
            studioClient.OutputConnectionStateUpdated += StudioClient_OutputConnectionStateUpdated;
            studioClient.InputAdded += StudioClient_InputAdded;
            studioClient.InputRemoved += StudioClient_InputRemoved;
            studioClient.OutputAdded += StudioClient_OutputAdded;
            studioClient.OutputRemoved += StudioClient_OutputRemoved;
            studioClient.PropertyChanged += StudioClient_PropertyChanged;
        }

        private void DetachEventHandlers(IStudioClient studioClient)
        {
            studioClient.InputConnectionStateUpdated -= StudioClient_InputConnectionStateUpdated;
            studioClient.OutputConnectionStateUpdated -= StudioClient_OutputConnectionStateUpdated;
            studioClient.InputAdded -= StudioClient_InputAdded;
            studioClient.InputRemoved -= StudioClient_InputRemoved;
            studioClient.OutputAdded -= StudioClient_OutputAdded;
            studioClient.OutputRemoved -= StudioClient_OutputRemoved;
            studioClient.PropertyChanged -= StudioClient_PropertyChanged;
        }

        private void StudioClient_InputConnectionStateUpdated(object sender, (int inputId, int connectionId, ConnectionState state) e)
        {
            var input = Inputs.FirstOrDefault(i => i.Id == e.inputId);
            if (input is null)
                return;

            InputConnectionStateUpdated?.Invoke(this, (input, e.connectionId, e.state));
        }

        private void StudioClient_OutputConnectionStateUpdated(object sender, (int outputId, int connectionId, ConnectionState state) e)
        {
            var output = Outputs.FirstOrDefault(i => i.Id == e.outputId);
            if (output is null)
                return;

            OutputConnectionStateUpdated?.Invoke(this, (output, e.connectionId, e.state));
        }

        private void StudioClient_InputAdded(object sender, StudioComponentEndpointDto e)
        {
            var input = new StudioComponentInput(e.Id, e.Name, e.DataKind, e.ConnectionType, this);
            Inputs.Add(input);
            InputAdded?.Invoke(this, input);
        }

        private void StudioClient_InputRemoved(object sender, int inputId)
        {
            var input = Inputs.FirstOrDefault(i => i.Id == inputId);
            if (input is null)
                return;

            Inputs.Remove(input);
        }

        private void StudioClient_OutputAdded(object sender, StudioComponentEndpointDto e)
        {
            var output = new StudioComponentOutput(e.Id, e.Name, e.DataKind, e.ConnectionType, this);
            Outputs.Add(output);
            OutputAdded?.Invoke(this, output);
        }

        private void StudioClient_OutputRemoved(object sender, int outputId)
        {
            var output = Outputs.FirstOrDefault(o => o.Id == outputId);
            if (output is null)
                return;

            Outputs.Remove(output);
        }
        private void StudioClient_PropertyChanged(object sender, (string propertyName, object value) e)
        {
            switch(e.propertyName)
            {
                case nameof(Name):
                    Name = e.value as string;
                    break;
                case nameof(Id):
                    Id = (int)e.value;
                    break;
            }
        }
    }
}
