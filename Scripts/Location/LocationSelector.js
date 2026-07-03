document.addEventListener("DOMContentLoaded", function () {

    const overlay = document.getElementById("locationSelectorOverlay");
    const btnOpen = document.getElementById("btnOpenLocationSelector");
    const btnClose = document.getElementById("btnCloseLocationSelector");
    const btnCancel = document.getElementById("btnCancelLocation");
    const txtSearch = document.getElementById("locationSearchInput");
    const btnSearch = document.getElementById("btnSearchLocation");
    const btnAccept = document.getElementById("btnAcceptLocation");
    const hiddenAddressName = document.getElementById("address_name");
    const hiddenLatitude = document.getElementById("address_latitude");
    const hiddenLongitude = document.getElementById("address_longitude");
    const hiddenDescription = document.getElementById("address_description");
    const previewPlaceholder = document.getElementById("locationPreviewPlaceholder");
    const previewName = document.getElementById("locationPreviewName");
    const previewDescription = document.getElementById("locationPreviewDescription");
    const btnCurrentLocation = document.getElementById("btnCurrentLocation");
    const userForm = document.getElementById("userForm");

    let map = null;
    let marker = null;
    let selectedLatitude = null;
    let selectedLongitude = null;
    let previewMap = null;
    let previewMarker = null;
    let selectedZoom = 18;

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

    function initPreviewMap(lat, lng) {

        const container = document.getElementById("locationPreviewMap");
        if (!previewMap) {
            previewMap = L.map(container, {
                zoomControl: false,
                attributionControl: false,
                dragging: false,
                scrollWheelZoom: false
            }).setView([lat, lng], selectedZoom);

            L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png").addTo(previewMap);
            previewMarker = L.marker([lat, lng]).addTo(previewMap);
        }
        else {
            previewMap.setView([lat, lng], selectedZoom);
            previewMarker.setLatLng([lat, lng]);
        }
    }

    async function loadLocationInformation(latitude, longitude) {

        selectedLatitude = latitude;
        selectedLongitude = longitude;

        try {

            const response = await fetch("https://nominatim.openstreetmap.org/reverse?format=json&accept-language=es&lat=" + latitude + "&lon=" + longitude);
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

    btnAccept.addEventListener("click", function () {

        if (selectedLatitude === null || selectedLongitude === null || selectedDescription === "") {
            showLocationAlert(
                "warning",
                "Seleccione una ubicación",
                "Debe seleccionar una ubicación antes de continuar."
            );            
            return;
        }

        hiddenAddressName.value = selectedName;
        hiddenLatitude.value = selectedLatitude;
        hiddenLongitude.value = selectedLongitude;
        hiddenDescription.value = selectedDescription;

        previewPlaceholder.style.display = "none";    
        previewName.textContent = selectedName;
        previewDescription.textContent = selectedDescription;
        initPreviewMap(selectedLatitude, selectedLongitude);

        btnOpen.textContent = "Cambiar ubicación";
        closeLocationModal();
    });

    async function searchLocation() {

        const query = txtSearch.value.trim();
        if (query === "")
            return;
        try {
            const response = await fetch("https://nominatim.openstreetmap.org/search?format=json&accept-language=es&q=" + encodeURIComponent(query));
            const results = await response.json();
            if (results.length === 0) {
                showLocationAlert(
                    "warning",
                    "Ubicación no encontrada",
                    "Intente escribir una dirección más específica."
                );                
                return;
            }
            const location = results[0];

            switch (location.type) {
                case "country":
                    selectedZoom = 6;
                    break;

                case "state":
                case "province":
                    selectedZoom = 8;
                    break;

                case "city":
                case "town":
                case "municipality":
                    selectedZoom = 12;
                    break;

                case "suburb":
                case "village":
                    selectedZoom = 14;
                    break;

                default:
                    selectedZoom = 18;
                    break;
            }

            selectedName = location.name || "Ubicación seleccionada";
            selectedDescription = location.display_name;
            document.getElementById("selectedLocationName").textContent = selectedName;
            document.getElementById("selectedLocationDescription").textContent = selectedDescription;

            const latitude = parseFloat(location.lat);
            const longitude = parseFloat(location.lon);
            selectedLatitude = latitude;
            selectedLongitude = longitude;

            if (map) {
                map.setView([latitude, longitude], selectedZoom);
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
            showLocationAlert(
                "error",
                "Error",
                "No fue posible buscar la ubicación."
            );            
        }
    }

    async function useCurrentLocation() {
        if (!navigator.geolocation) {
            showLocationAlert(
                "warning",
                "No disponible",
                "Su navegador no soporta geolocalización."
            );            
            return;
        }

        navigator.geolocation.getCurrentPosition(
            async function (position) {
                const latitude = position.coords.latitude;
                const longitude = position.coords.longitude;
                map.setView([latitude, longitude], selectedZoom);
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
                await loadLocationInformation(latitude, longitude);
            },
            function () {
                showLocationAlert(
                    "warning",
                    "Ubicación no disponible",
                    "No fue posible obtener su ubicación actual."
                );                
            },
            {
                enableHighAccuracy: true,
                timeout: 10000
            }
        );
    }

    function showLocationAlert(icon, title, text) {
        closeLocationModal();
        setTimeout(function () {
            Swal.fire({
                icon: icon,
                title: title,
                text: text
            });
        }, 150);
    }

    userForm.addEventListener("submit", function (e) {
        if (!hiddenLatitude.value || !hiddenLongitude.value) {
            e.preventDefault();
            showLocationAlert(
                "warning",
                "Ubicación obligatoria",
                "Debe seleccionar una ubicación antes de registrar el usuario."
            );
        }
    });

    btnSearch.addEventListener("click", searchLocation);
    btnCurrentLocation.addEventListener("click", useCurrentLocation);
    txtSearch.addEventListener("keydown", function (e) {

        if (e.key === "Enter") {
            e.preventDefault();
            searchLocation();
        }
    });    
});