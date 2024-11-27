import pandas as pd
from sklearn.linear_model import LogisticRegression
import joblib

# Load the training data
train_data = pd.read_csv('train_data.csv')  # Ensure this CSV file matches the provided structure

# Features: userId, eventId, and skillId
X = train_data[['userId', 'eventId', 'skillId']]  # Features (volunteer skill data)
y = train_data['match']   # Target: match (1 for match, 0 for no match)

# Train the Logistic Regression model
model = LogisticRegression(random_state=42, max_iter=1000)
model.fit(X, y)

# Save the trained model to a file
joblib.dump(model, 'volunteer_skill_matching_logistic_model.pkl')

print("Logistic Regression model trained and saved as 'volunteer_skill_matching_logistic_model.pkl'")
