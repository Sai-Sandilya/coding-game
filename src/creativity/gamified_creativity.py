# Core functionalities for Gamified Creativity

import random # Added missing import for random

def code_to_art(code_input: str, art_type: str = "visual", art_params: dict = None) -> dict:
    """Transforms code into artistic output (visuals or music).
    This version generates more nuanced outputs based on specific code patterns and customizable art parameters.
    """
    print(f"Generating {art_type} art from code.")

    if art_params is None:
        art_params = {"color": "blue", "tempo": 120, "shape": "circle"} # Default parameters

    art_output = ""
    complexity = len(code_input.replace('\n', '')) # Simple complexity measure

    # Analyze code for patterns relevant to art generation
    has_loop = "for " in code_input.lower() or "while " in code_input.lower()
    has_conditional = "if " in code_input.lower() or "else" in code_input.lower()
    has_recursion = "def " in code_input.lower() and ("(" + code_input.split("def ")[-1].split("(")[0] + "(") in code_input # Basic heuristic
    num_functions = code_input.lower().count("def ")
    num_lines = len(code_input.split('\n'))

    # Dynamic adjustment of art_params based on code properties (simplified heuristics)
    if has_loop:
        art_params['tempo'] = 180 # Faster tempo for loops
        art_params['shape'] = "spiral" # Spirals for loops
    if has_conditional:
        art_params['color'] = "green" if random.random() > 0.5 else "red" # Conditional color changes
    if has_recursion:
        art_params['shape'] = "fractal" # Fractals for recursion
    
    if num_functions > 3:
        art_params['visual_density'] = "high" # More complex visuals for many functions
    if num_lines > 50:
        art_params['intensity'] = "high" # Higher intensity for longer code

    if art_type.lower() == "visual":
        if art_params.get('shape') == "fractal":
            art_output = "# Self-similar, intricate recursive fractal pattern (simulated visual)"
        elif art_params.get('shape') == "spiral":
            art_output = "# Evolving spiral geometric pattern (simulated visual)"
        else:
            art_output = f"# Abstract visual with {art_params.get('color')} tones and {art_params.get('shape')} shapes (simulated visual)"
        return {"status": "success", "art_output": art_output, "type": "visual", "art_parameters": art_params}
    elif art_type.lower() == "music":
        if art_params.get('tempo') == 180:
            art_output = "# Fast-paced, repetitive melodic sequence (simulated music)"
        elif art_params.get('color') == "green": # Simulating color affecting music feel
            art_output = "# Uplifting, harmonic musical piece (simulated music)"
        else:
            art_output = f"# Procedural ambient soundscape with tempo {art_params.get('tempo')} (simulated music)"
        return {"status": "success", "art_output": art_output, "type": "music", "art_parameters": art_params}
    else:
        return {"status": "error", "message": "Unsupported art type."}

def enter_sandbox_mode(player_id: str, environment_theme: str = "default", unlocked_assets: list = None) -> dict:
    """Activates a free-build sandbox where players can experiment with code.
    This version describes the environment and tools, and includes configurable asset libraries.
    """
    if unlocked_assets is None:
        unlocked_assets = []

    print(f"Player {player_id} entering sandbox mode with theme: {environment_theme}. Unlocked assets: {len(unlocked_assets)}.")

    base_tools = [
        "Visual Code Block Editor",
        "Live Code Preview",
        "Debugging Console"
    ]
    
    # Simulate loading different asset libraries based on unlocked assets or theme
    asset_library = []
    if environment_theme == "sci-fi":
        asset_library.extend(["Sci-Fi Drones", "Holographic Displays"])
    elif environment_theme == "fantasy":
        asset_library.extend(["Magical Runes", "Mythical Creatures"])
    else: # Default
        asset_library.extend(["Basic Shapes", "Sound Effects"])

    asset_library.extend(unlocked_assets) # Add player's unlocked assets

    welcome_message = f"Welcome to the creative sandbox, {player_id}! Unleash your imagination. Environment: {environment_theme.capitalize()}. Available tools: {", ".join(base_tools)}. Loaded assets: {", ".join(asset_library) or 'None'}."

    # Placeholder for initializing a flexible environment for code experimentation and mini-game creation
    return {
        "status": "success",
        "mode": "sandbox",
        "message": welcome_message,
        "available_tools": base_tools,
        "loaded_assets": asset_library,
        "player_id": player_id,
        "environment_theme": environment_theme
    }

def create_mini_game(player_id: str, game_concept: dict) -> dict:
    """Allows players to define and build simple mini-games using in-game coding tools.
    This version generates game elements and logic based on the provided concept, includes validation,
    and simulates a basic game testing process with a performance report.
    """
    game_title = game_concept.get('title', 'Untitled Mini-Game')
    game_type = game_concept.get('type', 'puzzle').lower()
    main_objective = game_concept.get('objective', 'Solve the puzzle')
    player_provided_logic = game_concept.get('player_logic_code', '') # The code written by the player

    generated_elements = []
    generated_logic_modules = []
    validation_status = "success"
    validation_messages = []
    test_results = {} # New: for simulated game testing
    performance_report = {} # New: for simulated performance

    # Simulate basic logic validation: check for key components based on game type
    if game_type == "puzzle":
        generated_elements.append("logic_gates")
        generated_elements.append("movable_blocks")
        generated_logic_modules.append("function_sequencing_logic")
        generated_logic_modules.append("conditional_win_condition")
        if "solve_puzzle" not in player_provided_logic.lower():
            validation_status = "warning"
            validation_messages.append("Puzzle game logic might be missing a clear solve condition.")
        
        # Simulated testing for puzzle
        if "correct_sequence" in player_provided_logic.lower():
            test_results["puzzle_test_1"] = {"passed": True, "message": "Solved basic puzzle!"}
            performance_report["efficiency"] = "high"
        else:
            test_results["puzzle_test_1"] = {"passed": False, "message": "Failed to solve basic puzzle."}
            performance_report["efficiency"] = "low"

    elif game_type == "platformer":
        generated_elements.append("platforms")
        generated_elements.append("player_character")
        generated_logic_modules.append("jump_mechanics_code")
        generated_logic_modules.append("collision_detection_code")
        if "character.jump" not in player_provided_logic.lower():
            validation_status = "warning"
            validation_messages.append("Platformer game logic might be missing jump mechanics.")
        
        # Simulated testing for platformer
        if "jump_height" in player_provided_logic.lower() and "move_speed" in player_provided_logic.lower():
            test_results["platformer_test_1"] = {"passed": True, "message": "Character moves and jumps!"}
            performance_report["smoothness"] = "good"
        else:
            test_results["platformer_test_1"] = {"passed": False, "message": "Movement or jump logic incomplete."}
            performance_report["smoothness"] = "poor"

    elif game_type == "shooter":
        generated_elements.append("enemies")
        generated_elements.append("projectiles")
        generated_logic_modules.append("aiming_firing_logic")
        generated_logic_modules.append("health_system_code")
        if "fire_projectile" not in player_provided_logic.lower():
            validation_status = "warning"
            validation_messages.append("Shooter game logic might be missing projectile firing mechanics.")
        
        # Simulated testing for shooter
        if "enemy.take_damage" in player_provided_logic.lower() and "fire_rate" in player_provided_logic.lower():
            test_results["shooter_test_1"] = {"passed": True, "message": "Projectiles hit enemies!"}
            performance_report["responsiveness"] = "excellent"
        else:
            test_results["shooter_test_1"] = {"passed": False, "message": "Shooting or damage logic incomplete."}
            performance_report["responsiveness"] = "bad"

    else:
        generated_elements.append("basic_elements")
        generated_logic_modules.append("basic_game_loop")
        if not player_provided_logic.strip():
            validation_status = "warning"
            validation_messages.append("Generic mini-game has no player-provided logic.")

    # Simulate bug detection based on common issues
    if "while True" in player_provided_logic or "infinite_loop" in player_provided_logic.lower():
        validation_status = "error" # Critical error
        validation_messages.append("Critical Error: Detected a potential infinite loop. This will crash the game!")
        test_results["infinite_loop_check"] = {"passed": False, "message": "Infinite loop detected."}
        performance_report["stability"] = "very_poor"
    
    # Overall status based on validation and testing
    if validation_status == "success" and all(test['passed'] for test in test_results.values()):
        final_status = "ready_to_publish"
    elif validation_status == "warning" or any(not test['passed'] for test in test_results.values()):
        final_status = "needs_refinement"
    else:
        final_status = "critical_errors"

    game_id = f"mg_{hash(f'{player_id}-{game_title}-{random.random()}') & 0xFFFFFFFF}"

    print(f"Player {player_id} creating a {game_type} mini-game: {game_title}. Final Status: {final_status}.")
    # Placeholder for a mini-game construction kit that translates code to game logic
    return {
        "status": final_status,
        "game_id": game_id,
        "title": game_title,
        "type": game_type,
        "objective": main_objective,
        "generated_elements": generated_elements,
        "generated_logic_modules": generated_logic_modules,
        "player_id": player_id,
        "validation_status": validation_status,
        "validation_messages": validation_messages,
        "test_results": test_results, # New: simulated test results
        "performance_report": performance_report # New: simulated performance report
    } 