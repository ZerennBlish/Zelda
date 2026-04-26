using UnityEngine;
using TMPro;

public class BombUI : MonoBehaviour
{
    public TextMeshProUGUI bombText;
    public GameObject bombIcon;

    public void UpdateCount(int count)
    {
        if (bombText != null)
        {
            bombText.text = count.ToString();
        }
    }

    public void SetVisible(bool visible)
    {
        if (bombText != null) bombText.gameObject.SetActive(visible);
        if (bombIcon != null) bombIcon.SetActive(visible);
    }
}
