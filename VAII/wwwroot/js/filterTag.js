document.addEventListener("DOMContentLoaded", function () {

    document.getElementById("addTagFilter").addEventListener("click", function () {
        var newTag = document.getElementById("selectTag").value;

        if (newTag) {
            addNewTag(newTag);
        }
    });

    function addNewTag(tagName) {
        // Pridaj tag do zoznamu NewTags na stránke
        var selectedTagsContainer = document.getElementById("selectedTagsContainer");

        var existingTags = Array.from(document.querySelectorAll("input[name^='SelectedTags']")).map(input => input.value);
        if (existingTags.includes(tagName)) {
            alert("Tag already added!");
            return;
        }

        var newTagButton = document.createElement("button");
        newTagButton.classList.add("tag");
        newTagButton.textContent = tagName;
        newTagButton.type = "button";

        var newTagDiv = document.createElement("div");
        newTagDiv.classList.add("col-auto");
        newTagDiv.appendChild(newTagButton);
        selectedTagsContainer.appendChild(newTagDiv);
        // Pridaj tag do hidden poľa pre submit formu
        const form = document.getElementById("myForm");
        var inputTag = document.createElement("input");
        inputTag.type = "hidden";
        inputTag.name = `SelectedTags[${existingTags.length}]`;
        inputTag.value = tagName;

        form.appendChild(inputTag);
        updateSearchResults();
    }
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
            updateSearchResults()
        }
    });

    function updateHiddenInputs() {
        const hiddenInputs = document.querySelectorAll(`input[name ^= 'SelectedTags']`);
        index = 0
        hiddenInputs.forEach((input) => {
            input.name = `SelectedTags[${index}]`;
            index++;
        });
    }

    function updateSearchResults() {
        const form = document.getElementById("myForm");
        const formData = new FormData(form);

        // Convert form data to query parameters
        const params = new URLSearchParams();
        for (const [key, value] of formData.entries()) {
            params.append(key, value);
        }

        fetch(`/Home/Index?${params.toString()}`, {
            method: "GET",
            headers: {
                "X-Requested-With": "XMLHttpRequest"
            }
        })
            .then(response => {
                if (response.ok) {
                    return response.text();
                } else {
                    throw new Error("Failed to fetch search results.");
                }
            })
            .then(html => {
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, "text/html");
                const newContent = doc.querySelector(".partial");

                const gamesContainer = document.querySelector(".partial");
                console.log(gamesContainer);
                if (gamesContainer && newContent) {
                    gamesContainer.replaceWith(newContent);
                }
            })
            .catch(error => console.error("Error updating search results:", error));
    }

});