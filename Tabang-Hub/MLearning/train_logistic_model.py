import pandas as pd
from sklearn.linear_model import LogisticRegression
import joblib

# Load the attendance data
attendance_data = pd.read_csv('attendance_data.csv')  # Replace with your actual CSV file for attendance

# Prepare the data
X = attendance_data[['eventId']]  # Features (event IDs)
y = attendance_data['attended']   # Target (attendance: 1 or 0)

# Train the Logistic Regression model
logistic_model = LogisticRegression()
logistic_model.fit(X, y)

# Save the trained model to a file
joblib.dump(logistic_model, 'volunteer_logistic_model.pkl')

print("Logistic Regression model trained and saved as 'volunteer_logistic_model.pkl'")
