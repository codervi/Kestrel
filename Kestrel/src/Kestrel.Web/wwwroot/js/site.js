// Ask before anything destructive. Forms opt in with data-confirm="...".
document.addEventListener("submit", function (e) {
  var msg = e.target.getAttribute("data-confirm");
  if (msg && !window.confirm(msg)) e.preventDefault();
});

// Admin project form: show a preview of the chosen image before saving.
(function () {
  var input = document.getElementById("Image");
  if (!input) return;

  var preview = document.createElement("img");
  preview.className = "thumb";
  preview.alt = "Preview of the selected image";
  preview.hidden = true;
  input.parentNode.insertBefore(preview, input);

  input.addEventListener("change", function () {
    var file = input.files && input.files[0];
    if (!file) { preview.hidden = true; return; }
    preview.src = URL.createObjectURL(file);
    preview.hidden = false;
  });
})();
