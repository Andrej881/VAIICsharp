
    document.getElementById("addTag").addEventListener("click", function () {
        // Zobraziť modálne okno
        centerModal();
        document.getElementById("addTagModal").style.display = "block";
        document.body.classList.add("no-scroll");
    });

    document.getElementById("closeModal").addEventListener("click", function () {
        // Zatvoriť modálne okno
        document.getElementById("addTagModal").style.display = "none";
        document.body.classList.remove("no-scroll");
    });

    const centerModal = () => {
        const scrollY = window.scrollY; 
        const viewportHeight = window.innerHeight; 

        document.getElementById("addTagModal").style.top = `${scrollY + viewportHeight / 4}px`;
        document.getElementById("addTagModal").style.transform = "translate(0%,-50%)"; 
    };

    document.getElementById("confirmAddTag").addEventListener("click", function () {
        var newTag = document.getElementById("newTagInput").value;
        if (newTag) {
            addNewTag(newTag);

            document.getElementById("addTagModal").style.display = "none";
                
            document.getElementById("newTagInput").value = "";
            document.body.classList.remove("no-scroll");
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
    }
