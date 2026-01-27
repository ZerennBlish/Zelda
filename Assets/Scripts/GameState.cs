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
    
    public void AddRupees(int amount)
    {
        rupees += amount;
        
        if (rupeeUI != null)
        {
            rupeeUI.UpdateRupees(rupees);
        }
    }
}