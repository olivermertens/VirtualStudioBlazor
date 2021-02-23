using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtualStudio.Core;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Core.Arrangement;
using VirtualStudio.Core.Operations;
using VirtualStudio.Shared;
using VirtualStudio.Shared.Abstractions;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.Server
{
    public class ControlHub : Hub<IVirtualStudioUpdateListener>
    {
        #region static
        static ConcurrentDictionary<string, string> connectionIdToGroupMapping = new ConcurrentDictionary<string, string>();

        private async Task<OperationResponse> ExecuteOperation(IVirtualStudioOperation<bool> operation)
        {
            return (await ExecuteOperation<bool>(operation)).WithoutData();
        }

        private async Task<OperationResponse<T>> ExecuteOperation<T>(IVirtualStudioOperation<T> operation)
        {
            OperationResponse<T> response = new OperationResponse<T>();
            if (virtualStudios.TryGetVirtualStudio(connectionIdToGroupMapping[Context.ConnectionId], out Core.VirtualStudio virtualStudio))
            {
                response.Data = await operation.Process(virtualStudio);
                if (operation.Error is null)
                    response.Status = OperationStatus.Success;
                else
                {
                    response.Status = OperationStatus.Error;
                    response.Error = operation.Error;
                }
            }
            else
            {
                response.Status = OperationStatus.Error;
                response.Error = new OperationError(ErrorType.NotAuthorized, "The user did not join a VirtualStudio.");
            }
            if (response.Status == OperationStatus.Error)
                logger.LogWarning($"{operation}: Error: {response.Error.Type}, {response.Error.Message}");
            else
                logger.LogInformation($"{operation}: OK");

            return response;
        }
        #endregion

        private readonly VirtualStudioRepository virtualStudios;
        private readonly ILogger<ControlHub> logger;

        public ControlHub(VirtualStudioRepository virtualStudios, ILogger<ControlHub> logger)
        {
            this.virtualStudios = virtualStudios;
            this.logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            logger.LogInformation("Client connected: " + Context.ConnectionId);
            connectionIdToGroupMapping[Context.ConnectionId] = null;
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            logger.LogInformation("Client disconnected: " + Context.ConnectionId);
            connectionIdToGroupMapping.TryRemove(Context.ConnectionId, out string _);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task<bool> JoinVirtualStudio(string virtualStudioName)
        {
            string connectionId = Context.ConnectionId;
            if (connectionIdToGroupMapping.TryGetValue(connectionId, out string groupName) && groupName != null)
            {
                await Groups.RemoveFromGroupAsync(connectionId, groupName);
                logger.LogInformation($"Client '{Context.ConnectionId}' left {groupName}");
            }
            await Groups.AddToGroupAsync(connectionId, virtualStudioName);
            connectionIdToGroupMapping[connectionId] = virtualStudioName;
            logger.LogInformation($"Client '{Context.ConnectionId}' joined {virtualStudioName}");
            return true;
        }

        public async Task<bool> LeaveVirtualStudio(string virtualStudioName)
        {
            string connectionId = Context.ConnectionId;
            if (connectionIdToGroupMapping.TryGetValue(connectionId, out string groupName) && groupName != null)
            {
                await Groups.RemoveFromGroupAsync(connectionId, virtualStudioName);
                connectionIdToGroupMapping[connectionId] = null;
                logger.LogInformation($"Client '{Context.ConnectionId}' left {virtualStudioName}");
            }
            return true;
        }

        public Task<OperationResponse<VirtualStudioWithArrangementDto>> GetVirtualStudioWithArrangement()
            => ExecuteOperation(new GetVirtualStudioWithArrangementQuery());

        public Task<OperationResponse> AddPlaceholderToRepository(StudioComponentDto placeholder)
            => ExecuteOperation(new AddPlaceholderToRepositoryCommand(placeholder));

        public Task<OperationResponse> RemovePlaceholderFromRepository(int componentId)
            => ExecuteOperation(new RemovePlaceholderFromRepositoryCommand(componentId));

        public Task<OperationResponse> AddInputToPlaceholder(int componentId, StudioComponentEndpointDto input)
            => ExecuteOperation(new AddInputToPlaceholderCommand(componentId, input));

        public Task<OperationResponse> AddOutputToPlaceholder(int componentId, StudioComponentEndpointDto output)
            => ExecuteOperation(new AddOutputToPlaceholderCommand(componentId, output));

        public Task<OperationResponse> RemoveInputFromPlaceholder(int componentId, int inputId)
            => ExecuteOperation(new RemoveInputFromPlaceholderCommand(componentId, inputId));

        public Task<OperationResponse> RemoveOutputFromPlaceholder(int componentId, int outputId)
            => ExecuteOperation(new RemoveOutputFromPlaceholderCommand(componentId, outputId));

        public Task<OperationResponse> AddComponent(int componentId)
            => ExecuteOperation(new AddComponentCommand(componentId));

        public Task<OperationResponse> RemoveComponent(int componentId)
            => ExecuteOperation(new RemoveComponentCommand(componentId));

        public Task<OperationResponse> CreateConnection(int outputComponentId, int outputId, int inputComponentId, int inputId)
            => ExecuteOperation(new CreateConnectionCommand(outputComponentId, outputId, inputComponentId, inputId));

        public Task<OperationResponse> RemoveConnection(int connectionId)
            => ExecuteOperation(new RemoveConnectionCommand(connectionId));

        public Task<OperationResponse> ChangeTargetConnectionState(int connectionId, ConnectionState state)
            => ExecuteOperation(new ChangeTargetConnectionStateCommand(connectionId, state));

        public Task<OperationResponse> ChangeComponentProperty(int componentId, string propertyName, object value)
            => ExecuteOperation(new ChangeComponentPropertyCommand(componentId, propertyName, value));

        public Task<OperationResponse> MoveComponentNode(int componentId, Position2D position)
            => ExecuteOperation(new MoveComponentNodeCommand(componentId, position));
    }
}