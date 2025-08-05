using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class WorldManager : MonoBehaviour
{
    [Header("World Configuration")]
    [SerializeField] private string apiBaseUrl = "http://localhost:5000";
    [SerializeField] private bool enableProceduralWorld = true;
    [SerializeField] private float worldUpdateInterval = 60f; // 1 minute
    
    [Header("World Regions")]
    [SerializeField] private Transform worldContainer;
    [SerializeField] private GameObject forestRegionPrefab;
    [SerializeField] private GameObject desertRegionPrefab;
    [SerializeField] private GameObject mountainsRegionPrefab;
    [SerializeField] private GameObject valleyRegionPrefab;
    
    [Header("NPC System")]
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private Transform npcContainer;
    [SerializeField] private int maxNpcsPerRegion = 5;
    
    [Header("Interactive Objects")]
    [SerializeField] private GameObject gatePrefab;
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private Transform interactiveObjectsContainer;
    
    [Header("Environmental Effects")]
    [SerializeField] private ParticleSystem forestParticles;
    [SerializeField] private ParticleSystem desertParticles;
    [SerializeField] private ParticleSystem mountainParticles;
    [SerializeField] private ParticleSystem valleyParticles;
    
    [Header("Audio")]
    [SerializeField] private AudioSource worldAudioSource;
    [SerializeField] private AudioClip forestAmbience;
    [SerializeField] private AudioClip desertAmbience;
    [SerializeField] private AudioClip mountainAmbience;
    [SerializeField] private AudioClip valleyAmbience;
    
    private GameManager gameManager;
    private bool isInitialized = false;
    
    // World data
    private Dictionary<string, WorldRegion> regions = new Dictionary<string, WorldRegion>();
    private Dictionary<string, List<NPCData>> regionNpcs = new Dictionary<string, List<NPCData>>();
    private Dictionary<string, List<InteractiveObject>> regionObjects = new Dictionary<string, List<InteractiveObject>>();
    private string currentRegion = "Forest of Repetition";
    
    // Events
    public System.Action<string> OnRegionChanged;
    public System.Action<NPCData> OnNpcInteraction;
    public System.Action<InteractiveObject> OnObjectInteraction;
    public System.Action<string> OnWorldEvent;
    
    // Update coroutine
    private Coroutine worldUpdateCoroutine;
    
    public void Initialize(GameManager manager, string apiUrl)
    {
        gameManager = manager;
        apiBaseUrl = apiUrl;
        isInitialized = true;
        
        InitializeWorld();
        StartCoroutine(LoadWorldData());
        
        Debug.Log("World Manager initialized");
    }
    
    private void InitializeWorld()
    {
        // Initialize regions
        regions["Forest of Repetition"] = new WorldRegion
        {
            name = "Forest of Repetition",
            description = "A mystical forest where loops and repetition rule",
            concept = "loops",
            difficulty = "beginner",
            isUnlocked = true
        };
        
        regions["Binary Desert"] = new WorldRegion
        {
            name = "Binary Desert",
            description = "A vast desert where decisions are black and white",
            concept = "conditionals",
            difficulty = "intermediate",
            isUnlocked = false
        };
        
        regions["Spiral Mountains"] = new WorldRegion
        {
            name = "Spiral Mountains",
            description = "Towering peaks where recursion echoes through the valleys",
            concept = "recursion",
            difficulty = "advanced",
            isUnlocked = false
        };
        
        regions["Function Valley"] = new WorldRegion
        {
            name = "Function Valley",
            description = "A peaceful valley where functions flow like rivers",
            concept = "functions",
            difficulty = "expert",
            isUnlocked = false
        };
        
        // Initialize NPC and object containers
        regionNpcs = new Dictionary<string, List<NPCData>>();
        regionObjects = new Dictionary<string, List<InteractiveObject>>();
        
        foreach (string regionName in regions.Keys)
        {
            regionNpcs[regionName] = new List<NPCData>();
            regionObjects[regionName] = new List<InteractiveObject>();
        }
    }
    
    private IEnumerator LoadWorldData()
    {
        yield return StartCoroutine(LoadWorldSegments());
        yield return StartCoroutine(LoadNPCs());
        yield return StartCoroutine(LoadInteractiveObjects());
        
        StartWorldUpdates();
    }
    
    private IEnumerator LoadWorldSegments()
    {
        var request = new GenerateWorldSegmentRequest
        {
            player_id = gameManager.GetPlayerId(),
            current_region = currentRegion,
            story_progress = 0
        };
        
        string jsonData = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest webRequest = new UnityWebRequest($"{apiBaseUrl}/generate_world_segment", "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<GenerateWorldSegmentResponse>(webRequest.downloadHandler.text);
                    
                    if (response.status == "success")
                    {
                        CreateWorldSegment(response.world_segment);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error loading world segments: {e.Message}");
                }
            }
        }
    }
    
    private IEnumerator LoadNPCs()
    {
        var request = new LoadNPCsRequest
        {
            player_id = gameManager.GetPlayerId(),
            current_region = currentRegion
        };
        
        string jsonData = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest webRequest = new UnityWebRequest($"{apiBaseUrl}/load_npcs", "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<LoadNPCsResponse>(webRequest.downloadHandler.text);
                    
                    if (response.status == "success")
                    {
                        foreach (var npc in response.npcs)
                        {
                            CreateNPC(npc);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error loading NPCs: {e.Message}");
                }
            }
        }
    }
    
    private IEnumerator LoadInteractiveObjects()
    {
        var request = new LoadInteractiveObjectsRequest
        {
            player_id = gameManager.GetPlayerId(),
            current_region = currentRegion
        };
        
        string jsonData = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest webRequest = new UnityWebRequest($"{apiBaseUrl}/load_interactive_objects", "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<LoadInteractiveObjectsResponse>(webRequest.downloadHandler.text);
                    
                    if (response.status == "success")
                    {
                        foreach (var obj in response.objects)
                        {
                            CreateInteractiveObject(obj);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error loading interactive objects: {e.Message}");
                }
            }
        }
    }
    
    private void CreateWorldSegment(WorldSegmentData segmentData)
    {
        GameObject regionPrefab = GetRegionPrefab(segmentData.region_name);
        if (regionPrefab == null) return;
        
        Vector3 position = new Vector3(segmentData.position_x, segmentData.position_y, segmentData.position_z);
        GameObject segment = Instantiate(regionPrefab, position, Quaternion.identity, worldContainer);
        segment.name = $"Segment_{segmentData.segment_id}";
        
        // Apply environmental effects
        ApplyEnvironmentalEffects(segmentData.region_name);
        
        Debug.Log($"Created world segment: {segmentData.segment_id} in {segmentData.region_name}");
    }
    
    private void CreateNPC(NPCData npcData)
    {
        if (npcPrefab == null) return;
        
        Vector3 position = new Vector3(npcData.position_x, npcData.position_y, npcData.position_z);
        GameObject npc = Instantiate(npcPrefab, position, Quaternion.identity, npcContainer);
        npc.name = npcData.npc_id;
        
        // Set up NPC behavior
        var npcBehavior = npc.GetComponent<NPCBehavior>();
        if (npcBehavior != null)
        {
            npcBehavior.Initialize(npcData);
            npcBehavior.OnInteraction += (npc) => OnNpcInteraction?.Invoke(npc);
        }
        
        // Add to region NPCs
        if (regionNpcs.ContainsKey(npcData.region))
        {
            regionNpcs[npcData.region].Add(npcData);
        }
        
        Debug.Log($"Created NPC: {npcData.name} in {npcData.region}");
    }
    
    private void CreateInteractiveObject(InteractiveObjectData objData)
    {
        GameObject objPrefab = GetInteractiveObjectPrefab(objData.object_type);
        if (objPrefab == null) return;
        
        Vector3 position = new Vector3(objData.position_x, objData.position_y, objData.position_z);
        GameObject obj = Instantiate(objPrefab, position, Quaternion.identity, interactiveObjectsContainer);
        obj.name = objData.object_id;
        
        // Set up interactive object behavior
        var interactiveBehavior = obj.GetComponent<InteractiveObjectBehavior>();
        if (interactiveBehavior != null)
        {
            interactiveBehavior.Initialize(objData);
            interactiveBehavior.OnInteraction += (obj) => OnObjectInteraction?.Invoke(obj);
        }
        
        // Add to region objects
        if (regionObjects.ContainsKey(objData.region))
        {
            regionObjects[objData.region].Add(new InteractiveObject { data = objData, gameObject = obj });
        }
        
        Debug.Log($"Created interactive object: {objData.object_id} in {objData.region}");
    }
    
    private GameObject GetRegionPrefab(string regionName)
    {
        switch (regionName)
        {
            case "Forest of Repetition": return forestRegionPrefab;
            case "Binary Desert": return desertRegionPrefab;
            case "Spiral Mountains": return mountainsRegionPrefab;
            case "Function Valley": return valleyRegionPrefab;
            default: return forestRegionPrefab;
        }
    }
    
    private GameObject GetInteractiveObjectPrefab(string objectType)
    {
        switch (objectType)
        {
            case "gate": return gatePrefab;
            case "chest": return chestPrefab;
            case "portal": return portalPrefab;
            default: return null;
        }
    }
    
    private void ApplyEnvironmentalEffects(string regionName)
    {
        // Stop all particle systems
        if (forestParticles != null) forestParticles.Stop();
        if (desertParticles != null) desertParticles.Stop();
        if (mountainParticles != null) mountainParticles.Stop();
        if (valleyParticles != null) valleyParticles.Stop();
        
        // Start appropriate particle system
        switch (regionName)
        {
            case "Forest of Repetition":
                if (forestParticles != null) forestParticles.Play();
                break;
            case "Binary Desert":
                if (desertParticles != null) desertParticles.Play();
                break;
            case "Spiral Mountains":
                if (mountainParticles != null) mountainParticles.Play();
                break;
            case "Function Valley":
                if (valleyParticles != null) valleyParticles.Play();
                break;
        }
        
        // Change ambient audio
        ChangeAmbientAudio(regionName);
    }
    
    private void ChangeAmbientAudio(string regionName)
    {
        if (worldAudioSource == null) return;
        
        AudioClip newAmbience = null;
        switch (regionName)
        {
            case "Forest of Repetition":
                newAmbience = forestAmbience;
                break;
            case "Binary Desert":
                newAmbience = desertAmbience;
                break;
            case "Spiral Mountains":
                newAmbience = mountainAmbience;
                break;
            case "Function Valley":
                newAmbience = valleyAmbience;
                break;
        }
        
        if (newAmbience != null && worldAudioSource.clip != newAmbience)
        {
            worldAudioSource.clip = newAmbience;
            worldAudioSource.Play();
        }
    }
    
    public void ChangeRegion(string newRegion)
    {
        if (!regions.ContainsKey(newRegion) || !regions[newRegion].isUnlocked)
        {
            Debug.LogWarning($"Cannot change to region: {newRegion} - not unlocked");
            return;
        }
        
        string previousRegion = currentRegion;
        currentRegion = newRegion;
        
        // Hide previous region NPCs and objects
        HideRegionContent(previousRegion);
        
        // Show new region NPCs and objects
        ShowRegionContent(newRegion);
        
        // Apply environmental effects
        ApplyEnvironmentalEffects(newRegion);
        
        OnRegionChanged?.Invoke(newRegion);
        
        Debug.Log($"Changed region from {previousRegion} to {newRegion}");
    }
    
    private void HideRegionContent(string regionName)
    {
        // Hide NPCs
        if (regionNpcs.ContainsKey(regionName))
        {
            foreach (var npc in regionNpcs[regionName])
            {
                // Find and hide NPC GameObject
                GameObject npcObj = GameObject.Find(npc.npc_id);
                if (npcObj != null)
                {
                    npcObj.SetActive(false);
                }
            }
        }
        
        // Hide interactive objects
        if (regionObjects.ContainsKey(regionName))
        {
            foreach (var obj in regionObjects[regionName])
            {
                if (obj.gameObject != null)
                {
                    obj.gameObject.SetActive(false);
                }
            }
        }
    }
    
    private void ShowRegionContent(string regionName)
    {
        // Show NPCs
        if (regionNpcs.ContainsKey(regionName))
        {
            foreach (var npc in regionNpcs[regionName])
            {
                GameObject npcObj = GameObject.Find(npc.npc_id);
                if (npcObj != null)
                {
                    npcObj.SetActive(true);
                }
            }
        }
        
        // Show interactive objects
        if (regionObjects.ContainsKey(regionName))
        {
            foreach (var obj in regionObjects[regionName])
            {
                if (obj.gameObject != null)
                {
                    obj.gameObject.SetActive(true);
                }
            }
        }
    }
    
    public void UnlockRegion(string regionName)
    {
        if (regions.ContainsKey(regionName))
        {
            regions[regionName].isUnlocked = true;
            Debug.Log($"Unlocked region: {regionName}");
            
            // Generate content for the new region
            StartCoroutine(GenerateRegionContent(regionName));
        }
    }
    
    private IEnumerator GenerateRegionContent(string regionName)
    {
        // Generate world segments
        yield return StartCoroutine(GenerateRegionSegments(regionName));
        
        // Generate NPCs
        yield return StartCoroutine(GenerateRegionNPCs(regionName));
        
        // Generate interactive objects
        yield return StartCoroutine(GenerateRegionObjects(regionName));
    }
    
    private IEnumerator GenerateRegionSegments(string regionName)
    {
        var request = new GenerateWorldSegmentRequest
        {
            player_id = gameManager.GetPlayerId(),
            current_region = regionName,
            story_progress = 0
        };
        
        string jsonData = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest webRequest = new UnityWebRequest($"{apiBaseUrl}/generate_world_segment", "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<GenerateWorldSegmentResponse>(webRequest.downloadHandler.text);
                    
                    if (response.status == "success")
                    {
                        CreateWorldSegment(response.world_segment);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error generating region segments: {e.Message}");
                }
            }
        }
    }
    
    private IEnumerator GenerateRegionNPCs(string regionName)
    {
        // Generate NPCs for the new region
        for (int i = 0; i < maxNpcsPerRegion; i++)
        {
            yield return StartCoroutine(GenerateNPC(regionName));
        }
    }
    
    private IEnumerator GenerateNPC(string regionName)
    {
        var request = new GenerateNPCRequest
        {
            player_id = gameManager.GetPlayerId(),
            region_name = regionName
        };
        
        string jsonData = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest webRequest = new UnityWebRequest($"{apiBaseUrl}/evolve_npc_behavior", "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<GenerateNPCResponse>(webRequest.downloadHandler.text);
                    
                    if (response.status == "success")
                    {
                        CreateNPC(response.npc);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error generating NPC: {e.Message}");
                }
            }
        }
    }
    
    private IEnumerator GenerateRegionObjects(string regionName)
    {
        // Generate interactive objects for the new region
        for (int i = 0; i < 3; i++)
        {
            yield return StartCoroutine(GenerateInteractiveObject(regionName));
        }
    }
    
    private IEnumerator GenerateInteractiveObject(string regionName)
    {
        // This would generate interactive objects
        // For now, just create some basic objects
        yield return null;
    }
    
    private void StartWorldUpdates()
    {
        if (worldUpdateCoroutine != null)
            StopCoroutine(worldUpdateCoroutine);
        
        worldUpdateCoroutine = StartCoroutine(WorldUpdateCoroutine());
    }
    
    private IEnumerator WorldUpdateCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(worldUpdateInterval);
            
            // Update NPC behaviors
            yield return StartCoroutine(UpdateNPCBehaviors());
            
            // Trigger world events
            yield return StartCoroutine(TriggerWorldEvents());
        }
    }
    
    private IEnumerator UpdateNPCBehaviors()
    {
        // Update NPC behaviors based on player actions
        foreach (var npcList in regionNpcs.Values)
        {
            foreach (var npc in npcList)
            {
                yield return StartCoroutine(UpdateNPCBehavior(npc));
            }
        }
    }
    
    private IEnumerator UpdateNPCBehavior(NPCData npc)
    {
        var request = new UpdateNPCBehaviorRequest
        {
            player_id = gameManager.GetPlayerId(),
            npc_id = npc.npc_id,
            player_actions = GetPlayerActions()
        };
        
        string jsonData = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        using (UnityWebRequest webRequest = new UnityWebRequest($"{apiBaseUrl}/evolve_npc_behavior", "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<UpdateNPCBehaviorResponse>(webRequest.downloadHandler.text);
                    
                    if (response.status == "success")
                    {
                        // Update NPC behavior
                        UpdateNPCInWorld(response.npc);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error updating NPC behavior: {e.Message}");
                }
            }
        }
    }
    
    private IEnumerator TriggerWorldEvents()
    {
        // Trigger random world events
        if (Random.Range(0f, 1f) < 0.1f) // 10% chance
        {
            string eventType = GetRandomWorldEvent();
            OnWorldEvent?.Invoke(eventType);
            
            Debug.Log($"World event triggered: {eventType}");
        }
        
        yield return null;
    }
    
    private string GetRandomWorldEvent()
    {
        string[] events = { "code_storm", "debug_rain", "algorithm_wind", "function_lightning" };
        return events[Random.Range(0, events.Length)];
    }
    
    private List<string> GetPlayerActions()
    {
        // This would return recent player actions
        // For now, return empty list
        return new List<string>();
    }
    
    private void UpdateNPCInWorld(NPCData updatedNpc)
    {
        // Find and update NPC in the world
        GameObject npcObj = GameObject.Find(updatedNpc.npc_id);
        if (npcObj != null)
        {
            var npcBehavior = npcObj.GetComponent<NPCBehavior>();
            if (npcBehavior != null)
            {
                npcBehavior.UpdateBehavior(updatedNpc);
            }
        }
    }
    
    public string GetCurrentRegion()
    {
        return currentRegion;
    }
    
    public List<string> GetUnlockedRegions()
    {
        List<string> unlocked = new List<string>();
        foreach (var region in regions.Values)
        {
            if (region.isUnlocked)
            {
                unlocked.Add(region.name);
            }
        }
        return unlocked;
    }
    
    public WorldRegion GetRegionData(string regionName)
    {
        return regions.ContainsKey(regionName) ? regions[regionName] : null;
    }
    
    public List<NPCData> GetRegionNPCs(string regionName)
    {
        return regionNpcs.ContainsKey(regionName) ? new List<NPCData>(regionNpcs[regionName]) : new List<NPCData>();
    }
}

[System.Serializable]
public class WorldRegion
{
    public string name;
    public string description;
    public string concept;
    public string difficulty;
    public bool isUnlocked;
}

[System.Serializable]
public class WorldSegmentData
{
    public string segment_id;
    public string region_name;
    public float position_x;
    public float position_y;
    public float position_z;
    public string environment_type;
    public List<string> interactive_objects;
    public List<string> environmental_effects;
    public List<string> dynamic_events;
}

[System.Serializable]
public class NPCData
{
    public string npc_id;
    public string name;
    public string region;
    public float position_x;
    public float position_y;
    public float position_z;
    public string personality;
    public string dialogue;
    public List<string> offered_quests;
    public string offered_quest_difficulty;
    public Dictionary<string, string> behavior_traits;
}

[System.Serializable]
public class InteractiveObject
{
    public InteractiveObjectData data;
    public GameObject gameObject;
}

[System.Serializable]
public class InteractiveObjectData
{
    public string object_id;
    public string object_type;
    public string region;
    public float position_x;
    public float position_y;
    public float position_z;
    public string interaction_type;
    public bool is_interactive;
}

// Request/Response classes
[System.Serializable]
public class GenerateWorldSegmentRequest
{
    public string player_id;
    public string current_region;
    public int story_progress;
}

[System.Serializable]
public class GenerateWorldSegmentResponse
{
    public string status;
    public WorldSegmentData world_segment;
}

[System.Serializable]
public class LoadNPCsRequest
{
    public string player_id;
    public string current_region;
}

[System.Serializable]
public class LoadNPCsResponse
{
    public string status;
    public List<NPCData> npcs;
}

[System.Serializable]
public class LoadInteractiveObjectsRequest
{
    public string player_id;
    public string current_region;
}

[System.Serializable]
public class LoadInteractiveObjectsResponse
{
    public string status;
    public List<InteractiveObjectData> objects;
}

[System.Serializable]
public class GenerateNPCRequest
{
    public string player_id;
    public string region_name;
}

[System.Serializable]
public class GenerateNPCResponse
{
    public string status;
    public NPCData npc;
}

[System.Serializable]
public class UpdateNPCBehaviorRequest
{
    public string player_id;
    public string npc_id;
    public List<string> player_actions;
}

[System.Serializable]
public class UpdateNPCBehaviorResponse
{
    public string status;
    public NPCData npc;
} 