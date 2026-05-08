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
            player.position = new Vector3(roomX * roomWidth, roomY * roomHeight, 0);
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
            -direction.x * (roomWidth / 2f - 1f),
            -direction.y * (roomHeight / 2f - 1f)
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
        Vector2Int key = new Vector2Int(Mathf.RoundToInt(newRoom.x), Mathf.RoundToInt(newRoom.y));
        if (worldMap == null || !worldMap.Contains(key))
        {
            Debug.LogError($"[RoomManager] Target room {key} not in WorldMapData. Aborting transition.");
            return;
        }

        isTransitioning = true;

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
        player.position = roomCenter + (Vector3)spawnOffset;

        SaveGame();
        if (RoomTracker.Instance != null) RoomTracker.Instance.MarkVisited(currentRoom);
        if (MinimapUI.Instance != null) MinimapUI.Instance.OnRoomChanged();

        isTransitioning = false;
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
