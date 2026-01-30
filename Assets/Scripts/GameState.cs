using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance;
    
    public int rupees = 0;
    public RupeeUI rupeeUI;
    
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
        if (PlayerPrefs.HasKey("SavedRupees"))
        {
            rupees = PlayerPrefs.GetInt("SavedRupees");
            if (rupeeUI != null)
            {
                rupeeUI.UpdateRupees(rupees);
            }
        }
    }
    
    public void AddRupees(int amount)
    {
        rupees += amount;
        
        if (rupeeUI != null)
        {
            rupeeUI.UpdateRupees(rupees);
        }
    }
    
    public int StealRupees(int amount)
    {
        int stolen = Mathf.Min(amount, rupees);
        rupees -= stolen;
        
        if (rupeeUI != null)
        {
            rupeeUI.UpdateRupees(rupees);
        }
        
        return stolen;
    }
}