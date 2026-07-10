document.addEventListener("DOMContentLoaded", function () {

    const btnWaze = document.getElementById("btnWazeLink");

    if (!btnWaze)
        return;

    const isMobile = /Android|iPhone|iPad|iPod/i.test(navigator.userAgent);

    if (!isMobile) {
        btnWaze.classList.add("disabled");
        btnWaze.setAttribute("aria-disabled", "true");
        btnWaze.setAttribute("title", "Waze solo está disponible en dispositivos móviles.");

        btnWaze.addEventListener("click", function (e) {
            e.preventDefault();
        });
    }
});