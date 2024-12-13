let currentMonth = new Date().getMonth();
let currentYear = new Date().getFullYear();

// Initialize with ongoing events as default
let currentEvents = ongoingEvents;

const ongoingEventColors = [
    "#d9ffad", // Primary color
    "#c4f58c", // Second color
    "#b0e86b", // Third color
    "#9cdc4a", // Fourth color
    "#88d029"  // Fifth color
];

const pendingRequestColors = [
    "#ffe4e1", // Primary color
    "#ffc8c3", // Second color
    "#ffacab", // Third color
    "#ff908d", // Fourth color
    "#ff7474"  // Fifth color
];

const eventHistoryColors = [
    "#ffd8bd", // Primary color
    "#ffc8a1", // Second color
    "#ffb986", // Third color
    "#ffa96a", // Fourth color
    "#ff994f"  // Fifth color
];

function updateCalendar() {
    const calendar = document.getElementById("calendar");
    calendar.innerHTML = "";

    const firstDay = new Date(currentYear, currentMonth, 1).getDay();
    const daysInMonth = new Date(currentYear, currentMonth + 1, 0).getDate();

    document.getElementById("currentDate").textContent =
        `${new Date(currentYear, currentMonth).toLocaleString("default", { month: "long" })} ${currentYear}`;

    const weekdays = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
    weekdays.forEach(day => {
        const header = document.createElement("div");
        header.classList.add("header");
        header.textContent = day;
        calendar.appendChild(header);
    });

    for (let i = 0; i < firstDay; i++) {
        const emptyCell = document.createElement("div");
        emptyCell.classList.add("day");
        calendar.appendChild(emptyCell);
    }

    const activeTab = document.querySelector('.nav-tabs .nav-link.active').id;
    let eventColors;

    if (activeTab === "ongoingEvents-tab") {
        eventColors = ongoingEventColors;
    } else if (activeTab === "pendingRequest-tab") {
        eventColors = pendingRequestColors;
    } else if (activeTab === "eventHistory-tab") {
        eventColors = eventHistoryColors;
    }

    for (let date = 1; date <= daysInMonth; date++) {
        const dayCell = document.createElement("div");
        dayCell.classList.add("day");

        const dateKey = `${currentYear}-${String(currentMonth + 1).padStart(2, "0")}-${String(date).padStart(2, "0")}`;
        const eventsForDay = currentEvents.filter(event =>
            isDateInRange(dateKey, event.Start_Date, event.End_Date)
        );

        if (eventsForDay.length > 0) {
            eventsForDay.forEach((event, index) => {
                const eventColor = eventColors[index % eventColors.length];
                dayCell.style.backgroundColor = eventColor;
            });

            dayCell.addEventListener("click", () => {
                eventsForDay.forEach(event => openEventModal(event));
            });
        }

        const today = new Date();
        if (date === today.getDate() && currentMonth === today.getMonth() && currentYear === today.getFullYear()) {
            dayCell.classList.add("today");
        }

        dayCell.textContent = date;
        calendar.appendChild(dayCell);
    }
}


// Ensure event date format matches and compare them correctly
function isDateInRange(date, start, end) {
    const currentDate = new Date(date);
    const startDate = new Date(start);
    const endDate = new Date(end);

    return currentDate >= startDate && currentDate <= endDate;
}

let currentSection = '';
document.querySelectorAll('.nav-link').forEach(button => {
    button.addEventListener('click', () => {
        currentSection = button.getAttribute('data-section');
    });
});

// Function to open the modal and populate it with event details
function openEventModal(event) {
    // Populate modal content with event details
    document.getElementById("eventTitle").textContent = event.Title;
    document.getElementById("eventLocation").textContent = event.Location; // Add location below the title
    document.getElementById("eventDetails").textContent = event.Details;

    const eventImage = event.Image ? "/Content/Events/" + event.Image : "https://via.placeholder.com/400x200?text=Event+Image";
    document.getElementById("eventImage").src = eventImage;

    const skillContainer = document.getElementById("skillContainer");
    skillContainer.innerHTML = ""; // Clear previous skill content

    // Get the modal content container
    const modalContent = document.getElementById("eventModalContent");

    // Remove any existing date elements
    const existingDateContainer = modalContent.querySelector(".date-container");
    if (existingDateContainer) {
        existingDateContainer.remove();
    }

    // Add Start_Date and End_Date
    const dateContainer = document.createElement("div");
    dateContainer.className = "date-container"; // Add a class for easy identification
    dateContainer.style.marginTop = "15px";

    // Function to create date entries with an icon, label, and value
    const createDateEntry = (iconClass, label, value) => {
        const entryWrapper = document.createElement("div");
        entryWrapper.style.display = "flex";
        entryWrapper.style.alignItems = "center";
        entryWrapper.style.marginBottom = "10px";

        const icon = document.createElement("span");
        icon.className = iconClass; // Use a class for the icon (e.g., from FontAwesome)
        icon.style.marginRight = "8px";
        entryWrapper.appendChild(icon);

        const labelElement = document.createElement("span");
        labelElement.textContent = label;
        labelElement.style.fontWeight = "bold";
        labelElement.style.marginRight = "5px";
        entryWrapper.appendChild(labelElement);

        const valueElement = document.createElement("span");
        valueElement.textContent = value;
        entryWrapper.appendChild(valueElement);

        return entryWrapper;
    };

    const formatDate = (dateString) => {
        const date = new Date(dateString);
        return new Intl.DateTimeFormat("en-US", {
            month: "long",
            day: "2-digit",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit",
            hour12: true,
        }).format(date);
    };

    const startDateEntry = createDateEntry("fa fa-calendar", "Start Date:", formatDate(event.Start_Date));
    dateContainer.appendChild(startDateEntry);

    const endDateEntry = createDateEntry("fa fa-calendar", "End Date:", formatDate(event.End_Date));
    dateContainer.appendChild(endDateEntry);

    // Append the date container to the modal content
    modalContent.appendChild(dateContainer);

    if (event.SkillName && event.SkillName.length > 0) {
        // Add "Skills:" label
        const label = document.createElement("span");
        label.textContent = "Skills: ";
        label.style.fontWeight = "bold"; // Optional: style the label
        skillContainer.appendChild(label);

        // Add badges for each skill
        event.SkillName.forEach((skill, index) => {
            // Create a container for skill + rating
            const skillItem = document.createElement("div");
            skillItem.style.display = "inline-block";
            skillItem.style.marginRight = "10px";
            skillItem.style.marginBottom = "10px";

            // Create skill badge
            const badge = document.createElement("span");
            badge.className = "badge text-white border me-2"; // Add Bootstrap classes for badge styling
            badge.style.padding = "5px 10px";
            badge.style.borderRadius = "15px";
            badge.textContent = skill;

            skillItem.appendChild(badge);

            // Add rating stars and numeric value if in "history" section
            if (currentSection === "history" && event.Rating && event.Rating[index] !== undefined) {
                const ratingContainer = document.createElement("div");
                ratingContainer.style.display = "flex";
                ratingContainer.style.alignItems = "center";
                ratingContainer.style.marginTop = "5px";

                // Add numeric rating value first
                const rating = event.Rating[index]; // Assume Rating corresponds to the skill index
                const numericRating = document.createElement("span");
                numericRating.textContent = `${rating}`; // Display the numeric rating first
                numericRating.style.fontSize = "14px";
                numericRating.style.marginRight = "5px";
                numericRating.style.color = "black";
                ratingContainer.appendChild(numericRating);

                // Create stars for the rating
                for (let i = 0; i < 5; i++) {
                    const star = document.createElement("span");
                    star.textContent = "★"; // Star character
                    star.style.color = i < rating ? "orange" : "gray"; // Fill stars up to the rating
                    star.style.fontSize = "16px"; // Adjust size as needed
                    ratingContainer.appendChild(star);
                }

                skillItem.appendChild(ratingContainer);
            }

            // Append the skill item to the skill container
            skillContainer.appendChild(skillItem);
        });
    } else {
        // If no skills are available, show a message
        const noSkills = document.createElement("span");
        noSkills.textContent = "No skills required";
        noSkills.style.fontStyle = "italic";
        skillContainer.appendChild(noSkills);
    }

    // Show the modal
    document.getElementById("eventModal").style.display = "flex";
    document.body.classList.add("no-scroll");
}


// Close the modal when the 'x' button is clicked
document.getElementById("closeModal").addEventListener("click", () => {
    document.getElementById("eventModal").style.display = "none"; // Hide the modal

    document.body.classList.remove("no-scroll");
});

// Event listener for tab switching
document.getElementById("ongoingEvents-tab").addEventListener("click", () => {
    currentEvents = ongoingEvents; // Update current events to ongoing events
    updateCalendar(); // Re-render the calendar with updated events
});

document.getElementById("pendingRequest-tab").addEventListener("click", () => {
    currentEvents = pendingEvents; // Update current events to pending events
    updateCalendar(); // Re-render the calendar with updated events
});

document.getElementById("eventHistory-tab").addEventListener("click", () => {
    currentEvents = eventHistory; // Update current events to event history
    updateCalendar(); // Re-render the calendar with updated events
});

// Navigation Buttons: Move to next or previous month/year
document.getElementById("nextMonth").addEventListener("click", () => {
    currentMonth += 1;
    if (currentMonth > 11) { // If the month is greater than 11, move to next year
        currentMonth = 0;
        currentYear += 1;
    }
    updateCalendar();
});

document.getElementById("prevMonth").addEventListener("click", () => {
    currentMonth -= 1;
    if (currentMonth < 0) { // If the month is less than 0, move to previous year
        currentMonth = 11;
        currentYear -= 1;
    }
    updateCalendar();
});

document.getElementById("nextYear").addEventListener("click", () => {
    currentYear += 1; // Move to next year
    updateCalendar();
});

document.getElementById("prevYear").addEventListener("click", () => {
    currentYear -= 1; // Move to previous year
    updateCalendar();
});

// Initial load
updateCalendar();

// Ensure the modal is hidden on page load
document.getElementById("eventModal").style.display = "none";

// Add click event for View Details button in the card
document.querySelectorAll('.card .btn-primary').forEach(button => {
    button.addEventListener('click', function () {
        const eventDetails = JSON.parse(this.getAttribute('data-event').replace(/&quot;/g, '"'));  // Handle escaping issues
        openEventModal(eventDetails); // Open the modal with event details
    });
});
