let number_cards = document.querySelectorAll(".number_cards");

number_cards.forEach((number_card) => {
    number_card.addEventListener("input", () => {
        if (number_card.value.length > number_card.maxLength) {
            number_card.value = number_card.value.slice(0, number_card.maxLength)
        };
    });
});
