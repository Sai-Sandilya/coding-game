# Initial file for AI-Powered Adaptive Learning

def adapt_difficulty(player_performance: dict) -> float:
    """Adjusts difficulty based on player performance metrics.
    player_performance should include keys like 'correct_answers', 'time_taken', 'hints_used'.
    """
    score = player_performance.get('correct_answers', 0)
    time = player_performance.get('time_taken', 1)
    hints = player_performance.get('hints_used', 0)

    # Simple adaptive logic: higher score, faster time, fewer hints -> increase difficulty
    # This is a placeholder and will be replaced with a more sophisticated AI model
    difficulty_factor = (score / (time + 1) * 10) - (hints * 0.5)
    return max(0.1, min(2.0, difficulty_factor)) # Keep difficulty within a reasonable range

def conversational_tutor(user_input: str, context: list) -> str:
    """Simulates an AI mentor that explains concepts and debugs.
    context will store previous turns in the conversation.
    """
    user_input_lower = user_input.lower()
    response = ""

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
    else:
        response = "That's an interesting thought! Could you tell me more about what you're trying to learn or build?"

    return response

def provide_code_style_feedback(code_snippet: str) -> str:
    # Placeholder for code style feedback - will integrate linting or AI-based analysis later
    feedback = ""
    if "  " in code_snippet: # Simple check for double spaces for indentation
        feedback += "Consider using consistent indentation, typically 4 spaces for Python, to make your code more readable.\n"
    if not code_snippet.strip():
        feedback += "It looks like an empty code snippet. Try writing some code!\n"
    if len(code_snippet) > 100 and "\n" not in code_snippet:
        feedback += "Long lines of code can be hard to read. Try breaking them into multiple lines if possible.\n"
    
    if not feedback:
        feedback = "Your code looks good on a first pass! Keep practicing good style."
    
    return feedback 