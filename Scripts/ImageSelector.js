$(document).ready(function () {
    const overlay = $("#imageSelectorOverlay");
    const albumContainer = $("#albumContainer");
    const itemContainer = $("#itemContainer");
    const selectedItemId = $("#selectedItemId");
    const selectedItemImage = $("#selectedItemImage");
    const placeholder = $(".image-preview-placeholder");
    const removeButton = $("#btnRemoveItemSelector");
    const selectedItemImageUrl = $("#selectedItemImageUrl");
    const cardItem = document.getElementById("card_item") || document.getElementById("edit_card_item");

    function restoreSelection(itemId) {
        if (!itemId) {
            return;
        }
        $.getJSON("/Items/GetItem", { itemId: itemId }, function (response) {
            if (!response.success) {
                return;
            }
            $(".image-selector-album[data-id='" + response.item.albumId + "']").trigger("click");
        });
    }

    function initializeSelectedItem() {
        if (!selectedItemId.val()) {
            removeButton.hide();
            return;
        }
        selectedItemImage.attr("src", selectedItemImageUrl.val()).show();
        placeholder.hide();
        removeButton.show();
    }

    $("#btnOpenItemSelector").click(function () {
        overlay.css("display", "flex").hide().fadeIn(200);
        loadAlbums();
        itemContainer.empty();
    });

    $("#btnCloseItemSelector").click(function () {
        overlay.fadeOut(200);
    });

    overlay.click(function (e) {
        if (e.target === this) {
            overlay.fadeOut(200);
        }
    });

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
            const selectedItem = selectedItemId.val();
            if (selectedItem) {
                restoreSelection(selectedItem);
            }
        });
    }

    $(document).on("click", ".image-selector-album", function () {
        $(".image-selector-album").removeClass("image-selector-album-selected");
        $(this).addClass("image-selector-album-selected");
        const albumId = $(this).data("id");

        $.getJSON("/Items/GetItems", { albumId: albumId }, function (response) {
            itemContainer.stop(true, true).fadeOut(120, function () {
                itemContainer.empty();
                if (!response.success) {
                    itemContainer.html("<p>No se pudieron cargar las imágenes.</p>");
                }
                else if (response.items.length === 0) {
                    itemContainer.html("<p>Este álbum no posee imágenes.</p>");
                }
                else {
                    $.each(response.items, function (index, item) {
                        const card = $("<div>").addClass("image-selector-item").attr("data-id", item.id).attr("data-name", item.name).attr("data-image", item.image);
                        $("<img>").attr("src", item.image).appendTo(card);
                        $("<div>").addClass("image-selector-item-name").text(item.name).appendTo(card);
                        if (item.id == selectedItemId.val()) {
                            card.addClass("image-selector-item-selected");
                        }
                        itemContainer.append(card);
                    });
                }
                itemContainer.fadeIn(150);
            });
        });
    });

    $(document).on("click", ".image-selector-item", function () {
        $(".image-selector-item").removeClass("image-selector-item-selected");
        $(this).addClass("image-selector-item-selected");

        selectedItemId.val($(this).data("id"));
        selectedItemImage.attr("src", $(this).data("image")).show();
        if (cardItem) {
            cardItem.value = $(this).data("name");
            cardItem.dispatchEvent(new Event("input"));
        }
        placeholder.hide();
        removeButton.show();
        overlay.fadeOut(200);
    });

    removeButton.click(function () {
        selectedItemId.val("");
        selectedItemImage.attr("src", "").hide();
        if (cardItem) {
            cardItem.value = "";
            cardItem.dispatchEvent(new Event("input"));
        }
        placeholder.show();
        $(this).hide();
    });

    initializeSelectedItem();
});