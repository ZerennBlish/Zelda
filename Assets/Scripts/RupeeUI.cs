using UnityEngine;
using TMPro;

public class RupeeUI : MonoBehaviour
{
    public TextMeshProUGUI rupeeText;
    
    void Start()
    {
        UpdateRupees(0);
    }
    
    public void UpdateRupees(int amount)
    {
        if (rupeeText != null)
        {
            rupeeText.text = "x " + amount;
        }
    }
}