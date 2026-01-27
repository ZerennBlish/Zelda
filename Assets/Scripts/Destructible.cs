using UnityEngine;

public class Destructible : MonoBehaviour
{
    public int health = 1;
    private Dropper dropper;
    
    void Start()
    {
        dropper = GetComponent<Dropper>();
    }
    
    public void TakeDamage(int damage)
    {
        health -= damage;
        
        if (health <= 0)
        {
            if (dropper != null)
            {
                dropper.Drop();
            }
            Destroy(gameObject);
        }
    }
}