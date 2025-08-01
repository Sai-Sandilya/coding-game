# Core functionalities for Privacy & Ethics Lab

def generate_ethical_dilemma_quest(dilemma_type: str, complexity_level: str = "medium") -> dict:
    """Generates a quest that presents a coding-related ethical dilemma with specified type and complexity.
    """
    quest_id = f"eth_{hash(f'{dilemma_type}-{complexity_level}-{hash(str(import random; random.random()))}') & 0xFFFFFFFF}" # Unique ID
    
    # Define various ethical dilemma scenarios
    scenarios = {
        "data_privacy": {
            "easy": {
                "title": "The Anonymous Data Collection",
                "description": "Your task is to write code to collect user data for a new social app. Design it to collect only necessary data and ensure user anonymity.",
                "ethical_considerations": ["data minimization", "anonymization"]
            },
            "medium": {
                "title": "The Targeted Ad Dilemma",
                "description": "Develop an advertising system. How will you target ads while respecting user privacy and avoiding manipulative practices?",
                "ethical_considerations": ["informed consent", "data usage transparency"]
            },
            "hard": {
                "title": "The Surveillance Algorithm",
                "description": "You are tasked with building a city-wide surveillance system using AI. How do you balance public safety with individual privacy and avoid bias?",
                "ethical_considerations": ["algorithmic bias", "transparency", "accountability"]
            }
        },
        "ai_bias": {
            "easy": {
                "title": "Fairness in Recommendations",
                "description": "Your AI recommends content. How do you ensure it doesn't unfairly exclude certain creators or demographics?",
                "ethical_considerations": ["representational bias", "fairness metrics"]
            },
            "medium": {
                "title": "Algorithmic Hiring Decisions",
                "description": "Develop an AI to screen job applicants. How do you design it to avoid gender or racial bias in its selections?",
                "ethical_considerations": ["disparate impact", "feature selection bias"]
            },
            "hard": {
                "title": "Autonomous Decision-Making",
                "description": "An autonomous vehicle's AI must make a split-second decision in a no-win scenario. How do you program its ethics?",
                "ethical_considerations": ["moral philosophy in AI", "responsibility frameworks"]
            }
        },
        "security_vulnerabilities": {
             "easy": {
                "title": "Simple Password Protection",
                "description": "Implement a secure way to store user passwords for a small application. Focus on basic hashing.",
                "ethical_considerations": ["data security basics", "hashing"]
            },
            "medium": {
                "title": "Cross-Site Scripting Defense",
                "description": "A web application is vulnerable to XSS attacks. Write code to sanitize user input and prevent malicious scripts.",
                "ethical_considerations": ["input validation", "sanitization"]
            },
            "hard": {
                "title": "Supply Chain Security Audit",
                "description": "You discover a critical vulnerability in a third-party library. How do you responsibly disclose it and minimize harm?",
                "ethical_considerations": ["responsible disclosure", "supply chain risk"]
            }
        }
    }

    selected_scenario = scenarios.get(dilemma_type, {}).get(complexity_level, {})

    title = selected_scenario.get('title', "Ethical Challenge")
    description = selected_scenario.get('description', "An ethical coding challenge awaits!")
    ethical_considerations = selected_scenario.get('ethical_considerations', [])

    print(f"Generating ethical dilemma quest {quest_id} of type '{dilemma_type}' and complexity '{complexity_level}'.")

    return {
        "status": "success",
        "quest_id": quest_id,
        "title": title,
        "description": description,
        "dilemma_type": dilemma_type,
        "complexity_level": complexity_level,
        "ethical_considerations": ethical_considerations,
        "reward": {"moral_points": 50, "badge": "Ethical Coder"}
    }

def simulate_code_impact(code_solution: str, scenario_id: str) -> dict:
    """Simulates the societal or virtual world impact of a player's code solution.
    This version provides more detailed impact reports based on the code solution.
    """
    print(f"Simulating impact of code solution for scenario {scenario_id}.")
    
    impact_report = ""
    ethical_score_change = 0 # -100 to +100

    code_solution_lower = code_solution.lower()

    # Simulate impact based on keywords and scenario
    if "privacy" in code_solution_lower and "data" in code_solution_lower:
        if "encrypt" in code_solution_lower or "anonymize" in code_solution_lower:
            impact_report += "Positive impact: Data privacy measures are strong. User trust increased.\n"
            ethical_score_change += 30
        elif "collect_all" in code_solution_lower or "share_third_party" in code_solution_lower:
            impact_report += "Negative impact: Excessive data collection or sharing detected. Privacy concerns raised.\n"
            ethical_score_change -= 40
    
    if "bias" in code_solution_lower or "fairness" in code_solution_lower:
        if "debias" in code_solution_lower or "equal_opportunity" in code_solution_lower:
            impact_report += "Positive impact: Efforts made to reduce algorithmic bias. Promotes fairness.\n"
            ethical_score_change += 25
        elif "discriminat" in code_solution_lower or "favor_group" in code_solution_lower:
            impact_report += "Negative impact: Potential for algorithmic discrimination identified. Review for bias.\n"
            ethical_score_change -= 35
    
    if "security" in code_solution_lower or "vulnerab" in code_solution_lower:
        if "secure_hash" in code_solution_lower or "input_sanitize" in code_solution_lower:
            impact_report += "Positive impact: Robust security practices implemented. System is more resilient.\n"
            ethical_score_change += 20
        elif "weak_pass" in code_solution_lower or "sql_inject" in code_solution_lower:
            impact_report += "Negative impact: Security vulnerabilities detected. System is at risk.\n"
            ethical_score_change -= 30

    if not impact_report:
        impact_report = "Neutral impact: Code appears to have no significant ethical implications (simulated)."

    return {"status": "success", "scenario": scenario_id, "impact_report": impact_report, "ethical_score_change": ethical_score_change} 