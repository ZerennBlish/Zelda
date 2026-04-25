using UnityEngine;

public class Destructible : MonoBehaviour
{
    public int health = 1;
    private Dropper dropper;
    private bool isDead = false;

    void Start()
    {
        dropper = GetComponent<Dropper>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;

        if (health <= 0)
        {
            isDead = true;
            if (dropper != null)
            {
                dropper.Drop();
            }
            Destroy(gameObject);
        }
    }
}