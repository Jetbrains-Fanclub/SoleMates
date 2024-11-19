let preview = document.getElementById("preview");
let images = document.getElementsByClassName("selection-image");

for (let image of images) {
  image.addEventListener('click', (event) => {
    let target = event.target;
    if (target.classList.contains("active")) {
      return;
    }

    for (let element of images) {
      element.classList.remove("active");
    }

    target.classList.add("active");

    preview.setAttribute("src", target.getAttribute("src"));
  });
}