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
    async getSdpAnswer(handlerId, connectionId, sdpOffer, remotePeerSupportsInsertableStreams, videoElement, rtpTimestampParagraph, objRef) {
        return await this.webRtcServiceInstances.get(handlerId).getSdpAnswer(connectionId, sdpOffer, remotePeerSupportsInsertableStreams, videoElement, rtpTimestampParagraph, objRef);
    }
    async addIceCandidate(handlerId, connectionId, candidate, sdpMid, sdpMLineIndex, usernameFragement) {
        await this.webRtcServiceInstances.get(handlerId).addIceCandidate(connectionId, candidate, sdpMid, sdpMLineIndex, usernameFragement);
    }
    async connect(handlerId, connectionId, sdpAnswer, useInsertableStreams) {
        await this.webRtcServiceInstances.get(handlerId).connect(connectionId, sdpAnswer, useInsertableStreams);
    }
    disconnect(handlerId, connectionId) {
        this.webRtcServiceInstances.get(handlerId).disconnect(connectionId);
    }
    disposeHandler(handlerId) {
        this.webRtcServiceInstances.get(handlerId).dispose();
    }
    areInsertableStreamsSupported(handlerId) {
        return this.webRtcServiceInstances.get(handlerId).areInsertableStreamsSupported();
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
            this.stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: 'environment' }, audio: true });
            let videoTracks = this.stream.getVideoTracks();
            console.log(`Using video device: ${videoTracks[0].label}`);
            if (videoElement) {
                videoElement.srcObject = this.stream;
                videoElement.muted = true;
            }
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
    areInsertableStreamsSupported() {
        // @ts-ignore
        if (!!RTCRtpSender.prototype.createEncodedVideoStreams) {
            console.log("InsertableStreams are supported.");
            return true;
        }
        else {
            console.log("InsertableStreams are not supported.");
            return false;
        }
    }
    async getSdpOffer(connectionId, dataKind, objRef) {
        if (this.videoOutRtcPeerConnections.has(connectionId)) {
            console.error("RTCPeerConnection for connection ID " + connectionId + "already exists.");
            return;
        }
        const supportsInsertableStreams = this.areInsertableStreamsSupported();
        // @ts-ignore
        const rtcPeerConnection = new RTCPeerConnection({ iceServers: this.iceServers, encodedInsertableStreams: supportsInsertableStreams });
        rtcPeerConnection.onconnectionstatechange = () => this.dotNetInvokeOnConnectionStateChanged(objRef, connectionId, false, rtcPeerConnection.connectionState);
        rtcPeerConnection.onicecandidate = iceEvent => this.dotNetInvokeOnIceCandidate(objRef, iceEvent, connectionId);
        var constraints = this.getMediaStreamConstraints(dataKind);
        if (constraints.video) {
            this.stream.getVideoTracks().forEach(track => {
                const sender = rtcPeerConnection.addTrack(track, this.stream);
                console.log("Added track: " + track.label);
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
    appendUint64ValueToFrame(chunk, value) {
        const frameLength = chunk.data.byteLength;
        const metadataLength = 8;
        const dataBuffer = new ArrayBuffer(frameLength + metadataLength);
        const dataArray = new Uint8Array(dataBuffer);
        dataArray.set(new Uint8Array(chunk.data), 0);
        const dataView = new DataView(dataBuffer);
        dataView.setBigUint64(frameLength, BigInt(value));
        chunk.data = dataBuffer;
    }
    separateUint64ValueFromFrame(chunk) {
        const view = new DataView(chunk.data);
        const frameLength = chunk.data.byteLength - 8;
        chunk.data = chunk.data.slice(0, frameLength);
        return Number(view.getBigUint64(frameLength));
    }
    async getSdpAnswer(connectionId, sdpOffer, remotePeerSupportsInsertableStreams, videoElement, rtpTimestampParagraph, objRef) {
        if (this.videoInRtcPeerConnection != null) {
            console.log('RTCPeerConnection for Video In already exists.');
            return;
        }
        const useInsertableStreams = remotePeerSupportsInsertableStreams && this.areInsertableStreamsSupported();
        // @ts-ignore
        const rtcPeerConnection = new RTCPeerConnection({ iceServers: this.iceServers, encodedInsertableStreams: useInsertableStreams });
        rtcPeerConnection.onconnectionstatechange = () => this.dotNetInvokeOnConnectionStateChanged(objRef, connectionId, true, rtcPeerConnection.connectionState);
        rtcPeerConnection.onicecandidate = iceEvent => this.dotNetInvokeOnIceCandidate(objRef, iceEvent, connectionId);
        rtcPeerConnection.ondatachannel = dataChannelEvent => {
            console.log("ondatachannel");
            dataChannelEvent.channel.onopen = e => console.log("datachannel open");
            dataChannelEvent.channel.onclose = e => console.log("datachannel closed");
            dataChannelEvent.channel.onmessage = messageEvent => console.log("datachannel message");
            dataChannelEvent.channel.onerror = errorEvent => console.log("datachannel error: " + errorEvent.error);
        };
        rtcPeerConnection.ontrack = (e) => {
            console.log("Set remote stream.");
            let stream = e.streams.find((s) => s.getTracks().find(t => t == e.track));
            if (stream == null)
                stream = new MediaStream([e.track]);
            videoElement.srcObject = stream;
            const config = rtcPeerConnection.getConfiguration();
            // @ts-ignore
            if (useInsertableStreams) {
                try {
                    let receiverTransformStream = new TransformStream({
                        transform: (chunk, controller) => {
                            var time = this.separateUint64ValueFromFrame(chunk);
                            rtpTimestampParagraph.innerText = "Time: " + time;
                            controller.enqueue(chunk);
                        }
                    });
                    // @ts-ignore
                    var streams = e.receiver.createEncodedStreams();
                    streams.readableStream
                        .pipeThrough(receiverTransformStream)
                        .pipeTo(streams.writableStream);
                    console.log("Remote stream set.");
                }
                catch (ex) {
                    console.log("Setting remote stream failed.", ex);
                }
            }
        };
        try {
            const offer = new RTCSessionDescription({ sdp: sdpOffer, type: "offer" });
            await rtcPeerConnection.setRemoteDescription(offer);
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
    logFrameMetadata(now, metadata, rtpTimestampParagraph) {
        if (!!metadata.rtpTimestamp)
            rtpTimestampParagraph.innerText = JSON.stringify(metadata, null, 2);
        // @ts-ignore
        videoElement.requestVideoFrameCallback(logFrameMetadata);
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
    async connect(connectionId, sdpAnswer, useInsertableStreams) {
        const rtcPeerConnection = this.videoOutRtcPeerConnections.get(connectionId);
        if (!rtcPeerConnection) {
            console.log('RTCPeerConnection does not exist.');
            return;
        }
        const answer = new RTCSessionDescription({ sdp: sdpAnswer, type: "answer" });
        if (this.areInsertableStreamsSupported()) {
            try {
                let senderTransformStream = new TransformStream({
                    transform: (chunk, controller) => {
                        if (useInsertableStreams) {
                            this.appendUint64ValueToFrame(chunk, Date.now());
                        }
                        controller.enqueue(chunk);
                    }
                });
                const sender = rtcPeerConnection.getSenders().find(s => s.track.kind == "video");
                // @ts-ignore
                const streams = sender.createEncodedStreams();
                streams.readableStream
                    .pipeThrough(senderTransformStream)
                    .pipeTo(streams.writableStream);
            }
            catch {
                console.log("Init insertable stream failed.");
            }
        }
        await rtcPeerConnection.setRemoteDescription(answer);
        console.log("Remote description set for connection ID " + connectionId);
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
//# sourceMappingURL=WebRtcHandlerManager.js.map