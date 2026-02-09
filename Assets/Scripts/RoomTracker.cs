using UnityEngine;
using System.Collections.Generic;

public class RoomTracker : MonoBehaviour
{
    public static RoomTracker Instance;

    private HashSet<Vector2Int> visitedRooms = new HashSet<Vector2Int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        LoadVisitedRooms();

        // Mark starting room as visited
        if (RoomManager.Instance != null)
        {
            Vector2 current = RoomManager.Instance.GetCurrentRoom();
            MarkVisited(current);
        }
    }

    public void MarkVisited(Vector2 room)
    {
        Vector2Int key = new Vector2Int(Mathf.RoundToInt(room.x), Mathf.RoundToInt(room.y));

        if (visitedRooms.Add(key))
        {
            Debug.Log("New room discovered: " + key);
            SaveVisitedRooms();
        }
    }

    public bool HasVisited(Vector2Int room)
    {
        return visitedRooms.Contains(room);
    }

    public HashSet<Vector2Int> GetAllVisited()
    {
        return visitedRooms;
    }

    void SaveVisitedRooms()
    {
        // Store as comma-separated "x:y" pairs
        List<string> entries = new List<string>();
        foreach (Vector2Int room in visitedRooms)
        {
            entries.Add(room.x + ":" + room.y);
        }

        string data = string.Join(",", entries);
        PlayerPrefs.SetString("VisitedRooms", data);
        PlayerPrefs.Save();
    }

    void LoadVisitedRooms()
    {
        visitedRooms.Clear();

        if (!PlayerPrefs.HasKey("VisitedRooms")) return;

        string data = PlayerPrefs.GetString("VisitedRooms");
        if (string.IsNullOrEmpty(data)) return;

        string[] entries = data.Split(',');
        foreach (string entry in entries)
        {
            string[] parts = entry.Split(':');
            if (parts.Length == 2)
            {
                int x, y;
                if (int.TryParse(parts[0], out x) && int.TryParse(parts[1], out y))
                {
                    visitedRooms.Add(new Vector2Int(x, y));
                }
            }
        }

        Debug.Log("Loaded " + visitedRooms.Count + " visited rooms");
    }

    public void ClearVisitedRooms()
    {
        visitedRooms.Clear();
        PlayerPrefs.DeleteKey("VisitedRooms");
        PlayerPrefs.Save();
    }
}
