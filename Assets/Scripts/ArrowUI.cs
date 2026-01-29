using UnityEngine;
using TMPro;

public class ArrowUI : MonoBehaviour
{
    public TextMeshProUGUI arrowText;
    public GameObject arrowIcon;

    public void UpdateCount(int count)
    {
        if (arrowText != null)
        {
            arrowText.text = count.ToString();
        }
    }
}