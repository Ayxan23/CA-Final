let bars_li = document.querySelector(".bars_li");
let header_ul = document.querySelector(".header_ul");

let search_li = document.querySelector(".search_li");
let search = document.querySelector(".search");
let search_input = document.querySelector(".search_input");

bars_li.addEventListener("click", () => {
  header_ul.classList.toggle("display_flex");
});

search_li.addEventListener("click", () => {
  search.classList.add("display_flex");
  search_input.focus();
});

search_input.addEventListener("blur", () => {
  search.classList.remove("display_flex");
});

