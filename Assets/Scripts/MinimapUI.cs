using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinimapUI : MonoBehaviour
{
    public static MinimapUI Instance;

    [Header("Layout")]
    public RectTransform mapContainer;
    public float cellSize = 10f;
    public float cellSpacing = 2f;

    [Header("Colors")]
    public Color visitedColor = new Color(0.3f, 0.5f, 0.8f, 0.8f);
    public Color currentColor = new Color(1f, 1f, 0.3f, 1f);
    public Color borderColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);

    [Header("Toggle")]
    public KeyCode toggleKey = KeyCode.Tab;

    [Header("Background")]
    public Image backgroundImage;

    private Dictionary<Vector2Int, Image> roomCells = new Dictionary<Vector2Int, Image>();
    private Vector2Int currentRoom;
    private bool isVisible = true;

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
        RefreshMap();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isVisible = !isVisible;
            mapContainer.gameObject.SetActive(isVisible);
        }
    }

    public void RefreshMap()
    {
        if (RoomTracker.Instance == null) return;

        // Clear old cells
        foreach (var kvp in roomCells)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value.gameObject);
            }
        }
        roomCells.Clear();

        HashSet<Vector2Int> visited = RoomTracker.Instance.GetAllVisited();
        if (visited.Count == 0) return;

        // Find bounds of visited rooms
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        foreach (Vector2Int room in visited)
        {
            if (room.x < minX) minX = room.x;
            if (room.x > maxX) maxX = room.x;
            if (room.y < minY) minY = room.y;
            if (room.y > maxY) maxY = room.y;
        }

        // Get current room for highlighting
        if (RoomManager.Instance != null)
        {
            Vector2 current = RoomManager.Instance.GetCurrentRoom();
            currentRoom = new Vector2Int(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y));
        }

        // Size the background to fit
        float totalWidth = (maxX - minX + 1) * (cellSize + cellSpacing) + cellSpacing;
        float totalHeight = (maxY - minY + 1) * (cellSize + cellSpacing) + cellSpacing;

        if (backgroundImage != null)
        {
            backgroundImage.rectTransform.sizeDelta = new Vector2(totalWidth + 8f, totalHeight + 8f);
        }

        // Create a cell for each visited room
        foreach (Vector2Int room in visited)
        {
            CreateCell(room, minX, minY);
        }

        HighlightCurrentRoom();
    }

    void CreateCell(Vector2Int room, int originX, int originY)
    {
        GameObject cellObj = new GameObject("Room_" + room.x + "_" + room.y);
        cellObj.transform.SetParent(mapContainer, false);

        Image cellImage = cellObj.AddComponent<Image>();
        cellImage.color = visitedColor;

        RectTransform rect = cellObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(cellSize, cellSize);

        // Position relative to bottom-left of the grid
        float posX = (room.x - originX) * (cellSize + cellSpacing) + cellSpacing;
        float posY = (room.y - originY) * (cellSize + cellSpacing) + cellSpacing;

        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.pivot = new Vector2(0, 0);
        rect.anchoredPosition = new Vector2(posX, posY);

        roomCells[room] = cellImage;
    }

    void HighlightCurrentRoom()
    {
        // Reset all to visited color
        foreach (var kvp in roomCells)
        {
            kvp.Value.color = visitedColor;
        }

        // Highlight current
        if (roomCells.ContainsKey(currentRoom))
        {
            roomCells[currentRoom].color = currentColor;
        }
    }

    public void OnRoomChanged()
    {
        if (!isVisible) return;
        if (RoomManager.Instance == null) return;

        Vector2 current = RoomManager.Instance.GetCurrentRoom();
        Vector2Int newRoom = new Vector2Int(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y));

        // If this is a brand new room, rebuild the whole grid
        // (bounds may have changed)
        if (!roomCells.ContainsKey(newRoom))
        {
            RefreshMap();
        }
        else
        {
            // Just move the highlight
            currentRoom = newRoom;
            HighlightCurrentRoom();
        }
    }
}
