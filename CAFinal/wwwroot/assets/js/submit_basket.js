let cart_total_button = document.querySelector(".cart_total_button");
let buy_form = document.querySelector(".buy_form");

cart_total_button.addEventListener("click", () => {
    buy_form.submit();
});