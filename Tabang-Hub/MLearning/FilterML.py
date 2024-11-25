from flask import Flask, request, jsonify
import pandas as pd
import joblib
import numpy as np
from sklearn.metrics.pairwise import cosine_similarity

app = Flask(__name__)

# Load the pre-trained Random Forest model
try:
    rf_model = joblib.load('volunteer_skill_matching_model.pkl')
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


# Route: FilterRate (Filter volunteers by ratings)
@app.route('/filter_rate', methods=['POST'])
def filter_rate():
    data = request.get_json()

    # Convert data to Pandas DataFrames
    user_info_data = pd.DataFrame(data['user_info'], columns=['userId', 'fname', 'lname', 'overallRating', 'feedback', 'availability'])
    user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId'])

    # Create a fullName column
    user_info_data['fullName'] = user_info_data['fname'] + ' ' + user_info_data['lname']

    # Filter and sort volunteers by rating
    filtered_volunteers = user_info_data.sort_values(by='overallRating', ascending=False).to_dict(orient='records')

    # Add similarity score using Content-Based Filtering
    for volunteer in filtered_volunteers:
        volunteer_id = volunteer['userId']
        volunteer_skills = user_skills_data[user_skills_data['userId'] == volunteer_id]['skillId'].tolist()
        similarity_score = calculate_cbf_similarity(volunteer_skills, [])
        volunteer['similarityScore'] = similarity_score

    print('Filtered by rating: ', filtered_volunteers)
    return jsonify(filtered_volunteers)


# Route: FilterBySkillsAndRatings (Filter volunteers by skills and ratings)
@app.route('/filter_skills_and_ratings', methods=['POST'])
def filter_skills_and_ratings():
    data = request.get_json()

    # Convert data to DataFrames
    user_info_data = pd.DataFrame(data['user_info'], columns=['userId', 'fname', 'lname', 'overallRating', 'feedback', 'availability'])
    user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId'])
    selected_skills = set(data['event_skills'])  # Required skills

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

# Existing Route: Predict for Events
@app.route('/predict', methods=['POST'])
def predict_for_user():
    data = request.get_json()

    # Convert received data to Pandas DataFrames
    user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId'])
    event_data = pd.DataFrame(data['event_data'], columns=['eventId', 'eventDescription'])
    event_skills_data = pd.DataFrame(data['event_skills'], columns=['eventId', 'skillId'])

    filtered_events = []
    volunteer_skills = set(user_skills_data['skillId'].tolist())
    user_id = user_skills_data['userId'].iloc[0]  # Use the actual userId passed in the input

    for _, event_row in event_data.iterrows():
        event_id = event_row['eventId']
        required_skills = set(event_skills_data[event_skills_data['eventId'] == event_id]['skillId'].tolist())

        # Ensure all required event skills are present in the volunteer's skills
        if required_skills.issubset(volunteer_skills):
            # Step 1: Random Forest Prediction
            rf_input = [[1, event_id, skill] for skill in required_skills]  # Use actual userId from input
            rf_input_df = pd.DataFrame(rf_input, columns=['userId', 'eventId', 'skillId'])  # Match training format
            rf_predictions = rf_model.predict(rf_input_df)  # Predict for all required skills

            if all(rf_predictions):  # Include only if RF predicts positively for all required skills
                # Step 2: Calculate CBF similarity
                similarity_score = calculate_cbf_similarity(volunteer_skills, required_skills)
                
                # Step 3: Combine Results
                if similarity_score >= 0.5:  # Ensure similarity threshold is met
                    filtered_events.append({
                        'eventId': event_id,
                        'eventDescription': event_row['eventDescription'],
                        'requiredSkills': list(required_skills),
                        'similarityScore': similarity_score
                    })

    # Sort the events by similarity score in descending order
    filtered_events = sorted(filtered_events, key=lambda x: x['similarityScore'], reverse=True)
    print('Filtered Events: ', filtered_events)
    return jsonify(filtered_events)

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
    data = request.get_json()

    # Convert received data to Pandas DataFrames
    user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId'])
    event_skills_data = pd.DataFrame(data['event_skills'], columns=['eventId', 'skillId'])
    user_info_data = pd.DataFrame(data['user_info'], columns=['userId', 'fname', 'lname', 'overallRating', 'feedback', 'availability'])

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


# Map sentiment labels back to their string values
sentiment_map = {0: "Positive", 1: "Neutral", 2: "Negative"}

# Function to classify feedback
def classify_feedback(feedback_text):
    prediction = model_pipeline.predict([feedback_text])[0]
    return sentiment_map[prediction]

# Route to classify feedback for multiple users
@app.route('/classify_users_feedback', methods=['POST'])
def classify_users_feedback():
    try:
        # Parse JSON data from the request
        data = request.get_json()

        # Extract user information
        user_info_data = pd.DataFrame(data['user_info'], columns=['userId', 'fname', 'lname', 'overallRating', 'feedback', 'availability'])

        # Classify feedback for each user
        user_info_data['sentiment'] = user_info_data['feedback'].apply(classify_feedback)

        # Create FullName column
        user_info_data['FullName'] = user_info_data['fname'] + " " + user_info_data['lname']

        # Reorder columns for better clarity
        user_info_data = user_info_data[['userId', 'FullName', 'overallRating', 'feedback', 'availability', 'sentiment']]

        # Print for debugging
        print("Classified Feedback Data:")
        print(user_info_data)

        # Convert DataFrame to JSON and return
        result = user_info_data.to_dict(orient='records')
        return jsonify({"status": "success", "classified_feedback": result})

    except Exception as e:
        print("Error:", e)
        return jsonify({"status": "error", "message": str(e)}), 500



@app.route('/filter_by_availability', methods=['POST'])
def filter_by_availability_route():
    try:
        data = request.get_json()

        # Validate the sortBy parameter
        sort_by_availability = data.get('sortBy', '').strip() if data.get('sortBy') else None
        if not sort_by_availability:
            raise ValueError("Missing or invalid 'sortBy' parameter in the request.")

        # Extract user information
        user_info_data = pd.DataFrame(data['user_info'])

        # Filter users by availability
        filtered_users = filter_by_availability(user_info_data, sort_by_availability)

        # Add a fullName column for convenience
        filtered_users['FullName'] = filtered_users['fname'] + ' ' + filtered_users['lname']

        # Convert filtered users to dictionary
        filtered_users_dict = filtered_users.to_dict(orient='records')
        print("dict: ", filtered_users_dict)
        return jsonify({
            "success": True,
            "volunteers": filtered_users_dict
        })
    except ValueError as ve:
        print("Validation Error:", ve)
        return jsonify({"success": False, "message": str(ve)}), 400
    except Exception as e:
        print("Error in /filter_by_availability:", e)
        return jsonify({"success": False, "message": str(e)}), 500

    
    
# Route: Filter by Availability with Skills
@app.route('/filter_by_availability_with_skills', methods=['POST'])
def filter_by_availability_with_skills():
    try:
        data = request.get_json()

        # Convert data to DataFrames
        user_info_data = pd.DataFrame(data['user_info'])
        user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId'])
        event_skills = set(data['event_skills'])  # Required skills
        sort_by_availability = data.get('sortBy', '').strip().lower()  # Normalize for consistency

        # Filter users by availability
        user_info_data['availability'] = user_info_data['availability'].str.strip().str.lower()
        filtered_users_by_availability = user_info_data[user_info_data['availability'] == sort_by_availability]

        # Check if there are users matching the availability
        if filtered_users_by_availability.empty:
            return jsonify({
                "success": True,
                "volunteers": []
            })

        # Filter users by skills using CBF
        filtered_with_skills = []
        for user_id, group in user_skills_data.groupby('userId'):
            volunteer_skills = set(group['skillId'].tolist())

            # Check if the volunteer has all required skills
            if event_skills.issubset(volunteer_skills):
                volunteer_info = filtered_users_by_availability[filtered_users_by_availability['userId'] == user_id]
                if not volunteer_info.empty:
                    similarity_score = calculate_cbf_similarity(volunteer_skills, event_skills)
                    filtered_with_skills.append({
                        'userId': int(user_id),
                        'fullName': f"{volunteer_info.iloc[0]['fname']} {volunteer_info.iloc[0]['lname']}",
                        'availability': volunteer_info.iloc[0]['availability'],
                        'similarityScore': similarity_score
                    })

        # Sort filtered volunteers by similarity score (descending)
        filtered_with_skills = sorted(filtered_with_skills, key=lambda x: x['similarityScore'], reverse=True)

        # Return the filtered volunteers
        return jsonify({
            "success": True,
            "volunteers": filtered_with_skills
        })

    except Exception as e:
        print("Error in /filter_by_availability_with_skills:", e)
        return jsonify({"success": False, "message": str(e)}), 500



# Route: Filter by Ratings with Availability
@app.route('/filter_by_ratings_with_availability', methods=['POST'])
def filter_by_ratings_with_availability():
    try:
        data = request.get_json()

        # Use default value for sortBy if missing
        sort_by_availability = data.get('sortBy', 'Full Time').strip().lower()

        # Extract user information
        user_info_data = pd.DataFrame(data['user_info'])

        # Filter users by availability
        filtered_users = filter_by_availability(user_info_data, sort_by_availability)

        # Sort users by rating
        filtered_users = filtered_users.sort_values(by='overallRating', ascending=False).to_dict(orient='records')

        return jsonify({
            "success": True,
            "volunteers": filtered_users
        })
    except Exception as e:
        print("Error in /filter_by_ratings_with_availability:", e)
        return jsonify({"success": False, "message": str(e)}), 500



# Route: Filter by Rate, Availability, and Skills
@app.route('/filter_by_rate_availability_skills', methods=['POST'])
def filter_by_rate_availability_and_skills():
    try:
        data = request.get_json()

        # Convert data to DataFrames
        user_info_data = pd.DataFrame(data['user_info'])
        user_skills_data = pd.DataFrame(data['user_skills'], columns=['userId', 'skillId'])
        event_skills = set(data['event_skills'])  # Required skills
        sort_by_availability = data.get('sortBy', '').strip()

        # Filter users by availability
        filtered_users = filter_by_availability(user_info_data, sort_by_availability)

        # Filter users by skills using CBF and sort by rating
        filtered_with_skills = []
        for user_id, group in user_skills_data.groupby('userId'):
            volunteer_skills = set(group['skillId'].tolist())
            if event_skills.issubset(volunteer_skills):
                volunteer_info = filtered_users[filtered_users['userId'] == user_id]
                if not volunteer_info.empty:
                    similarity_score = calculate_cbf_similarity(volunteer_skills, event_skills)
                    filtered_with_skills.append({
                        'userId': int(user_id),
                        'fullName': f"{volunteer_info.iloc[0]['fname']} {volunteer_info.iloc[0]['lname']}",
                        'availability': volunteer_info.iloc[0]['availability'],
                        'overallRating': float(volunteer_info.iloc[0]['overallRating']),
                        'similarityScore': similarity_score
                    })

        # Sort by rating and similarity score
        filtered_with_skills = sorted(
            filtered_with_skills,
            key=lambda x: (-x['overallRating'], -x['similarityScore'])
        )

        return jsonify({
            "success": True,
            "volunteers": filtered_with_skills
        })
    except Exception as e:
        print("Error in /filter_by_rate_availability_skills:", e)
        return jsonify({"success": False, "message": str(e)}), 500

if __name__ == '__main__':
    app.run(debug=True, port=5000)
