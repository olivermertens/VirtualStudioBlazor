var NodeDragHandler = /** @class */ (function () {
    function NodeDragHandler() {
        this.startNodeDrag = function () {
            //console.log("starting node drag");
            document.addEventListener('mousemove', window['NodeDragHandler'].dragNode);
            document.addEventListener('mouseup', window['NodeDragHandler'].mouseUpListener);
        };
        this.dragNode = function (event) {
            window['DotNetNodeDragService'].invokeMethodAsync("DragNode", event.clientX, event.clientY);
        };
        this.mouseUpListener = function (event) {
            document.removeEventListener('mouseup', window['NodeDragHandler'].mouseUpListener);
            document.removeEventListener('mousemove', window['NodeDragHandler'].dragNode);
            window['DotNetNodeDragService'].invokeMethodAsync("DragFinished", event.clientX, event.clientY);
        };
    }
    return NodeDragHandler;
}());
window["NodeDragHandler"] = new NodeDragHandler();
//# sourceMappingURL=NodeDragHandler.js.map