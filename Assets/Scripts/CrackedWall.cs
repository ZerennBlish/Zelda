using UnityEngine;

public class CrackedWall : MonoBehaviour
{
    public int health = 1;
    public GameObject revealedArea;
    
    void Start()
    {
        if (revealedArea != null)
        {
            revealedArea.SetActive(false);
        }
    }
    
    public void TakeDamage(int damage)
    {
        health -= damage;
        
        if (health <= 0)
        {
            Break();
        }
    }
    
    void Break()
    {
        if (revealedArea != null)
        {
            revealedArea.SetActive(true);
        }
        
        Destroy(gameObject);
    }
}