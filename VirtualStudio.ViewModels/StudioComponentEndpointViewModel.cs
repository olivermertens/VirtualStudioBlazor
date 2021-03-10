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
