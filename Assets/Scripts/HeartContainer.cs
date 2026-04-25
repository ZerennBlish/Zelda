using UnityEngine;

public class HeartContainer : MonoBehaviour
{
    [Header("Visual")]
    public float bobSpeed = 2f;
    public float bobHeight = 0.15f;
    public float spinSpeed = 90f;

    [SerializeField] private string heartID;

    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        if (!string.IsNullOrEmpty(heartID) &&
            PlayerPrefs.GetInt("Heart_" + heartID, 0) == 1)
        {
            Destroy(gameObject);
            return;
        }

        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPosition.x, newY, transform.position.z);
        
        // Gentle spin
        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.IncreaseMaxHealth(1);

                if (!string.IsNullOrEmpty(heartID))
                {
                    PlayerPrefs.SetInt("Heart_" + heartID, 1);
                }

                Destroy(gameObject);
            }
        }
    }
}