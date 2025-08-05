using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class CodeExecutionManager : MonoBehaviour
{
    [Header("API Configuration")]
    [SerializeField] private string apiBaseUrl = "http://localhost:5000";
    
    [Header("Execution Settings")]
    [SerializeField] private float executionTimeout = 10f;
    [SerializeField] private bool enableDebugLogging = true;
    
    [Header("Game Effects")]
    [SerializeField] private GameObject gatePrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject spellEffectPrefab;
    [SerializeField] private GameObject colorChangeEffectPrefab;
    
    private GameManager gameManager;
    private bool isInitialized = false;
    
    // Events
    public System.Action<CodeExecutionResult> OnCodeExecuted;
    public System.Action<string> OnCodeError;
    public System.Action<string> OnGameEffectTriggered;
    
    // Execution queue for handling multiple code executions
    private Queue<CodeExecutionRequest> executionQueue = new Queue<CodeExecutionRequest>();
    private bool isExecuting = false;
    
    public void Initialize(GameManager manager, string apiUrl)
    {
        gameManager = manager;
        apiBaseUrl = apiUrl;
        isInitialized = true;
        
        if (enableDebugLogging)
            Debug.Log("CodeExecutionManager initialized");
    }
    
    public Coroutine ExecuteCode(string code, System.Action<CodeExecutionResult> callback = null)
    {
        return StartCoroutine(ExecuteCodeCoroutine(code, callback));
    }
    
    private IEnumerator ExecuteCodeCoroutine(string code, System.Action<CodeExecutionResult> callback)
    {
        if (!isInitialized)
        {
            Debug.LogError("CodeExecutionManager not initialized!");
            yield break;
        }
        
        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning("Attempted to execute empty code");
            yield break;
        }
        
        if (enableDebugLogging)
            Debug.Log($"Executing code: {code}");
        
        // Create execution request
        var request = new CodeExecutionRequest
        {
            player_id = gameManager.GetPlayerId(),
            code_block = code
        };
        
        string jsonData = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        // Create web request
        using (UnityWebRequest webRequest = new UnityWebRequest($"{apiBaseUrl}/execute_game_code", "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            // Set timeout
            webRequest.timeout = (int)executionTimeout;
            
            // Send request
            yield return webRequest.SendWebRequest();
            
            CodeExecutionResult result = new CodeExecutionResult();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<CodeExecutionResponse>(webRequest.downloadHandler.text);
                    
                    if (response.status == "success")
                    {
                        result.success = true;
                        result.output = response.executed_code_output;
                        result.gameEffect = response.game_effect;
                        result.playerId = response.player_id;
                        
                        if (enableDebugLogging)
                            Debug.Log($"Code executed successfully. Output: {result.output}, Game Effect: {result.gameEffect}");
                        
                        // Trigger game effect
                        yield return StartCoroutine(TriggerGameEffect(result.gameEffect));
                    }
                    else
                    {
                        result.success = false;
                        result.error = "Backend returned error status";
                        
                        if (enableDebugLogging)
                            Debug.LogError($"Backend error: {response.status}");
                    }
                }
                catch (System.Exception e)
                {
                    result.success = false;
                    result.error = $"JSON parsing error: {e.Message}";
                    
                    if (enableDebugLogging)
                        Debug.LogError($"JSON parsing error: {e.Message}");
                }
            }
            else
            {
                result.success = false;
                result.error = $"Network error: {webRequest.error}";
                
                if (enableDebugLogging)
                    Debug.LogError($"Network error: {webRequest.error}");
            }
            
            // Invoke callbacks
            OnCodeExecuted?.Invoke(result);
            callback?.Invoke(result);
            
            if (!result.success)
            {
                OnCodeError?.Invoke(result.error);
            }
        }
    }
    
    private IEnumerator TriggerGameEffect(string gameEffect)
    {
        if (string.IsNullOrEmpty(gameEffect))
            yield break;
        
        if (enableDebugLogging)
            Debug.Log($"Triggering game effect: {gameEffect}");
        
        OnGameEffectTriggered?.Invoke(gameEffect);
        
        switch (gameEffect.ToLower())
        {
            case "open_gate_a":
                yield return StartCoroutine(OpenGate("Gate_A"));
                break;
                
            case "spawn_goblin":
                yield return StartCoroutine(SpawnEnemy("Goblin"));
                break;
                
            case "set_light_blue":
                yield return StartCoroutine(ChangeColor(Color.lightBlue));
                break;
                
            case "level_complete":
                yield return StartCoroutine(CompleteLevel());
                break;
                
            case "code_error":
                yield return StartCoroutine(ShowCodeError());
                break;
                
            default:
                if (enableDebugLogging)
                    Debug.Log($"Unknown game effect: {gameEffect}");
                break;
        }
    }
    
    private IEnumerator OpenGate(string gateId)
    {
        if (enableDebugLogging)
            Debug.Log($"Opening gate: {gateId}");
        
        // Find the gate in the scene
        GameObject gate = GameObject.Find(gateId);
        if (gate != null)
        {
            // Animate gate opening
            yield return StartCoroutine(AnimateGateOpening(gate));
        }
        else
        {
            // Create gate if it doesn't exist
            if (gatePrefab != null)
            {
                Vector3 spawnPosition = new Vector3(0, 0, 0); // Set appropriate position
                GameObject newGate = Instantiate(gatePrefab, spawnPosition, Quaternion.identity);
                newGate.name = gateId;
                
                yield return StartCoroutine(AnimateGateOpening(newGate));
            }
        }
    }
    
    private IEnumerator AnimateGateOpening(GameObject gate)
    {
        // Simple gate opening animation
        Vector3 originalScale = gate.transform.localScale;
        Vector3 openScale = new Vector3(originalScale.x, 0.1f, originalScale.z);
        
        float duration = 1f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            gate.transform.localScale = Vector3.Lerp(originalScale, openScale, t);
            yield return null;
        }
        
        gate.transform.localScale = openScale;
    }
    
    private IEnumerator SpawnEnemy(string enemyType)
    {
        if (enableDebugLogging)
            Debug.Log($"Spawning enemy: {enemyType}");
        
        if (enemyPrefab != null)
        {
            Vector3 spawnPosition = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            enemy.name = $"{enemyType}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
            
            // Add spawn effect
            yield return StartCoroutine(SpawnEffect(enemy.transform.position));
        }
    }
    
    private IEnumerator ChangeColor(Color newColor)
    {
        if (enableDebugLogging)
            Debug.Log($"Changing color to: {newColor}");
        
        // Find objects to change color
        GameObject[] colorableObjects = GameObject.FindGameObjectsWithTag("Colorable");
        
        foreach (GameObject obj in colorableObjects)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = newColor;
            }
        }
        
        // Show color change effect
        if (colorChangeEffectPrefab != null)
        {
            Vector3 effectPosition = Camera.main.transform.position + Camera.main.transform.forward * 2f;
            GameObject effect = Instantiate(colorChangeEffectPrefab, effectPosition, Quaternion.identity);
            
            yield return new WaitForSeconds(2f);
            Destroy(effect);
        }
    }
    
    private IEnumerator CompleteLevel()
    {
        if (enableDebugLogging)
            Debug.Log("Level completed!");
        
        // Trigger level completion effects
        // This would integrate with the game's level system
        
        yield return new WaitForSeconds(1f);
    }
    
    private IEnumerator ShowCodeError()
    {
        if (enableDebugLogging)
            Debug.Log("Showing code error effect");
        
        // Show error effect (red flash, error sound, etc.)
        yield return new WaitForSeconds(0.5f);
    }
    
    private IEnumerator SpawnEffect(Vector3 position)
    {
        if (spellEffectPrefab != null)
        {
            GameObject effect = Instantiate(spellEffectPrefab, position, Quaternion.identity);
            
            yield return new WaitForSeconds(2f);
            Destroy(effect);
        }
    }
    
    public void QueueCodeExecution(string code, System.Action<CodeExecutionResult> callback = null)
    {
        executionQueue.Enqueue(new CodeExecutionRequest
        {
            player_id = gameManager.GetPlayerId(),
            code_block = code,
            callback = callback
        });
        
        if (!isExecuting)
        {
            StartCoroutine(ProcessExecutionQueue());
        }
    }
    
    private IEnumerator ProcessExecutionQueue()
    {
        isExecuting = true;
        
        while (executionQueue.Count > 0)
        {
            CodeExecutionRequest request = executionQueue.Dequeue();
            
            yield return StartCoroutine(ExecuteCodeCoroutine(request.code_block, request.callback));
            
            // Small delay between executions
            yield return new WaitForSeconds(0.1f);
        }
        
        isExecuting = false;
    }
    
    public void ClearExecutionQueue()
    {
        executionQueue.Clear();
        isExecuting = false;
    }
    
    public bool IsExecuting()
    {
        return isExecuting;
    }
    
    public int GetQueueSize()
    {
        return executionQueue.Count;
    }
}

[System.Serializable]
public class CodeExecutionRequest
{
    public string player_id;
    public string code_block;
    public System.Action<CodeExecutionResult> callback;
}

[System.Serializable]
public class CodeExecutionResponse
{
    public string status;
    public string player_id;
    public string executed_code_output;
    public string game_effect;
}

[System.Serializable]
public class CodeExecutionResult
{
    public bool success;
    public string output;
    public string gameEffect;
    public string error;
    public string playerId;
} 