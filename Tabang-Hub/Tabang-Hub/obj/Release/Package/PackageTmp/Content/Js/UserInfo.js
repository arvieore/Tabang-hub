document.addEventListener('DOMContentLoaded', function () {
    // Add event listener to the Next button in the "User information" section
    document.querySelector('.main.active .button .next').addEventListener('click', function () {
        // Validate inputs before proceeding
        if (validateInputs()) {
            // Move to the next step if inputs are valid
            moveToNextStep();
        } else {
            // Show error message for invalid inputs
            alert('Please fill out all required fields correctly.');
            // You can implement more sophisticated error handling here
        }
    });
});

// Function to validate inputs
function validateInputs() {
    // Get input elements
    var firstNameInput = document.querySelector('.main.active input[placeholder="Firstname"]');
    var lastNameInput = document.querySelector('.main.active input[placeholder="Lastname"]');
    var birthdayInput = document.getElementById('datepicker');
    var phoneNumberInput = document.querySelector('.main.active input[placeholder="Phone number"]');
    var zipCodeInput = document.querySelector('.main.active input[placeholder="Zip Code"]');
    var genderInputs = document.querySelectorAll('.main.active input[name="gender"]');

    // Check if inputs are not empty
    if (
        firstNameInput.value.trim() === '' ||
        lastNameInput.value.trim() === '' ||
        birthdayInput.value.trim() === '' ||
        phoneNumberInput.value.trim() === '' ||
        zipCodeInput.value.trim() === ''
    ) {
        return false; // Return false if any field is empty
    }

    // Validate birthday
    var birthday = new Date(birthdayInput.value);
    var minAge = 12; // Minimum age required
    var today = new Date();
    var minBirthDate = new Date(today.getFullYear() - minAge, today.getMonth(), today.getDate());
    if (birthday > minBirthDate) {
        return false; // Return false if user is younger than minimum age
    }

    // Validate phone number format
    var phoneRegex = /^09\d{9}$/; // Regex for Filipino phone numbers starting with 09
    if (!phoneRegex.test(phoneNumberInput.value.trim())) {
        return false; // Return false if phone number format is invalid
    }

    // Validate zip code format
    var zipCodeRegex = /^\d{4}$/; // Regex for 4-digit zip codes
    if (!zipCodeRegex.test(zipCodeInput.value.trim())) {
        return false; // Return false if zip code format is invalid
    }

    // Validate gender selection
    var genderSelected = false;
    genderInputs.forEach(function (input) {
        if (input.checked) {
            genderSelected = true;
        }
    });
    if (!genderSelected) {
        return false; // Return false if gender is not selected
    }

    // If all validations pass, return true
    return true;
}

// Function to move to the next step
function moveToNextStep() {
    // Code to move to the next step goes here
    console.log('Moving to the next step...');
}
