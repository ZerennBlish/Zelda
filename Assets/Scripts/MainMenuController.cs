using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Button continueButton;
    
    void Start()
    {
        if (continueButton != null)
        {
            continueButton.interactable = PlayerPrefs.GetInt("HasSave", 0) == 1;
        }
    }
    
    public void NewGame()
    {
        PlayerPrefs.DeleteKey("RoomX");
        PlayerPrefs.DeleteKey("RoomY");
        PlayerPrefs.DeleteKey("Lives");
        PlayerPrefs.DeleteKey("HasSave");
        PlayerPrefs.DeleteKey("SavedRupees");
        PlayerPrefs.DeleteKey("SavedArrows");
        PlayerPrefs.DeleteKey("SavedBombs");
        PlayerPrefs.DeleteKey("SavedMaxHealth");
        PlayerPrefs.DeleteKey("SavedClassTier");
        PlayerPrefs.DeleteKey("HasBoomerang");
        PlayerPrefs.DeleteKey("HasBombs");
        PlayerPrefs.DeleteKey("HasGrapple");
        PlayerPrefs.DeleteKey("HasWand");
        PlayerPrefs.DeleteKey("HasBook");
        PlayerPrefs.DeleteKey("EquippedWeaponIndex");
        PlayerPrefs.Save();
        
        SceneManager.LoadScene("Game");
    }
    
    public void ContinueGame()
    {
        SceneManager.LoadScene("Game");
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}