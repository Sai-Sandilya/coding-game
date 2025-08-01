# Core functionalities for Privacy & Ethics Lab
import random

def generate_ethical_dilemma_quest(dilemma_type: str, complexity_level: str = "medium") -> dict:
    """Generates a quest that presents a coding-related ethical dilemma with specified type and complexity.
    This version includes potential consequences for different choices within the ethical dilemmas.
    """
    quest_id = f"eth_{hash(f'{dilemma_type}-{complexity_level}-{hash(str(random.random()))}') & 0xFFFFFFFF}" # Unique ID
    
    # Define various ethical dilemma scenarios
    scenarios = {
        "data_privacy": {
            "easy": {
                "title": "The Anonymous Data Collection",
                "description": "Your task is to write code to collect user data for a new social app. Design it to collect only necessary data and ensure user anonymity.",
                "ethical_considerations": ["data minimization", "anonymization"],
                "choices": [
                    {"action": "Collect minimal anonymous data", "consequence": "Users trust the app more, but some advanced features are limited.", "ethical_impact": 20},
                    {"action": "Collect all data for future use", "consequence": "More features enabled, but public privacy concerns rise and data breach risk increases.", "ethical_impact": -30}
                ]
            },
            "medium": {
                "title": "The Targeted Ad Dilemma",
                "description": "Develop an advertising system. How will you target ads while respecting user privacy and avoiding manipulative practices?",
                "ethical_considerations": ["informed consent", "data usage transparency"],
                "choices": [
                    {"action": "Require explicit consent for all targeting", "consequence": "Ad revenue slightly lower, but users feel respected and privacy complaints are minimal.", "ethical_impact": 25},
                    {"action": "Infer user interests from behavior", "consequence": "Higher ad revenue, but some users feel tracked and privacy advocates raise concerns.", "ethical_impact": -20}
                ]
            },
            "hard": {
                "title": "The Surveillance Algorithm",
                "description": "You are tasked with building a city-wide surveillance system using AI. How do you balance public safety with individual privacy and avoid bias?",
                "ethical_considerations": ["algorithmic bias", "transparency", "accountability"],
                "choices": [
                    {"action": "Implement strict oversight and bias audits", "consequence": "System is slower to deploy and more costly, but public trust is higher and false positives are reduced.", "ethical_impact": 40},
                    {"action": "Prioritize efficiency over extensive oversight", "consequence": "System is deployed quickly and efficiently, but incidents of false accusations or biased enforcement increase.", "ethical_impact": -50}
                ]
            }
        },
        "ai_bias": {
            "easy": {
                "title": "Fairness in Recommendations",
                "description": "Your AI recommends content. How do you ensure it doesn't unfairly exclude certain creators or demographics?",
                "ethical_considerations": ["representational bias", "fairness metrics"],
                "choices": [
                    {"action": "Actively diversify recommendation sources", "consequence": "Recommendations become broader, appealing to a wider audience, but some user engagement metrics may dip initially.", "ethical_impact": 20},
                    {"action": "Optimize solely for engagement metrics", "consequence": "High engagement with popular content, but niche creators and diverse perspectives are marginalized.", "ethical_impact": -30}
                ]
            },
            "medium": {
                "title": "Algorithmic Hiring Decisions",
                "description": "Develop an AI to screen job applicants. How do you design it to avoid gender or racial bias in its selections?",
                "ethical_considerations": ["disparate impact", "feature selection bias"],
                "choices": [
                    {"action": "Conduct regular bias audits and retrain with balanced data", "consequence": "Hiring process is slower but more equitable, leading to a more diverse workforce and positive public image.", "ethical_impact": 35},
                    {"action": "Use historical data without bias mitigation", "consequence": "Hiring is efficient, but existing biases in historical data are perpetuated, leading to a less diverse workforce and potential lawsuits.", "ethical_impact": -45}
                ]
            },
            "hard": {
                "title": "Autonomous Decision-Making",
                "description": "An autonomous vehicle's AI must make a split-second decision in a no-win scenario. How do you program its ethics?",
                "ethical_considerations": ["moral philosophy in AI", "responsibility frameworks"],
                "choices": [
                    {"action": "Prioritize minimizing harm to vulnerable road users", "consequence": "Public trust in safety increases, but difficult ethical dilemmas remain in edge cases.", "ethical_impact": 50},
                    {"action": "Prioritize passenger safety above all else", "consequence": "Passengers feel secure, but external harm is prioritized less, leading to ethical debate.", "ethical_impact": -40}
                ]
            }
        },
        "security_vulnerabilities": {
             "easy": {
                "title": "Simple Password Protection",
                "description": "Implement a secure way to store user passwords for a small application. Focus on basic hashing.",
                "ethical_considerations": ["data security basics", "hashing"],
                "choices": [
                    {"action": "Use strong, salted hashing algorithms", "consequence": "User passwords are secure, but the implementation is slightly more complex.", "ethical_impact": 20},
                    {"action": "Store passwords as plain text or weak hash", "consequence": "Quick to implement, but a single breach exposes all user passwords, leading to massive trust loss.", "ethical_impact": -50}
                ]
            },
            "medium": {
                "title": "Cross-Site Scripting Defense",
                "description": "A web application is vulnerable to XSS attacks. Write code to sanitize user input and prevent malicious scripts.",
                "ethical_considerations": ["input validation", "sanitization"],
                "choices": [
                    {"action": "Implement robust input sanitization and output encoding", "consequence": "Application is secure from XSS, but some user input might be slightly restricted.", "ethical_impact": 30},
                    {"action": "Allow unfiltered user input for flexibility", "consequence": "More user flexibility, but the application is highly vulnerable to XSS attacks, potentially leading to data theft.", "ethical_impact": -40}
                ]
            },
            "hard": {
                "title": "Supply Chain Security Audit",
                "description": "You discover a critical vulnerability in a third-party library. How do you responsibly disclose it and minimize harm?",
                "ethical_considerations": ["responsible disclosure", "supply chain risk"],
                "choices": [
                    {"action": "Follow a coordinated vulnerability disclosure process", "consequence": "Vulnerability is patched responsibly, maintaining trust and minimizing disruption across the ecosystem.", "ethical_impact": 45},
                    {"action": "Publicly disclose immediately without vendor notification", "consequence": "Raises awareness quickly, but may lead to immediate exploitation before a patch is available, causing widespread damage.", "ethical_impact": -35}
                ]
            }
        }
    }

    selected_scenario = scenarios.get(dilemma_type, {}).get(complexity_level, {})

    title = selected_scenario.get('title', "Ethical Challenge")
    description = selected_scenario.get('description', "An ethical coding challenge awaits!")
    ethical_considerations = selected_scenario.get('ethical_considerations', [])
    choices = selected_scenario.get('choices', [])

    print(f"Generating ethical dilemma quest {quest_id} of type '{dilemma_type}' and complexity '{complexity_level}'.")

    return {
        "status": "success",
        "quest_id": quest_id,
        "title": title,
        "description": description,
        "dilemma_type": dilemma_type,
        "complexity_level": complexity_level,
        "ethical_considerations": ethical_considerations,
        "choices": choices, # New: potential choices and consequences
        "reward": {"moral_points": 50, "badge": "Ethical Coder"}
    }

def simulate_code_impact(code_solution: str, scenario_id: str, current_public_opinion: float = 0.5) -> dict:
    """Simulates the societal or virtual world impact of a player's code solution.
    This version provides more detailed impact reports and simulates changes in public opinion/reputation.
    """
    print(f"Simulating impact of code solution for scenario {scenario_id}. Current public opinion: {current_public_opinion}.")
    
    impact_report = ""
    ethical_score_change = 0 # -100 to +100
    public_opinion_change = 0.0 # -1.0 to +1.0

    code_solution_lower = code_solution.lower()

    # Simulate impact based on keywords and scenario
    if "privacy" in code_solution_lower and "data" in code_solution_lower:
        if "encrypt" in code_solution_lower or "anonymize" in code_solution_lower:
            impact_report += "Positive impact: Data privacy measures are strong. User trust increased.\n"
            ethical_score_change += 30
            public_opinion_change += 0.15
        elif "collect_all" in code_solution_lower or "share_third_party" in code_solution_lower:
            impact_report += "Negative impact: Excessive data collection or sharing detected. Privacy concerns raised.\n"
            ethical_score_change -= 40
            public_opinion_change -= 0.2
    
    if "bias" in code_solution_lower or "fairness" in code_solution_lower:
        if "debias" in code_solution_lower or "equal_opportunity" in code_solution_lower:
            impact_report += "Positive impact: Efforts made to reduce algorithmic bias. Promotes fairness.\n"
            ethical_score_change += 25
            public_opinion_change += 0.1
        elif "discriminat" in code_solution_lower or "favor_group" in code_solution_lower:
            impact_report += "Negative impact: Potential for algorithmic discrimination identified. Review for bias.\n"
            ethical_score_change -= 35
            public_opinion_change -= 0.18
    
    if "security" in code_solution_lower or "vulnerab" in code_solution_lower:
        if "secure_hash" in code_solution_lower or "input_sanitize" in code_solution_lower:
            impact_report += "Positive impact: Robust security practices implemented. System is more resilient.\n"
            ethical_score_change += 20
            public_opinion_change += 0.08
        elif "weak_pass" in code_solution_lower or "sql_inject" in code_solution_lower:
            impact_report += "Negative impact: Security vulnerabilities detected. System is at risk.\n"
            ethical_score_change -= 30
            public_opinion_change -= 0.15

    if not impact_report:
        impact_report = "Neutral impact: Code appears to have no significant ethical implications (simulated)."

    new_public_opinion = current_public_opinion + public_opinion_change
    new_public_opinion = max(0.0, min(1.0, new_public_opinion)) # Clamp between 0 and 1

    print(f"Simulated ethical score change: {ethical_score_change}. New public opinion: {new_public_opinion}.")

    return {"status": "success", "scenario": scenario_id, "impact_report": impact_report, "ethical_score_change": ethical_score_change, "new_public_opinion": new_public_opinion} 