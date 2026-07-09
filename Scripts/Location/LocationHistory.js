document.addEventListener("DOMContentLoaded", function () {

    const btnHistory = document.getElementById("btnLocationHistory");
    const historyOverlay = document.getElementById("locationHistoryOverlay");
    const btnCloseHistory = document.getElementById("btnCloseLocationHistory");
    const btnCloseHistoryFooter = document.getElementById("btnCloseLocationHistoryFooter");

    let historyMapsCreated = false;

    function openHistory() {
        historyOverlay.style.display = "flex";
        setTimeout(function () {
            initializeHistoryMaps();
        }, 50);
    }

    function closeHistory() {
        historyOverlay.style.display = "none";
    }

    if (btnHistory)
        btnHistory.addEventListener("click", openHistory);

    if (btnCloseHistory)
        btnCloseHistory.addEventListener("click", closeHistory);

    function initializeHistoryMaps() {

        if (historyMapsCreated)
            return;

        const cards = document.querySelectorAll(".location-history-card");

        cards.forEach(function (card) {

            const latitude = parseFloat(card.dataset.latitude);
            const longitude = parseFloat(card.dataset.longitude);
            const mapContainer = card.querySelector(".location-history-map");

            if (!mapContainer)
                return;
            const map = L.map(mapContainer, {
                zoomControl: false,
                attributionControl: false,
                dragging: false,
                scrollWheelZoom: false,
                doubleClickZoom: false,
                boxZoom: false,
                keyboard: false,
                tap: false
            }).setView([latitude, longitude], 18);
            L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png").addTo(map);
            L.marker([latitude, longitude]).addTo(map);
            setTimeout(function () {
                map.invalidateSize();
            }, 100);
        });
        const buttons = document.querySelectorAll(".btnUseOldLocation");

        buttons.forEach(function (button) {
            button.addEventListener("click", function () {
                const card = this.closest(".location-history-card");

                selectedName = card.dataset.name;
                selectedDescription = card.dataset.description;
                selectedLatitude = parseFloat(card.dataset.latitude);
                selectedLongitude = parseFloat(card.dataset.longitude);

                console.log("Datos del historial:", selectedName, selectedDescription, selectedLatitude, selectedLongitude);
                console.log("¿Existe LocationSelectorAPI?", window.LocationSelectorAPI);

                if (window.LocationSelectorAPI) {
                    window.LocationSelectorAPI.applyLocation(selectedName, selectedDescription, selectedLatitude, selectedLongitude);
                    console.log("applyLocation ejecutado");
                }

                button.style.display = "none";

                closeHistory();
            });
        });
        historyMapsCreated = true;
    }

    if (btnCloseHistoryFooter)
        btnCloseHistoryFooter.addEventListener("click", closeHistory);

    if (historyOverlay) {
        historyOverlay.addEventListener("click", function (e) {
            if (e.target === historyOverlay)
                closeHistory();
        });
    }

});