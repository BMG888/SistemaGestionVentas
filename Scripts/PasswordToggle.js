document.addEventListener("DOMContentLoaded", function () {

    const buttons = document.querySelectorAll(".toggle-password");

    buttons.forEach(function (button) {

        button.addEventListener("click", function () {

            const input = document.getElementById(button.dataset.target);

            if (input.type === "password") {
                input.type = "text";
                button.innerText = "Ocultar";
            }
            else {
                input.type = "password";
                button.innerText = "Mostrar";
            }
        });
    });
});