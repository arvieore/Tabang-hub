// Function to filter skills based on the selected order
function filterSkills() {
    const filterValue = document.getElementById("skillRankFilter").value;
    const skillList = document.getElementById("rankedSkillsList");
    const skillItems = Array.from(skillList.getElementsByClassName("skill-item"));

    // Sort the skills based on the selected value (Highest or Lowest)
    skillItems.sort((a, b) => {
        const ratingA = parseInt(a.getAttribute("data-rating")) || 0;
        const ratingB = parseInt(b.getAttribute("data-rating")) || 0;

        if (filterValue === "Highest") {
            return ratingB - ratingA; // Descending order for Highest
        } else {
            return ratingA - ratingB; // Ascending order for Lowest
        }
    });

    // Clear the current list and append the sorted items
    skillList.innerHTML = "";
    skillItems.forEach(item => skillList.appendChild(item));
}

// Call the function on page load to initially sort by Highest
document.addEventListener("DOMContentLoaded", filterSkills);