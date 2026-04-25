using UnityEngine;

public class Dropper : MonoBehaviour
{
    public GameObject[] possibleDrops;
    public float dropChance = 0.5f;
    
    public void Drop()
    {
        if (possibleDrops == null || possibleDrops.Length == 0) return;

        if (Random.value <= dropChance)
        {
            int randomIndex = Random.Range(0, possibleDrops.Length);
            GameObject drop = possibleDrops[randomIndex];
            if (drop == null) return;
            Instantiate(drop, transform.position, Quaternion.identity);
        }
    }
}