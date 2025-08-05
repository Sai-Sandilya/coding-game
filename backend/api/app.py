# Basic Flask application for Codebound Backend API
import sys
import os
import json

# Add necessary directories to the Python path
sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), '..' , '..' , 'src', 'ai')))
sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), '..' , '..' , 'src', 'multiplayer')))
sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), '..' , '..' , 'src', 'ethics')))

from flask import Flask, request, jsonify
from adaptive_learning import adapt_difficulty, conversational_tutor
from multiplayer_core import run_code_in_session # Reusing for single-player game code execution simulation
from privacy_ethics_lab import generate_ethical_dilemma_quest, simulate_code_impact, detect_algorithmic_bias, conduct_ethical_code_review

app = Flask(__name__)

# This would be a persistent storage in a real game (e.g., database)
player_models = {} 
# Also store conversation history per player
conversation_histories = {} # player_id: list of (user_message, ai_response) tuples
# Store ethical history per player
ethical_histories = {} # player_id: {'recent_ethical_score': 0, 'ethical_choices': [], 'public_opinion': 0.5}

@app.route('/adapt_difficulty', methods=['POST'])
def handle_adapt_difficulty():
    data = request.get_json()
    player_id = data.get('player_id')
    player_performance = data.get('player_performance')

    if not player_id or not player_performance:
        return jsonify({'error': 'Missing player_id or player_performance'}), 400

    # Retrieve or initialize player model
    current_player_model = player_models.get(player_id, {'mastery': 0.5, 'learning_rate': 0.1, 'difficulty_bias': 0.0})
    
    # Call the adaptive difficulty function
    new_difficulty = adapt_difficulty(player_performance, current_player_model)

    # Update the player model in our simulated storage
    # In a real game, this would involve database writes
    player_models[player_id] = current_player_model # Update the stored model with changes made by adapt_difficulty

    return jsonify({'status': 'success', 'player_id': player_id, 'new_difficulty': new_difficulty}), 200

@app.route('/conversational_tutor', methods=['POST'])
def handle_conversational_tutor():
    data = request.get_json()
    player_id = data.get('player_id')
    user_input = data.get('user_input')

    if not player_id or not user_input:
        return jsonify({'error': 'Missing player_id or user_input'}), 400

    # Retrieve or initialize player model and conversation history
    current_player_model = player_models.get(player_id, {'mastery': 0.5, 'learning_rate': 0.1, 'difficulty_bias': 0.0})
    current_conversation_history = conversation_histories.get(player_id, [])

    # Call the conversational tutor function
    tutor_response = conversational_tutor(user_input, current_conversation_history, current_player_model)

    # Update conversation history (append current turn)
    current_conversation_history.append((user_input, tutor_response))
    conversation_histories[player_id] = current_conversation_history

    return jsonify({'status': 'success', 'player_id': player_id, 'tutor_response': tutor_response}), 200

@app.route('/execute_game_code', methods=['POST'])
def handle_execute_game_code():
    data = request.get_json()
    player_id = data.get('player_id')
    code_block = data.get('code_block')

    if not player_id or not code_block:
        return jsonify({'error': 'Missing player_id or code_block'}), 400
    
    # Simulate code execution using run_code_in_session from multiplayer_core
    # In a real game, this would be a secure sandboxed environment.
    execution_result = run_code_in_session(session_id="single_player", code_block=code_block, player_id=player_id)

    # Interpret the execution result to determine game effects
    game_effect = ""
    if execution_result['status'] == "executed":
        if "open_gate" in code_block.lower():
            game_effect = "open_gate_A"
        elif "spawn_enemy" in code_block.lower():
            game_effect = "spawn_goblin"
        elif "change_color" in code_block.lower():
            game_effect = "set_light_blue"
        elif "win_level" in code_block.lower():
            game_effect = "level_complete"
        else:
            game_effect = "no_specific_effect"
    else:
        game_effect = "code_error"

    return jsonify({
        'status': 'success',
        'player_id': player_id,
        'executed_code_output': execution_result['output'],
        'game_effect': game_effect
    }), 200

@app.route('/generate_ethical_dilemma', methods=['POST'])
def handle_generate_ethical_dilemma():
    """Generate an ethical dilemma quest based on player's ethical history."""
    data = request.get_json()
    player_id = data.get('player_id')
    dilemma_type = data.get('dilemma_type', 'data_privacy')
    complexity_level = data.get('complexity_level', 'medium')

    if not player_id:
        return jsonify({'error': 'Missing player_id'}), 400

    # Retrieve player's ethical history for personalized dilemmas
    player_ethical_history = ethical_histories.get(player_id, {'recent_ethical_score': 0, 'ethical_choices': [], 'public_opinion': 0.5})

    # Generate the ethical dilemma
    dilemma_result = generate_ethical_dilemma_quest(dilemma_type, complexity_level, player_ethical_history)

    return jsonify({
        'status': 'success',
        'player_id': player_id,
        'ethical_dilemma': dilemma_result
    }), 200

@app.route('/simulate_code_impact', methods=['POST'])
def handle_simulate_code_impact():
    """Simulate the societal impact of a player's code solution."""
    data = request.get_json()
    player_id = data.get('player_id')
    code_solution = data.get('code_solution')
    scenario_id = data.get('scenario_id', 'default_scenario')
    stakeholder_weights = data.get('stakeholder_weights', None)

    if not player_id or not code_solution:
        return jsonify({'error': 'Missing player_id or code_solution'}), 400

    # Retrieve player's current public opinion
    player_ethical_history = ethical_histories.get(player_id, {'recent_ethical_score': 0, 'ethical_choices': [], 'public_opinion': 0.5})
    current_public_opinion = player_ethical_history.get('public_opinion', 0.5)

    # Simulate the impact
    impact_result = simulate_code_impact(code_solution, scenario_id, current_public_opinion, stakeholder_weights)

    # Update player's ethical history
    player_ethical_history['recent_ethical_score'] += impact_result.get('ethical_score_change', 0)
    player_ethical_history['public_opinion'] = impact_result.get('new_public_opinion', current_public_opinion)
    player_ethical_history['ethical_choices'].append({
        'scenario_id': scenario_id,
        'ethical_score_change': impact_result.get('ethical_score_change', 0),
        'timestamp': '2024-01-01T00:00:00Z'  # In real app, use actual timestamp
    })
    
    # Keep only recent choices (last 10)
    player_ethical_history['ethical_choices'] = player_ethical_history['ethical_choices'][-10:]
    ethical_histories[player_id] = player_ethical_history

    return jsonify({
        'status': 'success',
        'player_id': player_id,
        'impact_simulation': impact_result,
        'updated_ethical_history': player_ethical_history
    }), 200

@app.route('/detect_algorithmic_bias', methods=['POST'])
def handle_detect_algorithmic_bias():
    """Detect potential algorithmic bias in a code solution."""
    data = request.get_json()
    player_id = data.get('player_id')
    code_solution = data.get('code_solution')
    dataset_info = data.get('dataset_info', None)

    if not player_id or not code_solution:
        return jsonify({'error': 'Missing player_id or code_solution'}), 400

    # Detect bias
    bias_result = detect_algorithmic_bias(code_solution, dataset_info)

    return jsonify({
        'status': 'success',
        'player_id': player_id,
        'bias_analysis': bias_result
    }), 200

@app.route('/conduct_ethical_code_review', methods=['POST'])
def handle_conduct_ethical_code_review():
    """Conduct a comprehensive ethical code review."""
    data = request.get_json()
    player_id = data.get('player_id')
    code_solution = data.get('code_solution')
    review_criteria = data.get('review_criteria', None)

    if not player_id or not code_solution:
        return jsonify({'error': 'Missing player_id or code_solution'}), 400

    # Conduct the review
    review_result = conduct_ethical_code_review(code_solution, review_criteria)

    return jsonify({
        'status': 'success',
        'player_id': player_id,
        'ethical_review': review_result
    }), 200

@app.route('/get_ethical_history', methods=['GET'])
def handle_get_ethical_history():
    """Get a player's ethical history and current standing."""
    player_id = request.args.get('player_id')

    if not player_id:
        return jsonify({'error': 'Missing player_id'}), 400

    player_ethical_history = ethical_histories.get(player_id, {
        'recent_ethical_score': 0,
        'ethical_choices': [],
        'public_opinion': 0.5,
        'ethical_level': 'neutral'
    })

    # Calculate ethical level based on recent score
    recent_score = player_ethical_history['recent_ethical_score']
    if recent_score > 50:
        player_ethical_history['ethical_level'] = 'exemplary'
    elif recent_score > 20:
        player_ethical_history['ethical_level'] = 'ethical'
    elif recent_score > -20:
        player_ethical_history['ethical_level'] = 'neutral'
    elif recent_score > -50:
        player_ethical_history['ethical_level'] = 'concerning'
    else:
        player_ethical_history['ethical_level'] = 'unethical'

    return jsonify({
        'status': 'success',
        'player_id': player_id,
        'ethical_history': player_ethical_history
    }), 200

@app.route('/')
def index():
    return "Codebound Backend API is running!"

if __name__ == '__main__':
    # For development, run with debug=True. In production, use a production WSGI server.
    app.run(debug=True, host='0.0.0.0', port=5000) 