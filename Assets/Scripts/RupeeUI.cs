using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RupeeUI : MonoBehaviour
{
    public TextMeshProUGUI rupeeText;
    public Image rupeeIcon;

    public void UpdateRupees(int amount)
    {
        if (rupeeText != null)
        {
            rupeeText.text = amount.ToString();
        }
    }
}