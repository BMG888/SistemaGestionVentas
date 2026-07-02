document.addEventListener("DOMContentLoaded", function () {

    const overlay = document.getElementById("locationSelectorOverlay");
    const btnOpen = document.getElementById("btnOpenLocationSelector");
    const btnClose = document.getElementById("btnCloseLocationSelector");
    const btnCancel = document.getElementById("btnCancelLocation");
    const txtSearch = document.getElementById("locationSearchInput");
    const btnSearch = document.getElementById("btnSearchLocation");

    let map = null;
    let marker = null;

    function openLocationModal() {
        overlay.style.display = "flex";
        if (map === null) {
            map = L.map("leafletMap").setView([9.9281, -84.0907], 13);
            L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", { attribution: "&copy; OpenStreetMap contributors" }).addTo(map);

            map.on("click", function (e) {

                const latitude = e.latlng.lat;
                const longitude = e.latlng.lng;

                if (marker) {
                    marker.setLatLng([latitude, longitude]);
                }
                else {
                    marker = L.marker([latitude, longitude], { draggable: true }).addTo(map);
                    marker.on("dragend", function () {
                        const position = marker.getLatLng();
                        loadLocationInformation(position.lat, position.lng);
                    });
                }
                loadLocationInformation(latitude, longitude);
            });
        }
        requestAnimationFrame(function () {
            map.invalidateSize();
        });
    }

    function closeLocationModal() {
        overlay.style.display = "none";
    }

    btnOpen.addEventListener("click", openLocationModal);
    btnClose.addEventListener("click", closeLocationModal);
    btnCancel.addEventListener("click", closeLocationModal);

    overlay.addEventListener("click", function (e) {
        if (e.target === overlay) {
            closeLocationModal();
        }
    });

    let selectedName = "";
    let selectedDescription = "";

    async function loadLocationInformation(latitude, longitude) {

        try {

            const response = await fetch("https://nominatim.openstreetmap.org/reverse?format=json&lat=" + latitude + "&lon=" + longitude);
            const data = await response.json();

            selectedName = data.address.road || data.address.neighbourhood || data.address.suburb || data.address.village || data.address.town || data.address.city || "Ubicación seleccionada";
            selectedDescription = data.display_name;

            document.getElementById("selectedLocationName").textContent = selectedName;
            document.getElementById("selectedLocationDescription").textContent = selectedDescription;
        }
        catch {

            selectedName = "Ubicación seleccionada";
            selectedDescription = "";
            document.getElementById("selectedLocationName").textContent = selectedName;
            document.getElementById("selectedLocationDescription").textContent = "";
        }
    }

    async function searchLocation() {

        const query = txtSearch.value.trim();
        if (query === "")
            return;
        try {
            const response = await fetch(
                "https://nominatim.openstreetmap.org/search?format=json&q=" +
                encodeURIComponent(query)
            );

            const results = await response.json();
            if (results.length === 0) {
                Swal.fire({
                    icon: "warning",
                    title: "Ubicación no encontrada",
                    text: "Intente escribir una dirección más específica."
                });
                return;
            }
            const location = results[0];

            selectedName = location.name || "Ubicación seleccionada";
            selectedDescription = location.display_name;
            document.getElementById("selectedLocationName").textContent = selectedName;
            document.getElementById("selectedLocationDescription").textContent = selectedDescription;

            const latitude = parseFloat(location.lat);
            const longitude = parseFloat(location.lon);

            if (map) {
                map.setView([latitude, longitude], 18);
                if (marker) {
                    marker.setLatLng([latitude, longitude]);
                }
                else {
                    marker = L.marker([latitude, longitude], { draggable: true }).addTo(map);                    
                    marker.on("dragend", function () {
                        const position = marker.getLatLng();
                        loadLocationInformation(position.lat, position.lng);
                    });
                }                
            }
        }
        catch {
            Swal.fire({
                icon: "error",
                title: "Error",
                text: "No fue posible buscar la ubicación."
            });
        }
    }

    btnSearch.addEventListener("click", searchLocation);
    txtSearch.addEventListener("keydown", function (e) {

        if (e.key === "Enter") {
            e.preventDefault();
            searchLocation();
        }
    });
});