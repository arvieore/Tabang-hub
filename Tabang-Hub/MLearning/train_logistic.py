import pandas as pd
from sklearn.linear_model import LogisticRegression
from sklearn.preprocessing import OneHotEncoder
from sklearn.pipeline import Pipeline
from sklearn.compose import ColumnTransformer
import joblib

# Load the training data
train_data = pd.read_csv('train_data.csv')  # Ensure this CSV file matches the provided structure

# Features: userId, eventId, and skillId
X = train_data[['userId', 'eventId', 'skillId']]  # Features (volunteer skill data)
y = train_data['match']   # Target: match (1 for match, 0 for no match)

# Preprocessing: OneHotEncode categorical features
preprocessor = ColumnTransformer(
    transformers=[
        ('event_skill', OneHotEncoder(handle_unknown='ignore'), ['userId', 'eventId', 'skillId'])
    ]
)

# Logistic Regression pipeline
logistic_pipeline = Pipeline(steps=[
    ('preprocessor', preprocessor),
    ('classifier', LogisticRegression(random_state=42, max_iter=1000))
])

# Train the model
logistic_pipeline.fit(X, y)

# Save the trained model to a file
joblib.dump(logistic_pipeline, 'volunteer_skill_logistic_model.pkl')

print("Logistic Regression model trained and saved as 'volunteer_skill_logistic_model.pkl'")
