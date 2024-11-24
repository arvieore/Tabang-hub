import pandas as pd
from sklearn.ensemble import RandomForestClassifier
from sklearn.model_selection import train_test_split
from sklearn.metrics import accuracy_score
import joblib

# Train the Random Forest model
def train_random_forest():
    try:
        train_data = pd.read_csv('train_data.csv')  # Replace with your actual CSV file
    except FileNotFoundError:
        print("Error: 'train_data.csv' not found. Please check the file path.")
        return

    # Check for missing values
    if train_data.isnull().any().any():
        print("Warning: Missing values detected. Filling missing values with 0.")
        train_data.fillna(0, inplace=True)

    # Prepare the features (X) and target (y)
    X = train_data[['userId', 'eventId', 'skillId']]  # Keep as DataFrame
    y = train_data['match']

    # Split the data into training and testing sets (optional, for validation purposes)
    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)

    # Initialize the Random Forest model
    rf_model = RandomForestClassifier(n_estimators=100, random_state=42)

    # Train the model
    rf_model.fit(X_train, y_train)

    # Evaluate the model
    y_pred = rf_model.predict(X_test)
    accuracy = accuracy_score(y_test, y_pred)
    print(f"Model Accuracy: {accuracy * 100:.2f}%")

    # Save the trained model to a file
    model_path = 'volunteer_rf_model.pkl'
    joblib.dump(rf_model, model_path)
    print(f"Random Forest model trained and saved as '{model_path}'")

if __name__ == "__main__":
    train_random_forest()
