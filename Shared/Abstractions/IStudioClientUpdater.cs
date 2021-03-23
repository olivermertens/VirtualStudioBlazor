using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.Shared.Abstractions
{
    public interface IStudioClientUpdater
    {
        Task UpdateInputConnectionState(int inputId, int connectionId, ConnectionState state);
        Task UpdateOutputConnectionState(int outputId, int connectionId, ConnectionState state);
        Task AddInput(StudioComponentEndpointDto input);
        Task RemoveInput(int inputId);
        Task AddOutput(StudioComponentEndpointDto output);
        Task RemoveOutput(int outputId);
        Task ChangeProperty(string propertyName, object value);
    }
}
