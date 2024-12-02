document.getElementById('imageInput').addEventListener('change', function (event) {
    var file = event.target.files[0]; 

    if (file) {
        var reader = new FileReader();

        reader.onload = function (e) {
            var img = document.getElementById('imagePreview');
            img.src = e.target.result;  
        };

        reader.readAsDataURL(file);
    }
});