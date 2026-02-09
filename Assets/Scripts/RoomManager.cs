using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;
    
    public Transform player;
    public Transform mainCamera;
    public float roomWidth = 18f;
    public float roomHeight = 10f;

    private Vector2 currentRoom = Vector2.zero;

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
    }

    public void ChangeRoom(Vector2 direction, Vector2 spawnOffset)
    {
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
    }
    
    public void TeleportToRoom(Vector2 targetRoom, Vector2 spawnOffset)
    {
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
