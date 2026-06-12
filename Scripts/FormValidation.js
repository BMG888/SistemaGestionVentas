document.addEventListener("DOMContentLoaded", function () {

    // referencias del documento HTML

    // login
    const loginEmail = document.getElementById("login_email");
    const loginPassword = document.getElementById("login_password");
    const loginEmailCounter = document.getElementById("login_email_counter");
    const loginPasswordCounter = document.getElementById("login_password_counter");
    const currentPassword = document.getElementById("current_password");
    const currentPasswordCounter = document.getElementById("current_password_counter");
    const newPassword = document.getElementById("new_password");
    const newPasswordCounter = document.getElementById("new_password_counter");
    const confirmPassword = document.getElementById("confirm_password");
    const confirmPasswordCounter = document.getElementById("confirm_password_counter");
    
    // users
    const userName = document.getElementById("user_name");
    const userLastname = document.getElementById("user_lastname");
    const userNickname = document.getElementById("user_nickname");
    const userPhone = document.getElementById("user_phone");
    const userEmail = document.getElementById("user_email");
    const userPassword = document.getElementById("user_password");
    const userNameCounter = document.getElementById("user_name_counter");
    const userLastnameCounter = document.getElementById("user_lastname_counter");
    const userNicknameCounter = document.getElementById("user_nickname_counter");
    const userPhoneCounter = document.getElementById("user_phone_counter");
    const userEmailCounter = document.getElementById("user_email_counter");
    const userPasswordCounter = document.getElementById("user_password_counter");
    const editUserName = document.getElementById("edit_user_name");
    const editUserNameCounter = document.getElementById("edit_user_name_counter");
    const editUserLastname = document.getElementById("edit_user_lastname");
    const editUserLastnameCounter = document.getElementById("edit_user_lastname_counter");
    const editUserNickname = document.getElementById("edit_user_nickname");
    const editUserNicknameCounter = document.getElementById("edit_user_nickname_counter");
    const editUserPhone = document.getElementById("edit_user_phone");
    const editUserPhoneCounter = document.getElementById("edit_user_phone_counter");
    const editUserEmail = document.getElementById("edit_user_email");
    const editUserEmailCounter = document.getElementById("edit_user_email_counter");    

    // cards
    const cardPayday = document.getElementById("card_payday");
    const cardPaydayCounter = document.getElementById("card_payday_counter");
    const cardItem = document.getElementById("card_item");
    const cardItemCounter = document.getElementById("card_item_counter");
    const editCardPayday = document.getElementById("edit_card_payday");
    const editCardPaydayCounter = document.getElementById("edit_card_payday_counter");
    const editCardItem = document.getElementById("edit_card_item");
    const editCardItemCounter = document.getElementById("edit_card_item_counter");

    function initializeCounter(input, counter, maxLength) {

        if (!input || !counter) {
            return;
        }
        function updateCounter() {
            const remaining = maxLength - input.value.length;
            counter.textContent = remaining + " caracteres restantes";
        }
        input.addEventListener(
            "input",
            updateCounter
        );
        updateCounter();
    }    

    initializeCounter(
        loginEmail,
        loginEmailCounter,
        150
    );

    initializeCounter(
        loginPassword,
        loginPasswordCounter,
        20
    );

    initializeCounter(
        currentPassword,
        currentPasswordCounter,
        20
    );

    initializeCounter(
        newPassword,
        newPasswordCounter,
        20
    );

    initializeCounter(
        confirmPassword,
        confirmPasswordCounter,
        20
    );

    initializeCounter(
        userName,
        userNameCounter,
        100
    );

    initializeCounter(
        userLastname,
        userLastnameCounter,
        100
    );

    initializeCounter(
        userNickname,
        userNicknameCounter,
        100
    );

    initializeCounter(
        userPhone,
        userPhoneCounter,
        20
    );

    initializeCounter(
        userEmail,
        userEmailCounter,
        150
    );

    initializeCounter(
        userPassword,
        userPasswordCounter,
        20
    );

    initializeCounter(
        editUserName,
        editUserNameCounter,
        100
    );

    initializeCounter(
        editUserLastname,
        editUserLastnameCounter,
        100
    );

    initializeCounter(
        editUserNickname,
        editUserNicknameCounter,
        100
    );

    initializeCounter(
        editUserPhone,
        editUserPhoneCounter,
        20
    );

    initializeCounter(
        editUserEmail,
        editUserEmailCounter,
        150
    );

    initializeCounter(
        cardPayday,
        cardPaydayCounter,
        50
    );

    initializeCounter(
        cardItem,
        cardItemCounter,
        100
    );

    initializeCounter(
        editCardPayday,
        editCardPaydayCounter,
        100
    );

    initializeCounter(
        editCardItem,
        editCardItemCounter,
        100
    );
});