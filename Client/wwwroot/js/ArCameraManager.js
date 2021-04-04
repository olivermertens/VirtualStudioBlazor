class ArCameraManager {
    async startArCamera(canvas, div) {
        console.log("Start XR Camera");
        const gl = canvas.getContext("webgl", { xrCompatible: true });
        console.log("Created canvas webgl context");
        // @ts-ignore
        const xrSession = await navigator.xr.requestSession("immersive-ar");
        xrSession.updateRenderState({
            // @ts-ignore
            baseLayer: new XRWebGLLayer(xrSession, gl)
        });
        console.log("Requested XR Session");
        const referenceSpace = await xrSession.requestReferenceSpace('local');
        console.log("Requested reference space");
        const onXRFrame = (time, frame) => {
            xrSession.requestAnimationFrame(onXRFrame);
            // @ts-ignore
            //gl.bindFramebuffer(gl.FRAMEBUFFER, xrSession.renderState.baseLayer.framebuffer)
            const pose = frame.getViewerPose(referenceSpace);
            if (pose) {
                // In mobile AR, we only have one view.
                const view = pose.views[0];
                console.log(view.transform.matrix);
            }
        };
        xrSession.requestAnimationFrame(onXRFrame);
        console.log("Requested animation frame");
    }
}
window["ArCameraManager"] = new ArCameraManager();
//# sourceMappingURL=ArCameraManager.js.map