// JavaScript for image selection
$(document).ready(function () {
    $('.skills-container img').click(function () {
        $(this).toggleClass('selected');
        toggleButtonState();
        var store = logSelectedSkills();
        console.log("my skills: ", store);
    });
});

// Function to toggle button state based on image selection
function toggleButtonState() {
    var selectedImages = $('.skills-container img.selected').length;
    if (selectedImages > 0) {
        $('.button button').prop('disabled', false);
    } else {
        $('.button button').prop('disabled', true);
    }
}


// Add event listener to selectable images
document.querySelectorAll('.selectable').forEach(item => {
    item.addEventListener('click', event => {
        // Toggle the 'selected' class on click
        event.target.classList.toggle('selected');
        // Check if any skill is selected
        checkSkillsSelected();
    });
});

// Function to check if any skill is selected
function checkSkillsSelected() {
    // Get all selected skills
    const selectedSkills = document.querySelectorAll('.selectable.selected');
    // Enable or disable the submit button based on selection
    const submitButton = document.querySelector('.submit');
    if (selectedSkills.length > 0) {
        submitButton.disabled = false;
    } else {
        submitButton.disabled = true;
    }
}

function logSelectedSkills() {
    var storeSkill = [];
    $('.skills-container img.selected').each(function () {
        var skillAttribute = $(this).data('skill');
        if (skillAttribute && !storeSkill.includes(skillAttribute)) {
            storeSkill.push(skillAttribute);
        }
    });
    return storeSkill;
    //console.log("Log Selected Skills: ", storeSkill);
}