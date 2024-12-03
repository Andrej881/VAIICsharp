document.getElementById('addTagButton').addEventListener('click', function () {
    const newTag = document.getElementById('searchTag').value.trim();
    if (newTag && !document.querySelector(`.tag[data-tag-id='${newTag}']`)) {
        // Add new tag dynamically
        let newTagElement = document.createElement('div');
        newTagElement.classList.add('col-auto');
        newTagElement.innerHTML = `<button type="button" class="tag btn btn-light" data-tag-id="${newTag}">${newTag}</button>`;
        document.getElementById('selectedTags').appendChild(newTagElement);

        // Optionally, send a request to the server to create the new tag
        document.getElementById('selectedTagIds').innerHTML += `<option value="${newTag}">${newTag}</option>`;

        // Clear input
        document.getElementById('searchTag').value = '';
    }
});