using UnityEngine;
using TMPro;

public class BombUI : MonoBehaviour
{
    public TextMeshProUGUI bombText;

    public void UpdateCount(int count)
    {
        if (bombText != null)
        {
            bombText.text = count.ToString();
        }
    }
}