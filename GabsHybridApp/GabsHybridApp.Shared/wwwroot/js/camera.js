(function () {
    if (!window.gabsCameraSimple) window.gabsCameraSimple = {};

    window.gabsCameraSimple.start = async function (videoId) {
        const video = document.getElementById(videoId);
        if (!video) { console.warn("gabsCameraSimple.start: video not found"); return; }
        try {
            const stream = await navigator.mediaDevices.getUserMedia({
                video: { facingMode: "environment" },
                audio: false
            });
            video.srcObject = stream;
            await video.play();
        } catch (err) {
            console.error("Camera start failed:", err);
        }
    };

    window.gabsCameraSimple.stop = function (videoId) {
        const video = document.getElementById(videoId);
        if (!video) return;
        const stream = video.srcObject;
        if (stream && stream.getTracks) stream.getTracks().forEach(t => t.stop());
        video.srcObject = null;
    };

    // Draw current frame to canvas (no return payload)
    window.gabsCameraSimple.captureToCanvas = function (videoId, canvasId) {
        const video = document.getElementById(videoId);
        const canvas = document.getElementById(canvasId);
        if (!video || !canvas) { console.warn("captureToCanvas: elements not found"); return; }
        const ctx = canvas.getContext("2d");
        canvas.width = video.videoWidth || 640;
        canvas.height = video.videoHeight || 480;
        ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
    };

    // Convert canvas content to data URL (quality optional for jpeg)
    window.gabsCameraSimple.canvasToDataUrl = function (canvasId, mime, quality) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) { console.warn("canvasToDataUrl: canvas not found"); return null; }
        mime = (mime || "image/jpeg").toLowerCase();
        if (mime === "image/jpeg") return canvas.toDataURL("image/jpeg", quality ?? 0.8);
        return canvas.toDataURL("image/png");
    };
})();


(function () {
    if (!window.gabsBrowse) window.gabsBrowse = {};
    window.gabsBrowse.clickById = function (id) {
        var el = document.getElementById(id);
        if (el && typeof el.click === "function") el.click();
    };
})();
