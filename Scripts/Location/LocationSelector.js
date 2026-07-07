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
    const suggestionsContainer = document.getElementById("locationSuggestions");
    const loading = document.getElementById("locationLoading");

    //Variables utilizadas para mantener el estado actual del selector de ubicación
    let map = null;
    let marker = null;
    let selectedLatitude = null;
    let selectedLongitude = null;
    let previewMap = null;
    let previewMarker = null;
    let selectedZoom = 18;
    let selectedName = "";
    let selectedDescription = "";
    let searchTimeout = null;
    let searchCache = {};

    //Carga automáticamente la ubicación existente cuando se abre la vista de edición
    function initializeExistingLocation() {
        if (!hiddenLatitude.value || !hiddenLongitude.value)
            return;

        selectedLatitude = parseFloat(hiddenLatitude.value);
        selectedLongitude = parseFloat(hiddenLongitude.value);
        selectedName = hiddenAddressName.value;
        selectedDescription = hiddenDescription.value;

        previewPlaceholder.style.display = "none";
        previewName.textContent = selectedName;
        previewDescription.textContent = selectedDescription;

        initPreviewMap(selectedLatitude, selectedLongitude);
        btnOpen.textContent = "Cambiar ubicación";

        setTimeout(function () {
            previewMap.invalidateSize();
        }, 500);
    }

    //Abre el selector de ubicación y crea el mapa solo la primera vez que se utiliza
    function openLocationModal() {
        overlay.style.display = "flex";
        if (map === null) {
            let initialLatitude = 9.9281;
            let initialLongitude = -84.0907;
            let initialZoom = 13;

            if (selectedLatitude !== null && selectedLongitude !== null) {
                initialLatitude = selectedLatitude;
                initialLongitude = selectedLongitude;
                initialZoom = selectedZoom;
            }

            map = L.map("leafletMap").setView([initialLatitude, initialLongitude], initialZoom);
            L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", { attribution: "&copy; OpenStreetMap contributors" }).addTo(map);

            if (selectedLatitude !== null && selectedLongitude !== null) {
                updateMarker(selectedLatitude, selectedLongitude);
            }

            map.on("click", function (e) {

                const latitude = e.latlng.lat;
                const longitude = e.latlng.lng;

                updateMarker(latitude, longitude);

                loadLocationInformation(latitude, longitude);
            });
        }

        if (selectedLatitude !== null && selectedLongitude !== null) {

            map.setView([selectedLatitude, selectedLongitude], selectedZoom);
            updateMarker(selectedLatitude, selectedLongitude);

            document.getElementById("selectedLocationName").textContent = selectedName;
            document.getElementById("selectedLocationDescription").textContent = selectedDescription;
        }

        requestAnimationFrame(function () {
            map.invalidateSize();
        });
    }

    function closeLocationModal() {
        overlay.style.display = "none";
    }

    if (btnOpen)
        btnOpen.addEventListener("click", openLocationModal);

    if (btnClose)
        btnClose.addEventListener("click", closeLocationModal);

    if (btnCancel)
        btnCancel.addEventListener("click", closeLocationModal);

    overlay.addEventListener("click", function (e) {
        if (e.target === overlay) {
            closeLocationModal();
        }
    });

    //Inicializa o actualiza el minimapa mostrado en la vista previa de la ubicación seleccionada
    function initPreviewMap(lat, lng) {

        const container = document.getElementById("locationPreviewMap");
        if (!previewMap) {
            previewMap = L.map(container, {
                zoomControl: false,
                attributionControl: false,
                dragging: false,
                scrollWheelZoom: false
            }).setView([lat, lng], selectedZoom);

            /*L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png").addTo(previewMap);*/

            L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
                tileSize: 256,
                detectRetina: false,
                updateWhenIdle: true
            }).addTo(previewMap);

            previewMarker = L.marker([lat, lng]).addTo(previewMap);
            setTimeout(function () {
                previewMap.invalidateSize();
            }, 100);
        }
        else {
            previewMap.setView([lat, lng], selectedZoom);
            previewMarker.setLatLng([lat, lng]);
            setTimeout(function () {
                previewMap.invalidateSize();
            }, 100);
        }
    }

    //Obtiene el nombre y la descripción de la ubicación seleccionada mediante Reverse Geocoding
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

    //Busca una ubicación escrita por el usuario utilizando el servicio Nominatim
    async function searchLocation() {
        const query = txtSearch.value.trim();
        if (query === "")
            return;
        try {
            showLoading();
            const response = await fetch("https://nominatim.openstreetmap.org/search?format=json&accept-language=es&countrycodes=cr&q=" + encodeURIComponent(query));
            const results = await response.json();
            hideLoading();
            if (results.length === 0) {
                showLocationAlert(
                    "warning",
                    "Ubicación no encontrada",
                    "Intente escribir una dirección más específica."
                );                
                return;
            }
            const location = results[0];
            hideSuggestions();

            updateZoom(location.type);

            const latitude = parseFloat(location.lat);
            const longitude = parseFloat(location.lon);

            await loadLocationInformation(latitude, longitude);
            
            selectedLatitude = latitude;
            selectedLongitude = longitude;

            if (map) {
                map.setView([latitude, longitude], selectedZoom);
                updateMarker(latitude, longitude);
            }
        }
        catch {
            showLocationAlert(
                "error",
                "Error",
                "No fue posible buscar la ubicación."
            );
            hideLoading();
        }
    }

    //Obtiene y muestra sugerencias de búsqueda mientras el usuario escribe
    async function loadSuggestions(query) {

        if (query.length < 3) {
            hideSuggestions();
            return;
        }

        if (searchCache[query]) {
            renderSuggestions(searchCache[query]);
            return;
        }

        try {
            showLoading();
            const response = await fetch("https://nominatim.openstreetmap.org/search?format=json&accept-language=es&countrycodes=cr&limit=5&q=" + encodeURIComponent(query));
            const results = await response.json();
            searchCache[query] = results;
            hideLoading();
            renderSuggestions(results);
        }
        catch {
            hideSuggestions();
            hideLoading();
        }
    }

    //Obtiene la ubicación actual del usuario utilizando la geolocalización del navegador
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
                updateMarker(latitude, longitude);

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

    //Cierra el selector y muestra un mensaje utilizando SweetAlert
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

    //Oculta y limpia la lista de sugerencias de búsqueda
    function hideSuggestions() {
        suggestionsContainer.innerHTML = "";
        suggestionsContainer.style.display = "none";
    }

    function showLoading() {
        loading.style.display = "block";
    }

    function hideLoading() {
        loading.style.display = "none";
    }

    //Crea el marcador o actualiza su posición cuando cambia la ubicación seleccionada
    function updateMarker(latitude, longitude) {
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

    //Construye visualmente la lista de sugerencias obtenidas de Nominatim
    function renderSuggestions(results) {

        suggestionsContainer.innerHTML = "";
        if (results.length === 0) {
            hideSuggestions();
            return;
        }
        results.forEach(function (location) {

            const item = document.createElement("div");
            item.className = "location-suggestion-item";
            item.textContent = location.display_name;

            item.addEventListener("click", async function () {
                txtSearch.value = location.display_name;
                hideSuggestions();
                updateZoom(location.type);
                const latitude = parseFloat(location.lat);
                const longitude = parseFloat(location.lon);
                map.setView([latitude, longitude], selectedZoom);
                updateMarker(latitude, longitude);
                await loadLocationInformation(latitude, longitude);
            });
            suggestionsContainer.appendChild(item);
        });
        suggestionsContainer.style.display = "block";
    }

    //Ajusta automáticamente el nivel de zoom según el tipo de ubicación encontrada
    function updateZoom(locationType) {
        switch (locationType) {
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
    }

    document.addEventListener("click", function (e) {
        if (!txtSearch.contains(e.target) && !suggestionsContainer.contains(e.target)) {
            hideSuggestions();
        }
    });

    //Valida que exista una ubicación seleccionada antes de enviar el formulario
    if (userForm) {
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
    }

    //Espera unos milisegundos antes de consultar sugerencias para evitar solicitudes innecesarias
    txtSearch.addEventListener("input", function () {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function () {
            loadSuggestions(txtSearch.value.trim());
        }, 300);
    });

    if (btnSearch)
        btnSearch.addEventListener("click", searchLocation);

    if (btnCurrentLocation)
        btnCurrentLocation.addEventListener("click", useCurrentLocation);

    if (txtSearch)
        txtSearch.addEventListener("keydown", function (e) {
            if (e.key === "Enter") {
                e.preventDefault();
                searchLocation();
            }
        });

    initializeExistingLocation();
});