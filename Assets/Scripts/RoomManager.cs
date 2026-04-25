using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;
    
    public Transform player;
    public Transform mainCamera;
    public float roomWidth = 18f;
    public float roomHeight = 10f;

    private Vector2 currentRoom = Vector2.zero;
    private bool isTransitioning = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveData())
        {
            int roomX = SaveManager.Instance.GetSavedRoomX();
            int roomY = SaveManager.Instance.GetSavedRoomY();
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

    public void ChangeRoom(Vector2 direction, Vector2 spawnOffset)
    {
        if (isTransitioning) return;
        isTransitioning = true;

        currentRoom += direction;

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

    public void TeleportToRoom(Vector2 targetRoom, Vector2 spawnOffset)
    {
        if (isTransitioning) return;
        isTransitioning = true;

        currentRoom = targetRoom;

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
    
    public Vector2 GetCurrentRoom()
    {
        return currentRoom;
    }
}
