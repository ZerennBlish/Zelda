using UnityEngine;

public class SecretTransition : MonoBehaviour
{
    public Vector2 targetRoom;
    public Vector2 spawnOffset;

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Something entered: " + other.name + " Tag: " + other.tag);
        
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player detected, teleporting to " + targetRoom);
            RoomManager.Instance.TeleportToRoom(targetRoom, spawnOffset);
        }
    }
}