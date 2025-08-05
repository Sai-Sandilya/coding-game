using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.UI.Extensions;

public class DragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("Drag Settings")]
    [SerializeField] private bool canDrag = true;
    [SerializeField] private bool canDrop = true;
    [SerializeField] private float dragScale = 1.1f;
    [SerializeField] private float dragAlpha = 0.8f;
    
    [Header("Visual Feedback")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform dragPreview;
    [SerializeField] private Image dropZoneIndicator;
    
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private float originalAlpha;
    private Transform originalParent;
    private bool isDragging = false;
    
    // Events
    public System.Action<GameObject> OnDropped;
    public System.Action<GameObject> OnRemoved;
    public System.Action<GameObject> OnDragStarted;
    public System.Action<GameObject> OnDragEnded;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        originalScale = transform.localScale;
        originalAlpha = canvasGroup.alpha;
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!canDrag) return;
        
        isDragging = true;
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        
        // Create drag preview
        CreateDragPreview();
        
        // Visual feedback
        transform.localScale = originalScale * dragScale;
        canvasGroup.alpha = dragAlpha;
        canvasGroup.blocksRaycasts = false;
        
        // Move to top of hierarchy for proper rendering
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();
        
        OnDragStarted?.Invoke(gameObject);
        
        Debug.Log($"Started dragging {gameObject.name}");
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        // Update position
        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, 
            eventData.position, 
            eventData.pressEventCamera, 
            out localPointerPosition))
        {
            rectTransform.anchoredPosition = localPointerPosition;
        }
        
        // Update drag preview
        if (dragPreview != null)
        {
            dragPreview.anchoredPosition = rectTransform.anchoredPosition;
        }
        
        // Show drop zone indicators
        ShowDropZoneIndicators(eventData.position);
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        isDragging = false;
        
        // Restore original appearance
        transform.localScale = originalScale;
        canvasGroup.alpha = originalAlpha;
        canvasGroup.blocksRaycasts = true;
        
        // Hide drop zone indicators
        HideDropZoneIndicators();
        
        // Check if we dropped on a valid drop zone
        GameObject dropTarget = GetDropTarget(eventData.position);
        
        if (dropTarget != null && canDrop)
        {
            // Successfully dropped
            HandleSuccessfulDrop(dropTarget);
        }
        else
        {
            // Return to original position
            ReturnToOriginalPosition();
        }
        
        // Clean up drag preview
        DestroyDragPreview();
        
        OnDragEnded?.Invoke(gameObject);
        
        Debug.Log($"Ended dragging {gameObject.name}");
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        // This is called when something is dropped on this object
        if (!canDrop) return;
        
        GameObject droppedObject = eventData.pointerDrag;
        if (droppedObject != null)
        {
            DragAndDrop droppedDragAndDrop = droppedObject.GetComponent<DragAndDrop>();
            if (droppedDragAndDrop != null)
            {
                droppedDragAndDrop.OnDropped?.Invoke(gameObject);
            }
        }
    }
    
    private void CreateDragPreview()
    {
        if (dragPreview == null)
        {
            GameObject previewObj = new GameObject("DragPreview");
            dragPreview = previewObj.AddComponent<RectTransform>();
            
            // Copy the visual appearance
            Image originalImage = GetComponent<Image>();
            if (originalImage != null)
            {
                Image previewImage = previewObj.AddComponent<Image>();
                previewImage.sprite = originalImage.sprite;
                previewImage.color = originalImage.color;
                previewImage.raycastTarget = false;
            }
            
            // Set up the preview
            dragPreview.SetParent(canvas.transform);
            dragPreview.sizeDelta = rectTransform.sizeDelta;
            dragPreview.localScale = originalScale * dragScale;
            
            CanvasGroup previewCanvasGroup = previewObj.AddComponent<CanvasGroup>();
            previewCanvasGroup.alpha = 0.5f;
        }
    }
    
    private void DestroyDragPreview()
    {
        if (dragPreview != null)
        {
            Destroy(dragPreview.gameObject);
            dragPreview = null;
        }
    }
    
    private GameObject GetDropTarget(Vector2 screenPosition)
    {
        // Raycast to find drop targets
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPosition;
        
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        foreach (RaycastResult result in results)
        {
            // Check if the result is a valid drop zone
            DropZone dropZone = result.gameObject.GetComponent<DropZone>();
            if (dropZone != null && dropZone.CanAcceptDrop(gameObject))
            {
                return result.gameObject;
            }
            
            // Check if it's another drag and drop object that can accept drops
            DragAndDrop otherDragAndDrop = result.gameObject.GetComponent<DragAndDrop>();
            if (otherDragAndDrop != null && otherDragAndDrop.canDrop)
            {
                return result.gameObject;
            }
        }
        
        return null;
    }
    
    private void HandleSuccessfulDrop(GameObject dropTarget)
    {
        Debug.Log($"Dropped {gameObject.name} on {dropTarget.name}");
        
        // Notify the drop target
        DropZone dropZone = dropTarget.GetComponent<DropZone>();
        if (dropZone != null)
        {
            dropZone.OnObjectDropped(gameObject);
        }
        
        // Notify our own listeners
        OnDropped?.Invoke(dropTarget);
        
        // Position the object at the drop location
        RectTransform targetRect = dropTarget.GetComponent<RectTransform>();
        if (targetRect != null)
        {
            transform.SetParent(targetRect);
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
    
    private void ReturnToOriginalPosition()
    {
        Debug.Log($"Returning {gameObject.name} to original position");
        
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
        
        // Notify that the object was removed from its intended destination
        OnRemoved?.Invoke(gameObject);
    }
    
    private void ShowDropZoneIndicators(Vector2 screenPosition)
    {
        // Find all drop zones and show indicators for valid ones
        DropZone[] dropZones = FindObjectsOfType<DropZone>();
        
        foreach (DropZone dropZone in dropZones)
        {
            if (dropZone.CanAcceptDrop(gameObject))
            {
                dropZone.ShowDropIndicator(true);
            }
            else
            {
                dropZone.ShowDropIndicator(false);
            }
        }
    }
    
    private void HideDropZoneIndicators()
    {
        // Hide all drop zone indicators
        DropZone[] dropZones = FindObjectsOfType<DropZone>();
        
        foreach (DropZone dropZone in dropZones)
        {
            dropZone.ShowDropIndicator(false);
        }
    }
    
    public void SetCanDrag(bool canDrag)
    {
        this.canDrag = canDrag;
    }
    
    public void SetCanDrop(bool canDrop)
    {
        this.canDrop = canDrop;
    }
    
    public bool IsDragging()
    {
        return isDragging;
    }
    
    void OnDestroy()
    {
        // Clean up drag preview if it still exists
        if (dragPreview != null)
        {
            Destroy(dragPreview.gameObject);
        }
    }
}

// Helper class for drop zones
public class DropZone : MonoBehaviour
{
    [Header("Drop Zone Settings")]
    [SerializeField] private bool acceptsCodeBlocks = true;
    [SerializeField] private bool acceptsVariables = true;
    [SerializeField] private bool acceptsLoops = true;
    [SerializeField] private bool acceptsConditions = true;
    [SerializeField] private bool acceptsFunctions = true;
    [SerializeField] private bool acceptsActions = true;
    [SerializeField] private bool acceptsOperators = true;
    
    [Header("Visual Feedback")]
    [SerializeField] private Image dropIndicator;
    [SerializeField] private Color validDropColor = Color.green;
    [SerializeField] private Color invalidDropColor = Color.red;
    
    // Events
    public System.Action<GameObject> OnObjectDropped;
    
    void Awake()
    {
        if (dropIndicator != null)
        {
            dropIndicator.gameObject.SetActive(false);
        }
    }
    
    public bool CanAcceptDrop(GameObject droppedObject)
    {
        CodeBlock codeBlock = droppedObject.GetComponent<CodeBlock>();
        if (codeBlock == null) return false;
        
        switch (codeBlock.BlockType)
        {
            case CodeBlockType.Variable:
                return acceptsVariables;
            case CodeBlockType.Loop:
                return acceptsLoops;
            case CodeBlockType.Condition:
                return acceptsConditions;
            case CodeBlockType.Function:
                return acceptsFunctions;
            case CodeBlockType.Action:
                return acceptsActions;
            case CodeBlockType.Operator:
                return acceptsOperators;
            default:
                return acceptsCodeBlocks;
        }
    }
    
    public void OnObjectDropped(GameObject droppedObject)
    {
        Debug.Log($"Object dropped on drop zone: {droppedObject.name}");
        OnObjectDropped?.Invoke(droppedObject);
    }
    
    public void ShowDropIndicator(bool show)
    {
        if (dropIndicator != null)
        {
            dropIndicator.gameObject.SetActive(show);
            
            if (show)
            {
                // Set color based on whether we can accept the current drag
                GameObject draggedObject = GetCurrentDraggedObject();
                if (draggedObject != null && CanAcceptDrop(draggedObject))
                {
                    dropIndicator.color = validDropColor;
                }
                else
                {
                    dropIndicator.color = invalidDropColor;
                }
            }
        }
    }
    
    private GameObject GetCurrentDraggedObject()
    {
        // Find the currently dragged object
        DragAndDrop[] dragAndDrops = FindObjectsOfType<DragAndDrop>();
        foreach (DragAndDrop dragAndDrop in dragAndDrops)
        {
            if (dragAndDrop.IsDragging())
            {
                return dragAndDrop.gameObject;
            }
        }
        return null;
    }
} 