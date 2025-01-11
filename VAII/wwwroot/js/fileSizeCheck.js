const MAX_SIZE = 20 * 1024 * 1024;

document.getElementById('fileInput').addEventListener('change', function (event) {
    var file = event.target.files[0];

    if (file) {
        if (file.size > MAX_SIZE) {
            alert(`your game file is too big ${Math.round((file.size / (1024 * 1024)) * 1000) / 1000 }MB/${MAX_SIZE / (1024 * 1024)}MB`);
            //Clear Input
            document.getElementById('fileInput').value = '';
        } 

    }
});