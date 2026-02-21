using UnityEngine;

public class SecretTransition : MonoBehaviour
{
    public Vector2 targetRoom;
    public Vector2 spawnOffset;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            RoomManager.Instance.TeleportToRoom(targetRoom, spawnOffset);
        }
    }
}