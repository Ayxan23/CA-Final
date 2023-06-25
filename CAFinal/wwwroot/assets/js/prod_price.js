const priceProds = document.querySelectorAll(".price_prod");
priceProds.forEach((priceProd) => {
    priceProd.innerHTML = "$" + parseFloat(priceProd.innerHTML).toFixed(2)
});