document.addEventListener("DOMContentLoaded", function () {

    const fileInput = document.getElementById("imageFileInput");
    const previewImg = document.getElementById("itemImagePreviewImg");
    const placeholder = document.querySelector("#itemImagePreview .image-preview-placeholder");

    if (!fileInput)
        return;

    // Si ya viene con una imagen cargada (Edit), la muestra desde el inicio
    if (previewImg.getAttribute("src")) {
        previewImg.style.display = "block";
        placeholder.style.display = "none";
    }

    fileInput.addEventListener("change", function () {

        const file = fileInput.files[0];

        if (!file) {
            return;
        }

        const reader = new FileReader();

        reader.onload = function (e) {
            previewImg.src = e.target.result;
            previewImg.style.display = "block";
            placeholder.style.display = "none";
        };

        reader.readAsDataURL(file);
    });
});