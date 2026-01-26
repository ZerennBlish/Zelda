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

    public void ChangeRoom(Vector2 direction, Vector2 spawnOffset)
    {
        currentRoom += direction;
        
        Vector3 newCamPos = new Vector3(
            currentRoom.x * roomWidth,
            currentRoom.y * roomHeight,
            mainCamera.position.z
        );
        mainCamera.position = newCamPos;
        
        // Player spawns at room center plus offset
        Vector3 roomCenter = new Vector3(
            currentRoom.x * roomWidth,
            currentRoom.y * roomHeight,
            0
        );
        player.position = roomCenter + (Vector3)spawnOffset;
    }
}