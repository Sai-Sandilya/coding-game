# Core functionalities for Privacy & Ethics Lab
import random
import json
from typing import Dict, List, Tuple, Optional

def generate_ethical_dilemma_quest(dilemma_type: str, complexity_level: str = "medium", player_ethical_history: Optional[Dict] = None) -> dict:
    """Generates a quest that presents a coding-related ethical dilemma with specified type and complexity.
    This version includes dynamic consequences based on player's ethical history and more sophisticated scenario generation.
    """
    quest_id = f"eth_{hash(f'{dilemma_type}-{complexity_level}-{hash(str(random.random()))}') & 0xFFFFFFFF}" # Unique ID
    
    # Define various ethical dilemma scenarios with enhanced complexity
    scenarios = {
        "data_privacy": {
            "easy": {
                "title": "The Anonymous Data Collection",
                "description": "Your task is to write code to collect user data for a new social app. Design it to collect only necessary data and ensure user anonymity.",
                "ethical_considerations": ["data minimization", "anonymization", "consent"],
                "stakeholders": ["users", "developers", "business"],
                "choices": [
                    {"action": "Collect minimal anonymous data", "consequence": "Users trust the app more, but some advanced features are limited.", "ethical_impact": 20, "stakeholder_effects": {"users": 15, "business": -10}},
                    {"action": "Collect all data for future use", "consequence": "More features enabled, but public privacy concerns rise and data breach risk increases.", "ethical_impact": -30, "stakeholder_effects": {"users": -25, "business": 20}}
                ]
            },
            "medium": {
                "title": "The Targeted Ad Dilemma",
                "description": "Develop an advertising system. How will you target ads while respecting user privacy and avoiding manipulative practices?",
                "ethical_considerations": ["informed consent", "data usage transparency", "algorithmic manipulation"],
                "stakeholders": ["users", "advertisers", "platform"],
                "choices": [
                    {"action": "Require explicit consent for all targeting", "consequence": "Ad revenue slightly lower, but users feel respected and privacy complaints are minimal.", "ethical_impact": 25, "stakeholder_effects": {"users": 20, "advertisers": -15, "platform": -5}},
                    {"action": "Infer user interests from behavior", "consequence": "Higher ad revenue, but some users feel tracked and privacy advocates raise concerns.", "ethical_impact": -20, "stakeholder_effects": {"users": -15, "advertisers": 25, "platform": 10}}
                ]
            },
            "hard": {
                "title": "The Surveillance Algorithm",
                "description": "You are tasked with building a city-wide surveillance system using AI. How do you balance public safety with individual privacy and avoid bias?",
                "ethical_considerations": ["algorithmic bias", "transparency", "accountability", "surveillance ethics"],
                "stakeholders": ["citizens", "law_enforcement", "civil_rights_groups"],
                "choices": [
                    {"action": "Implement strict oversight and bias audits", "consequence": "System is slower to deploy and more costly, but public trust is higher and false positives are reduced.", "ethical_impact": 40, "stakeholder_effects": {"citizens": 30, "law_enforcement": -20, "civil_rights_groups": 35}},
                    {"action": "Prioritize efficiency over extensive oversight", "consequence": "System is deployed quickly and efficiently, but incidents of false accusations or biased enforcement increase.", "ethical_impact": -50, "stakeholder_effects": {"citizens": -40, "law_enforcement": 25, "civil_rights_groups": -45}}
                ]
            }
        },
        "ai_bias": {
            "easy": {
                "title": "Fairness in Recommendations",
                "description": "Your AI recommends content. How do you ensure it doesn't unfairly exclude certain creators or demographics?",
                "ethical_considerations": ["representational bias", "fairness metrics", "diversity"],
                "stakeholders": ["users", "content_creators", "platform"],
                "choices": [
                    {"action": "Actively diversify recommendation sources", "consequence": "Recommendations become broader, appealing to a wider audience, but some user engagement metrics may dip initially.", "ethical_impact": 20, "stakeholder_effects": {"users": 15, "content_creators": 25, "platform": -10}},
                    {"action": "Optimize solely for engagement metrics", "consequence": "High engagement with popular content, but niche creators and diverse perspectives are marginalized.", "ethical_impact": -30, "stakeholder_effects": {"users": 5, "content_creators": -20, "platform": 15}}
                ]
            },
            "medium": {
                "title": "Algorithmic Hiring Decisions",
                "description": "Develop an AI to screen job applicants. How do you design it to avoid gender or racial bias in its selections?",
                "ethical_considerations": ["disparate impact", "feature selection bias", "fairness in employment"],
                "stakeholders": ["applicants", "employers", "society"],
                "choices": [
                    {"action": "Conduct regular bias audits and retrain with balanced data", "consequence": "Hiring process is slower but more equitable, leading to a more diverse workforce and positive public image.", "ethical_impact": 35, "stakeholder_effects": {"applicants": 30, "employers": -15, "society": 25}},
                    {"action": "Use historical data without bias mitigation", "consequence": "Hiring is efficient, but existing biases in historical data are perpetuated, leading to a less diverse workforce and potential lawsuits.", "ethical_impact": -45, "stakeholder_effects": {"applicants": -35, "employers": 10, "society": -30}}
                ]
            },
            "hard": {
                "title": "Autonomous Decision-Making",
                "description": "An autonomous vehicle's AI must make a split-second decision in a no-win scenario. How do you program its ethics?",
                "ethical_considerations": ["moral philosophy in AI", "responsibility frameworks", "trolley problem"],
                "stakeholders": ["passengers", "pedestrians", "manufacturers", "regulators"],
                "choices": [
                    {"action": "Prioritize minimizing harm to vulnerable road users", "consequence": "Public trust in safety increases, but difficult ethical dilemmas remain in edge cases.", "ethical_impact": 50, "stakeholder_effects": {"passengers": -10, "pedestrians": 40, "manufacturers": 20, "regulators": 30}},
                    {"action": "Prioritize passenger safety above all else", "consequence": "Passengers feel secure, but external harm is prioritized less, leading to ethical debate.", "ethical_impact": -40, "stakeholder_effects": {"passengers": 25, "pedestrians": -30, "manufacturers": -15, "regulators": -25}}
                ]
            }
        },
        "security_vulnerabilities": {
             "easy": {
                "title": "Simple Password Protection",
                "description": "Implement a secure way to store user passwords for a small application. Focus on basic hashing.",
                "ethical_considerations": ["data security basics", "hashing", "user responsibility"],
                "stakeholders": ["users", "developers", "attackers"],
                "choices": [
                    {"action": "Use strong, salted hashing algorithms", "consequence": "User passwords are secure, but the implementation is slightly more complex.", "ethical_impact": 20, "stakeholder_effects": {"users": 25, "developers": -5, "attackers": -30}},
                    {"action": "Store passwords as plain text or weak hash", "consequence": "Quick to implement, but a single breach exposes all user passwords, leading to massive trust loss.", "ethical_impact": -50, "stakeholder_effects": {"users": -45, "developers": 10, "attackers": 40}}
                ]
            },
            "medium": {
                "title": "Cross-Site Scripting Defense",
                "description": "A web application is vulnerable to XSS attacks. Write code to sanitize user input and prevent malicious scripts.",
                "ethical_considerations": ["input validation", "sanitization", "defense in depth"],
                "stakeholders": ["users", "developers", "attackers"],
                "choices": [
                    {"action": "Implement robust input sanitization and output encoding", "consequence": "Application is secure from XSS, but some user input might be slightly restricted.", "ethical_impact": 30, "stakeholder_effects": {"users": 20, "developers": -10, "attackers": -40}},
                    {"action": "Allow unfiltered user input for flexibility", "consequence": "More user flexibility, but the application is highly vulnerable to XSS attacks, potentially leading to data theft.", "ethical_impact": -40, "stakeholder_effects": {"users": -30, "developers": 15, "attackers": 35}}
                ]
            },
            "hard": {
                "title": "Supply Chain Security Audit",
                "description": "You discover a critical vulnerability in a third-party library. How do you responsibly disclose it and minimize harm?",
                "ethical_considerations": ["responsible disclosure", "supply chain risk", "coordinated response"],
                "stakeholders": ["users", "library_maintainers", "security_researchers"],
                "choices": [
                    {"action": "Follow a coordinated vulnerability disclosure process", "consequence": "Vulnerability is patched responsibly, maintaining trust and minimizing disruption across the ecosystem.", "ethical_impact": 45, "stakeholder_effects": {"users": 30, "library_maintainers": 20, "security_researchers": 25}},
                    {"action": "Publicly disclose immediately without vendor notification", "consequence": "Raises awareness quickly, but may lead to immediate exploitation before a patch is available, causing widespread damage.", "ethical_impact": -35, "stakeholder_effects": {"users": -25, "library_maintainers": -30, "security_researchers": 15}}
                ]
            }
        }
    }

    selected_scenario = scenarios.get(dilemma_type, {}).get(complexity_level, {})

    title = selected_scenario.get('title', "Ethical Challenge")
    description = selected_scenario.get('description', "An ethical coding challenge awaits!")
    ethical_considerations = selected_scenario.get('ethical_considerations', [])
    stakeholders = selected_scenario.get('stakeholders', [])
    choices = selected_scenario.get('choices', [])

    # Adjust choices based on player's ethical history
    if player_ethical_history:
        ethical_trend = player_ethical_history.get('recent_ethical_score', 0)
        if ethical_trend > 20:
            # Player has been making ethical choices - present more nuanced dilemmas
            for choice in choices:
                choice['ethical_impact'] = int(choice['ethical_impact'] * 0.8)  # Slightly reduce impact
        elif ethical_trend < -20:
            # Player has been making unethical choices - present more obvious ethical choices
            for choice in choices:
                if choice['ethical_impact'] > 0:
                    choice['ethical_impact'] = int(choice['ethical_impact'] * 1.2)  # Increase positive impact

    print(f"Generating ethical dilemma quest {quest_id} of type '{dilemma_type}' and complexity '{complexity_level}'.")

    return {
        "status": "success",
        "quest_id": quest_id,
        "title": title,
        "description": description,
        "dilemma_type": dilemma_type,
        "complexity_level": complexity_level,
        "ethical_considerations": ethical_considerations,
        "stakeholders": stakeholders,
        "choices": choices,
        "reward": {"moral_points": 50, "badge": "Ethical Coder"}
    }

def simulate_code_impact(code_solution: str, scenario_id: str, current_public_opinion: float = 0.5, stakeholder_weights: Optional[Dict] = None) -> dict:
    """Simulates the societal or virtual world impact of a player's code solution.
    This version provides more detailed impact reports with stakeholder analysis and simulates changes in public opinion/reputation.
    """
    print(f"Simulating impact of code solution for scenario {scenario_id}. Current public opinion: {current_public_opinion}.")
    
    impact_report = ""
    ethical_score_change = 0 # -100 to +100
    public_opinion_change = 0.0 # -1.0 to +1.0
    stakeholder_impacts = {}

    code_solution_lower = code_solution.lower()

    # Enhanced keyword analysis with more sophisticated detection
    privacy_keywords = ["privacy", "data", "encrypt", "anonymize", "consent", "gdpr", "ccpa"]
    bias_keywords = ["bias", "fairness", "debias", "equal_opportunity", "discriminat", "favor_group"]
    security_keywords = ["security", "vulnerab", "secure_hash", "input_sanitize", "weak_pass", "sql_inject"]
    transparency_keywords = ["transparent", "audit", "explainable", "accountable", "oversight"]

    # Privacy impact analysis
    privacy_score = sum(1 for keyword in privacy_keywords if keyword in code_solution_lower)
    if privacy_score > 0:
        if any(pos in code_solution_lower for pos in ["encrypt", "anonymize", "consent"]):
            impact_report += "Positive impact: Strong data privacy measures implemented. User trust increased.\n"
            ethical_score_change += 30
            public_opinion_change += 0.15
            stakeholder_impacts["users"] = 20
            stakeholder_impacts["regulators"] = 15
        elif any(neg in code_solution_lower for neg in ["collect_all", "share_third_party", "tracking"]):
            impact_report += "Negative impact: Excessive data collection or sharing detected. Privacy concerns raised.\n"
            ethical_score_change -= 40
            public_opinion_change -= 0.2
            stakeholder_impacts["users"] = -25
            stakeholder_impacts["regulators"] = -20
    
    # Bias impact analysis
    bias_score = sum(1 for keyword in bias_keywords if keyword in code_solution_lower)
    if bias_score > 0:
        if any(pos in code_solution_lower for pos in ["debias", "equal_opportunity", "fairness"]):
            impact_report += "Positive impact: Efforts made to reduce algorithmic bias. Promotes fairness and inclusion.\n"
            ethical_score_change += 25
            public_opinion_change += 0.1
            stakeholder_impacts["minority_groups"] = 30
            stakeholder_impacts["society"] = 20
        elif any(neg in code_solution_lower for neg in ["discriminat", "favor_group", "bias"]):
            impact_report += "Negative impact: Potential for algorithmic discrimination identified. Review for bias.\n"
            ethical_score_change -= 35
            public_opinion_change -= 0.18
            stakeholder_impacts["minority_groups"] = -35
            stakeholder_impacts["society"] = -25
    
    # Security impact analysis
    security_score = sum(1 for keyword in security_keywords if keyword in code_solution_lower)
    if security_score > 0:
        if any(pos in code_solution_lower for pos in ["secure_hash", "input_sanitize", "encrypt"]):
            impact_report += "Positive impact: Robust security practices implemented. System is more resilient to attacks.\n"
            ethical_score_change += 20
            public_opinion_change += 0.08
            stakeholder_impacts["users"] = 15
            stakeholder_impacts["security_experts"] = 25
        elif any(neg in code_solution_lower for neg in ["weak_pass", "sql_inject", "plain_text"]):
            impact_report += "Negative impact: Security vulnerabilities detected. System is at risk of compromise.\n"
            ethical_score_change -= 30
            public_opinion_change -= 0.15
            stakeholder_impacts["users"] = -20
            stakeholder_impacts["security_experts"] = -30

    # Transparency impact analysis
    transparency_score = sum(1 for keyword in transparency_keywords if keyword in code_solution_lower)
    if transparency_score > 0:
        impact_report += "Positive impact: Transparency measures implemented. Builds trust and accountability.\n"
        ethical_score_change += 15
        public_opinion_change += 0.05
        stakeholder_impacts["public"] = 15
        stakeholder_impacts["regulators"] = 10

    if not impact_report:
        impact_report = "Neutral impact: Code appears to have no significant ethical implications (simulated)."

    # Apply stakeholder weights if provided
    if stakeholder_weights:
        weighted_public_opinion_change = 0.0
        for stakeholder, impact in stakeholder_impacts.items():
            weight = stakeholder_weights.get(stakeholder, 1.0)
            weighted_public_opinion_change += (impact / 100.0) * weight
        public_opinion_change = weighted_public_opinion_change

    new_public_opinion = current_public_opinion + public_opinion_change
    new_public_opinion = max(0.0, min(1.0, new_public_opinion)) # Clamp between 0 and 1

    print(f"Simulated ethical score change: {ethical_score_change}. New public opinion: {new_public_opinion}.")

    return {
        "status": "success", 
        "scenario": scenario_id, 
        "impact_report": impact_report, 
        "ethical_score_change": ethical_score_change, 
        "new_public_opinion": new_public_opinion,
        "stakeholder_impacts": stakeholder_impacts
    }

def detect_algorithmic_bias(code_solution: str, dataset_info: Optional[Dict] = None) -> dict:
    """Detects potential algorithmic bias in code solutions based on various bias indicators."""
    print(f"Analyzing code solution for algorithmic bias patterns.")
    
    bias_indicators = {
        "gender_bias": 0,
        "racial_bias": 0,
        "age_bias": 0,
        "socioeconomic_bias": 0,
        "geographic_bias": 0
    }
    
    bias_report = ""
    code_lower = code_solution.lower()
    
    # Gender bias detection
    gender_indicators = ["gender", "sex", "male", "female", "man", "woman"]
    if any(indicator in code_lower for indicator in gender_indicators):
        if "gender" in code_lower and "neutral" not in code_lower:
            bias_indicators["gender_bias"] += 30
            bias_report += "Potential gender bias detected: Code references gender without neutral handling.\n"
    
    # Racial bias detection
    racial_indicators = ["race", "ethnicity", "skin_color", "nationality"]
    if any(indicator in code_lower for indicator in racial_indicators):
        if "race" in code_lower and "fair" not in code_lower and "unbiased" not in code_lower:
            bias_indicators["racial_bias"] += 25
            bias_report += "Potential racial bias detected: Race-based logic without fairness measures.\n"
    
    # Age bias detection
    age_indicators = ["age", "young", "old", "senior", "junior"]
    if any(indicator in code_lower for indicator in age_indicators):
        if "age" in code_lower and "discriminat" in code_lower:
            bias_indicators["age_bias"] += 20
            bias_report += "Potential age bias detected: Age-based discrimination logic.\n"
    
    # Socioeconomic bias detection
    socioeconomic_indicators = ["income", "wealth", "education", "zipcode", "postal_code"]
    if any(indicator in code_lower for indicator in socioeconomic_indicators):
        if any(indicator in code_lower for indicator in ["income", "wealth"]) and "equal" not in code_lower:
            bias_indicators["socioeconomic_bias"] += 15
            bias_report += "Potential socioeconomic bias detected: Income/wealth-based logic.\n"
    
    # Geographic bias detection
    geographic_indicators = ["location", "city", "state", "country", "region"]
    if any(indicator in code_lower for indicator in geographic_indicators):
        if "location" in code_lower and "diverse" not in code_lower:
            bias_indicators["geographic_bias"] += 10
            bias_report += "Potential geographic bias detected: Location-based logic without diversity consideration.\n"
    
    total_bias_score = sum(bias_indicators.values())
    bias_level = "low" if total_bias_score < 20 else "medium" if total_bias_score < 50 else "high"
    
    if not bias_report:
        bias_report = "No significant bias indicators detected in the code."
    
    return {
        "status": "success",
        "bias_indicators": bias_indicators,
        "total_bias_score": total_bias_score,
        "bias_level": bias_level,
        "bias_report": bias_report,
        "recommendations": [
            "Consider using gender-neutral language and logic",
            "Implement fairness metrics and bias testing",
            "Use diverse and representative training data",
            "Apply bias mitigation techniques",
            "Conduct regular bias audits"
        ] if total_bias_score > 0 else ["Code appears to be bias-aware"]
    }

def conduct_ethical_code_review(code_solution: str, review_criteria: List[str] = None) -> dict:
    """Conducts a comprehensive ethical code review based on specified criteria."""
    print(f"Conducting ethical code review with criteria: {review_criteria}")
    
    if review_criteria is None:
        review_criteria = ["privacy", "security", "bias", "transparency", "accountability"]
    
    review_results = {}
    overall_score = 0
    max_score = len(review_criteria) * 20  # 20 points per criterion
    
    for criterion in review_criteria:
        criterion_score = 0
        issues = []
        recommendations = []
        
        if criterion == "privacy":
            if "encrypt" in code_solution.lower() or "hash" in code_solution.lower():
                criterion_score += 15
            if "consent" in code_solution.lower():
                criterion_score += 5
            if "data" in code_solution.lower() and "minimize" in code_solution.lower():
                criterion_score += 10
            else:
                issues.append("Data minimization not explicitly addressed")
                recommendations.append("Implement data minimization principles")
        
        elif criterion == "security":
            if "validate" in code_solution.lower() or "sanitize" in code_solution.lower():
                criterion_score += 10
            if "secure" in code_solution.lower():
                criterion_score += 10
            if "vulnerab" in code_solution.lower():
                issues.append("Security vulnerabilities mentioned")
                recommendations.append("Address identified security vulnerabilities")
        
        elif criterion == "bias":
            if "fair" in code_solution.lower() or "unbiased" in code_solution.lower():
                criterion_score += 10
            if "diverse" in code_solution.lower():
                criterion_score += 10
            if "bias" in code_solution.lower():
                issues.append("Bias considerations mentioned")
                recommendations.append("Implement bias detection and mitigation")
        
        elif criterion == "transparency":
            if "transparent" in code_solution.lower() or "explain" in code_solution.lower():
                criterion_score += 10
            if "audit" in code_solution.lower():
                criterion_score += 10
            else:
                issues.append("Transparency measures not evident")
                recommendations.append("Add transparency and explainability features")
        
        elif criterion == "accountability":
            if "log" in code_solution.lower() or "track" in code_solution.lower():
                criterion_score += 10
            if "oversight" in code_solution.lower():
                criterion_score += 10
            else:
                issues.append("Accountability measures not evident")
                recommendations.append("Implement logging and oversight mechanisms")
        
        review_results[criterion] = {
            "score": criterion_score,
            "issues": issues,
            "recommendations": recommendations
        }
        overall_score += criterion_score
    
    overall_percentage = (overall_score / max_score) * 100 if max_score > 0 else 0
    
    return {
        "status": "success",
        "review_criteria": review_criteria,
        "criterion_results": review_results,
        "overall_score": overall_score,
        "max_score": max_score,
        "overall_percentage": overall_percentage,
        "grade": "A" if overall_percentage >= 90 else "B" if overall_percentage >= 80 else "C" if overall_percentage >= 70 else "D" if overall_percentage >= 60 else "F"
    } 