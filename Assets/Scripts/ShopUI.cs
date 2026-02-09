using UnityEngine;
using TMPro;
using System.Collections;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance;
    public static bool IsActive = false;

    [Header("References")]
    public TextMeshProUGUI shopText;

    [Header("Item 1 — Arrows")]
    public int arrowQuantity = 10;
    public int arrowPrice = 20;

    [Header("Item 2 — Bombs")]
    public int bombQuantity = 5;
    public int bombPrice = 30;

    [Header("Item 3 — Heart Upgrade")]
    public int heartPrice = 100;

    private bool heartSoldOut = false;
    private Coroutine messageCoroutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!IsActive) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            BuyArrows();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            BuyBombs();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            BuyHeart();
        }
        else if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    public void Show()
    {
        IsActive = true;
        Time.timeScale = 0f;
        gameObject.SetActive(true);
        RefreshText();
    }

    void Close()
    {
        IsActive = false;
        Time.timeScale = 1f;

        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
            messageCoroutine = null;
        }

        gameObject.SetActive(false);
    }

    void BuyArrows()
    {
        if (GameState.Instance.rupees < arrowPrice)
        {
            ShowMessage("Not enough rupees!");
            return;
        }

        GameState.Instance.StealRupees(arrowPrice);

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.AddArrows(arrowQuantity);
        }

        RefreshText();
    }

    void BuyBombs()
    {
        if (GameState.Instance.rupees < bombPrice)
        {
            ShowMessage("Not enough rupees!");
            return;
        }

        GameState.Instance.StealRupees(bombPrice);

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.AddBombs(bombQuantity);
        }

        RefreshText();
    }

    void BuyHeart()
    {
        if (heartSoldOut)
        {
            ShowMessage("Already sold out!");
            return;
        }

        if (GameState.Instance.rupees < heartPrice)
        {
            ShowMessage("Not enough rupees!");
            return;
        }

        GameState.Instance.StealRupees(heartPrice);

        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health != null)
        {
            health.IncreaseMaxHealth(1);
        }

        heartSoldOut = true;
        RefreshText();
    }

    void RefreshText()
    {
        string item1 = "1. Arrows x" + arrowQuantity + " — " + arrowPrice + " rupees";
        string item2 = "2. Bombs x" + bombQuantity + " — " + bombPrice + " rupees";
        string item3 = heartSoldOut
            ? "3. Heart Upgrade — SOLD OUT"
            : "3. Heart Upgrade — " + heartPrice + " rupees";

        string rupeeCount = "Rupees: " + GameState.Instance.rupees;

        shopText.text = item1 + "\n" + item2 + "\n" + item3 + "\n\n" + rupeeCount;
    }

    void ShowMessage(string message)
    {
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }
        messageCoroutine = StartCoroutine(FlashMessage(message));
    }

    IEnumerator FlashMessage(string message)
    {
        shopText.text = message;
        yield return new WaitForSecondsRealtime(1f);
        RefreshText();
        messageCoroutine = null;
    }
}
