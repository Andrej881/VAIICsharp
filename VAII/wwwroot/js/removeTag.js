document.getElementById("selectedTagsContainer").addEventListener("click", function (event) {
    
    if (event.target.classList.contains("tag")) {
        var tagButton = event.target;
        var tagName = tagButton.textContent;

        tagButton.parentElement.remove();

        var hiddenInput = document.querySelector(`input[name^='SelectedTags'][value='${tagName}']`);
        if (hiddenInput) {
            hiddenInput.remove();
        }

        updateHiddenInputs();
    }
});

function updateHiddenInputs() {
    const hiddenInputs = document.querySelectorAll(`input[name ^= 'SelectedTags']`);
    index = 0
    hiddenInputs.forEach((input) => {
        input.name = `SelectedTags[${index}]`; 
        alert(`${input.name} ${index}`)
        index++;
    });
}