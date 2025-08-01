# Initial file for AI-Powered Adaptive Learning

def adapt_difficulty(player_performance: dict, mastery_data: dict = None) -> float:
    """Adjusts difficulty based on player performance metrics and concept mastery.
    player_performance should include keys like 'correct_answers', 'time_taken', 'hints_used', 'concept_id'.
    mastery_data is a dictionary where keys are concept_ids and values are their current mastery levels (0.0 to 1.0).
    """
    if mastery_data is None:
        mastery_data = {}

    score = player_performance.get('correct_answers', 0)
    time = player_performance.get('time_taken', 1)
    hints = player_performance.get('hints_used', 0)
    concept_id = player_performance.get('concept_id', 'general') # Default concept

    current_mastery = mastery_data.get(concept_id, 0.5) # Start with a neutral mastery if not present
    
    # Calculate performance impact on mastery
    performance_impact = 0.0
    if score > 0:
        performance_impact = (score * 0.1) / (time + 1) # Positive impact for correct answers and speed
    if hints > 0:
        performance_impact -= (hints * 0.05) # Negative impact for using hints

    # Update mastery (simple model, can be replaced by more complex algorithms)
    new_mastery = current_mastery + performance_impact
    new_mastery = max(0.0, min(1.0, new_mastery)) # Keep mastery between 0 and 1

    # Apply a small decay to mastery over time (simulated, in a real game this would be time-based)
    decay_rate = 0.01 # Concepts decay slowly if not practiced
    new_mastery = max(0.0, new_mastery - decay_rate)
    
    # Store updated mastery (in a real system, this would update a persistent player profile)
    mastery_data[concept_id] = new_mastery

    # Determine difficulty factor based on new mastery level
    # Lower mastery -> easier challenges (difficulty_factor closer to 0.1)
    # Higher mastery -> harder challenges (difficulty_factor closer to 2.0)
    difficulty_factor = 0.1 + (new_mastery * 1.9) # Scale from 0.1 to 2.0

    return max(0.1, min(2.0, difficulty_factor)) # Ensure bounds

def conversational_tutor(user_input: str, conversation_history: list = None) -> str:
    """Simulates an AI mentor that explains concepts and debugs, with basic context awareness.
    conversation_history is a list of previous (user_message, ai_response) tuples.
    """
    if conversation_history is None:
        conversation_history = []

    user_input_lower = user_input.lower()
    response = ""

    # Basic context awareness: check last few turns
    last_user_message = conversation_history[-1][0].lower() if conversation_history else ""
    last_ai_response = conversation_history[-1][1].lower() if conversation_history else ""

    if "hello" in user_input_lower or "hi" in user_input_lower:
        response = "Hello there, aspiring coder! How can I assist you on your journey through Codebound?"
    elif "loop" in user_input_lower:
        response = "A loop allows you to repeat a block of code multiple times. Think of it like a repeating spell! Do you want to try an example?"
    elif "conditional" in user_input_lower or "if" in user_input_lower:
        response = "Conditionals (like 'if' statements) help your code make decisions. It's like choosing which path to take based on a condition. What kind of decision are you trying to make?"
    elif "debug" in user_input_lower or "error" in user_input_lower:
        response = "Debugging is like solving a puzzle to find out why your code isn't working as expected. Can you describe the error you're seeing or what you're trying to achieve?"
    elif "thanks" in user_input_lower or "thank you" in user_input_lower:
        response = "You're most welcome! Keep up the great work. Let me know if you need more help."
    elif "example" in user_input_lower and "loop" in last_ai_response:
        response = "Great! Let's say you want to make a character jump 5 times. You could use a 'for' loop: 'for i in range(5): character.jump()'. This repeats the 'jump()' action 5 times!"
    elif "decision" in user_input_lower and "conditional" in last_ai_response:
        response = "If you want your character to only attack if they have enough mana, you could use: 'if character.mana > 10: character.attack()'. What kind of decision are you thinking of?"
    else:
        response = "That's an interesting thought! Could you tell me more about what you're trying to learn or build?"

    # In a real system, we'd add the current interaction to history here
    # conversation_history.append((user_input, response))

    return response

def provide_code_style_feedback(code_snippet: str) -> str:
    """Provides feedback on code style based on simple heuristics.
    This is a placeholder for actual linting or AI-based analysis.
    """
    feedback_items = []

    # Indentation check (basic)
    if "  " in code_snippet and "    " not in code_snippet: # Looks for 2 spaces but not 4
        feedback_items.append("Consider using consistent indentation, typically 4 spaces for Python, to make your code more readable.")
    elif "\t" in code_snippet: # Tab check
        feedback_items.append("Avoid mixing tabs and spaces for indentation; consistency improves readability.")

    # Empty code snippet check
    if not code_snippet.strip():
        feedback_items.append("It looks like an empty code snippet. Try writing some code!")

    # Line length check
    for line in code_snippet.split('\n'):
        if len(line) > 79: # Python's PEP 8 recommends 79 characters
            feedback_items.append("Long lines of code (over 79 characters) can be hard to read. Try breaking them into multiple lines if possible.")
            break # Only report once per snippet

    # Function length check (simple heuristic)
    function_lines = 0
    in_function = False
    for line in code_snippet.split('\n'):
        stripped_line = line.strip()
        if stripped_line.startswith("def "):
            in_function = True
            function_lines = 0
        if in_function and stripped_line:
            function_lines += 1
        if in_function and (not stripped_line or stripped_line.startswith("#")) and function_lines > 0:
            # If we hit an empty line or comment after a function, check its length
            if function_lines > 30: # Arbitrary limit for a single function
                feedback_items.append(f"Consider breaking down long functions ({function_lines} lines) into smaller, more focused functions for better readability and reusability.")
            in_function = False # Reset for next function
            function_lines = 0
    # Final check for last function in snippet
    if in_function and function_lines > 30:
         feedback_items.append(f"Consider breaking down long functions ({function_lines} lines) into smaller, more focused functions for better readability and reusability.")

    # Comment density (very basic: encourage comments for complex logic)
    code_lines = len([line for line in code_snippet.split('\n') if line.strip() and not line.strip().startswith("#")])
    comment_lines = len([line for line in code_snippet.split('\n') if line.strip().startswith("#")])
    
    if code_lines > 10 and comment_lines < 2: # If a good amount of code but few comments
        feedback_items.append("Adding comments to explain complex logic can significantly improve your code's readability for others (and your future self!).")


    if not feedback_items:
        return "Your code looks good on a first pass! Keep practicing good style."
    else:
        return "Here are some style suggestions for your code:\n" + "\n".join([f"- {item}" for item in feedback_items]) 