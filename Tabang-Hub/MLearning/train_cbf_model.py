import pandas as pd
from sklearn.linear_model import LogisticRegression
import joblib

# Load skill similarity data
skill_data = pd.read_csv('skill_data.csv')  # Replace with your actual CSV file for skill matching

# Check the distribution of skillRating
print(skill_data['skillRating'].value_counts())  # Useful for seeing the spread of ratings

# Prepare the data
# Adjust the threshold to ensure there are both 0s and 1s in the target variable 'y'
y = [1 if x >= 3 else 0 for x in skill_data['skillRating']]  # Adjust threshold to split between 1s and 0s
X = skill_data[['skillRating']]  # Skill ratings (features)

# Ensure there are two classes in y (0 and 1)
if len(set(y)) < 2:
    raise ValueError("The dataset needs at least two classes in the target variable 'y'.")

# Train a Logistic Regression model (as a classifier for similarity)
cbf_model = LogisticRegression()
cbf_model.fit(X, y)

# Save the trained model to a file
joblib.dump(cbf_model, 'volunteer_cbf_model.pkl')

print("Content-Based Filtering model trained and saved as 'volunteer_cbf_model.pkl'")
