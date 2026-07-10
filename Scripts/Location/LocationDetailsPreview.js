document.addEventListener("DOMContentLoaded", function () {

    const mapContainer = document.getElementById("locationPreviewMap");

    if (!mapContainer)
        return;

    const latitude = parseFloat(mapContainer.dataset.latitude);
    const longitude = parseFloat(mapContainer.dataset.longitude);

    if (isNaN(latitude) || isNaN(longitude))
        return;

    const map = L.map(mapContainer, {
        zoomControl: false,
        attributionControl: false,
        dragging: false,
        scrollWheelZoom: false
    }).setView([latitude, longitude], 18);

    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        tileSize: 256,
        detectRetina: false,
        updateWhenIdle: true
    }).addTo(map);

    L.marker([latitude, longitude]).addTo(map);

    setTimeout(function () {
        map.invalidateSize();
    }, 100);
});