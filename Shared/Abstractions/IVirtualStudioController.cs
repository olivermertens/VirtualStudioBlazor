using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.Shared.Abstractions
{
    public interface IVirtualStudioController
    {
        Task<OperationResponse> AddNewPlaceholderToRepository();

        Task<OperationResponse> AddPlaceholderToRepository(StudioComponentDto placeholder);

        Task<OperationResponse> RemovePlaceholderFromRepository(int componentId);

        Task<OperationResponse> AddInputToPlaceholder(int componentId, StudioComponentEndpointDto input);

        Task<OperationResponse> AddOutputToPlaceholder(int componentId, StudioComponentEndpointDto output);

        Task<OperationResponse> RemoveInputFromPlaceholder(int componentId, int inputId);

        Task<OperationResponse> RemoveOutputFromPlaceholder(int componentId, int outputId);

        Task<OperationResponse> AddComponent(int componentId);

        Task<OperationResponse> AddComponentNode(int componentId, float x, float y);

        Task<OperationResponse> RemoveComponent(int componentId);

        Task<OperationResponse> ChangeComponentProperty(int componentId, string propertyName, object value);

        Task<OperationResponse> MoveComponentNode(int componentId, float x, float y);

        Task<OperationResponse> CreateConnection(int outputComponentId, int outputId, int inputComponentId, int inputId);

        Task<OperationResponse> RemoveConnection(int connectionId);

        Task<OperationResponse> ChangeTargetConnectionState(int connectionId, ConnectionState state);

    }
}
