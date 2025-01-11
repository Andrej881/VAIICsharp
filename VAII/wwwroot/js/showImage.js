const ALLOWED_EXTENSIONS = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp", ".svg", ".ico"];

document.getElementById('imageInput').addEventListener('change', function (event) {
    var file = event.target.files[0]; 

    if (file) {        

        const fileName = file.name.toLowerCase();
        const fileExtension = fileName.slice(fileName.lastIndexOf('.'));
        if (ALLOWED_EXTENSIONS.includes(fileExtension)) {
            
            var reader = new FileReader();
            reader.onload = function (e) {
                var img = document.getElementById('imagePreview');
                img.src = e.target.result;
            };

            reader.readAsDataURL(file);
        } else {
            alert(`your image file does not have valid exstension ${fileExtension}`);
            //Clear Input
            document.getElementById('imageInput').value = ''; 
            document.getElementById('imagePreview').src = '';
        }

    }
});