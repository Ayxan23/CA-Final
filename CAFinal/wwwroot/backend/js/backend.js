const loadMoreBtn = document.getElementById("loadMoreBtn");
const productList = document.getElementById("productList");
const productCount = document.getElementById("productCount").value;

let skip = 6;
loadMoreBtn.addEventListener("click", function () {
    fetch(`/Shop/LoadMore?skip=${skip}`).then(response => response.text())
        .then(data => {
            productList.innerHTML += data;
        })
    skip += 6;
    if (skip >= productCount) {
        loadMoreBtn.remove();
    }
})

