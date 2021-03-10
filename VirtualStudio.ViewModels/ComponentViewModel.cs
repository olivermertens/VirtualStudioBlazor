using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.ViewModels
{
    public class ComponentViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int Id { get; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; InvokePropertyChanged(nameof(Name)); }
        }

        public ObservableCollection<StudioComponentEndpointViewModel> Inputs { get; } = new ObservableCollection<StudioComponentEndpointViewModel>();
        public ObservableCollection<StudioComponentEndpointViewModel> Outputs { get; } = new ObservableCollection<StudioComponentEndpointViewModel>();

        public ComponentViewModel(StudioComponentDto studioComponentDto)
        {
            Id = studioComponentDto.Id;
            Name = studioComponentDto.Name;
            if (studioComponentDto.Inputs != null)
                foreach (var input in studioComponentDto.Inputs)
                    Inputs.Add(new StudioComponentEndpointViewModel(this, input));
            if (studioComponentDto.Outputs != null)
                foreach (var output in studioComponentDto.Outputs)
                    Outputs.Add(new StudioComponentEndpointViewModel(this, output));
        }

        protected void InvokePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
