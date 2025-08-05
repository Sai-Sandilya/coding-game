using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class GameManager : MonoBehaviour
{
    [Header("Game Configuration")]
    [SerializeField] private string apiBaseUrl = "http://localhost:5000";
    [SerializeField] private string playerId = "player_001";
    
    [Header("Game Systems")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private CodeExecutionManager codeExecutionManager;
    [SerializeField] private WorldManager worldManager;
    [SerializeField] private AIMentorManager aiMentorManager;
    [SerializeField] private QuestManager questManager;
    
    [Header("Player Data")]
    [SerializeField] private PlayerData playerData;
    
    // Game state
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        InDialogue,
        InCodeEditor,
        Loading
    }
    
    private GameState currentState = GameState.MainMenu;
    private bool isInitialized = false;
    
    // Events
    public System.Action<GameState> OnGameStateChanged;
    public System.Action<PlayerData> OnPlayerDataUpdated;
    
    void Awake()
    {
        // Singleton pattern
        if (FindObjectsOfType<GameManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
        InitializeGame();
    }
    
    void Start()
    {
        StartCoroutine(InitializeGameAsync());
    }
    
    private void InitializeGame()
    {
        Debug.Log("Initializing Codebound: The Living Language...");
        
        // Initialize player data
        playerData = new PlayerData
        {
            playerId = playerId,
            mastery = 0.5f,
            learningRate = 0.1f,
            difficultyBias = 0.0f,
            ethicalScore = 0,
            publicOpinion = 0.5f,
            currentRegion = "Forest of Repetition",
            completedQuests = new List<string>(),
            unlockedConcepts = new List<string> { "variables", "basic_loops" }
        };
        
        // Set initial game state
        SetGameState(GameState.Loading);
    }
    
    private IEnumerator InitializeGameAsync()
    {
        Debug.Log("Loading game systems...");
        
        // Initialize UI
        if (uiManager != null)
        {
            uiManager.Initialize(this);
            yield return new WaitForSeconds(0.1f);
        }
        
        // Initialize code execution system
        if (codeExecutionManager != null)
        {
            codeExecutionManager.Initialize(this, apiBaseUrl);
            yield return new WaitForSeconds(0.1f);
        }
        
        // Initialize world manager
        if (worldManager != null)
        {
            worldManager.Initialize(this);
            yield return new WaitForSeconds(0.1f);
        }
        
        // Initialize AI mentor
        if (aiMentorManager != null)
        {
            aiMentorManager.Initialize(this, apiBaseUrl);
            yield return new WaitForSeconds(0.1f);
        }
        
        // Initialize quest system
        if (questManager != null)
        {
            questManager.Initialize(this, apiBaseUrl);
            yield return new WaitForSeconds(0.1f);
        }
        
        // Load player data from backend
        yield return StartCoroutine(LoadPlayerDataFromBackend());
        
        isInitialized = true;
        SetGameState(GameState.MainMenu);
        
        Debug.Log("Game initialization complete!");
    }
    
    private IEnumerator LoadPlayerDataFromBackend()
    {
        Debug.Log("Loading player data from backend...");
        
        // Load adaptive learning data
        yield return StartCoroutine(LoadAdaptiveLearningData());
        
        // Load ethical history
        yield return StartCoroutine(LoadEthicalHistory());
        
        OnPlayerDataUpdated?.Invoke(playerData);
    }
    
    private IEnumerator LoadAdaptiveLearningData()
    {
        var request = new { player_id = playerId };
        string jsonData = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest webRequest = new UnityWebRequest($"{apiBaseUrl}/adapt_difficulty", "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<AdaptiveLearningResponse>(webRequest.downloadHandler.text);
                if (response.status == "success")
                {
                    playerData.mastery = response.player_model.mastery;
                    playerData.learningRate = response.player_model.learning_rate;
                    playerData.difficultyBias = response.player_model.difficulty_bias;
                    Debug.Log($"Loaded adaptive learning data - Mastery: {playerData.mastery}");
                }
            }
        }
    }
    
    private IEnumerator LoadEthicalHistory()
    {
        string url = $"{apiBaseUrl}/get_ethical_history?player_id={playerId}";
        
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<EthicalHistoryResponse>(webRequest.downloadHandler.text);
                if (response.status == "success")
                {
                    playerData.ethicalScore = response.ethical_history.recent_ethical_score;
                    playerData.publicOpinion = response.ethical_history.public_opinion;
                    Debug.Log($"Loaded ethical history - Score: {playerData.ethicalScore}, Opinion: {playerData.publicOpinion}");
                }
            }
        }
    }
    
    public void SetGameState(GameState newState)
    {
        if (currentState == newState) return;
        
        GameState previousState = currentState;
        currentState = newState;
        
        Debug.Log($"Game state changed from {previousState} to {newState}");
        
        // Handle state-specific logic
        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
            case GameState.InDialogue:
                Time.timeScale = 0f;
                break;
            case GameState.InCodeEditor:
                Time.timeScale = 0f;
                break;
        }
        
        OnGameStateChanged?.Invoke(newState);
    }
    
    public GameState GetCurrentState()
    {
        return currentState;
    }
    
    public PlayerData GetPlayerData()
    {
        return playerData;
    }
    
    public void UpdatePlayerData(PlayerData newData)
    {
        playerData = newData;
        OnPlayerDataUpdated?.Invoke(playerData);
    }
    
    public string GetApiBaseUrl()
    {
        return apiBaseUrl;
    }
    
    public string GetPlayerId()
    {
        return playerId;
    }
    
    public bool IsInitialized()
    {
        return isInitialized;
    }
    
    // Public methods for other systems
    public void StartGame()
    {
        if (isInitialized)
        {
            SetGameState(GameState.Playing);
        }
    }
    
    public void PauseGame()
    {
        SetGameState(GameState.Paused);
    }
    
    public void ResumeGame()
    {
        SetGameState(GameState.Playing);
    }
    
    public void OpenCodeEditor()
    {
        SetGameState(GameState.InCodeEditor);
    }
    
    public void CloseCodeEditor()
    {
        SetGameState(GameState.Playing);
    }
    
    public void StartDialogue()
    {
        SetGameState(GameState.InDialogue);
    }
    
    public void EndDialogue()
    {
        SetGameState(GameState.Playing);
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && currentState == GameState.Playing)
        {
            PauseGame();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && currentState == GameState.Playing)
        {
            PauseGame();
        }
    }
}

[System.Serializable]
public class PlayerData
{
    public string playerId;
    public float mastery;
    public float learningRate;
    public float difficultyBias;
    public int ethicalScore;
    public float publicOpinion;
    public string currentRegion;
    public List<string> completedQuests;
    public List<string> unlockedConcepts;
    public Dictionary<string, float> conceptMastery = new Dictionary<string, float>();
}

[System.Serializable]
public class AdaptiveLearningResponse
{
    public string status;
    public string player_id;
    public PlayerModelData player_model;
    public float new_difficulty;
}

[System.Serializable]
public class PlayerModelData
{
    public float mastery;
    public float learning_rate;
    public float difficulty_bias;
    public Dictionary<string, float> concept_mastery;
}

[System.Serializable]
public class EthicalHistoryResponse
{
    public string status;
    public string player_id;
    public EthicalHistoryData ethical_history;
}

[System.Serializable]
public class EthicalHistoryData
{
    public int recent_ethical_score;
    public float public_opinion;
    public string ethical_level;
    public List<EthicalChoiceData> ethical_choices;
}

[System.Serializable]
public class EthicalChoiceData
{
    public string scenario_id;
    public int ethical_score_change;
    public string timestamp;
} 