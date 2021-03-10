using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using VirtualStudio.ViewModels;

namespace VirtualStudio.Client.Shared
{
    public interface INodeDragService
    {
        void OnStartNodeDrag(NodeViewModel nodeToDrag, MouseEventArgs e);
        void OnDrop(MouseEventArgs e);
        Task OnStartCreateNodeDrag(NodeViewModel nodeToDrag, DragEventArgs e);
        void CancelDrag();
        bool IsDrag(MouseEventArgs e);
    }

    public class NodeDragService : INodeDragService
    {
        private readonly IJSRuntime jsRuntime;
        private readonly VirtualStudioViaHubConnectionController virtualStudioController;

        private NodeViewModel nodeToDrag;
        private Vector2 cursorStartPos;
        private Vector2 nodeStartPos;

        public NodeDragService(IJSRuntime jsRuntime, VirtualStudioViaHubConnectionController virtualStudioController)
        {
            this.jsRuntime = jsRuntime;
            this.virtualStudioController = virtualStudioController;
            jsRuntime.InvokeVoidAsync("addDotNetSingletonService", "DotNetNodeDragService", DotNetObjectReference.Create(this));
        }

        public void OnStartNodeDrag(NodeViewModel nodeToDrag, MouseEventArgs e)
        {
            this.nodeToDrag = nodeToDrag;
            cursorStartPos = e.GetClientPos();
            nodeStartPos = new Vector2(nodeToDrag.PositionX, nodeToDrag.PositionY);
        }

        public bool IsDrag(MouseEventArgs e)
        {
            const int dragThreshold = 4; //Length in px to consider a drag (instead of a click)
            var mouseOffset = e.GetClientPos() - cursorStartPos;
            return mouseOffset.GetLength() > dragThreshold;
        }

        public Task OnStartCreateNodeDrag(NodeViewModel nodeToDrag, DragEventArgs e)
        {
            throw new NotImplementedException();
        }

        [JSInvokable]
        public void DragNode(double posX, double posY)
        {
            var dragOffset = (new Vector2(posX, posY) - cursorStartPos) / ZoomHandler.Zoom;
            nodeToDrag.PositionX = (float)(nodeStartPos.x + dragOffset.x);
            nodeToDrag.PositionY = (float)(nodeStartPos.y + dragOffset.y);
        }

        [JSInvokable]
        public void DragFinished(double posX, double posY)
        {
            Console.WriteLine($"Drag finished at position {posX}, {posY}");
            virtualStudioController.MoveComponentNode((nodeToDrag as ComponentNodeViewModel).Component.Id, nodeToDrag.PositionX, nodeToDrag.PositionY);
        }

        public void OnDrop(MouseEventArgs e)
        {
            Console.WriteLine("Dropping node");
        }

        public void CancelDrag()
        {
            nodeToDrag = null;
        }
    }
}
