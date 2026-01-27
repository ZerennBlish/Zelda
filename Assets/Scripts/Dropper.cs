using UnityEngine;

public class Dropper : MonoBehaviour
{
    public GameObject[] possibleDrops;
    public float dropChance = 0.5f;
    
    public void Drop()
    {
        if (possibleDrops.Length == 0) return;
        
        if (Random.value <= dropChance)
        {
            int randomIndex = Random.Range(0, possibleDrops.Length);
            Instantiate(possibleDrops[randomIndex], transform.position, Quaternion.identity);
        }
    }
}