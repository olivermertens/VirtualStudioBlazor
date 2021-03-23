using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using System.Linq;
using VirtualStudio.ViewModels;
using VirtualStudio.Shared.Abstractions;

namespace VirtualStudio.Client.Shared
{
    public interface INoodleDragService
    {
        NoodleDataCustom TempNoodle { get; }
        StudioComponentEndpointViewModel Output { get; }

        void CancelDrag();
        void OnDropNoodle(StudioComponentEndpointViewModel input);
        void OnStartNoodleDrag(StudioComponentEndpointViewModel output);
        void OnStartNoodleDrag(StudioComponentEndpointViewModel output, StudioComponentEndpointViewModel input);
    }

    public class NoodleDataCustom : INoodleData
    {
        public Vector2 StartPos { get; set; }
        public Vector2 EndPos { get; set; }
        public bool Connected { get; set; }

        public event EventHandler NoodleChanged;
        public string Color => "var(--col-connectionstate-disconnected)";

        public void Refresh()
        {
            NoodleChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class NoodleDragService : INoodleDragService
    {
        public StudioComponentEndpointViewModel Output { get; set; }
        public NoodleDataCustom TempNoodle { get; } = new NoodleDataCustom() { Connected = false };

        private readonly IJSRuntime jsRuntime;

        public NoodleDragService(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
        }

        public void OnStartNoodleDrag(StudioComponentEndpointViewModel output)
        {
            if (!IsEndpointValid(output) || output.IOType != VirtualStudio.Shared.EndpointIOType.Output)
                throw new ArgumentException(nameof(output));

            var position = output.Position.Value;
            OnStartNoodleDrag(output, position.x, position.y);
        }

        public void OnStartNoodleDrag(StudioComponentEndpointViewModel output, StudioComponentEndpointViewModel input)
        {
            if(!IsEndpointValid(output) || output.IOType != VirtualStudio.Shared.EndpointIOType.Output)
                throw new ArgumentException(nameof(output));

            if (!IsEndpointValid(input) || input.IOType != VirtualStudio.Shared.EndpointIOType.Input)
                throw new ArgumentException(nameof(input));

            var position = output.Position.Value;
            OnStartNoodleDrag(output, position.x, position.y);
        }

        private void OnStartNoodleDrag(StudioComponentEndpointViewModel output, float positionX, float positionY)
        {
            Output = output;
            TempNoodle.Connected = true;
            jsRuntime.InvokeAsync<object>("tempNoodle.startNoodleDrag", positionX, positionY);
            TempNoodle.Refresh();
        }

        public void OnDropNoodle(StudioComponentEndpointViewModel input)
        {
            if (!IsEndpointValid(input) || input.IOType != VirtualStudio.Shared.EndpointIOType.Input)
                throw new ArgumentException(nameof(input));

            TempNoodle.Connected = false;
            Output = null;
        }

        public void CancelDrag()
        {
            TempNoodle.Connected = false;
            Output = null;

            jsRuntime.InvokeVoidAsync("tempNoodle.endDrag");

            TempNoodle.Refresh();
        }

        private bool IsEndpointValid(StudioComponentEndpointViewModel output)
        {
            return
                output != null &&
                output.Position != null;
        }
    }
}
