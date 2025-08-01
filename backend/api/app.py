# Basic Flask application for Codebound Backend API
import sys
import os
import json

# Add necessary directories to the Python path
sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), '..' , '..' , 'src', 'ai')))
sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), '..' , '..' , 'src', 'multiplayer')))

from flask import Flask, request, jsonify
from adaptive_learning import adapt_difficulty, conversational_tutor
from multiplayer_core import run_code_in_session # Reusing for single-player game code execution simulation

app = Flask(__name__)

# This would be a persistent storage in a real game (e.g., database)
player_models = {} 
# Also store conversation history per player
conversation_histories = {} # player_id: list of (user_message, ai_response) tuples

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

@app.route('/')
def index():
    return "Codebound Backend API is running!"

if __name__ == '__main__':
    # For development, run with debug=True. In production, use a production WSGI server.
    app.run(debug=True, host='0.0.0.0', port=5000) 