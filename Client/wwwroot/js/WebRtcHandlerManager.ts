class WebRtcHandlerManager {

    handlerId = 0;

    webRtcServiceInstances = new Map<number, WebRtcHandler>();

    public async createHandler(): Promise<number> {
        ++this.handlerId;
        this.webRtcServiceInstances.set(this.handlerId, new WebRtcHandler());
        return this.handlerId;
    }

    public async openCameraStream(handlerId: number, videoElement: HTMLVideoElement) {
        await this.webRtcServiceInstances.get(handlerId).openCameraStream(videoElement);
    }

    public async getSdpOffer(handlerId: number, connectionId: number, dataKind: number, objRef: dotNetObjectRef): Promise<string> {
        return await this.webRtcServiceInstances.get(handlerId).getSdpOffer(connectionId, dataKind, objRef);
    }

    public async getSdpAnswer(handlerId: number, connectionId: number, sdpOffer: string, remotePeerSupportsInsertableStreams: boolean, videoElement: HTMLVideoElement, rtpTimestampParagraph: HTMLParagraphElement, objRef: dotNetObjectRef): Promise<string> {
        return await this.webRtcServiceInstances.get(handlerId).getSdpAnswer(connectionId, sdpOffer, remotePeerSupportsInsertableStreams, videoElement, rtpTimestampParagraph, objRef);
    }

    public async addIceCandidate(handlerId: number, connectionId: number, candidate: string, sdpMid: string, sdpMLineIndex: number, usernameFragement: string) {
        await this.webRtcServiceInstances.get(handlerId).addIceCandidate(connectionId, candidate, sdpMid, sdpMLineIndex, usernameFragement);
    }

    public async connect(handlerId: number, connectionId: number, sdpAnswer: string, useInsertableStreams: boolean) {
        await this.webRtcServiceInstances.get(handlerId).connect(connectionId, sdpAnswer, useInsertableStreams);
    }

    public disconnect(handlerId: number, connectionId: number) {
        this.webRtcServiceInstances.get(handlerId).disconnect(connectionId);
    }

    public disposeHandler(handlerId: number) {
        this.webRtcServiceInstances.get(handlerId).dispose();
    }

    public areInsertableStreamsSupported(handlerId: number): boolean {
        return this.webRtcServiceInstances.get(handlerId).areInsertableStreamsSupported();
    }
}

window["WebRtcHandlerManager"] = new WebRtcHandlerManager();

class WebRtcHandler {

    stream: MediaStream;
    videoInConnectionId: number;
    videoInRtcPeerConnection: RTCPeerConnection;
    videoOutRtcPeerConnections = new Map<number, RTCPeerConnection>();
    iceServers: RTCIceServer[] = [{ urls: "stun:stun.l.google.com:19302" }]

    public dispose(): void {
        this.videoInRtcPeerConnection?.close();
        this.videoOutRtcPeerConnections.forEach(connection => connection.close());
        this.stream.getTracks().forEach(track => track.stop());
    }

    public async openCameraStream(videoElement: HTMLVideoElement | null) {
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
        } catch (e) {
            if (e.name === 'ConstraintNotSatisfiedError') {
                console.error(`The resolution is not supported by your device.`);
            } else if (e.name === 'PermissionDeniedError') {
                console.error('Permissions have not been granted to use your camera and ' +
                    'microphone, you need to allow the page access to your devices in ' +
                    'order for the demo to work.');
            }
            console.error(`getUserMedia error: ${e.name}`, e);
        }
    }

    private getMediaStreamConstraints(dataKind: number): MediaStreamConstraints {
        switch (dataKind) {
            case 1: return { video: true };
            case 2: return { audio: true };
            case 3: return { video: true, audio: true };
        }
    }

    public areInsertableStreamsSupported(): boolean {
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

    public async getSdpOffer(connectionId: number, dataKind: number, objRef: dotNetObjectRef): Promise<string> {
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
        } catch (ex) {
            console.log('Creating sdp offer failed', ex);
        }
    }

    appendUint64ValueToFrame(chunk, value: number) {
        const frameLength: number = chunk.data.byteLength;
        const metadataLength = 8;

        const dataBuffer = new ArrayBuffer(frameLength + metadataLength);
        const dataArray = new Uint8Array(dataBuffer);
        dataArray.set(new Uint8Array(chunk.data), 0);
        const dataView = new DataView(dataBuffer);
        dataView.setBigUint64(frameLength, BigInt(value));

        chunk.data = dataBuffer;
    }

    separateUint64ValueFromFrame(chunk): number {
        const view = new DataView(chunk.data);
        const frameLength = chunk.data.byteLength - 8;
        chunk.data = chunk.data.slice(0, frameLength);
        return Number(view.getBigUint64(frameLength));
    }

    public async getSdpAnswer(connectionId: number, sdpOffer: string, remotePeerSupportsInsertableStreams: boolean, videoElement: HTMLVideoElement, rtpTimestampParagraph: HTMLParagraphElement, objRef: dotNetObjectRef): Promise<string> {
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
                } catch (ex) {
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
        } catch (ex) {
            console.log('Creating sdp answer failed', ex);
        }
    }

    logFrameMetadata(now: DOMHighResTimeStamp, metadata, rtpTimestampParagraph: HTMLParagraphElement) {
        if (!!metadata.rtpTimestamp)
            rtpTimestampParagraph.innerText = JSON.stringify(metadata, null, 2);
        // @ts-ignore
        videoElement.requestVideoFrameCallback(logFrameMetadata);
    }


    public async addIceCandidate(connectionId: number, candidate: string, sdpMid: string, sdpMLineIndex: number, usernameFragement: string) {
        if (candidate == null)
            console.log("End of ice candidates received.");
        else {
            const candidateJson: RTCIceCandidateInit = {
                candidate: candidate,
                sdpMid: sdpMid,
                sdpMLineIndex: sdpMLineIndex,
                usernameFragment: usernameFragement
            };
            let rtcPeerConnection: RTCPeerConnection;

            if (connectionId == this.videoInConnectionId)
                rtcPeerConnection = this.videoInRtcPeerConnection;
            else
                rtcPeerConnection = this.videoOutRtcPeerConnections.get(connectionId);

            await rtcPeerConnection.addIceCandidate(candidateJson);
            console.log('Ice candidate added for connection ID ' + connectionId);
        }
    }

    public async connect(connectionId: number, sdpAnswer: string, useInsertableStreams: boolean) {
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
                            this.appendUint64ValueToFrame(chunk, Date.now())
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
            } catch {
                console.log("Init insertable stream failed.");
            }
        }
        await rtcPeerConnection.setRemoteDescription(answer);
        console.log("Remote description set for connection ID " + connectionId);
    }

    public disconnect(connectionId: number) {
        let rtcPeerConnection: RTCPeerConnection;
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

    private dotNetInvokeOnConnectionStateChanged(objRef: dotNetObjectRef, connectionId: number, isInput: boolean, connectionState) {
        objRef.invokeMethodAsync("OnConnectionStateChanged", connectionId, isInput, connectionState);
    }

    private dotNetInvokeOnIceCandidate(objRef: dotNetObjectRef, iceEvent: RTCPeerConnectionIceEvent, connectionId: number) {
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
class NullTransform { // eslint-disable-line no-unused-vars
    /** @override */
    async init() { }
    /** @override */
    async transform(frame, controller) {
        controller.enqueue(frame);
    }
    /** @override */
    destroy() { }
}