var click_next = document.querySelectorAll(".next");
var click_back = document.querySelectorAll(".back_page");
var click_submit = document.querySelector(".submit");
var click_main = document.querySelectorAll(".main");
var progress = document.querySelectorAll(".progress_bar li");

let formnumber = 0;

function resetProgressBar() {
    formnumber = 0;
    progress.forEach(function (step) {
        step.classList.remove('active');
    });
    progress[0].classList.add('active');
    updateform();
}

function updateform() {
    click_main.forEach(function (form) {
        form.classList.remove('active');
    });
    click_main[formnumber].classList.add('active');
}

function progress_forward() {
    progress[formnumber].classList.add('active');
}

function progress_back() {
    var back_p = formnumber + 1;
    progress[back_p].classList.remove('active');
}

function validateform() {
    var validate = true;
    var inputs = document.querySelectorAll(".main.active input");

    inputs.forEach(function (inpt) {
        inpt.setCustomValidity(''); // Clear previous custom validity
        inpt.classList.remove('error'); // Remove previous error class

        if (inpt.hasAttribute("required") && inpt.value.length == "0") {
            validate = false;
            inpt.setCustomValidity('Please fill out this field.');
        } else {
            if (inpt.id === 'phoneNumber' && !inpt.value.match(/^09[0-9]{9}$/)) {
                validate = false;
                inpt.setCustomValidity('Phone number should start with 09 and contain 11 digits.');
            }
            if (inpt.id === 'zipCode' && !inpt.value.match(/^[0-9]{4}$/)) {
                validate = false;
                inpt.setCustomValidity('Zip Code should contain 4 digits.');
            }
            if (inpt.id === 'datepicker') {
                var today = new Date();
                var birthDate = new Date(inpt.value);
                var age = today.getFullYear() - birthDate.getFullYear();
                var monthDifference = today.getMonth() - birthDate.getMonth();
                if (monthDifference < 0 || (monthDifference === 0 && today.getDate() < birthDate.getDate())) {
                    age--;
                }
                if (age < 18) {
                    validate = false;
                    inpt.setCustomValidity('You must be at least 18 years old.');
                }
            }
        }

        // Check validity and apply error class
        if (!inpt.checkValidity()) {
            validate = false;
            inpt.reportValidity(); // Show the browser's default error tooltip
            inpt.classList.add('error'); // Add error class
        }
    });

    var genderInputs = document.getElementsByName('gender');
    var genderSelected = Array.from(genderInputs).some(input => input.checked);
    var genderErrorMessage = document.querySelector('.gender-inputs .error-message');
    if (!genderSelected) {
        validate = false;
        genderErrorMessage.innerText = 'Please select a gender.';
    } else {
        /*genderErrorMessage.innerText = '';*/
    }

    console.log("Form validation result:", validate);
    return validate;
}

// Function to check if any skill is selected
function checkSkillsSelected() {
    // Get all selected skills
    const selectedSkills = document.querySelectorAll('.selectable.selected');
    // Enable or disable the next button based on selection
    click_next.forEach(function (button) {
        if (selectedSkills.length > 0) {
            button.disabled = false;
            button.style.backgroundColor = '#fb4552';
            button.style.cursor = 'pointer';
        } else {
            button.disabled = true;
            button.style.backgroundColor = 'grey';
            button.style.cursor = 'not-allowed';
        }
    });
}

// Add event listeners to selectable images
document.querySelectorAll('.selectable').forEach(item => {
    item.addEventListener('click', event => {
        // Toggle the 'selected' class on click
        event.target.classList.toggle('selected');
        // Check if any skill is selected
        checkSkillsSelected();
    });
});

// Add event listeners for all 'next' buttons
click_next.forEach(function (button) {
    button.addEventListener('click', function () {
        if (validateform()) {
            console.log("All fields are valid. Proceeding to the next step.");
            formnumber++;
            updateform();
            progress_forward();
        } else {
            console.log("There are invalid fields. Showing errors.");
            // Ensure all invalid inputs show the red border
            document.querySelectorAll('.main.active input:invalid').forEach(function (inpt) {
                inpt.classList.add('error');
            });
        }
    });
});

// Add event listeners for all 'back' buttons
click_back.forEach(function (button) {
    button.addEventListener('click', function () {
        formnumber--;
        updateform();
        progress_back();
    });
});

// Event listener for the submit button
click_submit.addEventListener('click', function () {
    if (validateform()) {
        formnumber++;
        updateform();
        progress_forward();
    } else {
        // Ensure all invalid inputs show the red border
        document.querySelectorAll('.main.active input:invalid').forEach(function (inpt) {
            inpt.classList.add('error');
        });
    }
});

// Call this function when a new user logs in
function onNewUserLogin() {
    resetProgressBar();
    // Display the modal for the new user
    document.getElementById('modal-overlay').style.display = 'block';
}

// Example usage: Call this function when a new user logs in
onNewUserLogin();
