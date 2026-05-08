using UnityEngine;

public class RoomTransition : MonoBehaviour
{
    public Vector2 direction;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.transform == other.transform.root)
        {
            RoomManager.Instance.ChangeRoom(direction);
        }
    }
}
