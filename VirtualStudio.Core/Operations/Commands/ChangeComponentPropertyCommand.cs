using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Shared;

namespace VirtualStudio.Core.Operations
{
    public class ChangeComponentPropertyCommand : IVirtualStudioCommand<bool>
    {
        public OperationError Error { get; private set; }

        private readonly int componentId;
        private readonly string propertyName;
        private readonly object value;

        public ChangeComponentPropertyCommand(int componentId, string propertyName, object value)
        {
            this.componentId = componentId;
            this.propertyName = propertyName;
            this.value = value;
        }

        public Task<bool> Process(VirtualStudio virtualStudio)
        {
            return Task.FromResult(ProcessSync(virtualStudio));
        }

        private bool ProcessSync(VirtualStudio virtualStudio)
        {
            var component = virtualStudio.FindStudioComponentById(componentId);
            if (component is null)
            {
                Error = new OperationError(ErrorType.NotFound, $"Component with ID {componentId} not found.");
                return false;
            }

            return ChangeProperty(component, propertyName, value);
        }

        private bool ChangeProperty(IStudioComponent component, string propertyName, object value)
        {
            switch (propertyName)
            {
                case nameof(component.Name):
                    if (value is string nameString)
                    {
                        component.SetName(nameString);
                        return true;
                    }
                    break;
            }
            return false;
        }
    }
}
