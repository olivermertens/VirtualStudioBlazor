class WebRtcHandlerManager {
    constructor() {
        this.handlerId = 0;
        this.webRtcServiceInstances = new Map();
    }
    async createHandler() {
        ++this.handlerId;
        this.webRtcServiceInstances.set(this.handlerId, new WebRtcHandler());
        return this.handlerId;
    }
    async openCameraStream(handlerId, videoElement) {
        await this.webRtcServiceInstances.get(handlerId).openCameraStream(videoElement);
    }
    async getSdpOffer(handlerId, connectionId, dataKind, objRef) {
        return await this.webRtcServiceInstances.get(handlerId).getSdpOffer(connectionId, dataKind, objRef);
    }
    async getSdpAnswer(handlerId, connectionId, sdpOffer, videoElement, rtpTimestampParagraph, objRef) {
        return await this.webRtcServiceInstances.get(handlerId).getSdpAnswer(connectionId, sdpOffer, videoElement, rtpTimestampParagraph, objRef);
    }
    async addIceCandidate(handlerId, connectionId, candidate, sdpMid, sdpMLineIndex, usernameFragement) {
        await this.webRtcServiceInstances.get(handlerId).addIceCandidate(connectionId, candidate, sdpMid, sdpMLineIndex, usernameFragement);
    }
    async connect(handlerId, connectionId, sdpAnswer) {
        await this.webRtcServiceInstances.get(handlerId).connect(connectionId, sdpAnswer);
    }
    disconnect(handlerId, connectionId) {
        this.webRtcServiceInstances.get(handlerId).disconnect(connectionId);
    }
    disposeHandler(handlerId) {
        this.webRtcServiceInstances.get(handlerId).dispose();
    }
}
window["WebRtcHandlerManager"] = new WebRtcHandlerManager();
class WebRtcHandler {
    constructor() {
        this.videoOutRtcPeerConnections = new Map();
        this.iceServers = [{ urls: "stun:stun.l.google.com:19302" }];
    }
    dispose() {
        this.videoInRtcPeerConnection?.close();
        this.videoOutRtcPeerConnections.forEach(connection => connection.close());
        this.stream.getTracks().forEach(track => track.stop());
    }
    async openCameraStream(videoElement) {
        if (this.stream)
            return;
        try {
            this.stream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
            let videoTracks = this.stream.getVideoTracks();
            console.log(`Using video device: ${videoTracks[0].label}`);
            videoElement.srcObject = this.stream;
            videoElement.muted = true;
        }
        catch (e) {
            if (e.name === 'ConstraintNotSatisfiedError') {
                console.error(`The resolution is not supported by your device.`);
            }
            else if (e.name === 'PermissionDeniedError') {
                console.error('Permissions have not been granted to use your camera and ' +
                    'microphone, you need to allow the page access to your devices in ' +
                    'order for the demo to work.');
            }
            console.error(`getUserMedia error: ${e.name}`, e);
        }
    }
    getMediaStreamConstraints(dataKind) {
        switch (dataKind) {
            case 1: return { video: true };
            case 2: return { audio: true };
            case 3: return { video: true, audio: true };
        }
    }
    async getSdpOffer(connectionId, dataKind, objRef) {
        if (this.videoOutRtcPeerConnections.has(connectionId)) {
            console.error("RTCPeerConnection for connection ID " + connectionId + "already exists.");
            return;
        }
        // @ts-ignore
        const rtcPeerConnection = new RTCPeerConnection({ iceServers: this.iceServers, encodedInsertableStreams: true });
        rtcPeerConnection.onconnectionstatechange = () => this.dotNetInvokeOnConnectionStateChanged(objRef, connectionId, false, rtcPeerConnection.connectionState);
        rtcPeerConnection.onicecandidate = iceEvent => this.dotNetInvokeOnIceCandidate(objRef, iceEvent, connectionId);
        // @ts-ignore 
        const supportsInsertableStreams = !!RTCRtpSender.prototype.createEncodedVideoStreams;
        if (supportsInsertableStreams)
            console.log("InsertableStreams are supported.");
        else
            console.log("InsertableStreams are not supported.");
        var constraints = this.getMediaStreamConstraints(dataKind);
        if (constraints.video) {
            this.stream.getVideoTracks().forEach(track => {
                const sender = rtcPeerConnection.addTrack(track, this.stream);
                console.log("Added track: " + track.label);
                try {
                    let senderTransformStream = new TransformStream({
                        transform: (chunk, controller) => {
                            this.appendUint8ValueToFrame(chunk, Date.now());
                            controller.enqueue(chunk);
                        }
                    });
                    // @ts-ignore
                    const streams = sender.createEncodedStreams();
                    streams.readableStream
                        .pipeThrough(senderTransformStream)
                        .pipeTo(streams.writableStream);
                }
                catch {
                    console.log("Init insertable stream failed.");
                }
            });
        }
        if (constraints.audio) {
            this.stream.getAudioTracks().forEach(track => {
                rtcPeerConnection.addTrack(track, this.stream);
                console.log("Added track: " + track.label);
            });
        }
        console.log('Added local stream to rtcPeerConnection');
        try {
            const offer = await rtcPeerConnection.createOffer();
            await rtcPeerConnection.setLocalDescription(offer);
            this.videoOutRtcPeerConnections.set(connectionId, rtcPeerConnection);
            return offer.sdp;
        }
        catch (ex) {
            console.log('Creating sdp offer failed', ex);
        }
    }
    appendUint8ValueToFrame(chunk, value) {
        const frameLength = chunk.data.byteLength;
        const metadataLength = 4;
        const dataBuffer = new ArrayBuffer(frameLength + metadataLength);
        const dataArray = new Uint8Array(dataBuffer);
        dataArray.set(new Uint8Array(chunk.data), 0);
        const dataView = new DataView(dataBuffer);
        dataView.setUint32(frameLength, value);
        chunk.data = dataBuffer;
    }
    separateUint8ValueFromFrame(chunk) {
        const view = new DataView(chunk.data);
        const frameLength = chunk.data.byteLength - 4;
        chunk.data = chunk.data.slice(0, frameLength);
        return view.getUint32(frameLength);
    }
    async getSdpAnswer(connectionId, sdpOffer, videoElement, rtpTimestampParagraph, objRef) {
        if (this.videoInRtcPeerConnection != null) {
            console.log('RTCPeerConnection for Video In already exists.');
            return;
        }
        // @ts-ignore
        const rtcPeerConnection = new RTCPeerConnection({ iceServers: this.iceServers, encodedInsertableStreams: true });
        rtcPeerConnection.onconnectionstatechange = () => this.dotNetInvokeOnConnectionStateChanged(objRef, connectionId, true, rtcPeerConnection.connectionState);
        rtcPeerConnection.onicecandidate = iceEvent => this.dotNetInvokeOnIceCandidate(objRef, iceEvent, connectionId);
        const logFrameMetadata = (now, metadata) => {
            if (!!metadata.rtpTimestamp)
                rtpTimestampParagraph.innerText = JSON.stringify(metadata, null, 2);
            // @ts-ignore
            videoElement.requestVideoFrameCallback(logFrameMetadata);
        };
        rtcPeerConnection.ontrack = (e) => {
            try {
                console.log("Set remote stream.");
                // @ts-ignore 
                const supportsInsertableStreams = !!RTCRtpSender.prototype.createEncodedVideoStreams;
                if (supportsInsertableStreams) {
                    console.log("InsertableStreams are supported.");
                    let receiverTransformStream = new TransformStream({
                        transform: (chunk, controller) => {
                            var time = this.separateUint8ValueFromFrame(chunk);
                            rtpTimestampParagraph.innerText = "Time: " + time;
                            controller.enqueue(chunk);
                        }
                    });
                    // @ts-ignore
                    var streams = e.receiver.createEncodedStreams();
                    streams.readableStream
                        .pipeThrough(receiverTransformStream)
                        .pipeTo(streams.writableStream);
                }
                else {
                    console.log("InsertableStreams are not supported.");
                }
                videoElement.srcObject = e.streams[0];
                // @ts-ignore
                // videoElement.requestVideoFrameCallback(logFrameMetadata);
                console.log("Remote stream set.");
            }
            catch (ex) {
                console.log("Setting remote stream failed.", ex);
            }
        };
        const offer = new RTCSessionDescription({ sdp: sdpOffer, type: "offer" });
        await rtcPeerConnection.setRemoteDescription(offer);
        try {
            const answer = await rtcPeerConnection.createAnswer({ offerToReceiveVideo: true, offerToReceiveAudio: true });
            await rtcPeerConnection.setLocalDescription(answer);
            this.videoInRtcPeerConnection = rtcPeerConnection;
            this.videoInConnectionId = connectionId;
            console.log("Answer type: " + answer.type);
            return answer.sdp;
        }
        catch (ex) {
            console.log('Creating sdp answer failed', ex);
        }
    }
    async addIceCandidate(connectionId, candidate, sdpMid, sdpMLineIndex, usernameFragement) {
        if (candidate == null)
            console.log("End of ice candidates received.");
        else {
            const candidateJson = {
                candidate: candidate,
                sdpMid: sdpMid,
                sdpMLineIndex: sdpMLineIndex,
                usernameFragment: usernameFragement
            };
            let rtcPeerConnection;
            if (connectionId == this.videoInConnectionId)
                rtcPeerConnection = this.videoInRtcPeerConnection;
            else
                rtcPeerConnection = this.videoOutRtcPeerConnections.get(connectionId);
            await rtcPeerConnection.addIceCandidate(candidateJson);
            console.log('Ice candidate added for connection ID ' + connectionId);
        }
    }
    async connect(connectionId, sdpAnswer) {
        const rtcPeerConnection = this.videoOutRtcPeerConnections.get(connectionId);
        if (!rtcPeerConnection) {
            console.log('RTCPeerConnection does not exist.');
            return;
        }
        const answer = new RTCSessionDescription({ sdp: sdpAnswer, type: "answer" });
        try {
            await rtcPeerConnection.setRemoteDescription(answer);
            console.log("Remote description set for connection ID " + connectionId);
        }
        catch (ex) {
            console.log('Failed to connect.', ex);
        }
    }
    disconnect(connectionId) {
        let rtcPeerConnection;
        if (connectionId == this.videoInConnectionId) {
            rtcPeerConnection = this.videoInRtcPeerConnection;
            this.videoInRtcPeerConnection = null;
            this.videoInConnectionId = 0;
        }
        else {
            rtcPeerConnection = this.videoOutRtcPeerConnections.get(connectionId);
            this.videoOutRtcPeerConnections.delete(connectionId);
        }
        console.log("Disconnecting connection ID " + connectionId);
        rtcPeerConnection.close();
        console.log("Disconnected connection ID " + connectionId);
    }
    dotNetInvokeOnConnectionStateChanged(objRef, connectionId, isInput, connectionState) {
        objRef.invokeMethodAsync("OnConnectionStateChanged", connectionId, isInput, connectionState);
    }
    dotNetInvokeOnIceCandidate(objRef, iceEvent, connectionId) {
        const json = iceEvent.candidate?.toJSON();
        if (json != null)
            objRef.invokeMethodAsync("OnIceCandidate", connectionId, json.candidate, json.sdpMid, json.sdpMLineIndex, json.usernameFragment);
        else {
            console.log("End of ice candidates");
            objRef.invokeMethodAsync("OnIceCandidate", connectionId, null, null, 0, null);
        }
    }
}
/**
 * Does nothing.
 * @implements {FrameTransform} in pipeline.js
 */
class NullTransform {
    /** @override */
    async init() { }
    /** @override */
    async transform(frame, controller) {
        controller.enqueue(frame);
    }
    /** @override */
    destroy() { }
}
//# sourceMappingURL=WebRtcHandler.js.map