import pandas as pd
from sklearn.ensemble import RandomForestClassifier
from sklearn.preprocessing import LabelEncoder
import joblib

# Load the training data for recruitment
train_data = pd.read_csv('recruit_train_data.csv')

# Convert 'availability' to numerical values using LabelEncoder
label_encoder = LabelEncoder()
train_data['availability'] = label_encoder.fit_transform(train_data['availability'])

# Prepare the features (X) and target (y)
# X: userId, eventId, rating, availability
# y: match (1 or 0)
X = train_data[['userId', 'eventId', 'rating', 'availability']]
y = train_data['match']

# Initialize the Random Forest model for recruitment
rf_recruit_model = RandomForestClassifier(n_estimators=100, random_state=42)

# Train the model
rf_recruit_model.fit(X, y)

# Save the trained model to a file
joblib.dump(rf_recruit_model, 'recruit_rf_model.pkl')

print("Recruitment Random Forest model trained and saved as 'recruit_rf_model.pkl'")
