using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthUI : MonoBehaviour
{
    [Header("Heart Sprites")]
    public Sprite fullHeart;
    public Sprite emptyHeart;
    
    [Header("Heart Setup")]
    public GameObject heartPrefab;
    public Transform heartContainer;
    public float heartSpacing = 40f;
    
    private List<Image> heartImages = new List<Image>();

    public void UpdateHearts(int currentHealth, int maxHealth)
    {
        Debug.Log($"UpdateHearts called: current={currentHealth}, max={maxHealth}");
        Debug.Log($"heartPrefab is null? {heartPrefab == null}");
        Debug.Log($"heartContainer is null? {heartContainer == null}");
        
        while (heartImages.Count < maxHealth)
        {
            Debug.Log($"Adding heart {heartImages.Count + 1}");
            AddHeart();
        }
        
        while (heartImages.Count > maxHealth)
        {
            RemoveHeart();
        }
        
        for (int i = 0; i < heartImages.Count; i++)
        {
            heartImages[i].sprite = i < currentHealth ? fullHeart : emptyHeart;
        }
    }
    
    void AddHeart()
    {
        GameObject newHeart = Instantiate(heartPrefab, heartContainer);
        Debug.Log($"Created heart: {newHeart.name}");
        
        Image heartImage = newHeart.GetComponent<Image>();
        
        RectTransform rect = newHeart.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(heartImages.Count * heartSpacing, 0);
        
        heartImages.Add(heartImage);
    }
    
    void RemoveHeart()
    {
        if (heartImages.Count > 0)
        {
            int lastIndex = heartImages.Count - 1;
            Destroy(heartImages[lastIndex].gameObject);
            heartImages.RemoveAt(lastIndex);
        }
    }
}