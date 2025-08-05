using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CodeBlock : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI Components")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Text displayText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;
    
    [Header("Block Properties")]
    [SerializeField] private string displayName;
    [SerializeField] private string codeType;
    [SerializeField] private string description;
    [SerializeField] private CodeBlockType blockType;
    [SerializeField] private Color blockColor = Color.white;
    
    [Header("Visual Settings")]
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float animationDuration = 0.2f;
    
    private bool isExecutionMode = false;
    private bool isSelected = false;
    private bool isHovered = false;
    private Vector3 originalScale;
    private Color originalColor;
    
    // Events
    public System.Action<CodeBlock> OnBlockClicked;
    public System.Action<CodeBlock> OnBlockEdited;
    public System.Action<CodeBlock> OnBlockDeleted;
    
    // Properties
    public string DisplayName => displayName;
    public string CodeType => codeType;
    public string Description => description;
    public CodeBlockType BlockType => blockType;
    public Color BlockColor => blockColor;
    public bool IsExecutionMode => isExecutionMode;
    
    void Awake()
    {
        originalScale = transform.localScale;
        if (backgroundImage != null)
            originalColor = backgroundImage.color;
        
        SetupUI();
    }
    
    void Start()
    {
        UpdateVisuals();
    }
    
    private void SetupUI()
    {
        // Set up edit button
        if (editButton != null)
        {
            editButton.onClick.AddListener(() => OnBlockEdited?.Invoke(this));
            editButton.gameObject.SetActive(false); // Hidden by default
        }
        
        // Set up delete button
        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(() => OnBlockDeleted?.Invoke(this));
            deleteButton.gameObject.SetActive(false); // Hidden by default
        }
    }
    
    public void Initialize(string name, string type, string desc, CodeBlockType typeEnum, Color color)
    {
        displayName = name;
        codeType = type;
        description = desc;
        blockType = typeEnum;
        blockColor = color;
        
        UpdateVisuals();
    }
    
    public void SetExecutionMode(bool executionMode)
    {
        isExecutionMode = executionMode;
        UpdateVisuals();
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisuals();
    }
    
    private void UpdateVisuals()
    {
        // Update background color
        if (backgroundImage != null)
        {
            Color targetColor = blockColor;
            
            if (isSelected)
                targetColor = selectedColor;
            else if (isHovered)
                targetColor = hoverColor;
            
            backgroundImage.color = targetColor;
        }
        
        // Update text
        if (displayText != null)
        {
            displayText.text = displayName;
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = description;
            descriptionText.gameObject.SetActive(isHovered || isSelected);
        }
        
        // Show/hide edit and delete buttons in execution mode
        if (editButton != null)
        {
            editButton.gameObject.SetActive(isExecutionMode && (isHovered || isSelected));
        }
        
        if (deleteButton != null)
        {
            deleteButton.gameObject.SetActive(isExecutionMode && (isHovered || isSelected));
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateVisuals();
        
        // Animate scale up
        LeanTween.scale(gameObject, originalScale * hoverScale, animationDuration)
            .setEase(LeanTweenType.easeOutBack);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateVisuals();
        
        // Animate scale back to normal
        LeanTween.scale(gameObject, originalScale, animationDuration)
            .setEase(LeanTweenType.easeOutBack);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        OnBlockClicked?.Invoke(this);
        
        // Toggle selection in execution mode
        if (isExecutionMode)
        {
            SetSelected(!isSelected);
        }
    }
    
    public void AnimateExecution()
    {
        // Flash effect when code is being executed
        LeanTween.color(backgroundImage.rectTransform, Color.white, 0.1f)
            .setLoopPingPong(1)
            .setEase(LeanTweenType.easeInOutSine);
    }
    
    public void AnimateError()
    {
        // Red flash effect for errors
        LeanTween.color(backgroundImage.rectTransform, Color.red, 0.1f)
            .setLoopPingPong(1)
            .setEase(LeanTweenType.easeInOutSine);
    }
    
    public void AnimateSuccess()
    {
        // Green flash effect for success
        LeanTween.color(backgroundImage.rectTransform, Color.green, 0.1f)
            .setLoopPingPong(1)
            .setEase(LeanTweenType.easeInOutSine);
    }
    
    public void SetParameter(string parameterName, string value)
    {
        // This would be used to set specific parameters for the block
        // For example, setting the loop count, variable name, etc.
        Debug.Log($"Setting parameter {parameterName} to {value} for block {displayName}");
    }
    
    public string GetParameter(string parameterName)
    {
        // This would return the current value of a parameter
        // For now, return a default value
        return "default_value";
    }
    
    public bool ValidateParameters()
    {
        // Validate that all required parameters are set
        // This would check if the block has all necessary information
        return true;
    }
    
    public string GenerateCode()
    {
        // Generate the actual code string for this block
        switch (codeType)
        {
            case "var":
                return $"var {GetParameter("variable_name")} = {GetParameter("value")};";
            case "for":
                return $"for (int i = 0; i < {GetParameter("count")}; i++) {{";
            case "while":
                return $"while ({GetParameter("condition")}) {{";
            case "if":
                return $"if ({GetParameter("condition")}) {{";
            case "function":
                return $"function {GetParameter("function_name")}() {{";
            case "move":
                return $"player.Move({GetParameter("direction")});";
            case "jump":
                return "player.Jump();";
            case "attack":
                return "player.Attack();";
            case "spell":
                return $"player.CastSpell({GetParameter("spell_type")});";
            case "open_gate":
                return $"world.OpenGate({GetParameter("gate_id")});";
            case "spawn_enemy":
                return $"world.SpawnEnemy({GetParameter("enemy_type")});";
            case "change_color":
                return $"object.ChangeColor({GetParameter("new_color")});";
            default:
                return $"// {displayName}";
        }
    }
    
    public void ShowParameterEditor()
    {
        // This would open a parameter editor UI for this block
        Debug.Log($"Opening parameter editor for {displayName}");
        
        // In a real implementation, this would show a popup or panel
        // where the user can edit the block's parameters
    }
    
    public void CopyBlock()
    {
        // Create a copy of this block
        GameObject copy = Instantiate(gameObject, transform.parent);
        CodeBlock copyBlock = copy.GetComponent<CodeBlock>();
        
        if (copyBlock != null)
        {
            copyBlock.Initialize(displayName, codeType, description, blockType, blockColor);
            copyBlock.SetExecutionMode(isExecutionMode);
        }
    }
    
    public void DeleteBlock()
    {
        // Animate deletion
        LeanTween.scale(gameObject, Vector3.zero, animationDuration)
            .setEase(LeanTweenType.easeInBack)
            .setOnComplete(() => {
                OnBlockDeleted?.Invoke(this);
                Destroy(gameObject);
            });
    }
    
    void OnDestroy()
    {
        // Clean up any ongoing animations
        LeanTween.cancel(gameObject);
    }
} 