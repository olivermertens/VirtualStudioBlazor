using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Shared;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.ViewModels
{
    public class ConnectionViewModel
    {
        public int Id { get; }
        public StudioComponentEndpointViewModel Input { get; }
        public StudioComponentEndpointViewModel Output { get; }
        public ConnectionState State { get; set; }

        public ConnectionViewModel(int id, StudioComponentEndpointViewModel input, StudioComponentEndpointViewModel output, ConnectionState state)
        {
            Id = id;
            Input = input;
            Output = output;
            State = state;
        }
    }
}
