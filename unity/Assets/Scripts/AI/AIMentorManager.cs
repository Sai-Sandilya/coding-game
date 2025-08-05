using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class AIMentorManager : MonoBehaviour
{
    [Header("AI Configuration")]
    [SerializeField] private string apiBaseUrl = "http://localhost:5000";
    [SerializeField] private string mentorName = "CodeWizard";
    [SerializeField] private string mentorPersonality = "friendly_expert";
    
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Text mentorText;
    [SerializeField] private InputField playerInput;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private ScrollRect dialogueScrollRect;
    
    [Header("Visual Settings")]
    [SerializeField] private GameObject mentorAvatar;
    [SerializeField] private Animator mentorAnimator;
    [SerializeField] private AudioSource mentorAudioSource;
    [SerializeField] private AudioClip[] mentorVoiceClips;
    
    [Header("Learning Settings")]
    [SerializeField] private bool enableAdaptiveLearning = true;
    [SerializeField] private bool enableCodeStyleFeedback = true;
    [SerializeField] private float responseDelay = 0.5f;
    
    private GameManager gameManager;
    private bool isInitialized = false;
    private bool isInDialogue = false;
    private List<DialogueMessage> conversationHistory = new List<DialogueMessage>();
    
    // Events
    public System.Action<string> OnMentorResponse;
    public System.Action<string> OnLearningTip;
    public System.Action<string> OnCodeFeedback;
    public System.Action<bool> OnDialogueStateChanged;
    
    // Mentor personality traits
    private Dictionary<string, string> mentorPersonalities = new Dictionary<string, string>
    {
        {"friendly_expert", "I'm a friendly coding expert who loves helping beginners learn!"},
        {"wise_mentor", "I'm a wise mentor who guides you through the mysteries of code."},
        {"enthusiastic_teacher", "I'm an enthusiastic teacher who gets excited about coding!"},
        {"patient_guide", "I'm a patient guide who believes everyone can learn to code."}
    };
    
    public void Initialize(GameManager manager, string apiUrl)
    {
        gameManager = manager;
        apiBaseUrl = apiUrl;
        isInitialized = true;
        
        SetupUI();
        InitializeMentor();
        
        Debug.Log("AI Mentor Manager initialized");
    }
    
    private void SetupUI()
    {
        if (sendButton != null)
            sendButton.onClick.AddListener(SendPlayerMessage);
        
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseDialogue);
        
        if (playerInput != null)
        {
            playerInput.onEndEdit.AddListener((text) => {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    SendPlayerMessage();
                }
            });
        }
        
        // Hide dialogue panel initially
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
    
    private void InitializeMentor()
    {
        // Set mentor personality
        string personality = mentorPersonalities.ContainsKey(mentorPersonality) 
            ? mentorPersonalities[mentorPersonality] 
            : mentorPersonalities["friendly_expert"];
        
        // Add initial greeting
        AddMentorMessage($"Hello! I'm {mentorName}, your coding mentor. {personality}");
    }
    
    public void StartDialogue()
    {
        if (!isInitialized) return;
        
        isInDialogue = true;
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
        
        if (gameManager != null)
            gameManager.StartDialogue();
        
        OnDialogueStateChanged?.Invoke(true);
        
        // Focus on input field
        if (playerInput != null)
        {
            playerInput.Select();
            playerInput.ActivateInputField();
        }
        
        Debug.Log("AI Mentor dialogue started");
    }
    
    public void CloseDialogue()
    {
        isInDialogue = false;
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        
        if (gameManager != null)
            gameManager.EndDialogue();
        
        OnDialogueStateChanged?.Invoke(false);
        
        Debug.Log("AI Mentor dialogue closed");
    }
    
    public void SendPlayerMessage()
    {
        if (!isInDialogue || playerInput == null) return;
        
        string message = playerInput.text.Trim();
        if (string.IsNullOrEmpty(message)) return;
        
        // Add player message to conversation
        AddPlayerMessage(message);
        
        // Clear input field
        playerInput.text = "";
        
        // Get mentor response
        StartCoroutine(GetMentorResponse(message));
    }
    
    private IEnumerator GetMentorResponse(string playerMessage)
    {
        if (!isInitialized) yield break;
        
        // Show typing indicator
        ShowTypingIndicator(true);
        
        // Prepare request
        var request = new ConversationalTutorRequest
        {
            player_id = gameManager.GetPlayerId(),
            user_input = playerMessage,
            conversation_history = ConvertHistoryToBackendFormat(),
            mentor_personality = mentorPersonality
        };
        
        string jsonData = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        // Send request to backend
        using (UnityWebRequest webRequest = new UnityWebRequest($"{apiBaseUrl}/conversational_tutor", "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<ConversationalTutorResponse>(webRequest.downloadHandler.text);
                    
                    if (response.status == "success")
                    {
                        // Add mentor response to conversation
                        AddMentorMessage(response.tutor_response);
                        
                        // Handle any learning tips or code feedback
                        if (!string.IsNullOrEmpty(response.learning_tip))
                        {
                            OnLearningTip?.Invoke(response.learning_tip);
                        }
                        
                        if (!string.IsNullOrEmpty(response.code_feedback))
                        {
                            OnCodeFeedback?.Invoke(response.code_feedback);
                        }
                        
                        OnMentorResponse?.Invoke(response.tutor_response);
                    }
                    else
                    {
                        AddMentorMessage("I'm sorry, I'm having trouble responding right now. Let me try again.");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error parsing mentor response: {e.Message}");
                    AddMentorMessage("I'm sorry, there was an error processing my response.");
                }
            }
            else
            {
                Debug.LogError($"Network error getting mentor response: {webRequest.error}");
                AddMentorMessage("I'm sorry, I'm having trouble connecting right now.");
            }
        }
        
        // Hide typing indicator
        ShowTypingIndicator(false);
    }
    
    private void AddPlayerMessage(string message)
    {
        var dialogueMessage = new DialogueMessage
        {
            sender = "player",
            message = message,
            timestamp = System.DateTime.Now
        };
        
        conversationHistory.Add(dialogueMessage);
        UpdateDialogueDisplay();
    }
    
    private void AddMentorMessage(string message)
    {
        var dialogueMessage = new DialogueMessage
        {
            sender = "mentor",
            message = message,
            timestamp = System.DateTime.Now
        };
        
        conversationHistory.Add(dialogueMessage);
        UpdateDialogueDisplay();
        
        // Animate mentor
        AnimateMentor();
        
        // Play voice clip if available
        PlayMentorVoice();
    }
    
    private void UpdateDialogueDisplay()
    {
        if (mentorText == null) return;
        
        string fullConversation = "";
        foreach (var msg in conversationHistory)
        {
            string prefix = msg.sender == "player" ? "You: " : $"{mentorName}: ";
            fullConversation += prefix + msg.message + "\n\n";
        }
        
        mentorText.text = fullConversation;
        
        // Auto-scroll to bottom
        if (dialogueScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            dialogueScrollRect.verticalNormalizedPosition = 0f;
        }
    }
    
    private List<ConversationHistoryItem> ConvertHistoryToBackendFormat()
    {
        var history = new List<ConversationHistoryItem>();
        
        foreach (var msg in conversationHistory)
        {
            history.Add(new ConversationHistoryItem
            {
                role = msg.sender,
                content = msg.message
            });
        }
        
        return history;
    }
    
    private void ShowTypingIndicator(bool show)
    {
        if (mentorText != null)
        {
            if (show)
            {
                mentorText.text += $"{mentorName} is typing...\n";
            }
            else
            {
                // Remove typing indicator
                string text = mentorText.text;
                if (text.EndsWith($"{mentorName} is typing...\n"))
                {
                    mentorText.text = text.Substring(0, text.Length - ($"{mentorName} is typing...\n").Length);
                }
            }
        }
    }
    
    private void AnimateMentor()
    {
        if (mentorAnimator != null)
        {
            mentorAnimator.SetTrigger("Speak");
        }
        
        if (mentorAvatar != null)
        {
            // Simple scale animation
            LeanTween.scale(mentorAvatar, Vector3.one * 1.1f, 0.2f)
                .setEase(LeanTweenType.easeOutBack)
                .setLoopPingPong(1);
        }
    }
    
    private void PlayMentorVoice()
    {
        if (mentorAudioSource != null && mentorVoiceClips != null && mentorVoiceClips.Length > 0)
        {
            AudioClip randomClip = mentorVoiceClips[Random.Range(0, mentorVoiceClips.Length)];
            mentorAudioSource.PlayOneShot(randomClip);
        }
    }
    
    public void ProvideCodeStyleFeedback(string code)
    {
        if (!enableCodeStyleFeedback) return;
        
        StartCoroutine(GetCodeStyleFeedback(code));
    }
    
    private IEnumerator GetCodeStyleFeedback(string code)
    {
        var request = new CodeStyleFeedbackRequest
        {
            player_id = gameManager.GetPlayerId(),
            code_solution = code
        };
        
        string jsonData = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest webRequest = new UnityWebRequest($"{apiBaseUrl}/code_style_feedback", "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<CodeStyleFeedbackResponse>(webRequest.downloadHandler.text);
                    
                    if (response.status == "success")
                    {
                        OnCodeFeedback?.Invoke(response.feedback);
                        
                        // Add feedback to conversation
                        AddMentorMessage($"Here's some feedback on your code style:\n{response.feedback}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error parsing code style feedback: {e.Message}");
                }
            }
        }
    }
    
    public void AdaptDifficulty(float performance)
    {
        if (!enableAdaptiveLearning) return;
        
        StartCoroutine(UpdateDifficulty(performance));
    }
    
    private IEnumerator UpdateDifficulty(float performance)
    {
        var request = new AdaptiveDifficultyRequest
        {
            player_id = gameManager.GetPlayerId(),
            player_performance = performance
        };
        
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
                try
                {
                    var response = JsonUtility.FromJson<AdaptiveDifficultyResponse>(webRequest.downloadHandler.text);
                    
                    if (response.status == "success")
                    {
                        // Update game difficulty based on response
                        UpdateGameDifficulty(response.new_difficulty);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error parsing adaptive difficulty response: {e.Message}");
                }
            }
        }
    }
    
    private void UpdateGameDifficulty(float newDifficulty)
    {
        // Update game settings based on new difficulty
        Debug.Log($"Updating game difficulty to: {newDifficulty}");
        
        // This would integrate with the game's difficulty system
        // For example, adjusting enemy health, puzzle complexity, etc.
    }
    
    public bool IsInDialogue()
    {
        return isInDialogue;
    }
    
    public void SetMentorPersonality(string personality)
    {
        if (mentorPersonalities.ContainsKey(personality))
        {
            mentorPersonality = personality;
            Debug.Log($"Mentor personality changed to: {personality}");
        }
    }
    
    public void ClearConversation()
    {
        conversationHistory.Clear();
        UpdateDialogueDisplay();
    }
}

[System.Serializable]
public class DialogueMessage
{
    public string sender; // "player" or "mentor"
    public string message;
    public System.DateTime timestamp;
}

[System.Serializable]
public class ConversationalTutorRequest
{
    public string player_id;
    public string user_input;
    public List<ConversationHistoryItem> conversation_history;
    public string mentor_personality;
}

[System.Serializable]
public class ConversationHistoryItem
{
    public string role;
    public string content;
}

[System.Serializable]
public class ConversationalTutorResponse
{
    public string status;
    public string player_id;
    public string tutor_response;
    public string learning_tip;
    public string code_feedback;
}

[System.Serializable]
public class CodeStyleFeedbackRequest
{
    public string player_id;
    public string code_solution;
}

[System.Serializable]
public class CodeStyleFeedbackResponse
{
    public string status;
    public string feedback;
}

[System.Serializable]
public class AdaptiveDifficultyRequest
{
    public string player_id;
    public float player_performance;
}

[System.Serializable]
public class AdaptiveDifficultyResponse
{
    public string status;
    public string player_id;
    public float new_difficulty;
} 