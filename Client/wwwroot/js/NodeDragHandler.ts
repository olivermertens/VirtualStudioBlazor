class NodeDragHandler {

    public startNodeDrag = () => {
        //console.log("starting node drag");
        document.addEventListener('mousemove', window['NodeDragHandler'].dragNode);
        document.addEventListener('mouseup', window['NodeDragHandler'].mouseUpListener);
    }

    public dragNode = (event: MouseEvent) => {
        window['DotNetNodeDragService'].invokeMethodAsync("DragNode", event.clientX, event.clientY);
    }

    public mouseUpListener = (event: MouseEvent) => {
        document.removeEventListener('mouseup', window['NodeDragHandler'].mouseUpListener);
        document.removeEventListener('mousemove', window['NodeDragHandler'].dragNode);
        window['DotNetNodeDragService'].invokeMethodAsync("DragFinished", event.clientX, event.clientY);
    }
}

window["NodeDragHandler"] = new NodeDragHandler();