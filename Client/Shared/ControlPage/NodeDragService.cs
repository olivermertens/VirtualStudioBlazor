using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using VirtualStudio.ViewModels;
using VirtualStudio.Shared.Abstractions;

namespace VirtualStudio.Client.Shared
{
    public interface INodeDragService
    {
        void OnStartNodeDrag(NodeViewModel nodeToDrag, MouseEventArgs e, IVirtualStudioController virtualStudioController = null);
        void OnDrop(MouseEventArgs e);
        Task OnStartCreateNodeDrag(NodeViewModel nodeToDrag, DragEventArgs e);
        void CancelDrag();
    }

    public class NodeDragService : INodeDragService
    {
        private class NodeDragProcess
        {
            public int Id { get; }
            public bool IsDragCancelled { get; private set; } = false;

            private readonly NodeViewModel node;
            private readonly Vector2 cursorStartPosition;
            private readonly Vector2 nodeStartPosition;
            private readonly IVirtualStudioController virtualStudioController;

            public NodeDragProcess(int id, NodeViewModel node, Vector2 cursorStartPosition, IVirtualStudioController virtualStudioController = null)
            {
                Id = id;
                this.node = node ?? throw new ArgumentNullException(nameof(node));
                this.cursorStartPosition = cursorStartPosition;
                this.nodeStartPosition = new Vector2(node.PositionX, node.PositionY);
                this.virtualStudioController = virtualStudioController;
            }

            public void DragNode(double posX, double posY)
            {
                if (IsDragCancelled)
                    return;

                var dragOffset = (new Vector2(posX, posY) - cursorStartPosition) / ZoomHandler.Zoom;
                node.PositionX = (float)(nodeStartPosition.x + dragOffset.x);
                node.PositionY = (float)(nodeStartPosition.y + dragOffset.y);
            }

            public void DragFinished(double posX, double posY)
            {
                if (IsDragCancelled)
                    return;

                Console.WriteLine($"Drag finished at position {posX}, {posY}");
                virtualStudioController?.MoveComponentNode((node as ComponentNodeViewModel).Component.Id, node.PositionX, node.PositionY);
            }

            public void CancelDrag()
            {
                IsDragCancelled = true;
                node.PositionX = (float)nodeStartPosition.x;
                node.PositionY = (float)nodeStartPosition.y;
            }
        }

        private readonly IJSRuntime jsRuntime;
        private NodeDragProcess dragProcess;

        public NodeDragService(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
            jsRuntime.InvokeVoidAsync("addDotNetSingletonService", "DotNetNodeDragService", DotNetObjectReference.Create(this));
        }

        public void OnStartNodeDrag(NodeViewModel nodeToDrag, MouseEventArgs e, IVirtualStudioController virtualStudioController = null)
        {
            dragProcess = new NodeDragProcess(0, nodeToDrag, e.GetClientPos(), virtualStudioController);
        }

        public Task OnStartCreateNodeDrag(NodeViewModel nodeToDrag, DragEventArgs e)
        {
            throw new NotImplementedException();
        }

        [JSInvokable]
        public void DragNode(double posX, double posY)
        {
            dragProcess?.DragNode(posX, posY);
        }

        [JSInvokable]
        public void DragFinished(double posX, double posY)
        {
            dragProcess?.DragFinished(posX, posY);
            dragProcess = null;
        }

        public void CancelDrag()
        {
            dragProcess?.CancelDrag();
            dragProcess = null;
        }

        public void OnDrop(MouseEventArgs e)
        {
            Console.WriteLine("Dropping node");
        }
    }
}
