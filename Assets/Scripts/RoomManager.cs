using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    public Transform player;
    public Transform mainCamera;
    public float roomWidth = 18f;
    public float roomHeight = 10f;
    [SerializeField] private WorldMapData worldMap;

    private Vector2 currentRoom = Vector2.zero;
    private bool isTransitioning = false;
    private int transitionFrame = -1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveData())
        {
            int roomX = SaveManager.Instance.GetSavedRoomX();
            int roomY = SaveManager.Instance.GetSavedRoomY();
            if (worldMap != null && !worldMap.Contains(new Vector2Int(roomX, roomY)))
            {
                Debug.LogWarning($"[RoomManager] Saved room ({roomX},{roomY}) not in WorldMapData. Falling back to (0,0).");
                roomX = 0;
                roomY = 0;
            }
            currentRoom = new Vector2(roomX, roomY);

            Vector3 camPos = new Vector3(roomX * roomWidth, roomY * roomHeight, mainCamera.position.z);
            mainCamera.position = camPos;
            Vector2 resumeOffset = worldMap != null
                ? worldMap.GetResumeSpawnOffset(new Vector2Int(roomX, roomY))
                : Vector2.zero;
            PlacePlayer(new Vector3(roomX * roomWidth, roomY * roomHeight, 0) + (Vector3)resumeOffset);
        }

        if (RoomTracker.Instance != null)
            RoomTracker.Instance.MarkVisited(currentRoom);

        if (MinimapUI.Instance != null)
            MinimapUI.Instance.RefreshMap();
    }

    public void ChangeRoom(Vector2 direction)
    {
        if (isTransitioning) return;
        Vector2 target = currentRoom + direction;
        Vector2 spawnOffset = new Vector2(
            -direction.x * (roomWidth / 2f - 1.5f),
            -direction.y * (roomHeight / 2f - 1.5f)
        );
        EnterRoom(target, spawnOffset);
    }

    public void TeleportToRoom(Vector2 targetRoom, Vector2 spawnOffset)
    {
        if (isTransitioning) return;
        EnterRoom(targetRoom, spawnOffset);
    }

    private void EnterRoom(Vector2 newRoom, Vector2 spawnOffset)
    {
        // Multiple collider callbacks can arrive during the same physics step.
        if (isTransitioning || transitionFrame == Time.frameCount) return;

        Vector2Int key = new Vector2Int(Mathf.RoundToInt(newRoom.x), Mathf.RoundToInt(newRoom.y));
        if (worldMap == null || !worldMap.Contains(key))
        {
            Debug.LogError($"[RoomManager] Target room {key} not in WorldMapData. Aborting transition.");
            return;
        }

        isTransitioning = true;
        transitionFrame = Time.frameCount;

        DestroyRoomLocalProjectiles();

        currentRoom = newRoom;

        Vector3 newCamPos = new Vector3(
            currentRoom.x * roomWidth,
            currentRoom.y * roomHeight,
            mainCamera.position.z
        );
        mainCamera.position = newCamPos;

        Vector3 roomCenter = new Vector3(
            currentRoom.x * roomWidth,
            currentRoom.y * roomHeight,
            0
        );
        PlacePlayer(roomCenter + (Vector3)spawnOffset);

        SaveGame();
        if (RoomTracker.Instance != null) RoomTracker.Instance.MarkVisited(currentRoom);
        if (MinimapUI.Instance != null) MinimapUI.Instance.OnRoomChanged();

        isTransitioning = false;
    }

    private void PlacePlayer(Vector3 position)
    {
        player.position = position;
        Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
        if (playerBody != null)
        {
            playerBody.position = position;
            playerBody.linearVelocity = Vector2.zero;
        }
    }

    void SaveGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.SaveAll();
        }
    }

    private void DestroyRoomLocalProjectiles()
    {
        // Boomerang and grappling hook are room-local — leaving them alive
        // across rooms causes pickup leaks, kinematic-enemy locks, and
        // chains drawn across the world. Their OnDestroy handlers do
        // their own cleanup (release carried items, restore enemy
        // physics, clear player locks).
        Boomerang boomerang = FindFirstObjectByType<Boomerang>();
        if (boomerang != null) Destroy(boomerang.gameObject);

        GrapplingHook hook = FindFirstObjectByType<GrapplingHook>();
        if (hook != null) Destroy(hook.gameObject);
    }

    public Vector2 GetCurrentRoom()
    {
        return currentRoom;
    }
}
