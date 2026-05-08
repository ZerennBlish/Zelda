using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldMap", menuName = "Zerenn/WorldMapData")]
public class WorldMapData : ScriptableObject
{
    [System.Serializable]
    public struct RoomEntry
    {
        public Vector2Int coord;
        public bool isSpecial;
        public string note;
    }

    [SerializeField] private List<RoomEntry> rooms = new List<RoomEntry>();

    private HashSet<Vector2Int> _set;
    private HashSet<Vector2Int> _specialSet;

    void OnEnable() { Rebuild(); }
    void OnValidate() { Rebuild(); }

    void Rebuild()
    {
        _set = new HashSet<Vector2Int>();
        _specialSet = new HashSet<Vector2Int>();
        foreach (var r in rooms)
        {
            if (!_set.Add(r.coord))
            {
                Debug.LogWarning($"[WorldMapData] '{name}' contains duplicate coord {r.coord} — check authoring.");
            }
            if (r.isSpecial) _specialSet.Add(r.coord);
        }
    }

    public bool Contains(Vector2Int coord)
    {
        if (_set == null) Rebuild();
        return _set.Contains(coord);
    }

    public bool IsSpecial(Vector2Int coord)
    {
        if (_specialSet == null) Rebuild();
        return _specialSet.Contains(coord);
    }
}
