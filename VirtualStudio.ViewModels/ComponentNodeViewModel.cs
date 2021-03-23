using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.ViewModels
{
    public class ComponentNodeViewModel : NodeViewModel
    {
        private ComponentViewModel _component;
        public ComponentViewModel Component 
        {
            get => _component;
            set
            {
                if (_component != null)
                    _component.Node = null;
                _component = value;
                if (_component != null)
                    _component.Node = this;
            }
        }
    }
}
