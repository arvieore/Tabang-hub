import pandas as pd
from sklearn.model_selection import train_test_split
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import classification_report, confusion_matrix
from sklearn.pipeline import Pipeline
import joblib

# Load dataset
data = pd.read_csv("feedback_dataset.csv")  # Replace with your dataset file

# Encode sentiment labels to integers
label_map = {"Positive": 0, "Neutral": 1, "Negative": 2}
data['label'] = data['sentiment'].map(label_map)

# Remove rows with missing or invalid data
data = data.dropna(subset=['feedback', 'label'])  # Ensure no NaN in 'feedback' or 'label'

# Ensure all classes are represented
for label in label_map.values():
    if label not in data['label'].values:
        print(f"Class {label} is missing from the dataset. Adding dummy data.")
        dummy_feedback = "Placeholder feedback"  # Replace with realistic feedback
        dummy_sentiment = [k for k, v in label_map.items() if v == label][0]
        data = pd.concat(
            [data, pd.DataFrame({"feedback": [dummy_feedback], "sentiment": [dummy_sentiment], "label": [label]})],
            ignore_index=True
        )

# Split data into train and test sets
X_train, X_test, y_train, y_test = train_test_split(
    data['feedback'], data['label'], test_size=0.2, random_state=42
)

# Create a TF-IDF vectorizer + Logistic Regression pipeline
model_pipeline = Pipeline([
    ('tfidf', TfidfVectorizer(max_features=5000, ngram_range=(1, 2))),  # TF-IDF with bigrams
    ('logreg', LogisticRegression(max_iter=1000, random_state=42))      # Logistic Regression
])

# Train the model
model_pipeline.fit(X_train, y_train)

# Predict on test set
y_pred = model_pipeline.predict(X_test)

# Evaluate the model
print("Confusion Matrix:")
print(confusion_matrix(y_test, y_pred))
print("\nClassification Report:")
print(classification_report(
    y_test, y_pred,
    target_names=list(label_map.keys()),  # Ensure correct target names
    labels=list(label_map.values())      # Explicitly specify all labels
))

# Save the model
joblib.dump(model_pipeline, "logistic_regression_sentiment_model.pkl")
print("Model saved as 'logistic_regression_sentiment_model.pkl'")
