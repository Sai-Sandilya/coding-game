# Core functionalities for Real-World Integration

import json # Added for package.json generation
import hashlib # Added for certificate hash generation
import random # Added for simulated revocation

def export_code_to_ide(project_data: dict, target_format: str = "py") -> dict:
    """Exports in-game code projects to a format compatible with real IDEs.
    This version generates a more realistic file structure with basic project files and dependency management files.
    project_data should contain 'code_content', 'file_name', and optionally 'project_name' and 'dependencies'.
    """
    code_content = project_data.get('code_content', "# No code content provided.")
    file_name = project_data.get('file_name', "main_code")
    project_name = project_data.get('project_name', "exported_codebound_project")
    dependencies = project_data.get('dependencies', []) # e.g., ["flask==2.0.1", "requests>=2.26.0"]

    # Simulate creating a directory and files
    export_base_path = f"exported_projects/{project_name}"
    main_code_path = f"{export_base_path}/{file_name}.{target_format}"
    readme_path = f"{export_base_path}/README.md"
    vscode_settings_path = f"{export_base_path}/.vscode/settings.json"

    exported_files = [
        {"path": main_code_path, "content": code_content},
        {"path": readme_path, "content": f"# {project_name}\n\nThis project was exported from Codebound: The Living Language!\n\nExplore the {file_name}.{target_format} file to see your code."},
        {"path": vscode_settings_path, "content": "{\n  \"python.linting.pylintEnabled\": true,\n  \"python.formatting.provider\": \"black\"\n}" if target_format == "py" else ""} # Example for Python
    ]

    # Add dependency management files based on target_format
    if target_format == "py":
        requirements_content = "\n".join(dependencies) if dependencies else "# No specific Python dependencies"
        exported_files.append({"path": f"{export_base_path}/requirements.txt", "content": requirements_content})
    elif target_format == "js" or target_format == "ts":
        # Simulate a basic package.json
        package_json_content = {
            "name": project_name.lower().replace(" ", "-"),
            "version": "1.0.0",
            "description": "A project exported from Codebound.",
            "main": f"{file_name}.{target_format}",
            "scripts": {"start": f"node {file_name}.{target_format}" if target_format == "js" else f"ts-node {file_name}.{target_format}"},
            "keywords": ["codebound", "game", "learning"],
            "author": "Codebound Player",
            "license": "MIT",
            "dependencies": {dep.split('==')[0]: dep.split('==')[1] if '==' in dep else '*' for dep in dependencies} # Simplified dependency parsing
        }
        exported_files.append({"path": f"{export_base_path}/package.json", "content": json.dumps(package_json_content, indent=2)})

    print(f"Simulating export of project '{project_name}' to {export_base_path} with {len(exported_files)} files.")
    
    # In a real application, these would be actual file system writes.
    # For this simulation, we'll just return the simulated structure.
    
    return {"status": "success", "message": f"Project '{project_name}' exported. Check simulated paths for files.", "exported_files": exported_files, "base_path": export_base_path}

def issue_certification(player_id: str, course_id: str, mastery_level: float, skills_gained: list = None) -> dict:
    """Issues a digital certification or badge for completing a course/quest.
    This version simulates interaction with an external certification API, including blockchain-like verification and revocation status.
    """
    if skills_gained is None:
        skills_gained = []

    # Generate a more robust certificate ID that could serve as a blockchain-like hash
    cert_data_string = f"{player_id}-{course_id}-{mastery_level}-{sorted(skills_gained)}"
    certificate_hash = hashlib.sha256(cert_data_string.encode()).hexdigest()
    certificate_id = f"CB-{certificate_hash[:12]}" # Shortened for display

    # Simulate API call to an external certification service
    external_api_status = "success" # In a real scenario, this would be an actual API call result
    is_revoked = False # Simulate a potential revocation status
    if random.random() < 0.05: # 5% chance of simulated revocation
        is_revoked = True

    if external_api_status == "success":
        certificate_url = f"https://cert.codebound.game/certificates/{certificate_id}.pdf"
        print(f"Simulating external certification API call for player {player_id}, course {course_id}. Status: Success.")
        return {
            "status": "success",
            "certificate_id": certificate_id,
            "certificate_url": certificate_url,
            "player_id": player_id,
            "course_id": course_id,
            "mastery_level": mastery_level,
            "skills_gained": skills_gained,
            "external_api_response": {"message": "Certificate issued by external service.", "status": "success"},
            "is_revoked": is_revoked, # New: revocation status
            "verification_hash": certificate_hash # New: full hash for external verification
        }
    else:
        print(f"Simulating external certification API call for player {player_id}, course {course_id}. Status: Failed.")
        return {"status": "error", "message": "Failed to issue certification via external API (simulated)."}

def api_playground_call(player_id: str, api_endpoint: str, payload: dict, api_key_provided: str = None) -> dict:
    """Simulates or makes a real call to an external API within the game's API Playground.
    This version includes simulated security checks, rate limiting, and API key management.
    """
    print(f"Player {player_id} making API call to {api_endpoint}.")

    # Simulate API key validation
    required_api_key = "CODEBOUND_SECRET_KEY"
    if api_key_provided != required_api_key:
        return {"status": "error", "message": "Authentication Error: Invalid API Key provided.", "error_type": "authentication_failure"}

    # Simulate basic security check (e.g., checking for malicious keywords)
    if "delete_system" in str(payload).lower() or "drop_database" in str(payload).lower():
        return {"status": "error", "message": "Security violation detected! Malicious payload blocked.", "player_id": player_id, "api_endpoint": api_endpoint, "error_type": "security_violation"}

    # Simulate rate limiting (simple counter per player, reset in a real system)
    global _api_call_counts
    if '_api_call_counts' not in globals():
        _api_call_counts = {}
    _api_call_counts[player_id] = _api_call_counts.get(player_id, 0) + 1

    if _api_call_counts[player_id] > 5: # Allow 5 calls per simulated session
        return {"status": "error", "message": "API rate limit exceeded. Please wait before making more calls.", "player_id": player_id, "api_endpoint": api_endpoint, "error_type": "rate_limit_exceeded"}

    simulated_response = {}
    error_message = None
    error_type = None

    if "weather" in api_endpoint.lower():
        if "city" not in payload:
            error_message = "Invalid parameter: 'city' is required for weather API."
            error_type = "invalid_parameter"
        else:
            simulated_response = {"temperature": random.randint(10, 30), "unit": "celsius", "conditions": random.choice(["sunny", "cloudy", "rainy"]), "city": payload["city"]}
    elif "finance" in api_endpoint.lower():
        if "symbol" not in payload:
            error_message = "Invalid parameter: 'symbol' is required for finance API."
            error_type = "invalid_parameter"
        else:
            simulated_response = {"stock": payload["symbol"], "price": round(random.uniform(50, 500), 2), "currency": "USD", "change": round(random.uniform(-5, 5), 2)}
    elif "ai" in api_endpoint.lower():
        if "prompt" not in payload:
            error_message = "Invalid parameter: 'prompt' is required for AI API."
            error_type = "invalid_parameter"
        else:
            simulated_response = {"model_output": f"AI response to '{payload['prompt'][:20]}...'", "confidence": round(random.uniform(0.7, 0.99), 2)}
    else:
        error_message = "API endpoint not recognized."
        error_type = "unrecognized_endpoint"

    if error_message:
        return {"status": "error", "message": error_message, "error_type": error_type, "player_id": player_id, "api_endpoint": api_endpoint}
    
    # In a real scenario, this would involve making actual HTTP requests to external APIs
    # and managing security (e.g., API keys, rate limiting).

    return {"status": "success", "api_response": simulated_response, "player_id": player_id, "api_endpoint": api_endpoint} 