document.addEventListener("DOMContentLoaded", function () {

    document.querySelectorAll(".deleteReviewBtn").forEach(button => {
        button.addEventListener("click", function () {
            const reviewId = this.getAttribute("data-id");

            if (!confirm("Are you sure you want to delete this game?")) return;

            fetch(`/Review/DeleteReview/${reviewId}`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                }
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        // Remove the deleted game's row from the table
                        const row = document.getElementById(`reviewItem-${reviewId}`);
                        if (row) {
                            row.remove();
                        }
                        alert("Game deleted successfully.");
                    } else {
                        alert(data.message || "An error occurred while deleting the game.");
                    }
                })
                .catch(error => {
                    console.error("Error deleting game:", error);
                    alert("An error occurred. Please try again.");
                });
        });
    });
});
