function handleLogin(event) {
    event.preventDefault();
    // Perform login validation here (this example assumes login is always successful)
    showPopup();
}

function showPopup() {
    document.getElementById('popup').style.display = 'flex';
}

function closePopup() {
    document.getElementById('popup').style.display = 'none';
}
