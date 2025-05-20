// Clear Search
function clearSearch() {
    window.location.href = '/ToDo/Index';
}

// Search
document.getElementById("search").addEventListener("keypress", function (e) {
    if (e.key === "Enter") {
        e.preventDefault();
        const query = this.value.trim();
        window.location.href = `/ToDo/Index?search=${encodeURIComponent(query)}`;
    }
});

// Show Edit Modal
function showEditModal(id) {

    fetch(`/ToDo/Edit/${id}`, {
        method: "GET",
        headers: { "X-Requested-With": "XMLHttpRequest" }
    })
        .then(response => response.json())
        .then(data => {
            if (data) {
                document.getElementById("todoId").value = data.id;
                document.getElementById("todoTitle").value = data.title;
                document.getElementById("todoStatus").value = data.isCompleted ? "true" : "false";
                const modal = new bootstrap.Modal(document.getElementById("editTodoModal"));
                modal.show();
            } else {
                alert("Todo item not found!");
            }
        })
        .catch(error => {
            console.error("Error:", error);
            alert("Unable to load ToDo details.");
        });
}

// Show View Modal
async function showViewModal(id) {
    try {
        const response = await fetch(`/ToDo/Details/${id}`, {
            headers: { "X-Requested-With": "XMLHttpRequest" }
        });

        if (!response.ok) {
            throw new Error("Failed to fetch ToDo details.");
        }

        const data = await response.json();
        document.getElementById("viewTitle").value = data.title;
        document.getElementById("viewStatus").value = data.isCompleted ? "Completed" : "Pending";
        const modal = new bootstrap.Modal(document.getElementById("viewTodoModal"));
        modal.show();
    } catch (error) {
        console.error("Error:", error);
        alert("Unable to load ToDo details.");
    }
}

// Show Delete Modal
function showDeleteModal(id) {
    document.getElementById("confirmDeleteButton").onclick = function () {
        deleteTodoItem(id);
    };
    const modal = new bootstrap.Modal(document.getElementById("deleteTodoModal"));
    modal.show();
}

// Delete ToDo Item
async function deleteTodoItem(id) {
    try {
        const response = await fetch(`/ToDo/Delete/${id}`, {
            method: "POST"
        });

        if (response.ok) {
            location.reload();
        } else {
            alert("Failed to delete ToDo item.");
        }
    } catch (error) {
        console.error("Error:", error);
        alert("Unable to delete ToDo item.");
    }
}

// Save New ToDo Item
document.getElementById("saveTodoButton").addEventListener("click", async function () {

    const form = document.getElementById("addTodoForm");
    const formData = new FormData(form);

    try {
        const response = await fetch('/ToDo/Create', {
            method: "POST",
            body: new URLSearchParams(formData)
        });

        if (response.ok) {
            location.reload();
        } else {
            alert("Failed to save ToDo item.");
        }
    } catch (err) {
        console.error(err);
        alert("Error saving ToDo item.");
    }
});

// Edit ToDo Item
document.getElementById("todoForm").addEventListener("submit", async function (e) {
    e.preventDefault();

    const id = document.getElementById("todoId").value;
    const title = document.getElementById("todoTitle").value;
    const isCompleted = document.getElementById("todoStatus").value === "true";

    try {
        const response = await fetch(`/ToDo/Edit/${id}`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ id, title, isCompleted })
        });

        if (response.ok) {
            location.reload();
        } else {
            alert("Failed to update ToDo item.");
        }
    } catch (error) {
        console.error("Error:", error);
        alert("Unable to update ToDo item.");
    }
});


//// Utility to check if the user is an Admin
//function isUserAdmin() {
//    return document.body.getAttribute("data-role") === "Admin";
//}
