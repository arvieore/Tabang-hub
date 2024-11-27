from flask import Flask, request, jsonify
import pandas as pd
import joblib
import numpy as np
from sklearn.metrics.pairwise import cosine_similarity
from sklearn.preprocessing import StandardScaler  # For scaling features
from sklearn.metrics import classification_report  # For evaluating the model

app = Flask(__name__)

# Load the pre-trained Random Forest model
try:
    logistic_model = joblib.load('volunteer_skill_logistic_model.pkl')
    model_pipeline = joblib.load("logistic_regression_sentiment_model.pkl")
    print("Pretrained model loaded.")
except FileNotFoundError:
    print("No pretrained model found. Please train the model first.")
    exit()

# Helper function to calculate Content-Based Filtering (CBF) similarity using cosine similarity
def calculate_cbf_similarity(user_skills, event_skills):
    all_skills = list(set(user_skills).union(set(event_skills)))
    user_vector = [1 if skill in user_skills else 0 for skill in all_skills]
    event_vector = [1 if skill in event_skills else 0 for skill in all_skills]
    similarity = cosine_similarity([user_vector], [event_vector])[0][0]
    return similarity

# Reusable function: Filter by Availability
def filter_by_availability(user_info_data, availability):
    """Filters users based on their availability."""
    # Check if availability is provided and valid
    if not availability:
        raise ValueError("Availability filter is missing or invalid.")

    # Normalize availability fields for comparison
    user_info_data['availability'] = user_info_data['availability'].str.strip().str.lower()
    availability = availability.strip().lower()
    return user_info_data[user_info_data['availability'] == availability].copy()


@app.route('/filter_rate', methods=['POST'])
def filter_rate():
    try:
        # Parse the incoming data
        data = request.get_json()

        # Convert data to Pandas DataFrames
        user_info_data = pd.DataFrame(data['user_info'], columns=['userId', 'fname', 'lname', 'overallRating', 'feedback', 'availability'])
        user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId'])
        event_skills = set(skill['skillId'] for skill in data['event_skills'])  # Event-required skills

        # Handle missing or null values in 'availability' and 'feedback'
        user_info_data['availability'] = user_info_data['availability'].fillna('Unavailable').str.strip().str.lower()
        user_info_data['feedback'] = user_info_data['feedback'].fillna('No feedback provided')

        # Add sentiment analysis for feedback
        user_info_data['sentiment'] = user_info_data['feedback'].apply(classify_feedback)
        user_info_data['fullName'] = user_info_data['fname'] + " " + user_info_data['lname']

        filtered_volunteers = []

        # Filter volunteers based on skills matching with event skills
        for user_id, group in user_skills_data.groupby('userId'):
            volunteer_skills = set(group['skillId'].tolist())

            # Check if at least one skill matches the event-required skills
            similarity_score = calculate_partial_cbf_similarity(volunteer_skills, event_skills)
            if similarity_score > 0:  # Include only if at least one skill matches
                volunteer_info = user_info_data[user_info_data['userId'] == user_id]
                if not volunteer_info.empty:
                    filtered_volunteers.append({
                        'userId': int(user_id),
                        'fullName': volunteer_info.iloc[0]['fullName'],
                        'overallRating': float(volunteer_info.iloc[0]['overallRating']),
                        'feedback': volunteer_info.iloc[0]['feedback'],
                        'sentiment': volunteer_info.iloc[0]['sentiment'],
                        'availability': volunteer_info.iloc[0]['availability'],
                        'similarityScore': similarity_score
                    })

        # Sort by rating (descending) and return results
        filtered_volunteers = sorted(filtered_volunteers, key=lambda x: -x['overallRating'])

        print('Filtered by rating and event skills: ', filtered_volunteers)
        return jsonify({"success": True, "volunteers": filtered_volunteers})
    except KeyError as ke:
        print(f"KeyError in /filter_rate: {ke}")
        return jsonify({"success": False, "message": f"Missing key: {ke}"}), 400
    except ValueError as ve:
        print(f"ValueError in /filter_rate: {ve}")
        return jsonify({"success": False, "message": f"Invalid value: {ve}"}), 400
    except Exception as e:
        print("Error in /filter_rate:", e)
        return jsonify({"success": False, "message": str(e)}), 500



@app.route('/filter_skills_and_ratings', methods=['POST'])
def filter_skills_and_ratings():
    try:
        data = request.get_json()

        # Convert data to DataFrames
        user_info_data = pd.DataFrame(data['user_info'], columns=['userId', 'fname', 'lname', 'overallRating', 'feedback', 'availability'])
        user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId'])
        selected_skills = set(data['event_skills'])  # Required skills

        # Handle missing or null values
        user_info_data['feedback'] = user_info_data['feedback'].fillna("No feedback provided")  # Default feedback
        user_info_data['availability'] = user_info_data['availability'].fillna("Unavailable")  # Default availability

        filtered_volunteers = []

        # Filter volunteers based on skills
        for user_id, group in user_skills_data.groupby('userId'):
            volunteer_skills = set(group['skillId'].tolist())
            if selected_skills.issubset(volunteer_skills):  # Check if all required skills are present
                volunteer_info = user_info_data[user_info_data['userId'] == user_id]
                if not volunteer_info.empty:
                    full_name = f"{volunteer_info.iloc[0]['fname']} {volunteer_info.iloc[0]['lname']}"
                    overall_rating = float(volunteer_info.iloc[0]['overallRating']) or 0.0  # Convert to float
                    feedback = volunteer_info.iloc[0]['feedback']  # Retrieve the feedback
                    availability = volunteer_info.iloc[0]['availability'] if pd.notna(volunteer_info.iloc[0]['availability']) else "Unavailable"
                    sentiment = classify_feedback(feedback)  # Predict sentiment
                    similarity_score = calculate_cbf_similarity(volunteer_skills, selected_skills)

                    filtered_volunteers.append({
                        'userId': int(user_id),  # Convert to int
                        'fullName': full_name,
                        'overallRating': overall_rating,  # Already a float
                        'feedback': feedback,  # Include feedback
                        'availability': availability,
                        'sentiment': sentiment,  # Include sentiment
                        'similarityScore': similarity_score  # Already a float
                    })

        # Sort by rating and similarity score
        filtered_volunteers = sorted(filtered_volunteers, key=lambda x: (-x['overallRating'], -x['similarityScore']))
        print('Filtered Skill and Ratings: ', filtered_volunteers)

        return jsonify(filtered_volunteers)

    except Exception as e:
        print("Error in /filter_skills_and_ratings:", e)
        return jsonify({"success": False, "message": str(e)}), 500


# Existing Route: Predict for Events
# @app.route('/predict', methods=['POST'])
# def predict_for_user():
#     data = request.get_json()

#     # Convert received data to Pandas DataFrames
#     user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId'])
#     event_data = pd.DataFrame(data['event_data'], columns=['eventId', 'eventDescription'])
#     event_skills_data = pd.DataFrame(data['event_skills'], columns=['eventId', 'skillId'])

#     filtered_events = []
#     volunteer_skills = set(user_skills_data['skillId'].tolist())
#     user_id = user_skills_data['userId'].iloc[0]  # Use the actual userId passed in the input

#     for _, event_row in event_data.iterrows():
#         event_id = event_row['eventId']
#         required_skills = set(event_skills_data[event_skills_data['eventId'] == event_id]['skillId'].tolist())

#         # Debugging: Print volunteer and event skills
#         print(f"Volunteer Skills: {volunteer_skills}")
#         print(f"Event {event_id} Required Skills: {required_skills}")

#         # Recommend only if all required skills are in the volunteer's skills
#         if required_skills.issubset(volunteer_skills):
#             # Calculate similarity score (for ranking purposes)
#             similarity_score = calculate_cbf_similarity(volunteer_skills, required_skills)
            
#             # Add event to the recommendations
#             filtered_events.append({
#                 'eventId': event_id,
#                 'eventDescription': event_row['eventDescription'],
#                 'requiredSkills': list(required_skills),
#                 'similarityScore': similarity_score
#             })

#     # Sort events by similarity score in descending order
#     filtered_events = sorted(filtered_events, key=lambda x: x['similarityScore'], reverse=True)

#     # Debugging: Print the filtered events
#     print('Filtered Events: ', filtered_events)
#     return jsonify(filtered_events)
@app.route('/predict', methods=['POST'])
def predict_hybrid():
    try:
        data = request.get_json()

        # Convert received data to DataFrames
        user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId'])
        event_data = pd.DataFrame(data['event_data'], columns=['eventId', 'eventDescription'])
        event_skills_data = pd.DataFrame(data['event_skills'], columns=['eventId', 'skillId'])

        filtered_events = []
        volunteer_skills = set(user_skills_data['skillId'].tolist())
        user_id = user_skills_data['userId'].iloc[0]  # Use the actual userId passed in the input

        for _, event_row in event_data.iterrows():
            event_id = event_row['eventId']
            required_skills = set(event_skills_data[event_skills_data['eventId'] == event_id]['skillId'].tolist())

            # Step 1: CBF Filtering
            if required_skills.issubset(volunteer_skills):
                # Step 2: Logistic Regression Prediction
                logistic_input = [[user_id, event_id, skill] for skill in required_skills]
                logistic_input_df = pd.DataFrame(logistic_input, columns=['userId', 'eventId', 'skillId'])
                
                # Predict probabilities
                probabilities = logistic_model.predict_proba(logistic_input_df)
                predictions = (probabilities[:, 1] >= 0.5).astype(int)  # Adjust threshold as needed

                # Include event if all predictions are positive
                if all(predictions):
                    similarity_score = calculate_cbf_similarity(volunteer_skills, required_skills)
                    filtered_events.append({
                        'eventId': event_id,
                        'eventDescription': event_row['eventDescription'],
                        'requiredSkills': list(required_skills),
                        'similarityScore': similarity_score
                    })

        # Return the filtered events directly as a JSON array
        return jsonify(filtered_events)

    except Exception as e:
        print("Error in /predict:", e)
        return jsonify({"success": False, "message": str(e)}), 500



# Route: Recruitment Filtering based on Skill Match and Volunteer Availability (CBF only)
@app.route('/recruitSortVolunteer', methods=['POST'])
def recruit_for_event_volunteer():
    data = request.get_json()

    # Convert received data to Pandas DataFrames
    user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId', 'rating'])
    event_data = pd.DataFrame(data['event_data'], columns=['eventId', 'eventDescription'])
    event_skills_data = pd.DataFrame(data['event_skills'], columns=['eventId', 'skillId'])
    volunteer_info_data = pd.DataFrame(data['user_info'], columns=['userId', 'availability'])

    filtered_volunteers = []

    # Step 1: Get required skills for the event
    event_id = event_data['eventId'].values[0]
    required_skills = set(event_skills_data[event_skills_data['eventId'] == event_id]['skillId'].tolist())

    # Step 2: Filter volunteers based on skill match
    for user_id, group in user_skills_data.groupby('userId'):
        volunteer_skills = set(group['skillId'].tolist())
        matched_ratings = group[group['skillId'].isin(required_skills)]['rating'].tolist()

        # Ensure all required skills are in the volunteer's skills
        if required_skills.issubset(volunteer_skills):
            # Calculate CBF similarity
            similarity_score = calculate_cbf_similarity(volunteer_skills, required_skills)
            availability = volunteer_info_data[volunteer_info_data['userId'] == user_id]['availability'].values[0]
            avg_rating = np.mean(matched_ratings) if matched_ratings else 0

            filtered_volunteers.append({
                'userId': user_id,
                'matchedSkills': list(volunteer_skills),
                'rating': avg_rating,
                'similarityScore': similarity_score,
                'availability': availability
            })

    # Step 3: Sort volunteers by rating and similarity score
    ranked_volunteers_by_rating = sorted(
        filtered_volunteers, key=lambda x: (-x['rating'], -x['similarityScore'])
    )

    # Step 4: Sort volunteers by availability (prioritizing Full Time) and then by rating
    availability_order = {'Full Time': 0, 'Part Time': 1}  # Order for sorting availability
    ranked_volunteers_by_availability = sorted(
        filtered_volunteers, key=lambda x: (availability_order.get(x['availability'], 2), -x['rating'], -x['similarityScore'])
    )

    # Return the two sorted datasets
    return jsonify({
        'sortedByRating': ranked_volunteers_by_rating,
        'sortedByAvailability': ranked_volunteers_by_availability
    })


# Existing Route: Recruit for Event
@app.route('/recruit', methods=['POST'])
def recruit_for_event():
    try:
        data = request.get_json()

        # Convert received data to Pandas DataFrames
        user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId'])
        event_skills_data = pd.DataFrame(data['event_skills'], columns=['eventId', 'skillId'])
        user_info_data = pd.DataFrame(data['user_info'], columns=['userId', 'fname', 'lname', 'overallRating', 'feedback', 'availability'])

        # Handle missing or None values in 'feedback' and 'availability'
        user_info_data['feedback'] = user_info_data['feedback'].fillna("No feedback provided")
        user_info_data['availability'] = user_info_data['availability'].fillna("Unavailable")

        filtered_volunteers = []

        # Get the required skills for the event
        selected_skills = set(event_skills_data['skillId'].tolist())
        print(f"Selected Skills for Event: {selected_skills}")

        # Filter volunteers based on subset condition
        for user_id, group in user_skills_data.groupby('userId'):
            volunteer_skills = set(group['skillId'].tolist())
            print(f"Volunteer {user_id} Skills: {volunteer_skills}")

            # Check if the selected skills are a subset of the volunteer's skills
            if selected_skills.issubset(volunteer_skills):
                # Calculate similarity score for ranking purposes
                similarity_score = calculate_cbf_similarity(volunteer_skills, selected_skills)

                # Get the volunteer's details
                user_info = user_info_data[user_info_data['userId'] == user_id]
                if not user_info.empty:
                    full_name = f"{user_info.iloc[0]['fname']} {user_info.iloc[0]['lname']}"
                    overall_rating = int(user_info.iloc[0]['overallRating']) if not pd.isna(user_info.iloc[0]['overallRating']) else 0
                    feedback = user_info.iloc[0]['feedback']
                    availability = user_info.iloc[0]['availability']
                    sentiment = classify_feedback(feedback)  # Predict sentiment
                else:
                    full_name = f"Volunteer {user_id}"
                    overall_rating = 0
                    feedback = "No feedback available"
                    availability = "N/A"
                    sentiment = "Neutral"  # Default sentiment if no feedback available

                # Add the volunteer to the filtered list
                filtered_volunteers.append({
                    'userId': int(user_id),
                    'fullName': full_name,
                    'overallRating': overall_rating,
                    'skillIds': list(volunteer_skills),  # Include all skills of the volunteer
                    'similarityScore': similarity_score,
                    'feedback': feedback,  # Include feedback
                    'availability': availability,
                    'sentiment': sentiment,  # Include sentiment
                })

        print("Filtered Volunteers: ", filtered_volunteers)
        return jsonify(filtered_volunteers)

    except Exception as e:
        print("Error in /recruit:", e)
        return jsonify({"success": False, "message": str(e)}), 500



# Map sentiment labels back to their string values
sentiment_map = {0: "Positive", 1: "Neutral", 2: "Negative"}

# Define status mapping as a global constant
STATUS_MAPPING = {
    0: "Pending",
    1: "Available",
    2: "Invited"
}


# Function to classify feedback
def classify_feedback(feedback_text):
    prediction = model_pipeline.predict([feedback_text])[0]
    return sentiment_map[prediction]


# Helper function to calculate Content-Based Filtering (CBF) similarity allowing partial matches
def calculate_partial_cbf_similarity(user_skills, event_skills):
    all_skills = list(set(user_skills).union(set(event_skills)))
    user_vector = [1 if skill in user_skills else 0 for skill in all_skills]
    event_vector = [1 if skill in event_skills else 0 for skill in all_skills]
    similarity = cosine_similarity([user_vector], [event_vector])[0][0]
    
    # Check if there is at least one matching skill
    has_match = any(skill in user_skills for skill in event_skills)
    return similarity if has_match else 0  # Return 0 if no skills match


@app.route('/classify_users_feedback', methods=['POST'])
def classify_users_feedback():
    try:
        # Parse JSON data from the request
        data = request.get_json()

        # Extract user information
        user_info_data = pd.DataFrame(data['user_info'], columns=['userId', 'fname', 'lname', 'overallRating', 'feedback', 'availability'])
        user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId'])
        event_skills = set([skill['skillId'] for skill in data['event_skills']])

        # Handle missing or None values in 'feedback' and 'availability'
        user_info_data['feedback'] = user_info_data['feedback'].fillna("No feedback provided")
        user_info_data['availability'] = user_info_data['availability'].fillna("Unavailable").str.lower()

        # Classify feedback for each user
        user_info_data['sentiment'] = user_info_data['feedback'].apply(classify_feedback)

        # Create FullName column
        user_info_data['FullName'] = user_info_data['fname'] + " " + user_info_data['lname']

        # Perform content-based filtering
        filtered_users = []
        for user_id, group in user_skills_data.groupby('userId'):
            volunteer_skills = set(group['skillId'].tolist())

            # Check if at least one skill matches the event-required skills
            similarity_score = calculate_partial_cbf_similarity(volunteer_skills, event_skills)
            if similarity_score > 0:  # Only include users with at least one matching skill
                volunteer_info = user_info_data[user_info_data['userId'] == user_id]
                if not volunteer_info.empty:
                    filtered_users.append({
                        'userId': int(user_id),
                        'FullName': f"{volunteer_info.iloc[0]['fname']} {volunteer_info.iloc[0]['lname']}",
                        'overallRating': float(volunteer_info.iloc[0]['overallRating']),
                        'feedback': volunteer_info.iloc[0]['feedback'],
                        'sentiment': volunteer_info.iloc[0]['sentiment'],
                        'availability': volunteer_info.iloc[0]['availability'],
                        'similarityScore': similarity_score
                    })

        print(filtered_users)
        return jsonify({"status": "success", "classified_feedback": filtered_users})

    except Exception as e:
        print("Error:", e)
        return jsonify({"status": "error", "message": str(e)}), 500
    

@app.route('/filter_by_availability', methods=['POST'])
def filter_by_availability_route():
    try:
        data = request.get_json()

        # Validate the sortBy parameter
        sort_by_availability = data.get('sortBy', '').strip().lower() if data.get('sortBy') else None
        if not sort_by_availability:
            raise ValueError("Missing or invalid 'sortBy' parameter in the request.")

        # Extract user information and skills
        user_info_data = pd.DataFrame(data['user_info'], columns=['userId', 'fname', 'lname', 'overallRating', 'feedback', 'availability'])
        user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId'])
        
        # Extract only the skill IDs from event_skills_required
        event_skills = set(skill['skillId'] for skill in data['event_skills_required'])  # Extract skillId values

        # Handle missing values in 'availability' and 'feedback'
        user_info_data['availability'] = user_info_data['availability'].fillna("Unavailable").str.strip().str.lower()
        user_info_data['feedback'] = user_info_data['feedback'].fillna("No feedback provided")

        # Filter users by availability
        filtered_users = user_info_data[user_info_data['availability'] == sort_by_availability].copy()

        # Perform skill matching
        filtered_users_with_skills = []
        for user_id, group in user_skills_data.groupby('userId'):
            volunteer_skills = set(group['skillId'].tolist())

            # Check if at least one skill matches the event-required skills
            if volunteer_skills & event_skills:  # Intersection to check for matching skills
                volunteer_info = filtered_users[filtered_users['userId'] == user_id]
                if not volunteer_info.empty:
                    filtered_users_with_skills.append({
                        'userId': int(user_id),
                        'fullName': f"{volunteer_info.iloc[0]['fname']} {volunteer_info.iloc[0]['lname']}",
                        'overallRating': float(volunteer_info.iloc[0]['overallRating']),
                        'feedback': volunteer_info.iloc[0]['feedback'],
                        'sentiment': classify_feedback(volunteer_info.iloc[0]['feedback']),
                        'availability': volunteer_info.iloc[0]['availability']
                    })

        # Convert to JSON response
        print("Filtered Users with Skills: ", filtered_users_with_skills)
        return jsonify({"success": True, "volunteers": filtered_users_with_skills})

    except ValueError as ve:
        print("Validation Error:", ve)
        return jsonify({"success": False, "message": str(ve)}), 400
    except Exception as e:
        print("Error in /filter_by_availability:", e)
        return jsonify({"success": False, "message": str(e)}), 500



    
@app.route('/filter_by_availability_with_skills', methods=['POST'])
def filter_by_availability_with_skills():
    try:
        # Parse the JSON payload
        data = request.get_json()

        # Validate the required keys
        if not data or 'user_info' not in data or 'user_skills' not in data or 'event_skills' not in data or 'sortBy' not in data:
            raise ValueError("Missing one or more required fields in the request payload.")

        # Convert input data to Pandas DataFrames
        user_info_data = pd.DataFrame(data['user_info'], columns=['userId', 'fname', 'lname', 'overallRating', 'feedback', 'availability'])
        user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId'])
        event_skills = set(data['event_skills'])  # Required skills
        sort_by_availability = data.get('sortBy', '').strip().lower()

        # Handle missing or None values in 'availability' and normalize the availability field for filtering
        user_info_data['availability'] = user_info_data['availability'].fillna("Unavailable").str.strip().str.lower()

        # Step 1: Filter by availability
        filtered_users_by_availability = user_info_data[user_info_data['availability'] == sort_by_availability]

        # If no users match the availability, return an empty list
        if filtered_users_by_availability.empty:
            return jsonify({"success": True, "volunteers": []})

        # Step 2: Perform sentiment analysis on feedback
        filtered_users_by_availability['feedback'] = filtered_users_by_availability['feedback'].fillna("No feedback provided")
        filtered_users_by_availability['sentiment'] = filtered_users_by_availability['feedback'].apply(classify_feedback)

        # Step 3: Filter by skills using Content-Based Filtering
        filtered_with_skills = []
        for user_id, group in user_skills_data.groupby('userId'):
            volunteer_skills = set(group['skillId'].tolist())

            # Check if the volunteer has all required skills
            if event_skills.issubset(volunteer_skills):
                volunteer_info = filtered_users_by_availability[filtered_users_by_availability['userId'] == user_id]
                if not volunteer_info.empty:
                    # Calculate similarity score using CBF
                    similarity_score = calculate_cbf_similarity(volunteer_skills, event_skills)

                    # Add the volunteer details
                    filtered_with_skills.append({
                        'userId': int(user_id),
                        'fullName': f"{volunteer_info.iloc[0]['fname']} {volunteer_info.iloc[0]['lname']}",
                        'availability': volunteer_info.iloc[0]['availability'],
                        'overallRating': float(volunteer_info.iloc[0]['overallRating']),
                        'feedback': volunteer_info.iloc[0]['feedback'],
                        'sentiment': volunteer_info.iloc[0]['sentiment'],
                        'similarityScore': similarity_score
                    })

        # Step 4: Sort the filtered volunteers by similarity score (descending)
        filtered_with_skills = sorted(filtered_with_skills, key=lambda x: -x['similarityScore'])

        # Return the filtered and sorted volunteers
        return jsonify({
            "success": True,
            "volunteers": filtered_with_skills
        })

    except ValueError as ve:
        print("Validation Error:", ve)
        return jsonify({"success": False, "message": str(ve)}), 400
    except Exception as e:
        print("Error in /filter_by_availability_with_skills:", e)
        return jsonify({"success": False, "message": str(e)}), 500



@app.route('/filter_by_ratings_with_availability', methods=['POST'])
def filter_by_ratings_with_availability():
    try:
        data = request.get_json()

        # Debugging: Print received data
        print("Received data:", data)

        # Extract and validate the 'sortBy' parameter
        sort_by_availability = data.get('sortBy', '').strip().lower() if data.get('sortBy') else None
        if not sort_by_availability:
            raise ValueError("'sortBy' parameter is required.")

        # Convert input data to DataFrames
        user_info_data = pd.DataFrame(data['user_info'], columns=['userId', 'fname', 'lname', 'overallRating', 'feedback', 'availability'])
        user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId'])
        event_skills_required = set(skill['skillId'] for skill in data['event_skills_required'])  # Event-required skills

        # Handle missing or null values in 'availability' and 'feedback'
        user_info_data['availability'] = user_info_data['availability'].fillna('Unavailable').str.strip().str.lower()
        user_info_data['feedback'] = user_info_data['feedback'].fillna('No feedback provided')

        # Step 1: Filter by availability
        filtered_users = user_info_data[user_info_data['availability'] == sort_by_availability].copy()

        # If no users match the availability filter, return an empty list
        if filtered_users.empty:
            return jsonify({
                "success": True,
                "volunteers": []
            })

        # Step 2: Perform sentiment analysis on feedback
        filtered_users.loc[:, 'sentiment'] = filtered_users['feedback'].apply(classify_feedback)

        # Add a 'FullName' column for convenience
        filtered_users.loc[:, 'FullName'] = filtered_users['fname'] + ' ' + filtered_users['lname']

        # Step 3: Match skills with event-required skills
        matched_volunteers = []
        for user_id, group in user_skills_data.groupby('userId'):
            volunteer_skills = set(group['skillId'].tolist())

            # Check if at least one skill matches the event-required skills
            has_matching_skills = any(skill in event_skills_required for skill in volunteer_skills)
            if not has_matching_skills:
                continue

            # Retrieve the user information
            user_info = filtered_users[filtered_users['userId'] == user_id]
            if user_info.empty:
                continue

            # Add volunteer to the matched list
            matched_volunteers.append({
                'userId': int(user_id),
                'FullName': f"{user_info.iloc[0]['fname']} {user_info.iloc[0]['lname']}",
                'overallRating': float(user_info.iloc[0]['overallRating']),
                'feedback': user_info.iloc[0]['feedback'],
                'sentiment': user_info.iloc[0]['sentiment'],
                'availability': user_info.iloc[0]['availability'],
                'matchedSkills': list(volunteer_skills & event_skills_required)  # Skills that matched
            })

        # Step 4: Sort by overallRating in descending order
        matched_volunteers = sorted(matched_volunteers, key=lambda x: -x['overallRating'])

        return jsonify({"success": True, "volunteers": matched_volunteers})

    except ValueError as ve:
        print("Validation Error:", ve)
        return jsonify({"success": False, "message": str(ve)}), 400
    except Exception as e:
        print("Error in /filter_by_ratings_with_availability:", e)
        return jsonify({"success": False, "message": str(e)}), 500


@app.route('/filter_by_rate_availability_skills', methods=['POST'])
def filter_by_rate_availability_and_skills():
    try:
        data = request.get_json()

        # Convert received JSON into DataFrames
        user_info_data = pd.DataFrame(data['user_info'])
        user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId'])
        selected_event_skills = set(data['event_skills'])  # Skills selected by the organizer
        required_event_skills = set(skill['skillId'] for skill in data['event_skills_required'])  # All required skills
        sort_by_availability = data.get('sortBy', '').strip().lower()

        # Handle missing or null values
        user_info_data['availability'] = user_info_data['availability'].fillna('Unavailable').str.strip().str.lower()
        user_info_data['feedback'] = user_info_data['feedback'].fillna('No feedback provided')

        # Step 1: Filter by Availability
        filtered_users_by_availability = user_info_data[user_info_data['availability'] == sort_by_availability]

        # If no users match the availability filter, return an empty list
        if filtered_users_by_availability.empty:
            return jsonify({"success": True, "volunteers": []})

        # Step 2: Filter volunteers matching at least one required skill (Partial CBF)
        partially_matched_volunteers = []
        for user_id, group in user_skills_data.groupby('userId'):
            volunteer_skills = set(group['skillId'].tolist())

            # Check if the volunteer has at least one required skill
            if not volunteer_skills & required_event_skills:
                continue  # Skip if no required skills match

            volunteer_info = filtered_users_by_availability[filtered_users_by_availability['userId'] == user_id]
            if not volunteer_info.empty:
                partially_matched_volunteers.append({
                    'userId': user_id,
                    'volunteerSkills': volunteer_skills,
                    'volunteerInfo': volunteer_info.iloc[0].to_dict()
                })

        # Step 3: Further filter by selected skills
        fully_matched_volunteers = []
        for volunteer in partially_matched_volunteers:
            user_id = volunteer['userId']
            volunteer_skills = volunteer['volunteerSkills']

            # Check if the volunteer has all the selected skills
            if selected_event_skills.issubset(volunteer_skills):
                fully_matched_volunteers.append({
                    'userId': user_id,
                    'fullName': f"{volunteer['volunteerInfo']['fname']} {volunteer['volunteerInfo']['lname']}",
                    'availability': volunteer['volunteerInfo']['availability'].title(),
                    'overallRating': float(volunteer['volunteerInfo']['overallRating']),
                    'feedback': volunteer['volunteerInfo']['feedback'],
                    'sentiment': classify_feedback(volunteer['volunteerInfo']['feedback']),
                })

        # Step 4: Sort by overallRating in descending order
        fully_matched_volunteers = sorted(
            fully_matched_volunteers,
            key=lambda x: -x['overallRating']
        )

        return jsonify({"success": True, "volunteers": fully_matched_volunteers})

    except Exception as e:
        print("Error in /filter_by_rate_availability_skills:", e)
        return jsonify({"success": False, "message": str(e)}), 500




if __name__ == '__main__':
    app.run(debug=True, port=5000)
