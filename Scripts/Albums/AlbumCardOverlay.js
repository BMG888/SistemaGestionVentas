document.addEventListener("DOMContentLoaded", function () {

    const cards = document.querySelectorAll(".album-card");

    function closeAll(except) {
        cards.forEach(function (card) {
            if (card !== except) {
                card.classList.remove("album-card-active");
            }
        });
    }

    cards.forEach(function (card) {
        const collageWrapper = card.querySelector(".album-collage-wrapper");

        collageWrapper.addEventListener("click", function (e) {
            if (e.target.closest(".album-overlay-actions")) {
                return;
            }
            const isActive = card.classList.contains("album-card-active");
            closeAll(card);
            card.classList.toggle("album-card-active", !isActive);
        });

        card.addEventListener("mouseleave", function () {
            card.classList.remove("album-card-active");
        });
    });

    document.addEventListener("click", function (e) {
        if (!e.target.closest(".album-card")) {
            closeAll(null);
        }
    });
});