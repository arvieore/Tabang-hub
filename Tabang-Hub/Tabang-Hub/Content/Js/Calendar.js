let currentMonth = new Date().getMonth();
let currentYear = new Date().getFullYear();

// Initialize with ongoing events as default
let currentEvents = ongoingEvents;

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

    // Add empty cells for the first day of the month
    for (let i = 0; i < firstDay; i++) {
        const emptyCell = document.createElement("div");
        emptyCell.classList.add("day");
        calendar.appendChild(emptyCell);
    }

    // Loop through each day of the month and display events
    for (let date = 1; date <= daysInMonth; date++) {
        const dayCell = document.createElement("div");
        dayCell.classList.add("day");

        const dateKey = `${currentYear}-${String(currentMonth + 1).padStart(2, "0")}-${String(date).padStart(2, "0")}`;

        // Check if there are scheduled events for this day
        let hasScheduledEvent = false;

        currentEvents.forEach(event => {
            if (isDateInRange(dateKey, event.Start_Date, event.End_Date)) {
                hasScheduledEvent = true;
                // Apply color based on tab section
                if (event.Status === 0) {
                    dayCell.classList.add("event-scheduled");
                }
            }
        });

        // Apply background color depending on the active tab
        const activeTab = document.querySelector('.nav-tabs .nav-link.active').id;
        if (hasScheduledEvent) {
            if (activeTab === "ongoingEvents-tab") {
                dayCell.style.backgroundColor = "#d9ffad"; // Green for ongoing events
            } else if (activeTab === "pendingRequest-tab") {
                dayCell.style.backgroundColor = "#ffe4e1"; // Light pink for pending requests
            } else if (activeTab === "eventHistory-tab") {
                dayCell.style.backgroundColor = "#ffd8bd"; // Light orange for event history
            }
        }

        // Highlight today's date
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

// Function to open the modal and populate it with event details
function openEventModal(event) {
    document.getElementById("eventTitle").textContent = event.Title;
    document.getElementById("eventDetails").textContent = event.Details;

    const eventImage = event.Image ? "/Content/Events/" + event.Image : "https://via.placeholder.com/400x200?text=Event+Image"; // Fallback image if no image
    document.getElementById("eventImage").src = eventImage;

    // Show the modal
    document.getElementById("eventModal").style.display = "flex"; // Use "flex" to trigger flex centering
}

// Close the modal when the 'x' button is clicked
document.getElementById("closeModal").addEventListener("click", () => {
    document.getElementById("eventModal").style.display = "none"; // Hide the modal
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
