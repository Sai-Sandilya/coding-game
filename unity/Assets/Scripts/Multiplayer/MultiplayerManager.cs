using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class MultiplayerManager : MonoBehaviour
{
    [Header("Multiplayer Configuration")]
    [SerializeField] private string apiBaseUrl = "http://localhost:5000";
    [SerializeField] private bool enableMultiplayer = true;
    [SerializeField] private float syncInterval = 0.5f;
    [SerializeField] private int maxPlayersPerSession = 4;
    
    [Header("UI References")]
    [SerializeField] private GameObject multiplayerPanel;
    [SerializeField] private Transform sessionListContent;
    [SerializeField] private GameObject sessionItemPrefab;
    [SerializeField] private Button createSessionButton;
    [SerializeField] private Button joinSessionButton;
    [SerializeField] private Button startDuelButton;
    [SerializeField] private Text sessionInfoText;
    [SerializeField] private Text playerListText;
    
    [Header("Code Battle Settings")]
    [SerializeField] private GameObject duelPanel;
    [SerializeField] private Text duelProblemText;
    [SerializeField] private Text duelTimerText;
    [SerializeField] private Text duelScoreText;
    [SerializeField] private float duelTimeLimit = 300f; // 5 minutes
    
    [Header("Guild System")]
    [SerializeField] private GameObject guildPanel;
    [SerializeField] private Text guildInfoText;
    [SerializeField] private Transform guildMembersContent;
    [SerializeField] private GameObject guildMemberItemPrefab;
    
    private GameManager gameManager;
    private bool isInitialized = false;
    private bool isInMultiplayerSession = false;
    private bool isInDuel = false;
    
    // Session data
    private string currentSessionId = "";
    private List<MultiplayerPlayer> sessionPlayers = new List<MultiplayerPlayer>();
    private string sharedCode = "";
    private int codeVersion = 0;
    
    // Duel data
    private string currentDuelId = "";
    private float duelTimeRemaining = 0f;
    private Dictionary<string, int> duelScores = new Dictionary<string, int>();
    private string duelProblem = "";
    
    // Guild data
    private string currentGuildId = "";
    private GuildData currentGuild = null;
    
    // Events
    public System.Action<string> OnSessionJoined;
    public System.Action<string> OnSessionLeft;
    public System.Action<string> OnCodeUpdated;
    public System.Action<string> OnDuelStarted;
    public System.Action<string> OnDuelEnded;
    public System.Action<string> OnGuildJoined;
    
    // Sync coroutine
    private Coroutine syncCoroutine;
    
    public void Initialize(GameManager manager, string apiUrl)
    {
        gameManager = manager;
        apiBaseUrl = apiUrl;
        isInitialized = true;
        
        SetupUI();
        StartCoroutine(RefreshSessionList());
        
        Debug.Log("Multiplayer Manager initialized");
    }
    
    private void SetupUI()
    {
        if (createSessionButton != null)
            createSessionButton.onClick.AddListener(CreateSession);
        
        if (joinSessionButton != null)
            joinSessionButton.onClick.AddListener(JoinSelectedSession);
        
        if (startDuelButton != null)
            startDuelButton.onClick.AddListener(StartCodeDuel);
        
        // Hide panels initially
        if (multiplayerPanel != null)
            multiplayerPanel.SetActive(false);
        
        if (duelPanel != null)
            duelPanel.SetActive(false);
        
        if (guildPanel != null)
            guildPanel.SetActive(false);
    }
    
    public void ShowMultiplayerPanel()
    {
        if (multiplayerPanel != null)
        {
            multiplayerPanel.SetActive(true);
            StartCoroutine(RefreshSessionList());
        }
    }
    
    public void HideMultiplayerPanel()
    {
        if (multiplayerPanel != null)
            multiplayerPanel.SetActive(false);
    }
    
    public void CreateSession()
    {
        if (!isInitialized) return;
        
        StartCoroutine(CreateSessionCoroutine());
    }
    
    private IEnumerator CreateSessionCoroutine()
    {
        var request = new CreateSessionRequest
        {
            player_id = gameManager.GetPlayerId(),
            session_name = $"Session_{System.DateTime.Now:HHmm}",
            max_players = maxPlayersPerSession
        };
        
        string jsonData = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest webRequest = new UnityWebRequest($"{apiBaseUrl}/create_multiplayer_session", "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<CreateSessionResponse>(webRequest.downloadHandler.text);
                    
                    if (response.status == "success")
                    {
                        currentSessionId = response.session_id;
                        isInMultiplayerSession = true;
                        
                        Debug.Log($"Created session: {currentSessionId}");
                        
                        // Start sync coroutine
                        StartSessionSync();
                        
                        OnSessionJoined?.Invoke(currentSessionId);
                        UpdateSessionInfo();
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error creating session: {e.Message}");
                }
            }
        }
    }
    
    public void JoinSession(string sessionId)
    {
        if (!isInitialized) return;
        
        StartCoroutine(JoinSessionCoroutine(sessionId));
    }
    
    private IEnumerator JoinSessionCoroutine(string sessionId)
    {
        var request = new JoinSessionRequest
        {
            player_id = gameManager.GetPlayerId(),
            session_id = sessionId
        };
        
        string jsonData = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest webRequest = new UnityWebRequest($"{apiBaseUrl}/join_multiplayer_session", "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<JoinSessionResponse>(webRequest.downloadHandler.text);
                    
                    if (response.status == "success")
                    {
                        currentSessionId = sessionId;
                        isInMultiplayerSession = true;
                        sharedCode = response.shared_code;
                        codeVersion = response.code_version;
                        
                        Debug.Log($"Joined session: {sessionId}");
                        
                        // Start sync coroutine
                        StartSessionSync();
                        
                        OnSessionJoined?.Invoke(sessionId);
                        UpdateSessionInfo();
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error joining session: {e.Message}");
                }
            }
        }
    }
    
    private void JoinSelectedSession()
    {
        // This would get the selected session from UI
        // For now, just join the first available session
        if (sessionListContent != null && sessionListContent.childCount > 0)
        {
            var firstSession = sessionListContent.GetChild(0);
            var sessionItem = firstSession.GetComponent<SessionListItem>();
            if (sessionItem != null)
            {
                JoinSession(sessionItem.SessionId);
            }
        }
    }
    
    public void LeaveSession()
    {
        if (!isInMultiplayerSession) return;
        
        StartCoroutine(LeaveSessionCoroutine());
    }
    
    private IEnumerator LeaveSessionCoroutine()
    {
        var request = new LeaveSessionRequest
        {
            player_id = gameManager.GetPlayerId(),
            session_id = currentSessionId
        };
        
        string jsonData = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest webRequest = new UnityWebRequest($"{apiBaseUrl}/end_multiplayer_session", "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return webRequest.SendWebRequest();
            
            // Stop sync coroutine
            StopSessionSync();
            
            isInMultiplayerSession = false;
            currentSessionId = "";
            sharedCode = "";
            codeVersion = 0;
            
            OnSessionLeft?.Invoke(currentSessionId);
            UpdateSessionInfo();
        }
    }
    
    public void UpdateSharedCode(string newCode)
    {
        if (!isInMultiplayerSession) return;
        
        StartCoroutine(UpdateSharedCodeCoroutine(newCode));
    }
    
    private IEnumerator UpdateSharedCodeCoroutine(string newCode)
    {
        var request = new UpdateSharedCodeRequest
        {
            player_id = gameManager.GetPlayerId(),
            session_id = currentSessionId,
            new_code = newCode,
            code_version = codeVersion
        };
        
        string jsonData = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest webRequest = new UnityWebRequest($"{apiBaseUrl}/update_shared_code", "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<UpdateSharedCodeResponse>(webRequest.downloadHandler.text);
                    
                    if (response.status == "success")
                    {
                        sharedCode = response.updated_code;
                        codeVersion = response.new_version;
                        
                        OnCodeUpdated?.Invoke(sharedCode);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error updating shared code: {e.Message}");
                }
            }
        }
    }
    
    public void StartCodeDuel()
    {
        if (!isInMultiplayerSession) return;
        
        StartCoroutine(StartCodeDuelCoroutine());
    }
    
    private IEnumerator StartCodeDuelCoroutine()
    {
        var request = new StartDuelRequest
        {
            player_id = gameManager.GetPlayerId(),
            session_id = currentSessionId,
            time_limit = duelTimeLimit
        };
        
        string jsonData = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest webRequest = new UnityWebRequest($"{apiBaseUrl}/start_code_duel", "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<StartDuelResponse>(webRequest.downloadHandler.text);
                    
                    if (response.status == "success")
                    {
                        currentDuelId = response.duel_id;
                        duelProblem = response.problem;
                        duelTimeRemaining = response.time_limit;
                        isInDuel = true;
                        
                        // Show duel panel
                        if (duelPanel != null)
                            duelPanel.SetActive(true);
                        
                        // Start duel timer
                        StartCoroutine(DuelTimer());
                        
                        OnDuelStarted?.Invoke(currentDuelId);
                        UpdateDuelInfo();
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error starting duel: {e.Message}");
                }
            }
        }
    }
    
    private IEnumerator DuelTimer()
    {
        while (isInDuel && duelTimeRemaining > 0)
        {
            duelTimeRemaining -= Time.deltaTime;
            UpdateDuelInfo();
            
            if (duelTimeRemaining <= 0)
            {
                EndDuel();
            }
            
            yield return null;
        }
    }
    
    public void EndDuel()
    {
        if (!isInDuel) return;
        
        StartCoroutine(EndDuelCoroutine());
    }
    
    private IEnumerator EndDuelCoroutine()
    {
        var request = new EndDuelRequest
        {
            player_id = gameManager.GetPlayerId(),
            duel_id = currentDuelId
        };
        
        string jsonData = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest webRequest = new UnityWebRequest($"{apiBaseUrl}/end_code_duel", "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return webRequest.SendWebRequest();
            
            isInDuel = false;
            currentDuelId = "";
            
            // Hide duel panel
            if (duelPanel != null)
                duelPanel.SetActive(false);
            
            OnDuelEnded?.Invoke(currentDuelId);
        }
    }
    
    private void StartSessionSync()
    {
        if (syncCoroutine != null)
            StopCoroutine(syncCoroutine);
        
        syncCoroutine = StartCoroutine(SessionSyncCoroutine());
    }
    
    private void StopSessionSync()
    {
        if (syncCoroutine != null)
        {
            StopCoroutine(syncCoroutine);
            syncCoroutine = null;
        }
    }
    
    private IEnumerator SessionSyncCoroutine()
    {
        while (isInMultiplayerSession)
        {
            yield return StartCoroutine(SyncSessionData());
            yield return new WaitForSeconds(syncInterval);
        }
    }
    
    private IEnumerator SyncSessionData()
    {
        var request = new SyncSessionRequest
        {
            player_id = gameManager.GetPlayerId(),
            session_id = currentSessionId
        };
        
        string jsonData = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest webRequest = new UnityWebRequest($"{apiBaseUrl}/sync_session", "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<SyncSessionResponse>(webRequest.downloadHandler.text);
                    
                    if (response.status == "success")
                    {
                        // Update session data
                        sessionPlayers = response.players;
                        if (response.code_version > codeVersion)
                        {
                            sharedCode = response.shared_code;
                            codeVersion = response.code_version;
                            OnCodeUpdated?.Invoke(sharedCode);
                        }
                        
                        UpdateSessionInfo();
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error syncing session: {e.Message}");
                }
            }
        }
    }
    
    private IEnumerator RefreshSessionList()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get($"{apiBaseUrl}/list_sessions"))
        {
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<ListSessionsResponse>(webRequest.downloadHandler.text);
                    
                    if (response.status == "success")
                    {
                        UpdateSessionList(response.sessions);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error refreshing session list: {e.Message}");
                }
            }
        }
    }
    
    private void UpdateSessionList(List<SessionData> sessions)
    {
        if (sessionListContent == null) return;
        
        // Clear existing items
        foreach (Transform child in sessionListContent)
        {
            Destroy(child.gameObject);
        }
        
        // Add session items
        foreach (var session in sessions)
        {
            if (sessionItemPrefab != null)
            {
                GameObject sessionItem = Instantiate(sessionItemPrefab, sessionListContent);
                var sessionListItem = sessionItem.GetComponent<SessionListItem>();
                if (sessionListItem != null)
                {
                    sessionListItem.Initialize(session);
                }
            }
        }
    }
    
    private void UpdateSessionInfo()
    {
        if (sessionInfoText != null)
        {
            if (isInMultiplayerSession)
            {
                sessionInfoText.text = $"Session: {currentSessionId}\nPlayers: {sessionPlayers.Count}/{maxPlayersPerSession}";
            }
            else
            {
                sessionInfoText.text = "Not in session";
            }
        }
        
        if (playerListText != null)
        {
            string playerList = "Players:\n";
            foreach (var player in sessionPlayers)
            {
                playerList += $"- {player.player_name}\n";
            }
            playerListText.text = playerList;
        }
    }
    
    private void UpdateDuelInfo()
    {
        if (duelProblemText != null)
            duelProblemText.text = duelProblem;
        
        if (duelTimerText != null)
        {
            int minutes = Mathf.FloorToInt(duelTimeRemaining / 60);
            int seconds = Mathf.FloorToInt(duelTimeRemaining % 60);
            duelTimerText.text = $"{minutes:00}:{seconds:00}";
        }
        
        if (duelScoreText != null)
        {
            string scoreText = "Scores:\n";
            foreach (var score in duelScores)
            {
                scoreText += $"{score.Key}: {score.Value}\n";
            }
            duelScoreText.text = scoreText;
        }
    }
    
    public bool IsInMultiplayerSession()
    {
        return isInMultiplayerSession;
    }
    
    public bool IsInDuel()
    {
        return isInDuel;
    }
    
    public string GetSharedCode()
    {
        return sharedCode;
    }
    
    public List<MultiplayerPlayer> GetSessionPlayers()
    {
        return new List<MultiplayerPlayer>(sessionPlayers);
    }
}

[System.Serializable]
public class MultiplayerPlayer
{
    public string player_id;
    public string player_name;
    public bool is_online;
    public float last_seen;
}

[System.Serializable]
public class SessionData
{
    public string session_id;
    public string session_name;
    public string host_id;
    public int player_count;
    public int max_players;
    public bool is_active;
}

[System.Serializable]
public class GuildData
{
    public string guild_id;
    public string guild_name;
    public string description;
    public List<GuildMember> members;
    public int level;
    public int experience;
}

[System.Serializable]
public class GuildMember
{
    public string player_id;
    public string player_name;
    public string role;
    public int contribution;
}

// Request/Response classes
[System.Serializable]
public class CreateSessionRequest
{
    public string player_id;
    public string session_name;
    public int max_players;
}

[System.Serializable]
public class CreateSessionResponse
{
    public string status;
    public string session_id;
    public string shared_code;
}

[System.Serializable]
public class JoinSessionRequest
{
    public string player_id;
    public string session_id;
}

[System.Serializable]
public class JoinSessionResponse
{
    public string status;
    public string shared_code;
    public int code_version;
}

[System.Serializable]
public class LeaveSessionRequest
{
    public string player_id;
    public string session_id;
}

[System.Serializable]
public class UpdateSharedCodeRequest
{
    public string player_id;
    public string session_id;
    public string new_code;
    public int code_version;
}

[System.Serializable]
public class UpdateSharedCodeResponse
{
    public string status;
    public string updated_code;
    public int new_version;
}

[System.Serializable]
public class StartDuelRequest
{
    public string player_id;
    public string session_id;
    public float time_limit;
}

[System.Serializable]
public class StartDuelResponse
{
    public string status;
    public string duel_id;
    public string problem;
    public float time_limit;
}

[System.Serializable]
public class EndDuelRequest
{
    public string player_id;
    public string duel_id;
}

[System.Serializable]
public class SyncSessionRequest
{
    public string player_id;
    public string session_id;
}

[System.Serializable]
public class SyncSessionResponse
{
    public string status;
    public List<MultiplayerPlayer> players;
    public string shared_code;
    public int code_version;
}

[System.Serializable]
public class ListSessionsResponse
{
    public string status;
    public List<SessionData> sessions;
} 