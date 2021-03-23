using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.ConnectionTypes.WebRtc;
using VirtualStudio.Core;
using VirtualStudio.Shared;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.StudioClient
{
    public interface IStudioClient : IWebRtcConnectionHandler
    {
        event EventHandler<(int inputId, int connectionId, ConnectionState state)> InputConnectionStateUpdated;
        event EventHandler<(int outputId, int connectionId, ConnectionState state)> OutputConnectionStateUpdated;
        event EventHandler<StudioComponentEndpointDto> InputAdded;
        event EventHandler<int> InputRemoved;
        event EventHandler<StudioComponentEndpointDto> OutputAdded;
        event EventHandler<int> OutputRemoved;
        event EventHandler<(string propertyName, object value)> PropertyChanged;
    }
}
