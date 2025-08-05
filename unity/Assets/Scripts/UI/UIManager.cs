using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Main UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject codeEditorPanel;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject questPanel;
    [SerializeField] private GameObject multiplayerPanel;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject achievementsPanel;
    
    [Header("Main Menu UI")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button continueGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button multiplayerButton;
    
    [Header("Game UI")]
    [SerializeField] private Text playerNameText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text experienceText;
    [SerializeField] private Slider experienceBar;
    [SerializeField] private Text regionText;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button codeEditorButton;
    [SerializeField] private Button questButton;
    [SerializeField] private Button inventoryButton;
    [SerializeField] private Button mentorButton;
    
    [Header("Pause Menu UI")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsFromPauseButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button saveGameButton;
    [SerializeField] private Button loadGameButton;
    
    [Header("Settings UI")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Dropdown qualityDropdown;
    [SerializeField] private Dropdown languageDropdown;
    [SerializeField] private Button applySettingsButton;
    [SerializeField] private Button resetSettingsButton;
    
    [Header("Notifications")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private Text notificationText;
    [SerializeField] private float notificationDuration = 3f;
    
    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider loadingProgressBar;
    [SerializeField] private Text loadingText;
    
    [Header("Tooltips")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private Text tooltipText;
    [SerializeField] private float tooltipDelay = 0.5f;
    
    [Header("Achievements")]
    [SerializeField] private GameObject achievementPopup;
    [SerializeField] private Text achievementTitleText;
    [SerializeField] private Text achievementDescriptionText;
    [SerializeField] private Image achievementIcon;
    
    private GameManager gameManager;
    private bool isInitialized = false;
    private GameObject currentActivePanel;
    private Coroutine notificationCoroutine;
    private Coroutine tooltipCoroutine;
    
    // UI State
    private bool isPaused = false;
    private bool isInCodeEditor = false;
    private bool isInDialogue = false;
    
    // Events
    public System.Action OnGameStarted;
    public System.Action OnGamePaused;
    public System.Action OnGameResumed;
    public System.Action OnSettingsChanged;
    
    public void Initialize(GameManager manager)
    {
        gameManager = manager;
        isInitialized = true;
        
        SetupUI();
        ShowMainMenu();
        
        Debug.Log("UI Manager initialized");
    }
    
    private void SetupUI()
    {
        // Main Menu Buttons
        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);
        
        if (continueGameButton != null)
            continueGameButton.onClick.AddListener(ContinueGame);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(ShowSettings);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
        
        if (multiplayerButton != null)
            multiplayerButton.onClick.AddListener(ShowMultiplayer);
        
        // Game UI Buttons
        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseGame);
        
        if (codeEditorButton != null)
            codeEditorButton.onClick.AddListener(OpenCodeEditor);
        
        if (questButton != null)
            questButton.onClick.AddListener(ShowQuests);
        
        if (inventoryButton != null)
            inventoryButton.onClick.AddListener(ShowInventory);
        
        if (mentorButton != null)
            mentorButton.onClick.AddListener(ShowMentor);
        
        // Pause Menu Buttons
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        
        if (settingsFromPauseButton != null)
            settingsFromPauseButton.onClick.AddListener(ShowSettings);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        
        if (saveGameButton != null)
            saveGameButton.onClick.AddListener(SaveGame);
        
        if (loadGameButton != null)
            loadGameButton.onClick.AddListener(LoadGame);
        
        // Settings UI
        if (applySettingsButton != null)
            applySettingsButton.onClick.AddListener(ApplySettings);
        
        if (resetSettingsButton != null)
            resetSettingsButton.onClick.AddListener(ResetSettings);
        
        // Initialize settings
        InitializeSettings();
    }
    
    private void InitializeSettings()
    {
        // Load saved settings
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
        
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }
        
        if (qualityDropdown != null)
        {
            qualityDropdown.value = PlayerPrefs.GetInt("QualityLevel", 2);
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        }
        
        if (languageDropdown != null)
        {
            languageDropdown.value = PlayerPrefs.GetInt("Language", 0);
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }
    }
    
    public void ShowMainMenu()
    {
        ShowPanel(mainMenuPanel);
        UpdateMainMenuUI();
    }
    
    public void ShowGameUI()
    {
        ShowPanel(gamePanel);
        UpdateGameUI();
    }
    
    public void ShowPauseMenu()
    {
        ShowPanel(pausePanel);
        isPaused = true;
        OnGamePaused?.Invoke();
    }
    
    public void ShowSettings()
    {
        ShowPanel(settingsPanel);
    }
    
    public void ShowCodeEditor()
    {
        ShowPanel(codeEditorPanel);
        isInCodeEditor = true;
        gameManager.OpenCodeEditor();
    }
    
    public void ShowDialogue()
    {
        ShowPanel(dialoguePanel);
        isInDialogue = true;
    }
    
    public void ShowQuests()
    {
        ShowPanel(questPanel);
        var questManager = FindObjectOfType<QuestManager>();
        if (questManager != null)
        {
            questManager.ShowQuestPanel();
        }
    }
    
    public void ShowMultiplayer()
    {
        ShowPanel(multiplayerPanel);
        var multiplayerManager = FindObjectOfType<MultiplayerManager>();
        if (multiplayerManager != null)
        {
            multiplayerManager.ShowMultiplayerPanel();
        }
    }
    
    public void ShowInventory()
    {
        ShowPanel(inventoryPanel);
    }
    
    public void ShowAchievements()
    {
        ShowPanel(achievementsPanel);
    }
    
    private void ShowPanel(GameObject panel)
    {
        // Hide current panel
        if (currentActivePanel != null)
        {
            currentActivePanel.SetActive(false);
        }
        
        // Show new panel
        if (panel != null)
        {
            panel.SetActive(true);
            currentActivePanel = panel;
        }
    }
    
    public void HideCurrentPanel()
    {
        if (currentActivePanel != null)
        {
            currentActivePanel.SetActive(false);
            currentActivePanel = null;
        }
    }
    
    private void UpdateMainMenuUI()
    {
        // Check if there's a saved game
        bool hasSaveGame = PlayerPrefs.HasKey("SaveGame");
        if (continueGameButton != null)
        {
            continueGameButton.interactable = hasSaveGame;
        }
    }
    
    private void UpdateGameUI()
    {
        if (gameManager == null) return;
        
        var playerData = gameManager.GetPlayerData();
        
        if (playerNameText != null)
            playerNameText.text = $"Player: {playerData.playerId}";
        
        if (levelText != null)
            levelText.text = $"Level: {CalculateLevel(playerData.mastery)}";
        
        if (experienceText != null)
            experienceText.text = $"XP: {playerData.mastery:F1}";
        
        if (experienceBar != null)
            experienceBar.value = playerData.mastery;
        
        if (regionText != null)
            regionText.text = $"Region: {playerData.currentRegion}";
    }
    
    private int CalculateLevel(float mastery)
    {
        return Mathf.FloorToInt(mastery * 10) + 1;
    }
    
    public void StartGame()
    {
        ShowLoadingScreen("Starting new game...");
        StartCoroutine(StartGameCoroutine());
    }
    
    private IEnumerator StartGameCoroutine()
    {
        // Simulate loading
        yield return new WaitForSeconds(2f);
        
        HideLoadingScreen();
        ShowGameUI();
        OnGameStarted?.Invoke();
        
        Debug.Log("New game started");
    }
    
    public void ContinueGame()
    {
        ShowLoadingScreen("Loading saved game...");
        StartCoroutine(ContinueGameCoroutine());
    }
    
    private IEnumerator ContinueGameCoroutine()
    {
        // Load saved game data
        yield return new WaitForSeconds(1f);
        
        HideLoadingScreen();
        ShowGameUI();
        
        Debug.Log("Saved game loaded");
    }
    
    public void PauseGame()
    {
        ShowPauseMenu();
    }
    
    public void ResumeGame()
    {
        HideCurrentPanel();
        ShowGameUI();
        isPaused = false;
        OnGameResumed?.Invoke();
    }
    
    public void ReturnToMainMenu()
    {
        HideCurrentPanel();
        ShowMainMenu();
        isPaused = false;
    }
    
    public void OpenCodeEditor()
    {
        ShowCodeEditor();
    }
    
    public void ShowMentor()
    {
        var aiMentorManager = FindObjectOfType<AIMentorManager>();
        if (aiMentorManager != null)
        {
            aiMentorManager.StartDialogue();
            ShowDialogue();
        }
    }
    
    public void SaveGame()
    {
        // Save game logic would go here
        ShowNotification("Game saved successfully!");
        Debug.Log("Game saved");
    }
    
    public void LoadGame()
    {
        ShowLoadingScreen("Loading game...");
        StartCoroutine(LoadGameCoroutine());
    }
    
    private IEnumerator LoadGameCoroutine()
    {
        yield return new WaitForSeconds(1f);
        HideLoadingScreen();
        ShowGameUI();
        ShowNotification("Game loaded successfully!");
    }
    
    public void ApplySettings()
    {
        // Apply current settings
        PlayerPrefs.Save();
        ShowNotification("Settings applied!");
        OnSettingsChanged?.Invoke();
    }
    
    public void ResetSettings()
    {
        // Reset to default settings
        PlayerPrefs.DeleteKey("MusicVolume");
        PlayerPrefs.DeleteKey("SFXVolume");
        PlayerPrefs.DeleteKey("Fullscreen");
        PlayerPrefs.DeleteKey("QualityLevel");
        PlayerPrefs.DeleteKey("Language");
        
        InitializeSettings();
        ShowNotification("Settings reset to defaults!");
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    // Settings change handlers
    private void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        // Apply to audio system
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        // Apply to audio system
    }
    
    private void OnFullscreenChanged(bool isFullscreen)
    {
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        Screen.fullScreen = isFullscreen;
    }
    
    private void OnQualityChanged(int qualityLevel)
    {
        PlayerPrefs.SetInt("QualityLevel", qualityLevel);
        QualitySettings.SetQualityLevel(qualityLevel);
    }
    
    private void OnLanguageChanged(int languageIndex)
    {
        PlayerPrefs.SetInt("Language", languageIndex);
        // Apply language change
    }
    
    // Notification system
    public void ShowNotification(string message)
    {
        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);
        
        notificationCoroutine = StartCoroutine(ShowNotificationCoroutine(message));
    }
    
    private IEnumerator ShowNotificationCoroutine(string message)
    {
        if (notificationPanel != null && notificationText != null)
        {
            notificationPanel.SetActive(true);
            notificationText.text = message;
            
            yield return new WaitForSeconds(notificationDuration);
            
            notificationPanel.SetActive(false);
        }
    }
    
    // Loading screen
    public void ShowLoadingScreen(string message = "Loading...")
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            
            if (loadingText != null)
                loadingText.text = message;
            
            if (loadingProgressBar != null)
                loadingProgressBar.value = 0f;
        }
    }
    
    public void UpdateLoadingProgress(float progress, string message = null)
    {
        if (loadingPanel != null && loadingPanel.activeSelf)
        {
            if (loadingProgressBar != null)
                loadingProgressBar.value = progress;
            
            if (loadingText != null && !string.IsNullOrEmpty(message))
                loadingText.text = message;
        }
    }
    
    public void HideLoadingScreen()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }
    
    // Tooltip system
    public void ShowTooltip(string text, Vector3 position)
    {
        if (tooltipCoroutine != null)
            StopCoroutine(tooltipCoroutine);
        
        tooltipCoroutine = StartCoroutine(ShowTooltipCoroutine(text, position));
    }
    
    private IEnumerator ShowTooltipCoroutine(string text, Vector3 position)
    {
        yield return new WaitForSeconds(tooltipDelay);
        
        if (tooltipPanel != null && tooltipText != null)
        {
            tooltipPanel.SetActive(true);
            tooltipText.text = text;
            tooltipPanel.transform.position = position;
        }
    }
    
    public void HideTooltip()
    {
        if (tooltipCoroutine != null)
            StopCoroutine(tooltipCoroutine);
        
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
    
    // Achievement system
    public void ShowAchievement(string title, string description, Sprite icon = null)
    {
        if (achievementPopup != null)
        {
            if (achievementTitleText != null)
                achievementTitleText.text = title;
            
            if (achievementDescriptionText != null)
                achievementDescriptionText.text = description;
            
            if (achievementIcon != null && icon != null)
                achievementIcon.sprite = icon;
            
            achievementPopup.SetActive(true);
            
            // Auto-hide after 3 seconds
            StartCoroutine(HideAchievementAfterDelay(3f));
        }
    }
    
    private IEnumerator HideAchievementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (achievementPopup != null)
            achievementPopup.SetActive(false);
    }
    
    // UI State getters
    public bool IsPaused()
    {
        return isPaused;
    }
    
    public bool IsInCodeEditor()
    {
        return isInCodeEditor;
    }
    
    public bool IsInDialogue()
    {
        return isInDialogue;
    }
    
    public void SetCodeEditorState(bool state)
    {
        isInCodeEditor = state;
    }
    
    public void SetDialogueState(bool state)
    {
        isInDialogue = state;
    }
    
    // Handle game state changes
    public void OnGameStateChanged(GameManager.GameState newState)
    {
        switch (newState)
        {
            case GameManager.GameState.MainMenu:
                ShowMainMenu();
                break;
            case GameManager.GameState.Playing:
                ShowGameUI();
                break;
            case GameManager.GameState.Paused:
                ShowPauseMenu();
                break;
            case GameManager.GameState.InCodeEditor:
                ShowCodeEditor();
                break;
            case GameManager.GameState.InDialogue:
                ShowDialogue();
                break;
        }
    }
} 