using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using VirtualStudio.Shared;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.ViewModels
{
    public class StudioComponentEndpointViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public EndpointIOType IOType { get; }
        public int Id { get; }
        public DataKind DataKind { get; }
        public string ConnectionType { get; }
        public ComponentViewModel ComponentViewModel { get; }
        public (float x, float y)? Position => GetPosition();

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; InvokePropertyChanged(nameof(Name)); }
        }

        public StudioComponentEndpointViewModel(ComponentViewModel componentViewModel, StudioComponentEndpointDto dto)
            : this(componentViewModel, dto.IOType, dto.Id, dto.DataKind, dto.ConnectionType)
        {
            Name = dto.Name;
        }

        private (float, float)? GetPosition()
        {
            if (ComponentViewModel.Node is null)
                return null;
            int endpointIndex;
            if (IOType == EndpointIOType.Input)
                endpointIndex = ComponentViewModel.Inputs.IndexOf(this);
            else
                endpointIndex = ComponentViewModel.Outputs.IndexOf(this);
            float yOffset = ComponentViewModel.TitleHeight + endpointIndex * ComponentViewModel.EndpointHeight + (ComponentViewModel.EndpointHeight / 2);
            float xOffset = IOType == EndpointIOType.Input ? 0 : ComponentViewModel.Width;
            return (ComponentViewModel.Node.PositionX + xOffset, ComponentViewModel.Node.PositionY + yOffset);
        }

        public StudioComponentEndpointViewModel(ComponentViewModel componentViewModel, EndpointIOType endpointIOType, int id, DataKind dataKind, string connectionType)
        {
            ComponentViewModel = componentViewModel;
            IOType = endpointIOType;
            Id = id;
            DataKind = dataKind;
            ConnectionType = connectionType;
        }

        protected void InvokePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
