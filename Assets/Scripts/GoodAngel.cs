using UnityEngine;

public class GoodAngel : MonoBehaviour
{
    [Header("Gift")]
    public int heartsToGive = 1;

    [Header("Visual")]
    public float hoverHeight = 0.2f;
    public float hoverSpeed = 2f;

    [SerializeField] private string angelID;

    private Vector3 startPosition;

    void Start()
    {
        if (!string.IsNullOrEmpty(angelID) &&
            PlayerPrefs.GetInt("Angel_" + angelID, 0) == 1)
        {
            Destroy(gameObject);
            return;
        }

        startPosition = transform.position;
    }

    void Update()
    {
        // Gentle hover animation
        float newY = startPosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        transform.position = new Vector3(startPosition.x, newY, transform.position.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!string.IsNullOrEmpty(angelID) &&
            PlayerPrefs.GetInt("Angel_" + angelID, 0) == 1) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.IncreaseMaxHealth(heartsToGive);

                if (!string.IsNullOrEmpty(angelID))
                {
                    PlayerPrefs.SetInt("Angel_" + angelID, 1);
                }

                // Optional: Add particle effect, sound, or animation here

                Destroy(gameObject, 0.5f); // Small delay so player sees what happened
            }
        }
    }
}