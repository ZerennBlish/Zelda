using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    public float lifetime = 0.5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}