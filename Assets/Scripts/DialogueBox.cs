using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueBox : MonoBehaviour
{
    public static DialogueBox Instance;
    public static bool IsActive = false;

    [Header("References")]
    public TextMeshProUGUI dialogueText;
    public Image bubbleBackground;

    [Header("Settings")]
    public float charDelay = 0.05f;

    private string[] lines;
    private int currentLine;
    private bool isTyping;
    private Coroutine typewriterCoroutine;

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

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                // Instantly complete current line
                StopCoroutine(typewriterCoroutine);
                dialogueText.text = lines[currentLine];
                isTyping = false;
            }
            else
            {
                // Advance to next line or close
                currentLine++;
                if (currentLine < lines.Length)
                {
                    typewriterCoroutine = StartCoroutine(TypeLine(lines[currentLine]));
                }
                else
                {
                    Close();
                }
            }
        }
    }

    public void Show(string[] newLines)
    {
        if (newLines == null || newLines.Length == 0) return;

        lines = newLines;
        currentLine = 0;
        IsActive = true;
        Time.timeScale = 0f;

        gameObject.SetActive(true);
        typewriterCoroutine = StartCoroutine(TypeLine(lines[0]));
    }

    void Close()
    {
        IsActive = false;
        Time.timeScale = 1f;
        dialogueText.text = "";
        gameObject.SetActive(false);
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        for (int i = 0; i < line.Length; i++)
        {
            dialogueText.text += line[i];
            yield return new WaitForSecondsRealtime(charDelay);
        }

        isTyping = false;
    }
}
