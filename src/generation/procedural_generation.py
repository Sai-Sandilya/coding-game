# Core functionalities for Procedural & AI-Generated Content
import random

def generate_quest(player_skill_level: float, concept_focus: str = None, story_progress: dict = None) -> dict:
    """Generates a new coding quest based on player skill, an optional concept focus, and story progression.
    This version generates a more detailed quest structure with dependency chains and narrative hooks.
    """
    if story_progress is None:
        story_progress = {"chapter": 1, "quests_completed_in_chapter": 0}

    quest_id = f"q_{hash(f'{player_skill_level}-{concept_focus}-{story_progress['chapter']}-{story_progress['quests_completed_in_chapter']}-{hash(str(random.random()))}') & 0xFFFFFFFF}" # Generate a unique ID
    
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

    # Simulate quest dependencies and story progression
    required_quests = []
    narrative_hook = "The ancient runes glow faintly, waiting for your code to awaken them."
    if story_progress['chapter'] == 2 and story_progress['quests_completed_in_chapter'] < 3:
        required_quests.append(f"q_chapter1_quest{story_progress['quests_completed_in_chapter'] + 1}")
        narrative_hook = "Whispers of ancient code echo in the air, hinting at a larger mystery."

    print(f"Generated quest {quest_id} for skill level {player_skill_level}, focusing on {concept_focus or 'any'}. Chapter: {story_progress['chapter']}.")

    return {
        "status": "success",
        "quest_id": quest_id,
        "title": title,
        "description": description,
        "objectives": [f"Solve the {selected_concept} puzzle"],
        "narrative_hook": narrative_hook,
        "hints": hints,
        "required_skills": [selected_concept],
        "required_quests": required_quests, # New: Quest dependencies
        "reward": {"exp": 100, "item": "Logic Gem"}
    }

def generate_world_segment(theme: str, difficulty: float, environment_type: str = "forest", dynamic_events: list = None) -> dict:
    """Generates a segment of the game world with a specific coding theme, difficulty, environment, and dynamic events.
    This version generates more detailed world elements, puzzles, interactive NPCs, and environmental effects.
    """
    if dynamic_events is None:
        dynamic_events = []

    segment_id = f"ws_{hash(f'{theme}-{difficulty}-{environment_type}-{str(dynamic_events)}-{hash(str(random.random()))}') & 0xFFFFFFFF}" # Unique ID
    
    elements = []
    puzzles = []
    npcs = []
    interactive_objects = []
    environmental_effects = [] # New: environmental effects

    # Define elements based on theme and environment
    if theme.lower() == "loops":
        elements.extend(["repeating_patterns", "automated_machinery"])
        if environment_type == "forest":
            elements.append("winding_paths")
        elif environment_type == "desert":
            elements.append("sand_dunes")
        npcs.append("Ancient Looper")
        puzzles.append("for_loop_puzzle" if difficulty < 0.5 else "while_loop_optimization")
        interactive_objects.append("ancient_contraption_that_repeats_actions")
    elif theme.lower() == "conditionals":
        elements.extend(["branching_paths", "decision_gates"])
        if environment_type == "forest":
            elements.append("misty_forks")
        elif environment_type == "desert":
            elements.append("shifting_sands")
        npcs.append("Guardian of Choices")
        puzzles.append("if_else_riddle" if difficulty < 0.5 else "nested_conditional_challenge")
        interactive_objects.append("enchanted_door_with_conditions")
    elif theme.lower() == "recursion":
        elements.extend(["spiral_structures", "echoing_chambers"])
        if environment_type == "forest":
            elements.append("treacherous_vines")
        elif environment_type == "desert":
            elements.append("recursive_oasis")
        npcs.append("Echoing Sage")
        puzzles.append("recursive_descent_puzzle" if difficulty < 0.5 else "fractal_pattern_generation")
        interactive_objects.append("mystical_mirror_reflecting_code")
    else: # General or unspecified theme
        elements.extend(["modular_structures", "interconnected_systems"])
        npcs.append("Wise Architect")
        puzzles.append("basic_function_composition" if difficulty < 0.5 else "object_oriented_design_challenge")
        interactive_objects.append("glowing_runic_console")

    # Add general environment elements
    if environment_type == "forest":
        elements.extend(["dense_foliage", "ancient_trees"])
        if "rain_storm" in dynamic_events:
            environmental_effects.append("heavy_rain")
            environmental_effects.append("muddy_paths_affect_movement")
    elif environment_type == "desert":
        elements.extend(["scorching_sands", "rock_formations"])
        if "sand_storm" in dynamic_events:
            environmental_effects.append("blinding_sandstorm")
            environmental_effects.append("reduced_visibility")
    elif environment_type == "mountains":
        elements.extend(["craggy_peaks", "hidden_caves"])
        if "rockfall" in dynamic_events:
            environmental_effects.append("falling_boulders")
            environmental_effects.append("blocked_paths")

    print(f"Generated world segment {segment_id} with theme: {theme}, difficulty: {difficulty}, environment: {environment_type}. Events: {len(dynamic_events)}.")

    return {
        "status": "success",
        "segment_id": segment_id,
        "theme": theme,
        "difficulty": difficulty,
        "environment_type": environment_type,
        "elements": elements,
        "puzzles": puzzles,
        "npcs": npcs,
        "interactive_objects": interactive_objects,
        "environmental_effects": environmental_effects, # New: environmental effects
        "description": f"A {environment_type} themed area, with {len(puzzles)} coding puzzles to solve and {len(interactive_objects)} interactive elements, and {len(environmental_effects)} environmental effects."
    }

def evolve_npc_behavior(npc_id: str, player_interaction_data: dict, current_npc_trait: str = "neutral", npc_state: dict = None) -> dict:
    """Adjusts an NPC's behavior or dialogue based on player coding interactions.
    This version simulates trait evolution, potential changes in quest offerings/dialogue trees, and reactions to code style/ethics.
    """
    if npc_state is None:
        npc_state = {"dialogue_tree_id": "initial", "quest_offer_id": "none", "offered_quest_difficulty": "medium"}

    print(f"Evolving NPC {npc_id} with current trait '{current_npc_trait}' based on player interactions.")
    
    new_trait = current_npc_trait
    new_dialogue_tree_id = npc_state['dialogue_tree_id']
    new_quest_offer_id = npc_state['quest_offer_id']
    new_offered_quest_difficulty = npc_state['offered_quest_difficulty']

    # Simulate trait evolution based on player's performance or interaction type
    interaction_type = player_interaction_data.get('type', 'unknown')
    player_performance_score = player_interaction_data.get('score', 0) # e.g., correct answers, code efficiency
    player_code_style_quality = player_interaction_data.get('code_style_quality', 0.5) # From 0 to 1
    player_ethical_score_change = player_interaction_data.get('ethical_score_change', 0) # From -100 to 100

    if interaction_type == "solved_challenge" and player_performance_score > 0.8:
        if current_npc_trait == "neutral":
            new_trait = "impressed"
            new_dialogue_tree_id = "impressed_dialogue"
            new_quest_offer_id = "advanced_challenge"
            new_offered_quest_difficulty = "hard" # Offer harder quest
        elif current_npc_trait == "skeptical":
            new_trait = "curious"
            new_dialogue_tree_id = "curious_dialogue"
        elif current_npc_trait == "friendly":
            new_trait = "admiring"
            new_quest_offer_id = "master_quest_line"
            new_offered_quest_difficulty = "expert" # Offer very hard quest
    elif interaction_type == "failed_challenge" and player_performance_score < 0.3:
        if current_npc_trait == "neutral":
            new_trait = "sympathetic"
            new_dialogue_tree_id = "sympathetic_dialogue"
            new_quest_offer_id = "tutorial_review"
            new_offered_quest_difficulty = "easy" # Suggest easier content
        elif current_npc_trait == "impressed":
            new_trait = "concerned"
            new_dialogue_tree_id = "concerned_dialogue"
    elif interaction_type == "asked_for_hint":
        if current_npc_trait == "admiring":
            new_trait = "supportive"
            new_dialogue_tree_id = "supportive_dialogue"

    # NPC reaction to code style
    if player_code_style_quality < 0.3:
        new_dialogue_tree_id = "style_critique_dialogue" # NPC comments on bad style
    elif player_code_style_quality > 0.8:
        new_dialogue_tree_id = "style_praise_dialogue" # NPC praises good style

    # NPC reaction to ethical choices
    if player_ethical_score_change < -20:
        new_trait = "disapproving"
        new_dialogue_tree_id = "ethical_warning_dialogue"
    elif player_ethical_score_change > 20:
        new_trait = "approving"
        new_dialogue_tree_id = "ethical_praise_dialogue"

    print(f"NPC {npc_id} evolved to new trait: {new_trait}. New dialogue: {new_dialogue_tree_id}. New quest offer: {new_quest_offer_id} (Difficulty: {new_offered_quest_difficulty}).")
    # In a real system, this would update NPC state in the game engine.

    return {"status": "success", "npc_id": npc_id, "old_trait": current_npc_trait, "new_trait": new_trait, "new_dialogue_tree_id": new_dialogue_tree_id, "new_quest_offer_id": new_quest_offer_id, "offered_quest_difficulty": new_offered_quest_difficulty} 