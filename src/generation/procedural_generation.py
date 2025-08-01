# Core functionalities for Procedural & AI-Generated Content

def generate_quest(player_skill_level: float, concept_focus: str = None) -> dict:
    """Generates a new coding quest based on player skill and an optional concept focus.
    This version generates a more detailed quest structure.
    """
    quest_id = f"q_{hash(f'{player_skill_level}-{concept_focus}-{hash(str(import random; random.random()))}') & 0xFFFFFFFF}" # Generate a unique ID
    
    # Simulate dynamic problem generation based on skill and concept focus
    problem_statements = {
        "loop": {
            "beginner": "The Ancient Looper needs help automating his repetitive tasks. Write a loop to make a character jump 5 times.",
            "intermediate": "A magical portal requires a sequence of 10 complex actions. Create nested loops to perform this. Consider efficiency!",
            "advanced": "The Chronoscepter has fallen out of sync. Write a recursive loop to fix its temporal iterations, ensuring no stack overflow."
        },
        "conditional": {
            "beginner": "The Guardian of Choices only allows passage if you meet specific conditions. Write an 'if' statement to determine if you can pass.",
            "intermediate": "The Binary Desert has shifting sands. Create a conditional logic tree to navigate based on wind direction and time of day.",
            "advanced": "You've encountered a rogue AI. Develop a robust conditional system to predict and counter its moves based on its emotional state and power level."
        },
        "general": {
            "beginner": "The first task is simple: make the glowing orb turn on. Write a basic function for it.",
            "intermediate": "A complex puzzle mechanism requires a sequence of function calls. Chain them correctly to open the path.",
            "advanced": "The world is glitching. Write a program that can identify and correct anomalies in real-time, leveraging principles of functional programming."
        }
    }

    selected_concept = concept_focus if concept_focus and concept_focus in problem_statements else "general"
    
    difficulty_tier = "beginner"
    if player_skill_level >= 0.7:
        difficulty_tier = "advanced"
    elif player_skill_level >= 0.4:
        difficulty_tier = "intermediate"

    title = f"Quest: The {selected_concept.capitalize()} Challenge ({difficulty_tier.capitalize()})"
    description = problem_statements.get(selected_concept, {}).get(difficulty_tier, "A challenging coding quest awaits!")
    
    # Simulate hints based on difficulty
    hints = []
    if difficulty_tier == "beginner":
        hints.append("Remember the basic syntax for your chosen coding construct.")
    elif difficulty_tier == "intermediate":
        hints.append("Think about edge cases and efficiency.")
    elif difficulty_tier == "advanced":
        hints.append("Consider advanced data structures or algorithmic optimizations.")

    print(f"Generated quest {quest_id} for skill level {player_skill_level}, focusing on {concept_focus or 'any'}.")

    return {
        "status": "success",
        "quest_id": quest_id,
        "title": title,
        "description": description,
        "objectives": [f"Solve the {selected_concept} puzzle"],
        "narrative_hook": "The ancient runes glow faintly, waiting for your code to awaken them.",
        "hints": hints,
        "required_skills": [selected_concept],
        "reward": {"exp": 100, "item": "Logic Gem"}
    }

def generate_world_segment(theme: str, difficulty: float) -> dict:
    """Generates a segment of the game world with a specific coding theme and difficulty.
    This version generates more detailed world elements and puzzles.
    """
    segment_id = f"ws_{hash(f'{theme}-{difficulty}-{hash(str(import random; random.random()))}') & 0xFFFFFFFF}" # Unique ID
    
    elements = []
    puzzles = []

    if theme.lower() == "loops":
        elements.append("repeating_patterns")
        elements.append("automated_machinery")
        if difficulty < 0.5: # Easier loop puzzles
            puzzles.append("simple_for_loop_puzzle")
        else:
            puzzles.append("nested_while_loop_challenge")
        npcs = ["Ancient Looper"]
    elif theme.lower() == "conditionals":
        elements.append("branching_paths")
        elements.append("decision_gates")
        if difficulty < 0.5:
            puzzles.append("basic_if_else_riddle")
        else:
            puzzles.append("complex_multi_conditional_maze")
        npcs = ["Guardian of Choices"]
    elif theme.lower() == "recursion":
        elements.append("spiral_structures")
        elements.append("echoing_chambers")
        if difficulty < 0.5:
            puzzles.append("simple_recursive_descent")
        else:
            puzzles.append("fractal_pattern_generation_challenge")
        npcs = ["Echoing Sage"]
    else: # General or unspecified theme
        elements.append("modular_structures")
        elements.append("interconnected_systems")
        if difficulty < 0.5:
            puzzles.append("basic_function_composition")
        else:
            puzzles.append("object_oriented_design_challenge")
        npcs = ["Wise Architect"]

    print(f"Generated world segment {segment_id} with theme: {theme}, difficulty: {difficulty}.")

    return {
        "status": "success",
        "segment_id": segment_id,
        "theme": theme,
        "difficulty": difficulty,
        "elements": elements,
        "puzzles": puzzles,
        "npcs": npcs,
        "description": f"A {theme} themed area, with {len(puzzles)} coding puzzles to solve."
    }

def evolve_npc_behavior(npc_id: str, player_interaction_data: dict, current_npc_trait: str = "neutral") -> dict:
    """Adjusts an NPC's behavior or dialogue based on player coding interactions.
    This version simulates simple trait evolution based on interaction data.
    """
    print(f"Evolving NPC {npc_id} with current trait '{current_npc_trait}' based on player interactions.")
    
    new_trait = current_npc_trait

    # Simulate trait evolution based on player's performance or interaction type
    interaction_type = player_interaction_data.get('type', 'unknown')
    player_performance_score = player_interaction_data.get('score', 0) # e.g., correct answers, code efficiency

    if interaction_type == "solved_challenge" and player_performance_score > 0.8:
        if current_npc_trait == "neutral":
            new_trait = "impressed"
        elif current_npc_trait == "skeptical":
            new_trait = "curious"
        elif current_npc_trait == "friendly":
            new_trait = "admiring"
    elif interaction_type == "failed_challenge" and player_performance_score < 0.3:
        if current_npc_trait == "neutral":
            new_trait = "sympathetic"
        elif current_npc_trait == "impressed":
            new_trait = "concerned"
    elif interaction_type == "asked_for_hint":
        if current_npc_trait == "admiring":
            new_trait = "supportive"

    print(f"NPC {npc_id} evolved to new trait: {new_trait}.")
    # In a real system, this would update NPC state in the game engine.

    return {"status": "success", "npc_id": npc_id, "old_trait": current_npc_trait, "new_trait": new_trait} 