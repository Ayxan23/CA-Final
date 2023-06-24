const priceProds = document.querySelectorAll(".price_prod");
priceProds.forEach((priceProd) => {
    priceProd.innerHTML = "$" + parseFloat(priceProd.innerHTML.substring(1)).toFixed(2)
});