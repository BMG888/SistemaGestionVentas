document.addEventListener("DOMContentLoaded", function () {

    const trigger = document.getElementById("itemLargeImageTrigger");
    const btnExpand = document.getElementById("btnExpandImage");
    const overlay = document.getElementById("imageZoomOverlay");
    const zoomImg = document.getElementById("imageZoomFull");
    const btnClose = document.getElementById("btnCloseZoom");
    const btnShrink = document.getElementById("btnShrinkImage");

    if (!trigger)
        return;

    const sourceImg = trigger.querySelector("img");

    function openZoom() {
        zoomImg.src = sourceImg.src;
        overlay.classList.add("image-zoom-active");
        document.body.style.overflow = "hidden";
    }

    function closeZoom() {
        overlay.classList.remove("image-zoom-active");
        document.body.style.overflow = "";
    }

    btnExpand.addEventListener("click", openZoom);
    btnClose.addEventListener("click", closeZoom);
    btnShrink.addEventListener("click", closeZoom);

    overlay.addEventListener("click", function (e) {
        if (e.target === overlay) {
            closeZoom();
        }
    });

    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape" && overlay.classList.contains("image-zoom-active")) {
            closeZoom();
        }
    });
});