const vauleCount = document.querySelector(".count_value");
const plusCount = document.querySelector(".count_plus");
const minCount = document.querySelector(".count_min");
const priceCount = document.querySelector(".price_count");

priceCount.innerHTML = parseFloat(priceCount.innerHTML).toFixed(2)

let count = 1;
plusCount.addEventListener("click", () => {
    priceCount.innerHTML = parseFloat(priceCount.innerHTML / count).toFixed(2);
    vauleCount.innerHTML = ++count;
    priceCount.innerHTML = parseFloat(priceCount.innerHTML * count).toFixed(2);
    fetch(`/Shop/Detail/id/?count=${count}`)
});

minCount.addEventListener("click", () => {
    if (count > 1) {
        priceCount.innerHTML = parseFloat(priceCount.innerHTML / count).toFixed(2);
        vauleCount.innerHTML = --count;
        priceCount.innerHTML = parseFloat(priceCount.innerHTML * count).toFixed(2);
    }
});

