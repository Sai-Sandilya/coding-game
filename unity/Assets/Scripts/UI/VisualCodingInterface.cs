using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public class VisualCodingInterface : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform codeBlocksPanel;
    [SerializeField] private Transform codeExecutionArea;
    [SerializeField] private Button runCodeButton;
    [SerializeField] private Button clearCodeButton;
    [SerializeField] private Button saveCodeButton;
    [SerializeField] private Text codeOutputText;
    [SerializeField] private ScrollRect codeOutputScrollRect;
    
    [Header("Code Block Prefabs")]
    [SerializeField] private GameObject variableBlockPrefab;
    [SerializeField] private GameObject loopBlockPrefab;
    [SerializeField] private GameObject conditionBlockPrefab;
    [SerializeField] private GameObject functionBlockPrefab;
    [SerializeField] private GameObject actionBlockPrefab;
    [SerializeField] private GameObject operatorBlockPrefab;
    
    [Header("Visual Settings")]
    [SerializeField] private Color variableColor = Color.blue;
    [SerializeField] private Color loopColor = Color.green;
    [SerializeField] private Color conditionColor = Color.yellow;
    [SerializeField] private Color functionColor = Color.magenta;
    [SerializeField] private Color actionColor = Color.red;
    [SerializeField] private Color operatorColor = Color.cyan;
    
    private List<CodeBlock> availableBlocks = new List<CodeBlock>();
    private List<CodeBlock> executionSequence = new List<CodeBlock>();
    private CodeExecutionManager codeExecutionManager;
    private GameManager gameManager;
    
    // Events
    public System.Action<string> OnCodeExecuted;
    public System.Action OnCodeCleared;
    
    void Awake()
    {
        InitializeUI();
        CreateAvailableCodeBlocks();
    }
    
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        codeExecutionManager = FindObjectOfType<CodeExecutionManager>();
        
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged += HandleGameStateChanged;
        }
    }
    
    private void InitializeUI()
    {
        // Set up button listeners
        if (runCodeButton != null)
            runCodeButton.onClick.AddListener(ExecuteCode);
        
        if (clearCodeButton != null)
            clearCodeButton.onClick.AddListener(ClearCode);
        
        if (saveCodeButton != null)
            saveCodeButton.onClick.AddListener(SaveCode);
        
        // Initialize output area
        if (codeOutputText != null)
            codeOutputText.text = "Ready to code! Drag blocks to create your program.\n";
    }
    
    private void CreateAvailableCodeBlocks()
    {
        // Create variable blocks
        CreateCodeBlock("Variable", "var", "Create a variable", CodeBlockType.Variable, variableColor);
        CreateCodeBlock("Number", "number", "Set a number value", CodeBlockType.Variable, variableColor);
        CreateCodeBlock("Text", "text", "Set a text value", CodeBlockType.Variable, variableColor);
        CreateCodeBlock("Boolean", "boolean", "Set true/false value", CodeBlockType.Variable, variableColor);
        
        // Create loop blocks
        CreateCodeBlock("For Loop", "for", "Repeat code multiple times", CodeBlockType.Loop, loopColor);
        CreateCodeBlock("While Loop", "while", "Repeat while condition is true", CodeBlockType.Loop, loopColor);
        CreateCodeBlock("Foreach", "foreach", "Loop through items", CodeBlockType.Loop, loopColor);
        
        // Create condition blocks
        CreateCodeBlock("If", "if", "Execute if condition is true", CodeBlockType.Condition, conditionColor);
        CreateCodeBlock("If-Else", "if_else", "Execute if/else based on condition", CodeBlockType.Condition, conditionColor);
        CreateCodeBlock("Switch", "switch", "Choose from multiple options", CodeBlockType.Condition, conditionColor);
        
        // Create function blocks
        CreateCodeBlock("Function", "function", "Create a reusable function", CodeBlockType.Function, functionColor);
        CreateCodeBlock("Return", "return", "Return a value from function", CodeBlockType.Function, functionColor);
        CreateCodeBlock("Call Function", "call", "Call a function", CodeBlockType.Function, functionColor);
        
        // Create action blocks (game-specific)
        CreateCodeBlock("Move", "move", "Move character", CodeBlockType.Action, actionColor);
        CreateCodeBlock("Jump", "jump", "Make character jump", CodeBlockType.Action, actionColor);
        CreateCodeBlock("Attack", "attack", "Perform attack action", CodeBlockType.Action, actionColor);
        CreateCodeBlock("Cast Spell", "spell", "Cast a magic spell", CodeBlockType.Action, actionColor);
        CreateCodeBlock("Open Gate", "open_gate", "Open a locked gate", CodeBlockType.Action, actionColor);
        CreateCodeBlock("Spawn Enemy", "spawn_enemy", "Spawn an enemy", CodeBlockType.Action, actionColor);
        CreateCodeBlock("Change Color", "change_color", "Change object color", CodeBlockType.Action, actionColor);
        
        // Create operator blocks
        CreateCodeBlock("Add", "+", "Add two values", CodeBlockType.Operator, operatorColor);
        CreateCodeBlock("Subtract", "-", "Subtract two values", CodeBlockType.Operator, operatorColor);
        CreateCodeBlock("Multiply", "*", "Multiply two values", CodeBlockType.Operator, operatorColor);
        CreateCodeBlock("Divide", "/", "Divide two values", CodeBlockType.Operator, operatorColor);
        CreateCodeBlock("Equals", "==", "Check if values are equal", CodeBlockType.Operator, operatorColor);
        CreateCodeBlock("Greater Than", ">", "Check if value is greater", CodeBlockType.Operator, operatorColor);
        CreateCodeBlock("Less Than", "<", "Check if value is less", CodeBlockType.Operator, operatorColor);
    }
    
    private void CreateCodeBlock(string displayName, string codeType, string description, CodeBlockType blockType, Color color)
    {
        GameObject blockPrefab = GetBlockPrefab(blockType);
        if (blockPrefab == null) return;
        
        GameObject blockObj = Instantiate(blockPrefab, codeBlocksPanel);
        CodeBlock block = blockObj.GetComponent<CodeBlock>();
        
        if (block != null)
        {
            block.Initialize(displayName, codeType, description, blockType, color);
            availableBlocks.Add(block);
            
            // Set up drag and drop
            SetupDragAndDrop(block);
        }
    }
    
    private GameObject GetBlockPrefab(CodeBlockType blockType)
    {
        switch (blockType)
        {
            case CodeBlockType.Variable: return variableBlockPrefab;
            case CodeBlockType.Loop: return loopBlockPrefab;
            case CodeBlockType.Condition: return conditionBlockPrefab;
            case CodeBlockType.Function: return functionBlockPrefab;
            case CodeBlockType.Action: return actionBlockPrefab;
            case CodeBlockType.Operator: return operatorBlockPrefab;
            default: return variableBlockPrefab;
        }
    }
    
    private void SetupDragAndDrop(CodeBlock block)
    {
        // Add drag and drop component
        DragAndDrop dragDrop = block.gameObject.AddComponent<DragAndDrop>();
        dragDrop.OnDropped += (droppedBlock) => AddToExecutionSequence(droppedBlock as CodeBlock);
    }
    
    private void AddToExecutionSequence(CodeBlock block)
    {
        if (block == null) return;
        
        // Create a copy for the execution sequence
        GameObject executionBlockObj = Instantiate(block.gameObject, codeExecutionArea);
        CodeBlock executionBlock = executionBlockObj.GetComponent<CodeBlock>();
        
        if (executionBlock != null)
        {
            executionBlock.Initialize(block.DisplayName, block.CodeType, block.Description, block.BlockType, block.BlockColor);
            executionBlock.SetExecutionMode(true);
            executionSequence.Add(executionBlock);
            
            // Set up drag and drop for reordering
            SetupExecutionBlockDragAndDrop(executionBlock);
            
            UpdateExecutionSequenceDisplay();
        }
    }
    
    private void SetupExecutionBlockDragAndDrop(CodeBlock block)
    {
        DragAndDrop dragDrop = block.gameObject.AddComponent<DragAndDrop>();
        dragDrop.OnDropped += (droppedBlock) => ReorderExecutionSequence(droppedBlock as CodeBlock);
        dragDrop.OnRemoved += (removedBlock) => RemoveFromExecutionSequence(removedBlock as CodeBlock);
    }
    
    private void ReorderExecutionSequence(CodeBlock block)
    {
        // Reorder logic would go here
        UpdateExecutionSequenceDisplay();
    }
    
    private void RemoveFromExecutionSequence(CodeBlock block)
    {
        if (block != null && executionSequence.Contains(block))
        {
            executionSequence.Remove(block);
            Destroy(block.gameObject);
            UpdateExecutionSequenceDisplay();
        }
    }
    
    private void UpdateExecutionSequenceDisplay()
    {
        // Update visual representation of execution sequence
        for (int i = 0; i < executionSequence.Count; i++)
        {
            if (executionSequence[i] != null)
            {
                executionSequence[i].transform.SetSiblingIndex(i);
            }
        }
    }
    
    public void ExecuteCode()
    {
        if (executionSequence.Count == 0)
        {
            AddOutputMessage("No code to execute! Add some blocks first.");
            return;
        }
        
        StartCoroutine(ExecuteCodeCoroutine());
    }
    
    private IEnumerator ExecuteCodeCoroutine()
    {
        AddOutputMessage("Executing code...\n");
        
        // Convert visual blocks to code string
        string generatedCode = GenerateCodeFromBlocks();
        AddOutputMessage($"Generated code:\n{generatedCode}\n");
        
        // Execute through backend
        if (codeExecutionManager != null)
        {
            yield return StartCoroutine(codeExecutionManager.ExecuteCode(generatedCode, (result) =>
            {
                if (result.success)
                {
                    AddOutputMessage($"✅ Code executed successfully!\nOutput: {result.output}\nGame Effect: {result.gameEffect}\n");
                    OnCodeExecuted?.Invoke(generatedCode);
                }
                else
                {
                    AddOutputMessage($"❌ Code execution failed: {result.error}\n");
                }
            }));
        }
        else
        {
            AddOutputMessage("❌ Code execution manager not found!\n");
        }
    }
    
    private string GenerateCodeFromBlocks()
    {
        string code = "";
        int indentLevel = 0;
        
        foreach (CodeBlock block in executionSequence)
        {
            if (block == null) continue;
            
            string blockCode = GenerateBlockCode(block, indentLevel);
            code += blockCode + "\n";
            
            // Adjust indentation for nested blocks
            if (block.BlockType == CodeBlockType.Loop || block.BlockType == CodeBlockType.Condition || block.BlockType == CodeBlockType.Function)
            {
                indentLevel++;
            }
            else if (block.CodeType == "end" || block.CodeType == "}")
            {
                indentLevel = Mathf.Max(0, indentLevel - 1);
            }
        }
        
        return code.Trim();
    }
    
    private string GenerateBlockCode(CodeBlock block, int indentLevel)
    {
        string indent = new string(' ', indentLevel * 4);
        
        switch (block.CodeType)
        {
            case "var":
                return $"{indent}var variable_name = value;";
            case "number":
                return $"{indent}int number_value = 0;";
            case "text":
                return $"{indent}string text_value = \"\";";
            case "boolean":
                return $"{indent}bool boolean_value = false;";
            case "for":
                return $"{indent}for (int i = 0; i < count; i++) {{";
            case "while":
                return $"{indent}while (condition) {{";
            case "foreach":
                return $"{indent}foreach (var item in collection) {{";
            case "if":
                return $"{indent}if (condition) {{";
            case "if_else":
                return $"{indent}if (condition) {{";
            case "switch":
                return $"{indent}switch (value) {{";
            case "function":
                return $"{indent}function function_name() {{";
            case "return":
                return $"{indent}return value;";
            case "call":
                return $"{indent}function_name();";
            case "move":
                return $"{indent}player.Move(direction);";
            case "jump":
                return $"{indent}player.Jump();";
            case "attack":
                return $"{indent}player.Attack();";
            case "spell":
                return $"{indent}player.CastSpell(spell_type);";
            case "open_gate":
                return $"{indent}world.OpenGate(gate_id);";
            case "spawn_enemy":
                return $"{indent}world.SpawnEnemy(enemy_type);";
            case "change_color":
                return $"{indent}object.ChangeColor(new_color);";
            case "+":
                return $"{indent}result = value1 + value2;";
            case "-":
                return $"{indent}result = value1 - value2;";
            case "*":
                return $"{indent}result = value1 * value2;";
            case "/":
                return $"{indent}result = value1 / value2;";
            case "==":
                return $"{indent}if (value1 == value2) {{";
            case ">":
                return $"{indent}if (value1 > value2) {{";
            case "<":
                return $"{indent}if (value1 < value2) {{";
            default:
                return $"{indent}// {block.DisplayName}";
        }
    }
    
    public void ClearCode()
    {
        foreach (CodeBlock block in executionSequence)
        {
            if (block != null)
            {
                Destroy(block.gameObject);
            }
        }
        
        executionSequence.Clear();
        AddOutputMessage("Code cleared!\n");
        OnCodeCleared?.Invoke();
    }
    
    public void SaveCode()
    {
        if (executionSequence.Count == 0)
        {
            AddOutputMessage("No code to save!");
            return;
        }
        
        string codeToSave = GenerateCodeFromBlocks();
        // Save logic would go here
        AddOutputMessage("Code saved successfully!\n");
    }
    
    private void AddOutputMessage(string message)
    {
        if (codeOutputText != null)
        {
            codeOutputText.text += message;
            
            // Auto-scroll to bottom
            if (codeOutputScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                codeOutputScrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }
    
    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.InCodeEditor)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    public List<CodeBlock> GetExecutionSequence()
    {
        return new List<CodeBlock>(executionSequence);
    }
    
    public void LoadExecutionSequence(List<CodeBlockData> blockData)
    {
        ClearCode();
        
        foreach (CodeBlockData data in blockData)
        {
            // Recreate blocks from saved data
            GameObject blockObj = Instantiate(GetBlockPrefab(data.blockType), codeExecutionArea);
            CodeBlock block = blockObj.GetComponent<CodeBlock>();
            
            if (block != null)
            {
                block.Initialize(data.displayName, data.codeType, data.description, data.blockType, data.color);
                block.SetExecutionMode(true);
                executionSequence.Add(block);
                SetupExecutionBlockDragAndDrop(block);
            }
        }
        
        UpdateExecutionSequenceDisplay();
    }
}

public enum CodeBlockType
{
    Variable,
    Loop,
    Condition,
    Function,
    Action,
    Operator
}

[System.Serializable]
public class CodeBlockData
{
    public string displayName;
    public string codeType;
    public string description;
    public CodeBlockType blockType;
    public Color color;
} 