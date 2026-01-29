using UnityEngine;

public class RoomTransition : MonoBehaviour
{
    public Vector2 direction;
    public Vector2 spawnOffset;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            RoomManager.Instance.ChangeRoom(direction, spawnOffset);
        }
    }
}