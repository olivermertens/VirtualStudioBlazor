using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Shared.Abstractions;
using VirtualStudio.Shared.DTOs;
using VirtualStudio.Shared.DTOs.WebRtc;

namespace VirtualStudio.Shared.Connection
{
    public class SignalRVirtualStudioConnection : IVirtualStudioConnection, IAsyncDisposable
    {
        const string VIRTUAL_STUDIO_HUB_NAME = "virtualstudio";

        public event EventHandler<SdpOfferRequestArgs> SdpOfferRequestReceived;
        public event EventHandler<SdpAnswerRequestArgs> SdpAnswerRequestReceived;
        public event EventHandler<IceCandidateArgs> IceCandidateReceived;
        public event EventHandler<ConnectWebRtcCommandArgs> ConnectCommandReceived;
        public event EventHandler<DisconnectWebRtcCommandArgs> DisconnectCommandReceived;

        public HubConnectionState ConnectionState => hubConnection.State;

        private readonly HubConnection hubConnection;
        private HashSet<IWebRtcClient> webRtcClients = new HashSet<IWebRtcClient>();
        private List<IDisposable> eventHandlers = new List<IDisposable>();

        public static Task<SignalRVirtualStudioConnection> CreateAsync(string host, int port, bool allowUnsafeConnection = false)
        {
            return CreateAsync($"https://{host}:{port}/", allowUnsafeConnection);
        }

        public static async Task<SignalRVirtualStudioConnection> CreateAsync(string baseUri, bool allowUnsafeConnection = false)
        {
            var connection = new SignalRVirtualStudioConnection(baseUri, allowUnsafeConnection);
            await connection.hubConnection.StartAsync();
            return connection;
        }

        private SignalRVirtualStudioConnection(string baseUri, bool allowUnsafeConnection)
        {
            hubConnection = new HubConnectionBuilder().WithUrl($"{baseUri}{VIRTUAL_STUDIO_HUB_NAME}", options =>
                {
                    if (allowUnsafeConnection)
                    {
                        options.HttpMessageHandlerFactory = factory =>
                        {
                            var handler = new HttpClientHandler();
                            handler.ServerCertificateCustomValidationCallback = (a, b, c, d) => true;
                            return handler;
                        };
                    }
                })
                .Build();

            eventHandlers.Add(hubConnection.On<SdpOfferRequestArgs>(nameof(RequestSdpOffer), RequestSdpOffer));
            eventHandlers.Add(hubConnection.On<SdpAnswerRequestArgs>(nameof(RequestSdpAnswer), RequestSdpAnswer));
            eventHandlers.Add(hubConnection.On<IceCandidateArgs>(nameof(AddIceCandidate), AddIceCandidate));
            eventHandlers.Add(hubConnection.On<ConnectWebRtcCommandArgs>(nameof(Connect), Connect));
            eventHandlers.Add(hubConnection.On<DisconnectWebRtcCommandArgs>(nameof(Disconnect), Disconnect));
        }

        public async ValueTask DisposeAsync()
        {
            DisposeHubConnectionEventHandlers();
            await hubConnection.DisposeAsync();
        }

        private void DisposeHubConnectionEventHandlers()
        {
            foreach (var handler in eventHandlers)
                handler.Dispose();

            eventHandlers.Clear();
        }

        public void AddListener(IWebRtcClient webRtcClient)
        {
            webRtcClients.Add(webRtcClient);
        }

        public void RemoveListener(IWebRtcClient webRtcClient)
        {
            webRtcClients.Remove(webRtcClient);
        }

        public void ClearListeners()
        {
            webRtcClients.Clear();
        }

        public Task<bool> JoinVirtualStudio(string virtualStudioName, StudioComponentDto studioComponent)
            => hubConnection.InvokeAsync<bool>(nameof(JoinVirtualStudio), virtualStudioName, studioComponent);


        public Task<bool> LeaveVirtualStudio(string virtualStudioName)
            => hubConnection.InvokeAsync<bool>(nameof(LeaveVirtualStudio), virtualStudioName);

        #region IVirtualStudioConnection Methods
        public Task UpdateInputConnectionState(int inputId, int connectionId, ConnectionState state)
            => hubConnection.InvokeAsync(nameof(UpdateInputConnectionState), inputId, connectionId, state);

        public Task UpdateOutputConnectionState(int outputId, int connectionId, ConnectionState state)
            => hubConnection.InvokeAsync(nameof(UpdateOutputConnectionState), outputId, connectionId, state);

        public Task AddInput(StudioComponentEndpointDto input)
            => hubConnection.InvokeAsync(nameof(AddInput), input);

        public Task RemoveInput(int inputId)
            => hubConnection.InvokeAsync(nameof(RemoveInput), inputId);

        public Task AddOutput(StudioComponentEndpointDto output)
            => hubConnection.InvokeAsync(nameof(AddOutput), output);

        public Task RemoveOutput(int outputId)
            => hubConnection.InvokeAsync(nameof(RemoveOutput), outputId);

        public Task ChangeProperty(string propertyName, object value)
            => hubConnection.InvokeAsync(nameof(ChangeProperty), propertyName, value);

        public Task SendSdpOffer(SdpOfferResponseArgs args)
            => hubConnection.InvokeAsync(nameof(SendSdpOffer), args);

        public Task SendSdpAnswer(SdpAnswerResponseArgs args)
            => hubConnection.InvokeAsync(nameof(SendSdpAnswer), args);

        public Task SendIceCandidate(IceCandidateArgs args)
            => hubConnection.InvokeAsync(nameof(SendIceCandidate), args);

        #endregion

        #region IWebRtcClient
        private async Task RequestSdpOffer(SdpOfferRequestArgs args)
        {
            await InvokeOnClients(webRtcClient => webRtcClient.RequestSdpOffer(args));
            SdpOfferRequestReceived?.Invoke(this, args);
        }

        private async Task RequestSdpAnswer(SdpAnswerRequestArgs args)
        {
            await InvokeOnClients(webRtcClient => webRtcClient.RequestSdpAnswer(args));
            SdpAnswerRequestReceived?.Invoke(this, args);
        }

        private async Task AddIceCandidate(IceCandidateArgs args)
        {
            await InvokeOnClients(webRtcClient => webRtcClient.AddIceCandidate(args));
            IceCandidateReceived?.Invoke(this, args);
        }

        private async Task Connect(ConnectWebRtcCommandArgs args)
        {
            await InvokeOnClients(webRtcClient => webRtcClient.Connect(args));
            ConnectCommandReceived?.Invoke(this, args);
        }
        private async Task Disconnect(DisconnectWebRtcCommandArgs args)
        {
            await InvokeOnClients(webRtcClient => webRtcClient.Disconnect(args));
            DisconnectCommandReceived?.Invoke(this, args);
        }

        private async Task InvokeOnClients(Func<IWebRtcClient, Task> functionCall)
        {
            foreach (var client in webRtcClients)
                await functionCall(client);
        }

        #endregion
    }
}
