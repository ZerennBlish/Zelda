using UnityEngine;

public class CrackedWall : MonoBehaviour
{
    public int health = 1;
    public GameObject revealedArea;

    [SerializeField] private string wallID;

    void Start()
    {
        if (!string.IsNullOrEmpty(wallID) &&
            PlayerPrefs.GetInt("Wall_" + wallID, 0) == 1)
        {
            if (revealedArea != null)
                revealedArea.SetActive(true);
            Destroy(gameObject);
            return;
        }

        if (revealedArea != null)
            revealedArea.SetActive(false);
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

        if (!string.IsNullOrEmpty(wallID))
        {
            PlayerPrefs.SetInt("Wall_" + wallID, 1);
        }

        Destroy(gameObject);
    }
}