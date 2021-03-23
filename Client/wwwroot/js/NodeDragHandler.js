class NodeDragHandler {
    constructor() {
        this.startNodeDrag = () => {
            //console.log("starting node drag");
            document.addEventListener('mousemove', window['NodeDragHandler'].dragNode);
            document.addEventListener('mouseup', window['NodeDragHandler'].mouseUpListener);
        };
        this.dragNode = (event) => {
            window['DotNetNodeDragService'].invokeMethodAsync("DragNode", event.clientX, event.clientY);
        };
        this.mouseUpListener = (event) => {
            document.removeEventListener('mouseup', window['NodeDragHandler'].mouseUpListener);
            document.removeEventListener('mousemove', window['NodeDragHandler'].dragNode);
            window['DotNetNodeDragService'].invokeMethodAsync("DragFinished", event.clientX, event.clientY);
        };
    }
}
window["NodeDragHandler"] = new NodeDragHandler();
//# sourceMappingURL=NodeDragHandler.js.map