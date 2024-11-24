function nextStep() {
    const phoneNumber = document.getElementById('phoneNumber').value;
    const phoneNumberError = document.getElementById('phoneNumberError');

    // AJAX call to check if the phone number is already in use
    $.ajax({
        type: 'POST',
        url: '/Volunteer/CheckPhoneNumber', // Endpoint to validate phone number
        contentType: 'application/json',
        data: JSON.stringify({ phoneNumber: phoneNumber }),
        success: function (response) {
            if (response.success) {
                // Phone number is valid, proceed to the next step
                phoneNumberError.textContent = ''; // Clear any previous error message
                document.getElementById('step1').style.display = 'none';
                document.getElementById('step2').style.display = 'flex';
                validateSkills(); // Initial validation check for the second step
            } else {
                // Display error message if phone number is already in use
                phoneNumberError.textContent = response.message;
                phoneNumberError.style.color = 'red';
                scrollToFirstError();
            }
        },
        error: function (xhr, status, error) {
            phoneNumberError.textContent = 'There was an issue validating the phone number. Please try again.';
            phoneNumberError.style.color = 'red';
            scrollToFirstError();
        }
    });
}

function scrollToFirstError() {
    // Find the first error message with content
    const errors = document.querySelectorAll('.error-message');
    for (let error of errors) {
        if (error.textContent.trim() !== '') {
            error.scrollIntoView({ behavior: 'smooth', block: 'center' });
            break;
        }
    }
}


function prevStep() {
    document.getElementById('step2').style.display = 'none';
    document.getElementById('step1').style.display = 'flex';
}

function completeRegistration() {
    const formData = {
        fname: document.getElementById('firstName').value,
        lname: document.getElementById('lastName').value,
        bday: document.getElementById('birthday').value,
        phoneNum: document.getElementById('phoneNumber').value,
        street: document.getElementById('street').value,
        city: document.getElementById('city').value,
        province: document.getElementById('province').value,
        zipCode: document.getElementById('zipCode').value,
        gender: document.getElementById('gender').value,
        availability: document.getElementById('availability').value // Capture availability from dropdown
    };

    var storeSkill = Array.from(document.querySelectorAll('.grid-item.selected'))
        .map(skill => skill.querySelector('span').textContent.trim());

    console.log('Form Data:', formData);
    console.log('Selected Skills:', storeSkill);

    SaveDatas(formData, storeSkill);
}

function SaveDatas(formData, storeSkill) {
    console.log("Save informations Function: ", formData);
    console.log("Log Selected Skills: ", storeSkill);

    $.ajax({
        type: 'POST',
        url: '/Volunteer/SaveInformation',
        contentType: 'application/json',
        data: JSON.stringify({
            model: formData,
            volunteerSkill: storeSkill
        }),
        success: function (response) {
            
            Swal.fire({
                title: '',
                text: 'Your information has been saved successfully!',
                icon: 'success',
                confirmButtonText: 'Ok'
            }).then(() => {
                location.reload();
            });
        },
        error: function (xhr, status, error) {
            Swal.fire({
                title: 'Error!',
                text: 'There was a problem saving your information. Please try again.',
                icon: 'error',
                confirmButtonText: 'OK'
            });
        }
    });
}

function toggleSkill(element) {
    element.classList.toggle('selected');
    validateSkills();
}

function validateTextOnly(input) {
    const errorMessage = document.getElementById(input.id + 'Error');
    if (/[^a-zA-Z\s-ñÑ-]/.test(input.value)) {
        input.value = input.value.replace(/[^a-zA-Z\s-ñÑ-]/g, '');
        errorMessage.textContent = 'Invalid input. Only letters and dashes are allowed.';
        errorMessage.style.color = 'red';
    } else {
        errorMessage.textContent = '';
    }
    validateForm();
}

function validateBirthday(input) {
    const errorMessage = document.getElementById('birthdayError');
    const birthdayDate = new Date(input.value);
    const today = new Date();
    let age = today.getFullYear() - birthdayDate.getFullYear();
    const monthDifference = today.getMonth() - birthdayDate.getMonth();

    if (monthDifference < 0 || (monthDifference === 0 && today.getDate() < birthdayDate.getDate())) {
        age--;
    }

    if (isNaN(age) || age < 18) {
        errorMessage.textContent = 'You must be 18 years or older.';
        errorMessage.style.color = 'red';
    } else {
        errorMessage.textContent = '';
    }
    validateForm();
}

function validatePhoneNumber(input) {
    const errorMessage = document.getElementById('phoneNumberError');
    input.value = input.value.replace(/[^0-9]/g, '');
    if (!/^09\d{9}$/.test(input.value)) {
        errorMessage.textContent = 'Phone number must be 11 digits and start with "09".';
        errorMessage.style.color = 'red';
    } else {
        errorMessage.textContent = '';
    }
    validateForm();
}

function validateZipCode(input) {
    const errorMessage = document.getElementById('zipCodeError');
    input.value = input.value.replace(/[^0-9]/g, '');
    if (!/^\d{4}$/.test(input.value)) {
        errorMessage.textContent = 'Zip code must be exactly 4 digits.';
        errorMessage.style.color = 'red';
    } else {
        errorMessage.textContent = '';
    }
    validateForm();
}

function validateForm() {
    const firstName = document.getElementById('firstName').value;
    const lastName = document.getElementById('lastName').value;
    const birthday = document.getElementById('birthday').value;
    const phoneNumber = document.getElementById('phoneNumber').value;
    const street = document.getElementById('street').value;
    const city = document.getElementById('city').value;
    const province = document.getElementById('province').value;
    const zipCode = document.getElementById('zipCode').value;
    const gender = document.getElementById('gender').value;
    const availability = document.getElementById('availability').value;

    const firstNameError = document.getElementById('firstNameError').textContent;
    const lastNameError = document.getElementById('lastNameError').textContent;
    const birthdayError = document.getElementById('birthdayError').textContent;
    const phoneNumberError = document.getElementById('phoneNumberError').textContent;
    const zipCodeError = document.getElementById('zipCodeError').textContent;

    const isValid = firstName && lastName && birthday && phoneNumber && street && city && province && zipCode && gender && availability &&
        !firstNameError && !lastNameError && !birthdayError && !phoneNumberError && !zipCodeError;

    const nextButton = document.getElementById('nextButton');
    nextButton.disabled = !isValid;
    if (isValid) {
        nextButton.classList.remove('btn-disabled');
    } else {
        nextButton.classList.add('btn-disabled');
    }
}

function validateSkills() {
    const selectedSkills = document.querySelectorAll('.grid-item.selected').length;
    const completeButton = document.getElementById('completeButton');
    completeButton.disabled = selectedSkills === 0;
    if (selectedSkills > 0) {
        completeButton.classList.remove('btn-disabled');
    } else {
        completeButton.classList.add('btn-disabled');
    }
}