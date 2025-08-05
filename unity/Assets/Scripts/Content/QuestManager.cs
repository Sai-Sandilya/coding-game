using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class QuestManager : MonoBehaviour
{
    [Header("Quest Configuration")]
    [SerializeField] private string apiBaseUrl = "http://localhost:5000";
    [SerializeField] private bool enableProceduralQuests = true;
    [SerializeField] private int maxActiveQuests = 5;
    [SerializeField] private float questRefreshInterval = 300f; // 5 minutes
    
    [Header("UI References")]
    [SerializeField] private GameObject questPanel;
    [SerializeField] private Transform questListContent;
    [SerializeField] private GameObject questItemPrefab;
    [SerializeField] private Text questDescriptionText;
    [SerializeField] private Button acceptQuestButton;
    [SerializeField] private Button completeQuestButton;
    [SerializeField] private Button abandonQuestButton;
    
    [Header("Quest Types")]
    [SerializeField] private string[] availableQuestTypes = { "coding_challenge", "ethical_dilemma", "multiplayer_mission", "creative_project" };
    [SerializeField] private string[] availableRegions = { "Forest of Repetition", "Binary Desert", "Spiral Mountains", "Function Valley" };
    
    [Header("Story Progression")]
    [SerializeField] private int currentStoryProgress = 0;
    [SerializeField] private string currentRegion = "Forest of Repetition";
    [SerializeField] private List<string> unlockedRegions = new List<string>();
    
    private GameManager gameManager;
    private bool isInitialized = false;
    
    // Quest data
    private List<QuestData> availableQuests = new List<QuestData>();
    private List<QuestData> activeQuests = new List<QuestData>();
    private List<QuestData> completedQuests = new List<QuestData>();
    private QuestData selectedQuest = null;
    
    // Story data
    private Dictionary<string, int> regionProgress = new Dictionary<string, int>();
    private List<StoryEvent> storyEvents = new List<StoryEvent>();
    
    // Events
    public System.Action<QuestData> OnQuestAccepted;
    public System.Action<QuestData> OnQuestCompleted;
    public System.Action<QuestData> OnQuestFailed;
    public System.Action<string> OnStoryProgress;
    public System.Action<string> OnRegionUnlocked;
    
    // Refresh coroutine
    private Coroutine questRefreshCoroutine;
    
    public void Initialize(GameManager manager, string apiUrl)
    {
        gameManager = manager;
        apiBaseUrl = apiUrl;
        isInitialized = true;
        
        SetupUI();
        InitializeStoryProgress();
        StartCoroutine(LoadInitialQuests());
        
        Debug.Log("Quest Manager initialized");
    }
    
    private void SetupUI()
    {
        if (acceptQuestButton != null)
            acceptQuestButton.onClick.AddListener(AcceptSelectedQuest);
        
        if (completeQuestButton != null)
            completeQuestButton.onClick.AddListener(CompleteSelectedQuest);
        
        if (abandonQuestButton != null)
            abandonQuestButton.onClick.AddListener(AbandonSelectedQuest);
        
        // Hide quest panel initially
        if (questPanel != null)
            questPanel.SetActive(false);
    }
    
    private void InitializeStoryProgress()
    {
        // Initialize region progress
        foreach (string region in availableRegions)
        {
            regionProgress[region] = 0;
        }
        
        // Set starting region
        currentRegion = "Forest of Repetition";
        unlockedRegions.Add(currentRegion);
        
        // Initialize story events
        InitializeStoryEvents();
    }
    
    private void InitializeStoryEvents()
    {
        storyEvents = new List<StoryEvent>
        {
            new StoryEvent { id = "forest_intro", title = "Welcome to the Forest", description = "Begin your journey in the Forest of Repetition", region = "Forest of Repetition", progress_required = 0 },
            new StoryEvent { id = "first_loop", title = "The First Loop", description = "Learn about loops in the Forest", region = "Forest of Repetition", progress_required = 1 },
            new StoryEvent { id = "desert_unlock", title = "The Binary Desert", description = "Unlock the Binary Desert", region = "Binary Desert", progress_required = 5 },
            new StoryEvent { id = "conditionals", title = "Conditional Logic", description = "Master conditionals in the Desert", region = "Binary Desert", progress_required = 6 },
            new StoryEvent { id = "mountains_unlock", title = "The Spiral Mountains", description = "Unlock the Spiral Mountains", region = "Spiral Mountains", progress_required = 10 },
            new StoryEvent { id = "recursion", title = "Recursive Thinking", description = "Learn recursion in the Mountains", region = "Spiral Mountains", progress_required = 11 },
            new StoryEvent { id = "valley_unlock", title = "Function Valley", description = "Unlock Function Valley", region = "Function Valley", progress_required = 15 },
            new StoryEvent { id = "functions", title = "Function Mastery", description = "Master functions in the Valley", region = "Function Valley", progress_required = 16 }
        };
    }
    
    private IEnumerator LoadInitialQuests()
    {
        yield return StartCoroutine(GenerateQuests());
        StartQuestRefresh();
    }
    
    public void ShowQuestPanel()
    {
        if (questPanel != null)
        {
            questPanel.SetActive(true);
            RefreshQuestDisplay();
        }
    }
    
    public void HideQuestPanel()
    {
        if (questPanel != null)
            questPanel.SetActive(false);
    }
    
    public void GenerateNewQuests()
    {
        StartCoroutine(GenerateQuests());
    }
    
    private IEnumerator GenerateQuests()
    {
        if (!isInitialized) yield break;
        
        var request = new GenerateQuestRequest
        {
            player_id = gameManager.GetPlayerId(),
            current_region = currentRegion,
            story_progress = currentStoryProgress,
            unlocked_concepts = gameManager.GetPlayerData().unlockedConcepts,
            completed_quests = gameManager.GetPlayerData().completedQuests
        };
        
        string jsonData = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest webRequest = new UnityWebRequest($"{apiBaseUrl}/generate_quest", "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<GenerateQuestResponse>(webRequest.downloadHandler.text);
                    
                    if (response.status == "success")
                    {
                        availableQuests.Add(response.quest);
                        RefreshQuestDisplay();
                        
                        Debug.Log($"Generated new quest: {response.quest.title}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error generating quest: {e.Message}");
                }
            }
        }
    }
    
    public void AcceptQuest(QuestData quest)
    {
        if (activeQuests.Count >= maxActiveQuests)
        {
            Debug.LogWarning("Maximum active quests reached");
            return;
        }
        
        if (availableQuests.Contains(quest))
        {
            availableQuests.Remove(quest);
            activeQuests.Add(quest);
            quest.status = "active";
            
            OnQuestAccepted?.Invoke(quest);
            RefreshQuestDisplay();
            
            Debug.Log($"Accepted quest: {quest.title}");
        }
    }
    
    public void CompleteQuest(QuestData quest, string solution = "")
    {
        if (activeQuests.Contains(quest))
        {
            StartCoroutine(CompleteQuestCoroutine(quest, solution));
        }
    }
    
    private IEnumerator CompleteQuestCoroutine(QuestData quest, string solution)
    {
        var request = new CompleteQuestRequest
        {
            player_id = gameManager.GetPlayerId(),
            quest_id = quest.quest_id,
            solution = solution,
            completion_time = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        
        string jsonData = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest webRequest = new UnityWebRequest($"{apiBaseUrl}/complete_quest", "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<CompleteQuestResponse>(webRequest.downloadHandler.text);
                    
                    if (response.status == "success")
                    {
                        // Update quest status
                        activeQuests.Remove(quest);
                        completedQuests.Add(quest);
                        quest.status = "completed";
                        
                        // Update player data
                        var playerData = gameManager.GetPlayerData();
                        playerData.completedQuests.Add(quest.quest_id);
                        
                        // Update story progress
                        UpdateStoryProgress(quest);
                        
                        OnQuestCompleted?.Invoke(quest);
                        RefreshQuestDisplay();
                        
                        Debug.Log($"Completed quest: {quest.title}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error completing quest: {e.Message}");
                }
            }
        }
    }
    
    public void AbandonQuest(QuestData quest)
    {
        if (activeQuests.Contains(quest))
        {
            activeQuests.Remove(quest);
            availableQuests.Add(quest);
            quest.status = "available";
            
            RefreshQuestDisplay();
            
            Debug.Log($"Abandoned quest: {quest.title}");
        }
    }
    
    private void UpdateStoryProgress(QuestData quest)
    {
        // Update region progress
        if (regionProgress.ContainsKey(quest.region))
        {
            regionProgress[quest.region]++;
        }
        
        // Check for story events
        CheckStoryEvents();
        
        // Check for region unlocks
        CheckRegionUnlocks();
    }
    
    private void CheckStoryEvents()
    {
        foreach (var storyEvent in storyEvents)
        {
            if (storyEvent.region == currentRegion && 
                regionProgress[currentRegion] >= storyEvent.progress_required &&
                !storyEvent.triggered)
            {
                TriggerStoryEvent(storyEvent);
            }
        }
    }
    
    private void TriggerStoryEvent(StoryEvent storyEvent)
    {
        storyEvent.triggered = true;
        OnStoryProgress?.Invoke(storyEvent.id);
        
        Debug.Log($"Story event triggered: {storyEvent.title}");
        
        // Show story event UI
        ShowStoryEvent(storyEvent);
    }
    
    private void CheckRegionUnlocks()
    {
        // Check if player can unlock new regions
        if (regionProgress["Forest of Repetition"] >= 5 && !unlockedRegions.Contains("Binary Desert"))
        {
            UnlockRegion("Binary Desert");
        }
        
        if (regionProgress["Binary Desert"] >= 5 && !unlockedRegions.Contains("Spiral Mountains"))
        {
            UnlockRegion("Spiral Mountains");
        }
        
        if (regionProgress["Spiral Mountains"] >= 5 && !unlockedRegions.Contains("Function Valley"))
        {
            UnlockRegion("Function Valley");
        }
    }
    
    private void UnlockRegion(string regionName)
    {
        unlockedRegions.Add(regionName);
        OnRegionUnlocked?.Invoke(regionName);
        
        Debug.Log($"Unlocked new region: {regionName}");
        
        // Generate region-specific quests
        StartCoroutine(GenerateRegionQuests(regionName));
    }
    
    private IEnumerator GenerateRegionQuests(string regionName)
    {
        // Generate quests specific to the new region
        for (int i = 0; i < 3; i++)
        {
            yield return StartCoroutine(GenerateQuests());
        }
    }
    
    private void ShowStoryEvent(StoryEvent storyEvent)
    {
        // This would show a story event UI
        // For now, just log it
        Debug.Log($"Story Event: {storyEvent.title}\n{storyEvent.description}");
    }
    
    private void RefreshQuestDisplay()
    {
        if (questListContent == null) return;
        
        // Clear existing quest items
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }
        
        // Add available quests
        foreach (var quest in availableQuests)
        {
            CreateQuestItem(quest, "available");
        }
        
        // Add active quests
        foreach (var quest in activeQuests)
        {
            CreateQuestItem(quest, "active");
        }
        
        // Add completed quests
        foreach (var quest in completedQuests)
        {
            CreateQuestItem(quest, "completed");
        }
    }
    
    private void CreateQuestItem(QuestData quest, string status)
    {
        if (questItemPrefab == null) return;
        
        GameObject questItem = Instantiate(questItemPrefab, questListContent);
        var questListItem = questItem.GetComponent<QuestListItem>();
        
        if (questListItem != null)
        {
            questListItem.Initialize(quest, status);
            questListItem.OnQuestSelected += (selectedQuest) => SelectQuest(selectedQuest);
        }
    }
    
    private void SelectQuest(QuestData quest)
    {
        selectedQuest = quest;
        
        if (questDescriptionText != null)
        {
            questDescriptionText.text = $"Title: {quest.title}\n\nDescription: {quest.description}\n\nRegion: {quest.region}\nDifficulty: {quest.difficulty_level}\nReward: {quest.reward.experience_points} XP";
        }
        
        // Update button states
        UpdateQuestButtons();
    }
    
    private void UpdateQuestButtons()
    {
        if (selectedQuest == null) return;
        
        bool isAvailable = availableQuests.Contains(selectedQuest);
        bool isActive = activeQuests.Contains(selectedQuest);
        bool isCompleted = completedQuests.Contains(selectedQuest);
        
        if (acceptQuestButton != null)
            acceptQuestButton.gameObject.SetActive(isAvailable);
        
        if (completeQuestButton != null)
            completeQuestButton.gameObject.SetActive(isActive);
        
        if (abandonQuestButton != null)
            abandonQuestButton.gameObject.SetActive(isActive);
    }
    
    private void AcceptSelectedQuest()
    {
        if (selectedQuest != null)
        {
            AcceptQuest(selectedQuest);
        }
    }
    
    private void CompleteSelectedQuest()
    {
        if (selectedQuest != null)
        {
            CompleteQuest(selectedQuest);
        }
    }
    
    private void AbandonSelectedQuest()
    {
        if (selectedQuest != null)
        {
            AbandonQuest(selectedQuest);
        }
    }
    
    private void StartQuestRefresh()
    {
        if (questRefreshCoroutine != null)
            StopCoroutine(questRefreshCoroutine);
        
        questRefreshCoroutine = StartCoroutine(QuestRefreshCoroutine());
    }
    
    private IEnumerator QuestRefreshCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(questRefreshInterval);
            
            // Generate new quests if needed
            if (availableQuests.Count < 3)
            {
                yield return StartCoroutine(GenerateQuests());
            }
        }
    }
    
    public void ChangeRegion(string newRegion)
    {
        if (unlockedRegions.Contains(newRegion))
        {
            currentRegion = newRegion;
            Debug.Log($"Changed to region: {newRegion}");
            
            // Generate region-specific quests
            StartCoroutine(GenerateQuests());
        }
    }
    
    public List<QuestData> GetAvailableQuests()
    {
        return new List<QuestData>(availableQuests);
    }
    
    public List<QuestData> GetActiveQuests()
    {
        return new List<QuestData>(activeQuests);
    }
    
    public List<QuestData> GetCompletedQuests()
    {
        return new List<QuestData>(completedQuests);
    }
    
    public string GetCurrentRegion()
    {
        return currentRegion;
    }
    
    public List<string> GetUnlockedRegions()
    {
        return new List<string>(unlockedRegions);
    }
    
    public int GetRegionProgress(string region)
    {
        return regionProgress.ContainsKey(region) ? regionProgress[region] : 0;
    }
}

[System.Serializable]
public class QuestData
{
    public string quest_id;
    public string title;
    public string description;
    public string quest_type;
    public string region;
    public string difficulty_level;
    public string status; // "available", "active", "completed", "failed"
    public QuestReward reward;
    public List<string> objectives;
    public string story_progress;
    public System.DateTime created_at;
}

[System.Serializable]
public class QuestReward
{
    public int experience_points;
    public int moral_points;
    public string badge;
    public List<string> unlocked_concepts;
}

[System.Serializable]
public class StoryEvent
{
    public string id;
    public string title;
    public string description;
    public string region;
    public int progress_required;
    public bool triggered = false;
}

[System.Serializable]
public class GenerateQuestRequest
{
    public string player_id;
    public string current_region;
    public int story_progress;
    public List<string> unlocked_concepts;
    public List<string> completed_quests;
}

[System.Serializable]
public class GenerateQuestResponse
{
    public string status;
    public QuestData quest;
}

[System.Serializable]
public class CompleteQuestRequest
{
    public string player_id;
    public string quest_id;
    public string solution;
    public string completion_time;
}

[System.Serializable]
public class CompleteQuestResponse
{
    public string status;
    public QuestData quest;
    public int experience_gained;
    public List<string> unlocked_concepts;
} 