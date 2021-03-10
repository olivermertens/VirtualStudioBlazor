using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VirtualStudio.ViewModels
{
    public class NodeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; InvokePropertyChanged(nameof(IsSelected)); }
        }

        private float _x, _y;
        public float PositionX { get => _x; set { _x = value; InvokePropertyChanged(nameof(PositionX)); } }
        public float PositionY { get => _y; set { _y = value; InvokePropertyChanged(nameof(PositionY)); } }
        protected void InvokePropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
