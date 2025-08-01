# Core functionalities for Multiplayer Collaboration & Code Battles
import random

# Simulated persistent storage for multiplayer sessions
# In a real game, this would be a database or a dedicated session management service.
multiplayer_sessions = {}

# Predefined duel problems for variety
duel_problems = [
    {"id": "p001", "title": "FizzBuzz Challenge", "description": "Write a function that prints numbers from 1 to 100. For multiples of three print 'Fizz' instead of the number and for the multiples of five print 'Buzz'. For numbers which are multiples of both three and five print 'FizzBuzz'."},
    {"id": "p002", "title": "Palindrome Checker", "description": "Write a function that checks if a given string is a palindrome (reads the same forwards and backwards)."},
    {"id": "p003", "title": "Array Sorter", "description": "Implement a sorting algorithm (e.g., bubble sort, quicksort) for a given array of numbers. Efficiency matters!"}
]

def create_multiplayer_session(session_id: str, host_player_id: str, max_players: int = 4) -> dict:
    """Creates a new multiplayer coding session with a specified maximum player capacity.
    Returns session details or a success/failure status.
    """
    if session_id in multiplayer_sessions:
        return {"status": "error", "message": f"Session {session_id} already exists."}
    if max_players <= 1:
        return {"status": "error", "message": "Minimum 2 players required for a multiplayer session."}

    new_session = {
        "session_id": session_id,
        "host": host_player_id,
        "max_players": max_players,
        "players": [host_player_id],
        "shared_code": "# Start your collaborative code here!\n",
        "code_version": 0, # New: track code versions
        "status": "waiting_for_players", # new session state
        "duel_active": False,
        "current_problem": None,
        "player_scores": {host_player_id: 0},
        "code_execution_logs": [] # New: to store execution history
    }
    multiplayer_sessions[session_id] = new_session
    print(f"Session {session_id} created by {host_player_id} with max players: {max_players}.")
    return {"status": "success", "session_info": new_session}

def join_multiplayer_session(session_id: str, player_id: str) -> dict:
    """Allows a player to join an existing multiplayer session.
    Returns updated session details or an error if the session is full or doesn't exist.
    """
    session = multiplayer_sessions.get(session_id)

    if not session:
        return {"status": "error", "message": f"Session {session_id} does not exist."}
    if player_id in session['players']:
        return {"status": "error", "message": f"Player {player_id} already in session {session_id}."}
    if len(session['players']) >= session['max_players']:
        return {"status": "error", "message": f"Session {session_id} is full."}

    session['players'].append(player_id)
    session['player_scores'][player_id] = 0 # Initialize score for new player
    
    # Update session status if now full
    if len(session['players']) == session['max_players']:
        session['status'] = "ready_to_start"

    print(f"Player {player_id} joined session {session_id}. Current players: {session['players']}.")
    return {"status": "success", "session_info": session}

def update_shared_code(session_id: str, player_id: str, new_code_segment: str, start_line: int, end_line: int, expected_version: int = -1) -> dict:
    """Applies code changes from one player to the shared code in a session.
    Includes optimistic concurrency control with expected_version.
    """
    session = multiplayer_sessions.get(session_id)

    if not session:
        return {"status": "error", "message": f"Session {session_id} does not exist."}
    if player_id not in session['players']:
        return {"status": "error", "message": f"Player {player_id} not in session {session_id}."}

    # Optimistic concurrency control
    if expected_version != -1 and session['code_version'] != expected_version:
        return {"status": "conflict", "message": "Code version mismatch. Please re-sync.", "current_version": session['code_version'], "current_shared_code": session['shared_code']}

    current_shared_code = session['shared_code'].split('\n')
    
    # Basic simulation of merging: replace lines in the given range
    start_idx = max(0, start_line - 1)
    end_idx = min(len(current_shared_code), end_line - 1)

    new_code_lines = new_code_segment.split('\n')
    
    updated_shared_code_lines = \
        current_shared_code[:start_idx] + \
        new_code_lines + \
        current_shared_code[end_idx:]
    
    session['shared_code'] = "\n".join(updated_shared_code_lines)
    session['code_version'] += 1 # Increment version on successful update

    print(f"Player {player_id} updated code in session {session_id}. New code length: {len(session['shared_code'])}, Version: {session['code_version']}.")

    return {"status": "success", "session_id": session_id, "new_shared_code": session['shared_code'], "new_version": session['code_version']}

def run_code_in_session(session_id: str, code_block: str, player_id: str) -> dict:
    """Executes a block of code within the shared session environment.
    Results could be game state changes, visual effects, or console output.
    This version simulates different execution outcomes based on simple patterns.
    Also logs execution to the session.
    """
    session = multiplayer_sessions.get(session_id)
    if not session:
        return {"status": "error", "message": f"Session {session_id} does not exist."}
    if player_id not in session['players']:
        return {"status": "error", "message": f"Player {player_id} not in session {session_id}."}

    print(f"Player {player_id} attempting to execute code in session {session_id}.")
    output = ""
    status = "executed"

    code_block_lower = code_block.lower()

    if "error" in code_block_lower or "fail" in code_block_lower:
        output = "Simulated runtime error: Check your logic!"
        status = "error"
    elif "print(" in code_block_lower or "console.log(" in code_block_lower:
        output = "Simulated console output: Hello Codebound!"
    elif "loop" in code_block_lower and "infinite" in code_block_lower:
        output = "Simulated infinite loop detected: Optimize your iterations!"
        status = "warning"
    elif "success" in code_block_lower or "win" in code_block_lower:
        output = "Code executed successfully! Game state updated (simulated)."
    else:
        output = "Code ran successfully (simulated). No specific output."

    execution_log = {
        "timestamp": "simulated_time", # In a real system, use datetime
        "player_id": player_id,
        "code_executed": code_block, # Could be truncated for large blocks
        "output": output,
        "status": status
    }
    session['code_execution_logs'].append(execution_log)

    return {"status": status, "output": output, "player_id": player_id, "session_id": session_id}

def start_code_duel(session_id: str, players: list, problem_id: str = None, time_limit_seconds: int = 300) -> dict:
    """Initiates a competitive code duel within a session with a defined problem and timer.
    Manages duel state within the session and selects a problem dynamically.
    """
    session = multiplayer_sessions.get(session_id)
    if not session:
        return {"status": "error", "message": f"Session {session_id} does not exist."}
    if session['duel_active']:
        return {"status": "error", "message": f"Duel already active in session {session_id}."}
    if not all(p in session['players'] for p in players): # Ensure all players in duel are in session
        return {"status": "error", "message": "One or more duel participants not in session."}
    if len(players) < 2:
        return {"status": "error", "message": "At least two players are required for a duel."}

    selected_problem = None
    if problem_id:
        selected_problem = next((p for p in duel_problems if p['id'] == problem_id), None)
        if not selected_problem:
            return {"status": "error", "message": f"Problem with ID {problem_id} not found."}
    else:
        selected_problem = random.choice(duel_problems) # Select a random problem if not specified

    print(f"Code duel started in session {session_id} with players: {players}. Problem: {selected_problem['title']}. Time limit: {time_limit_seconds}s.")
    
    session['duel_active'] = True
    session['current_problem'] = selected_problem
    session['time_limit_seconds'] = time_limit_seconds
    session['player_scores'] = {player_id: 0 for player_id in players} # Reset scores for duel
    session['status'] = "in_duel"

    # Placeholder for more complex duel setup, e.g., distributing initial code, setting up test cases
    return {
        "status": "duel_started",
        "session_id": session_id,
        "players": players,
        "problem": selected_problem,
        "time_limit_seconds": time_limit_seconds,
        "current_scores": session['player_scores']
    }

def update_duel_score(session_id: str, player_id: str, points: int) -> dict:
    """Updates a player's score in an active code duel.
    """
    session = multiplayer_sessions.get(session_id)
    if not session:
        return {"status": "error", "message": f"Session {session_id} does not exist."}
    if not session['duel_active']:
        return {"status": "error", "message": f"No active duel in session {session_id}."}
    if player_id not in session['player_scores']:
        return {"status": "error", "message": f"Player {player_id} not a participant in the current duel."}

    session['player_scores'][player_id] += points
    print(f"Player {player_id} in session {session_id} scored {points} points. New score: {session['player_scores'][player_id]}.")
    return {"status": "success", "session_id": session_id, "player_id": player_id, "new_score": session['player_scores'][player_id], "current_scores": session['player_scores']}

def end_code_duel(session_id: str) -> dict:
    """Ends an active code duel and determines the winner(s).
    """
    session = multiplayer_sessions.get(session_id)
    if not session:
        return {"status": "error", "message": f"Session {session_id} does not exist."}
    if not session['duel_active']:
        return {"status": "error", "message": f"No active duel to end in session {session_id}."}

    session['duel_active'] = False
    session['status'] = "ready_to_start" # Back to waiting for next activity

    # Determine winner(s)
    winning_score = -1
    winners = []
    if session['player_scores']:
        winning_score = max(session['player_scores'].values())
        winners = [player_id for player_id, score in session['player_scores'].items() if score == winning_score]

    print(f"Code duel in session {session_id} ended. Winner(s): {winners or 'None'} with score: {winning_score}.")
    
    return {"status": "duel_ended", "session_id": session_id, "winner_ids": winners, "final_scores": session['player_scores']}

def end_multiplayer_session(session_id: str) -> dict:
    """Ends a multiplayer session and removes it from active sessions.
    """
    session = multiplayer_sessions.pop(session_id, None)
    if session:
        print(f"Multiplayer session {session_id} ended and removed.")
        return {"status": "success", "message": f"Session {session_id} ended.", "session_id": session_id}
    else:
        return {"status": "error", "message": f"Session {session_id} not found."} 