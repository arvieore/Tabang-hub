var originalValues = {};
var originalProfilePicSrc = "";
var originalSkills = [];
var originalDropdownOptions = []; // Store the original dropdown options

function previewProfilePic() {
    var file = document.getElementById('profilePic').files[0];
    var reader = new FileReader();
    reader.onloadend = function () {
        document.getElementById('profilePicPreview').src = reader.result;
    }
    if (file) {
        reader.readAsDataURL(file);
    }
}

function enterEditMode() {
    originalValues.street = document.getElementById('streetInput').value;
    originalValues.city = document.getElementById('cityInput').value;
    originalValues.province = document.getElementById('provinceInput').value;
    originalValues.phone = document.getElementById('phoneInput').value;
    originalValues.availability = document.querySelector('input[name="availability"]:checked').value;
    originalProfilePicSrc = document.getElementById('profilePicPreview').src;

    // Store the original dropdown options
    var options = document.getElementById('skillsDropdown').options;
    originalDropdownOptions = Array.from(options).map(option => ({ value: option.value, text: option.text }));

    toggleInputs(true);
}

function saveChanges() {
    if (validateInputs()) {
        var street = document.getElementById('streetInput').value;
        var city = document.getElementById('cityInput').value;
        var province = document.getElementById('provinceInput').value;
        var phone = document.getElementById('phoneInput').value;
        var availability = document.querySelector('input[name="availability"]:checked').value;
        var profilePic = document.getElementById('profilePic').files[0];

        SaveChanges(street, city, province, phone, availability, profilePic);
    } else {
        console.log("Validation error.");
        return;
    }
}

function toggleInputs(isEditing) {
    document.getElementById('address').classList.toggle('d-none', isEditing);
    document.getElementById('addressInput').classList.toggle('d-none', !isEditing);

    document.getElementById('phone').classList.toggle('d-none', isEditing);
    document.getElementById('phoneInput').classList.toggle('d-none', !isEditing);

    document.getElementById('availability').classList.toggle('d-none', isEditing);
    document.getElementById('availabilityInput').classList.toggle('d-none', !isEditing);

    document.getElementById('profile').classList.toggle('d-none', !isEditing);
    document.getElementById('profilePic').classList.toggle('d-none', !isEditing);

    document.getElementById('editButton').innerText = isEditing ? 'Save' : 'Edit';
    document.getElementById('cancelButton').classList.toggle('d-none', !isEditing);

    if (isEditing) {
        var availability = document.querySelector('input[name="availability"]:checked').value;
        var availabilityRadios = document.getElementsByName('availability');
        for (var i = 0; i < availabilityRadios.length; i++) {
            if (availabilityRadios[i].value === availability) {
                availabilityRadios[i].checked = true;
            }
        }
    }
}

function toggleEdit() {
    var isEditing = document.getElementById('editButton').innerText === 'Edit';

    if (isEditing) {
        enterEditMode();
    } else {
        saveChanges();
    }
}

function validateInputs() {
    var inputs = ['streetInput', 'cityInput', 'provinceInput', 'phoneInput'];
    var isValid = true;

    inputs.forEach(function (inputId) {
        var input = document.getElementById(inputId);
        var errorMessage = input.nextElementSibling;

        if (!input.value.trim()) {
            input.classList.add('border-danger');
            errorMessage.textContent = 'This field is required';
            isValid = false;
        } else if (inputId === 'phoneInput' && !/^09[0-9]{9}$/.test(input.value)) {
            input.classList.add('border-danger');
            errorMessage.textContent = 'Phone number should start with 09 and contain 11 digits';
            isValid = false;
        } else {
            input.classList.remove('border-danger');
            errorMessage.textContent = '';
        }
    });
    return isValid;
}

function cancelEdit() {
    document.getElementById('streetInput').value = originalValues.street;
    document.getElementById('cityInput').value = originalValues.city;
    document.getElementById('provinceInput').value = originalValues.province;
    document.getElementById('phoneInput').value = originalValues.phone;

    var availabilityRadios = document.getElementsByName('availability');
    for (var i = 0; i < availabilityRadios.length; i++) {
        if (availabilityRadios[i].value === originalValues.availability) {
            availabilityRadios[i].checked = true;
        }
    }

    document.getElementById('profilePicPreview').src = originalProfilePicSrc;

    var inputs = ['streetInput', 'cityInput', 'provinceInput', 'phoneInput'];
    inputs.forEach(function (inputId) {
        var input = document.getElementById(inputId);
        var errorMessage = input.nextElementSibling;
        input.classList.remove('border-danger');
        errorMessage.textContent = '';
    });

    toggleInputs(false);
}

function SaveChanges(street, city, province, phone, availability, profilePic) {
    var formData = new FormData();
    formData.append('street', street);
    formData.append('city', city);
    formData.append('province', province);
    formData.append('phone', phone);
    formData.append('availability', availability);
    if (profilePic) {
        formData.append('profilePic', profilePic);
    }

    $.ajax({
        url: '/Volunteer/EditBasicInfo',
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            if (response.success) {
                console.log("Updated");
                $('#address').text(street + ', ' + city + ' City, ' + province);
                $('#phone').text(phone);
                $('#availability').text(availability);
                if (profilePic) {
                    $('#profilePicPreview').attr('src', URL.createObjectURL(profilePic));
                }
                window.location.reload();
            } else {
                console.log("Error: " + response.message);
            }
        },
        error: function (xhr, status, error) {
            console.log("Error function " + error);
        }
    });
}

function toggleEditAboutMe() {
    var isEditing = document.getElementById('editButtonAboutMe').innerText === 'Edit';

    if (isEditing) {
        originalValues.aboutMe = document.getElementById('aboutMeInput').value;
        originalSkills = getSkillsDisplayData(); // Store the original skills

        // Store the original dropdown options
        var options = document.getElementById('skillsDropdown').options;
        originalDropdownOptions = Array.from(options).map(option => ({ value: option.value, text: option.text }));

    } else {
        saveAboutMeChanges();
    }

    toggleAboutMeInputs(isEditing);
}

function toggleAboutMeInputs(isEditing) {
    document.getElementById('aboutMe').classList.toggle('d-none', isEditing);
    document.getElementById('aboutMeInput').classList.toggle('d-none', !isEditing);

    document.getElementById('skillsDropdown').classList.toggle('d-none', !isEditing);

    document.querySelectorAll('.remove-skill').forEach(element => {
        element.classList.toggle('d-none', !isEditing);
    });

    document.getElementById('editButtonAboutMe').innerText = isEditing ? 'Save' : 'Edit';
    document.getElementById('cancelButtonAboutMe').classList.toggle('d-none', !isEditing);
}

function saveAboutMeChanges() {
    var aboutMe = document.getElementById('aboutMeInput').value;

    var getDisplaySkill = getSkillsDisplayData();

    SaveAboutMe(aboutMe, getDisplaySkill);
}

function cancelEditAboutMe() {
    document.getElementById('aboutMeInput').value = originalValues.aboutMe;

    // Restore skills
    var skillsDisplay = document.getElementById('skillsDisplay');
    skillsDisplay.innerHTML = ''; // Clear current skills

    originalSkills.forEach(function (skill) {
        var skillBadge = document.createElement('span');
        skillBadge.className = 'skill-badge';
        skillBadge.setAttribute('data-skill-id', skill.id);
        skillBadge.style.backgroundColor = 'mediumseagreen';
        skillBadge.innerHTML = `${skill.name} <span class="remove-skill d-none" onclick="removeSkill(this)">x</span>`;
        skillsDisplay.appendChild(skillBadge);
    });

    // Restore the dropdown options, including the placeholder if it doesn't exist
    var skillsDropdown = document.getElementById('skillsDropdown');
    skillsDropdown.innerHTML = ''; // Clear current options

    var placeholderOption = document.createElement('option');
    placeholderOption.value = "";
    placeholderOption.text = "Select a skill";
    skillsDropdown.add(placeholderOption);

    originalDropdownOptions.forEach(option => {
        if (option.value !== "") { // Avoid adding a duplicate placeholder
            var newOption = document.createElement('option');
            newOption.value = option.value;
            newOption.text = option.text;
            skillsDropdown.add(newOption);
        }
    });

    toggleAboutMeInputs(false);
}

function SaveAboutMe(aboutMe, getDisplaySkill) {
    var getSkills = getDisplaySkill.map(skill => skill.id);

    $.ajax({
        url: '/Volunteer/EditAboutMe',
        type: 'POST',
        data: {
            aboutMe: aboutMe,
            skills: getSkills
        },
        success: function (response) {
            if (response.success) {
                $('#aboutMe').text(aboutMe);
                toggleAboutMeInputs(false);
                location.reload();
            } else {
                console.log("Error:!! " + response.message);
                Swal.fire({
                    icon: 'error',
                    text: "This skill is in use for an ongoing or applied event. You can't remove it until the event is completed or your application is withdrawn.",
                }).then(() => {
                    location.reload();
                });
            }
        },
        error: function (xhr, status, error) {
            console.log("Error function " + error);
        }
    });
}

function addSkill() {
    var selectedSkill = document.getElementById('skillsDropdown').value;
    var selectedSkillName = document.getElementById('skillsDropdown').options[document.getElementById('skillsDropdown').selectedIndex].text;

    if (selectedSkill) {
        var skillsDisplay = document.getElementById('skillsDisplay');
        var newSkill = document.createElement('span');
        newSkill.className = 'skill-badge';
        newSkill.setAttribute('data-skill-id', selectedSkill);
        newSkill.style.backgroundColor = 'mediumseagreen';
        newSkill.innerHTML = `${selectedSkillName} <span class="remove-skill" onclick="removeSkill(this)">x</span>`;
        skillsDisplay.appendChild(newSkill);

        var options = document.getElementById('skillsDropdown').options;
        for (var i = 0; i < options.length; i++) {
            if (options[i].value === selectedSkill) {
                options[i].remove();
                break;
            }
        }
    }
}

function removeSkill(element) {
    var skillBadge = element.parentNode;
    var skillId = skillBadge.getAttribute('data-skill-id');
    var skillName = skillBadge.innerText.replace(' x', '');

    var skillsDropdown = document.getElementById('skillsDropdown');
    var option = document.createElement('option');
    option.value = skillId;
    option.text = skillName;
    skillsDropdown.add(option);

    skillBadge.remove();
}

function getSkillsDisplayData() {
    var skillsDisplay = document.getElementById('skillsDisplay');
    var skillElements = skillsDisplay.getElementsByClassName('skill-badge');
    var skills = [];

    for (var i = 0; i < skillElements.length; i++) {
        skills.push({
            id: skillElements[i].getAttribute('data-skill-id'),
            name: skillElements[i].innerText.replace(' x', '')
        });
    }

    return skills;
}
