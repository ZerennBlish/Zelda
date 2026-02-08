using UnityEngine;
using TMPro;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;
    
    [Header("UI References")]
    public GameObject dialogPanel;
    public TextMeshProUGUI dialogText;
    public GameObject continueIndicator;
    
    [Header("Typewriter Effect")]
    public float charsPerSecond = 30f;
    
    private string[] currentLines;
    private int currentLineIndex;
    private NPC currentNPC;
    private bool isActive = false;
    
    // Typewriter state
    private string fullCurrentLine;
    private float charTimer;
    private int charsRevealed;
    private bool lineComplete = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
        
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }
    }

    void Update()
    {
        if (!isActive) return;
        
        // Typewriter effect
        if (!lineComplete)
        {
            charTimer += Time.deltaTime;
            int targetChars = Mathf.FloorToInt(charTimer * charsPerSecond);
            
            if (targetChars >= fullCurrentLine.Length)
            {
                // Show full line
                charsRevealed = fullCurrentLine.Length;
                dialogText.text = fullCurrentLine;
                lineComplete = true;
                
                if (continueIndicator != null)
                {
                    continueIndicator.SetActive(true);
                }
            }
            else if (targetChars > charsRevealed)
            {
                charsRevealed = targetChars;
                dialogText.text = fullCurrentLine.Substring(0, charsRevealed);
            }
        }
        
        // E to advance
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!lineComplete)
            {
                // Skip typewriter — show full line immediately
                charsRevealed = fullCurrentLine.Length;
                dialogText.text = fullCurrentLine;
                lineComplete = true;
                
                if (continueIndicator != null)
                {
                    continueIndicator.SetActive(true);
                }
            }
            else
            {
                // Advance to next line or close
                currentLineIndex++;
                
                if (currentLineIndex >= currentLines.Length)
                {
                    EndDialog();
                }
                else
                {
                    ShowLine(currentLines[currentLineIndex]);
                }
            }
        }
    }
    
    public void StartDialog(string[] lines, NPC speaker)
    {
        if (isActive) return;
        
        currentLines = lines;
        currentNPC = speaker;
        currentLineIndex = 0;
        isActive = true;
        
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(true);
        }
        
        // Freeze player movement
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.enabled = false;
        }
        
        ShowLine(currentLines[0]);
    }
    
    void ShowLine(string line)
    {
        fullCurrentLine = line;
        charTimer = 0f;
        charsRevealed = 0;
        lineComplete = false;
        dialogText.text = "";
        
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }
    }
    
    void EndDialog()
    {
        isActive = false;
        
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
        
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }
        
        // Unfreeze player
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.enabled = true;
        }
        
        // Notify the NPC
        if (currentNPC != null)
        {
            currentNPC.DialogFinished();
        }
        
        currentNPC = null;
        currentLines = null;
    }
    
    public bool IsDialogActive()
    {
        return isActive;
    }
}