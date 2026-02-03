using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    [Header("Landing")]
    public Vector2 landingOffset = Vector2.zero;
    
    [Header("Visual")]
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.05f;
    
    private Vector3 baseScale;
    
    void Start()
    {
        baseScale = transform.localScale;
    }
    
    void Update()
    {
        // Gentle pulse so player can spot grapple points
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = baseScale * pulse;
    }
    
    public Vector3 GetLandingPosition()
    {
        return transform.position + (Vector3)landingOffset;
    }
}