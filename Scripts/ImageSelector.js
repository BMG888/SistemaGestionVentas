$(document).ready(function () {
    const overlay = $("#imageSelectorOverlay");
    const albumContainer = $("#albumContainer");
    const itemContainer = $("#itemContainer");
    const selectedItemId = $("#selectedItemId");
    const selectedItemImage = $("#selectedItemImage");
    const selectedItemName = $("#selectedItemName");
    const placeholder = $(".image-selector-placeholder");
    const removeButton = $("#btnRemoveItemSelector");
    let albumsLoaded = false;

    // Abrir selector
    $("#btnOpenItemSelector").click(function () {
        overlay.css("display", "flex").hide().fadeIn(200);
        if (!albumsLoaded) {
            loadAlbums();
        }
        itemContainer.empty();
    });

    // Cerrar selector
    $("#btnCloseItemSelector").click(function () {
        overlay.fadeOut(200);
    });

    overlay.click(function (e) {
        if (e.target === this) {
            overlay.fadeOut(200);
        }
    });

    // Cargar álbumes
    function loadAlbums() {
        albumContainer.empty();
        $.getJSON("/Items/GetAlbums", function (response) {

            if (!response.success) {
                albumContainer.html("<p>No se pudieron cargar los álbumes.</p>");
                return;
            }

            $.each(response.albums, function (index, album) {
                const card = $("<div>").addClass("image-selector-album").attr("data-id", album.id).text(album.name);
                albumContainer.append(card);
            });
        });
        albumsLoaded = true;
    }

    // Cargar imágenes del álbum
    $(document).on("click", ".image-selector-album", function () {

        const albumId = $(this).data("id");
        itemContainer.empty();

        $.getJSON("/Items/GetItems", { albumId: albumId }, function (response) {

            if (!response.success) {
                itemContainer.html("<p>No se pudieron cargar las imágenes.</p>");
                return;
            }

            if (response.items.length === 0) {
                itemContainer.html("<p>Este álbum no posee imágenes.</p>");
                return;
            }

            $.each(response.items, function (index, item) {

                const card = $("<div>").addClass("image-selector-item").attr("data-id", item.id).attr("data-name", item.name).attr("data-image", item.image);

                $("<img>").attr("src", item.image).appendTo(card);

                $("<div>").addClass("image-selector-item-name").text(item.name).appendTo(card);
                itemContainer.append(card);
            });
        });
    });

    // Seleccionar imagen
    $(document).on("click", ".image-selector-item", function () {

        selectedItemId.val($(this).data("id"));
        selectedItemImage.attr("src", $(this).data("image")).show();
        selectedItemName.text($(this).data("name"));        
        const cardItem = document.getElementById("card_item");
        cardItem.value = $(this).data("name");
        cardItem.dispatchEvent(new Event("input"));
        placeholder.hide();
        removeButton.prop("disabled", false);
        overlay.fadeOut(200);
    });

    // Quitar imagen
    removeButton.click(function () {
        selectedItemId.val("");
        selectedItemImage.attr("src", "").hide();
        selectedItemName.empty();
        const cardItem = document.getElementById("card_item");
        cardItem.value = "";
        cardItem.dispatchEvent(new Event("input"));
        placeholder.show();
        $(this).prop("disabled", true);
    });
});