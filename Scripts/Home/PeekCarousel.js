document.addEventListener("DOMContentLoaded", function () {

    const track = document.getElementById("peekTrack");
    const carousel = document.getElementById("peekCarousel");
    const btnPrev = document.getElementById("peekPrev");
    const btnNext = document.getElementById("peekNext");
    const dotsContainer = document.getElementById("peekDots");

    if (!track)
        return;

    const slides = Array.from(track.querySelectorAll(".peek-slide"));
    const dots = dotsContainer ? Array.from(dotsContainer.querySelectorAll(".peek-dot")) : [];
    let currentIndex = 0;
    let autoplayTimer = null;

    function goTo(index) {

        if (index < 0)
            index = slides.length - 1;

        if (index >= slides.length)
            index = 0;

        currentIndex = index;

        const viewport = track.parentElement;
        const slide = slides[currentIndex];

        const viewportCenter = viewport.offsetWidth / 2;
        const slideCenter = slide.offsetLeft + slide.offsetWidth / 2;

        const offset = viewportCenter - slideCenter;
        track.style.transform = "translateX(" + offset + "px)";

        slides.forEach(function (s, i) {
            s.classList.toggle("peek-slide-active", i === currentIndex);
        });

        dots.forEach(function (d, i) {
            d.classList.toggle("peek-dot-active", i === currentIndex);
        });
    }

    function startAutoplay() {
        stopAutoplay();
        autoplayTimer = setInterval(function () {
            goTo(currentIndex + 1);
        }, 4000);
    }

    function stopAutoplay() {
        if (autoplayTimer) {
            clearInterval(autoplayTimer);
            autoplayTimer = null;
        }
    }

    if (btnPrev)
        btnPrev.addEventListener("click", function () {
            goTo(currentIndex - 1);
            startAutoplay();
        });

    if (btnNext)
        btnNext.addEventListener("click", function () {
            goTo(currentIndex + 1);
            startAutoplay();
        });

    slides.forEach(function (slide, i) {
        slide.addEventListener("click", function () {
            goTo(i);
            startAutoplay();
        });
    });

    dots.forEach(function (dot, i) {
        dot.addEventListener("click", function () {
            goTo(i);
            startAutoplay();
        });
    });

    window.addEventListener("resize", function () {
        goTo(currentIndex);
    });

    goTo(0);

    if (slides.length > 1) {
        startAutoplay();
    }
});