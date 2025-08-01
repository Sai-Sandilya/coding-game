# Initial file for AI-Powered Adaptive Learning

def adapt_difficulty(player_performance: dict, player_model: dict = None) -> float:
    """Adjusts difficulty based on player performance metrics and a dynamic player model.
    player_performance should include keys like 'correct_answers', 'time_taken', 'hints_used', 'concept_id'.
    player_model is a dictionary storing dynamic player attributes, including per-concept mastery and learning rates.
    """
    if player_model is None:
        player_model = {
            'global_mastery': 0.5,       # Overall mastery
            'global_learning_rate': 0.1, # Overall learning rate
            'difficulty_bias': 0.0,      # Personal difficulty adjustment factor
            'concept_mastery': {}        # Per-concept mastery (concept_id: {mastery, learning_rate})
        }
    
    score = player_performance.get('correct_answers', 0)
    time = player_performance.get('time_taken', 1) # Ensure time is not zero
    hints = player_performance.get('hints_used', 0)
    concept_id = player_performance.get('concept_id', 'general') # Default concept

    # Initialize concept-specific mastery if not present
    if concept_id not in player_model['concept_mastery']:
        player_model['concept_mastery'][concept_id] = {
            'mastery': player_model['global_mastery'],
            'learning_rate': player_model['global_learning_rate']
        }
    
    concept_mastery_data = player_model['concept_mastery'][concept_id]

    # --- Update concept-specific mastery based on current performance ---
    performance_metric = (score * 0.2) / (time + 1) - (hints * 0.05) # Higher is better

    # Adjust concept mastery
    concept_mastery_data['mastery'] += concept_mastery_data['learning_rate'] * performance_metric
    concept_mastery_data['mastery'] = max(0.0, min(1.0, concept_mastery_data['mastery'])) # Clamp mastery

    # Adjust concept learning rate
    if performance_metric > 0.05:
        concept_mastery_data['learning_rate'] = min(0.2, concept_mastery_data['learning_rate'] + 0.005)
    elif performance_metric < -0.05:
        concept_mastery_data['learning_rate'] = max(0.05, concept_mastery_data['learning_rate'] - 0.005)

    # Apply a small decay to concept mastery (simulated per interaction)
    decay_rate = 0.005 # Concepts decay slowly if not practiced
    concept_mastery_data['mastery'] = max(0.0, concept_mastery_data['mastery'] - decay_rate)

    # --- Update global mastery (optional, as an aggregate) ---
    # This could be an average of all concept masteries or weighted.
    # For now, a simple update based on the current concept.
    player_model['global_mastery'] = sum(data['mastery'] for data in player_model['concept_mastery'].values()) / len(player_model['concept_mastery']) if player_model['concept_mastery'] else 0.5
    player_model['global_mastery'] = max(0.0, min(1.0, player_model['global_mastery']))

    # In a real system, player_model would be persistently stored and loaded.

    # --- Determine difficulty factor based on updated player model and concept mastery ---
    # Use concept-specific mastery for difficulty, if available, otherwise global.
    effective_mastery = concept_mastery_data['mastery'] # Use specific concept mastery
    
    # Base difficulty from mastery: lower mastery -> easier, higher mastery -> harder
    base_difficulty = 0.1 + (effective_mastery * 1.5) # Scales from 0.1 to 1.6

    # Apply personal difficulty bias
    difficulty_factor = base_difficulty + player_model['difficulty_bias']

    return max(0.1, min(2.0, difficulty_factor)) # Ensure difficulty remains within bounds

def conversational_tutor(user_input: str, conversation_history: list = None, player_model: dict = None) -> str:
    """Simulates an AI mentor that explains concepts and debugs, with advanced context awareness and personalized responses.
    conversation_history is a list of previous (user_message, ai_response) tuples.
    player_model is used to personalize responses based on player's global and concept-specific mastery.
    """
    if conversation_history is None:
        conversation_history = []
    if player_model is None:
        player_model = {'global_mastery': 0.5, 'concept_mastery': {}}

    user_input_lower = user_input.lower()
    response = ""

    # Analyze recent conversation history for context
    recent_topics = set()
    for i in range(len(conversation_history) - 1, max(-1, len(conversation_history) - 3), -1): # Look at last 3 turns
        last_user_msg = conversation_history[i][0].lower()
        last_ai_resp = conversation_history[i][1].lower()
        if "loop" in last_user_msg or "loop" in last_ai_resp:
            recent_topics.add("loop")
        if "conditional" in last_user_msg or "conditional" in last_ai_resp:
            recent_topics.add("conditional")
        if "debug" in last_user_msg or "error" in user_input_lower: # Check user_input for immediate debug request
            recent_topics.add("debug")

    # Personalize response based on player's *global* mastery or a specific concept's mastery if known from context
    effective_mastery_for_tutor = player_model.get('global_mastery', 0.5)
    current_concept_in_focus = "general"
    
    if "loop" in user_input_lower or "loop" in recent_topics:
        current_concept_in_focus = "loop"
    elif "conditional" in user_input_lower or "conditional" in recent_topics:
        current_concept_in_focus = "conditional"
    elif "recursion" in user_input_lower or "recursion" in recent_topics:
        current_concept_in_focus = "recursion"

    if current_concept_in_focus != "general" and current_concept_in_focus in player_model['concept_mastery']:
        effective_mastery_for_tutor = player_model['concept_mastery'][current_concept_in_focus]['mastery']

    mastery_level_desc = "beginner" if effective_mastery_for_tutor < 0.4 else \
                         "intermediate" if effective_mastery_for_tutor < 0.7 else "advanced"

    # Dynamic examples and explanations based on effective mastery
    examples = {
        "loop": {
            "beginner": "A simple 'for' loop example: `for i in range(3): print('Hello')` will print 'Hello' three times.",
            "intermediate": "For more complex repetition, consider a 'while' loop: `count = 0; while count < 5: print(count); count += 1`.",
            "advanced": "When optimizing, consider list comprehensions or generator expressions for concise loops."
        },
        "conditional": {
            "beginner": "An 'if-else' statement: `if score > 100: print('Win') else: print('Try again')`.",
            "intermediate": "Using 'elif' for multiple conditions: `if grade >= 90: ... elif grade >= 80: ...`.",
            "advanced": "For complex decision trees, think about strategy pattern or polymorphism instead of deeply nested conditionals."
        },
        "recursion": {
            "beginner": "Recursion is a function calling itself. Like `def countdown(n): if n > 0: print(n); countdown(n-1)`.",
            "intermediate": "Understanding base cases is crucial to avoid infinite recursion. Recursive solutions often simplify complex problems.",
            "advanced": "Consider memoization or dynamic programming to optimize recursive functions and avoid recomputing results."
        },
        "general": {
            "beginner": "Let's start with basics. What's a variable? It's like a named box for data.",
            "intermediate": "Functions help organize your code. They're blocks of reusable logic.",
            "advanced": "For large projects, object-oriented programming can help manage complexity with classes and objects."
        }
    }

    # Response generation logic
    if "hello" in user_input_lower or "hi" in user_input_lower:
        response = f"Hello there, {mastery_level_desc} aspiring coder! How can I assist you on your journey through Codebound?"
    elif "loop" in user_input_lower:
        current_example = examples["loop"].get(mastery_level_desc, examples["loop"]["beginner"])
        response = f"A loop allows you to repeat a block of code. {current_example} What specific challenge are you facing?"
    elif "conditional" in user_input_lower or "if" in user_input_lower:
        current_example = examples["conditional"].get(mastery_level_desc, examples["conditional"]["beginner"])
        response = f"Conditionals help your code make decisions. {current_example} What kind of decision are you trying to make?"
    elif "recursion" in user_input_lower:
        current_example = examples["recursion"].get(mastery_level_desc, examples["recursion"]["beginner"])
        response = f"Recursion is a powerful concept where a function calls itself. {current_example} Do you have a recursive problem in mind?"
    elif "debug" in user_input_lower or "error" in user_input_lower:
        if "debug" in recent_topics:
            response = "Still having trouble debugging? Let's break it down. Tell me the exact error message or what behavior you're seeing."
        else:
            response = "Debugging is like solving a puzzle to find out why your code isn't working as expected. Can you describe the error you're seeing or what you're trying to achieve?"
    elif "thanks" in user_input_lower or "thank you" in user_input_lower:
        response = f"You're most welcome, {mastery_level_desc} coder! Keep up the great work. Let me know if you need more help."
    elif "example" in user_input_lower:
        # Try to provide an example for the most recent topic or the current input's concept
        if current_concept_in_focus != "general" and current_concept_in_focus in examples:
             current_example = examples[current_concept_in_focus].get(mastery_level_desc, examples[current_concept_in_focus]["beginner"])
             response = f"Here's an example related to {current_concept_in_focus}: {current_example}"
        else:
            response = examples["general"].get(mastery_level_desc, examples["general"]["beginner"])
    else:
        response = "That's an interesting thought! Could you tell me more about what you're trying to learn or build?"

    # In a real system, we'd add the current interaction to history here
    # conversation_history.append((user_input, response))

    return response

def provide_code_style_feedback(code_snippet: str) -> str:
    """Provides feedback on code style based on simple heuristics and common Python style guidelines.
    This version includes simulated static analysis and context-aware suggestions.
    """
    feedback_items = []

    # Indentation check (basic - focus on common issues)
    lines = code_snippet.split('\n')
    for i, line in enumerate(lines):
        stripped_line = line.lstrip()
        if stripped_line and not stripped_line.startswith("#"): # Ignore empty lines and comments
            # Check for inconsistent leading whitespace (tabs vs spaces)
            if '\t' in line and ' ' in line.lstrip():
                feedback_items.append(f"Line {i+1}: Avoid mixing tabs and spaces for indentation; consistency improves readability (PEP 8).")
            
            # Basic check for non-PEP8 indentation (e.g., 2 spaces instead of 4)
            # This is heuristic and might need more context for accuracy.
            if line.startswith("  ") and not line.startswith("    ") and not line.lstrip().startswith("class") and not line.lstrip().startswith("def"): # Avoid false positives for class/function defs
                feedback_items.append(f"Line {i+1}: Consider using 4 spaces for indentation to follow Python's PEP 8 style guide.")


    # Empty code snippet check
    if not code_snippet.strip():
        feedback_items.append("It looks like an empty code snippet. Try writing some code!")

    # Line length check (PEP 8: 79 characters)
    for i, line in enumerate(lines):
        if len(line) > 79: 
            feedback_items.append(f"Line {i+1}: Long lines of code (over 79 characters) can be hard to read. Try breaking them into multiple lines (PEP 8).")
            

    # Function length check (simple heuristic - adjust as needed)
    function_lines = 0
    in_function = False
    for i, line in enumerate(lines):
        stripped_line = line.strip()
        if stripped_line.startswith("def "):
            in_function = True
            function_lines = 0 # Reset for new function
        if in_function and stripped_line:
            function_lines += 1
        # Check function length when we exit a function block (e.g., empty line, new def, or end of snippet)
        if in_function and (not stripped_line or stripped_line.startswith("def ") or stripped_line.startswith("class ")):
            if function_lines > 30: # Arbitrary limit for a single function
                feedback_items.append(f"Function starting around line {i - function_lines + 1}: Consider breaking down long functions ({function_lines} lines) into smaller, more focused functions for better readability and reusability.")
            in_function = False # Reset
            function_lines = 0
    # Final check for a function that ends at the very end of the snippet
    if in_function and function_lines > 30:
        feedback_items.append(f"Function starting around line {len(lines) - function_lines + 1}: Consider breaking down long functions ({function_lines} lines) into smaller, more focused functions for better readability and reusability.")

    # Comment density (basic: encourage comments for complex logic/docstrings)
    code_lines_without_comments = len([line for line in lines if line.strip() and not line.strip().startswith("#") and not line.strip().startswith("'''") and not line.strip().startswith("\"\"\"")])
    comment_or_docstring_lines = len([line for line in lines if line.strip().startswith("#") or line.strip().startswith("'''") or line.strip().startswith("\"\"\"")])
    
    if code_lines_without_comments > 15 and comment_or_docstring_lines < 3: 
        feedback_items.append("Consider adding more comments or docstrings to explain complex logic, function purpose, or tricky parts of your code. This greatly improves readability and maintainability.")

    # Variable naming convention (very basic, can be expanded)
    import re
    for i, line in enumerate(lines):
        # Look for simple assignments that might not follow snake_case
        if re.search(r'[a-zA-Z0-9_]+\s*=\s*', line):
            variable_name_match = re.match(r'^\s*([a-zA-Z0-9_]+)\s*=', line)
            if variable_name_match:
                variable_name = variable_name_match.group(1)
                if not re.match(r'^[a-z_][a-z0-9_]*$', variable_name) and not variable_name.isupper(): # Not snake_case and not all caps (for constants)
                    feedback_items.append(f"Line {i+1}: Consider using `snake_case` for variable and function names in Python, as per PEP 8.")

    # Simulated static analysis / context-aware suggestions
    if "magic_number" in code_snippet.lower():
        feedback_items.append("Avoid using 'magic numbers' directly in your code. Define them as named constants for better readability and maintainability.")
    if re.search(r'(if\s+\w+\s*==\s*True)|(if\s+\w+\s*==\s*False)', code_snippet):
        feedback_items.append("Simplify boolean comparisons: instead of `if x == True`, use `if x:`, and instead of `if x == False`, use `if not x:`.")
    if re.search(r'try:\s*pass\s*except:', code_snippet, re.DOTALL):
        feedback_items.append("Avoid using empty `except` blocks with `pass`. This can hide important errors. Be specific about exceptions caught or handle them appropriately.")
    
    # Encourage function comments/docstrings more strongly
    if "def " in code_snippet and "\"\"\"" not in code_snippet and "'''" not in code_snippet:
        feedback_items.append("Consider adding a docstring to your functions to explain their purpose, arguments, and return values. This is crucial for code documentation.")

    if not feedback_items:
        return "Your code looks good on a first pass! Keep practicing good style."
    else:
        # Remove duplicates while preserving order for better user experience
        unique_feedback_items = []
        seen = set()
        for item in feedback_items:
            if item not in seen:
                unique_feedback_items.append(item)
                seen.add(item)

        return "Here are some style suggestions for your code:\n" + "\n".join([f"- {item}" for item in unique_feedback_items]) 