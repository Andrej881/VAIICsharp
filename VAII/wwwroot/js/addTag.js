document.getElementById("addTag").addEventListener("click", function () {
    // Zobraziť modálne okno
    document.getElementById("addTagModal").style.display = "block";
});

document.getElementById("closeModal").addEventListener("click", function () {
    // Zatvoriť modálne okno
    document.getElementById("addTagModal").style.display = "none";
});

document.getElementById("confirmAddTag").addEventListener("click", function () {
    var newTag = document.getElementById("newTagInput").value;

    if (newTag) {
        // Pridať nový tag do zoznamu
        addNewTag(newTag);

        // Zavrieť modálne okno
        document.getElementById("addTagModal").style.display = "none";

        // Vyčistiť input po pridaní
        document.getElementById("newTagInput").value = "";
    }
});

function addNewTag(tagName) {
    // Pridaj tag do zoznamu NewTags na stránke
    var selectedTagsContainer = document.getElementById("selectedTagsContainer");

    var newTagButton = document.createElement("button");
    newTagButton.classList.add("tag");
    newTagButton.textContent = tagName;
    newTagButton.type = "button";

    var newTagDiv = document.createElement("div");
    newTagDiv.classList.add("col-auto");
    newTagDiv.appendChild(newTagButton);

    selectedTagsContainer.appendChild(newTagDiv);

    // Pridaj tag do hidden poľa pre submit formu
    var inputTag = document.createElement("input");
    inputTag.type = "hidden";
    inputTag.name = "NewTags";
    inputTag.value = tagName;

    document.querySelector("form").appendChild(inputTag);
}
