using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using VirtualStudio.Shared;
using VirtualStudio.Shared.Abstractions;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.Client
{
    public class VirtualStudioViaHubConnectionController : IVirtualStudioController
    {
        public string VirtualStudioName { get; set; }
        public HubConnection HubConnection { get; set; }

        private async Task<OperationResponse> InvokeIfConnected(Func<Task<OperationResponse>> hubInvocation)
        {
            if (HubConnection != null && HubConnection.State == HubConnectionState.Connected)
                return await hubInvocation();

            return new OperationResponse(new OperationError(ErrorType.Disconnected, "Not connected."));
        }

        public Task<OperationResponse> AddNewPlaceholderToRepository()
            => InvokeIfConnected(() => HubConnection.InvokeAsync<OperationResponse>(nameof(AddNewPlaceholderToRepository)));

        public Task<OperationResponse> AddPlaceholderToRepository(StudioComponentDto placeholder)
            => InvokeIfConnected(() => HubConnection.InvokeAsync<OperationResponse>(nameof(AddPlaceholderToRepository), placeholder));

        public Task<OperationResponse> RemovePlaceholderFromRepository(int componentId)
            => InvokeIfConnected(() => HubConnection.InvokeAsync<OperationResponse>(nameof(RemovePlaceholderFromRepository), componentId));

        public Task<OperationResponse> AddInputToPlaceholder(int componentId, StudioComponentEndpointDto input)
            => InvokeIfConnected(() => HubConnection.InvokeAsync<OperationResponse>(nameof(AddInputToPlaceholder), componentId, input));

        public Task<OperationResponse> AddOutputToPlaceholder(int componentId, StudioComponentEndpointDto output)
            => InvokeIfConnected(() => HubConnection.InvokeAsync<OperationResponse>(nameof(AddOutputToPlaceholder), componentId, output));

        public Task<OperationResponse> RemoveInputFromPlaceholder(int componentId, int inputId)
            => InvokeIfConnected(() => HubConnection.InvokeAsync<OperationResponse>(nameof(RemoveInputFromPlaceholder), componentId, inputId));

        public Task<OperationResponse> RemoveOutputFromPlaceholder(int componentId, int outputId)
            => InvokeIfConnected(() => HubConnection.InvokeAsync<OperationResponse>(nameof(RemoveOutputFromPlaceholder), componentId, outputId));

        public Task<OperationResponse> AddComponent(int componentId)
            => InvokeIfConnected(() => HubConnection.InvokeAsync<OperationResponse>(nameof(AddComponent), componentId));

        public Task<OperationResponse> AddComponentNode(int componentId, float x, float y)
            => InvokeIfConnected(() => HubConnection.InvokeAsync<OperationResponse>(nameof(AddComponentNode), componentId, x, y));

        public Task<OperationResponse> RemoveComponent(int componentId)
            => InvokeIfConnected(() => HubConnection.InvokeAsync<OperationResponse>(nameof(RemoveComponent), componentId));

        public Task<OperationResponse> ChangeComponentProperty(int componentId, string propertyName, object value)
            => InvokeIfConnected(() => HubConnection.InvokeAsync<OperationResponse>(nameof(ChangeComponentProperty), componentId, propertyName, value));

        public Task<OperationResponse> MoveComponentNode(int componentId, float x, float y)
            => InvokeIfConnected(() => HubConnection.InvokeAsync<OperationResponse>(nameof(MoveComponentNode), componentId, x, y));

        public Task<OperationResponse> CreateConnection(int outputComponentId, int outputId, int inputComponentId, int inputId)
            => InvokeIfConnected(() => HubConnection.InvokeAsync<OperationResponse>(nameof(CreateConnection), outputComponentId, outputId, inputComponentId, inputId));

        public Task<OperationResponse> RemoveConnection(int connectionId)
            => InvokeIfConnected(() => HubConnection.InvokeAsync<OperationResponse>(nameof(RemoveConnection), connectionId));

        public Task<OperationResponse> ChangeTargetConnectionState(int connectionId, ConnectionState state)
            => InvokeIfConnected(() => HubConnection.InvokeAsync<OperationResponse>(nameof(ChangeTargetConnectionState), connectionId, state));
    }
}
