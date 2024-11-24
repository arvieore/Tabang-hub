function nextStep() {
    document.getElementById('step1').style.display = 'none';
    document.getElementById('step2').style.display = 'flex';
    validateSkills(); // Initial validation check for the second step
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
            console.log("Response is: ", response);
            location.reload();
        },
        error: function (xhr, status, error) {
            console.log("Error response is: ", error);
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

    const firstNameError = document.getElementById('firstNameError').textContent;
    const lastNameError = document.getElementById('lastNameError').textContent;
    const birthdayError = document.getElementById('birthdayError').textContent;
    const phoneNumberError = document.getElementById('phoneNumberError').textContent;
    const zipCodeError = document.getElementById('zipCodeError').textContent;

    const isValid = firstName && lastName && birthday && phoneNumber && street && city && province && zipCode && gender &&
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