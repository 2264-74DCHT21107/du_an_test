using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
[ExecuteAlways]
public class LeverManager : MonoBehaviour
{
    [Title("References")]
    public GameObject Player;
    public PlayerController PlayerController;
    public GameObject EmptyPreb;
    public GameObject WallPreb;
    public GameObject FinishPreb;
    public GameObject CoinPreb;
    public GameObject NeedCoinPreb;
    public GameObject RedirectUpRightPreb;
    public GameObject RedirectDownLeftPreb;
    public GameObject RedirectUpLeftPrep;
    public GameObject RedirectDownRightPreb;
    public ObjectPool ObjectPool;
    public float spacing = 1.2f;

    [Title("Level Data")]
    public LevelDataSO levelData;

    [ReadOnly]
    [ShowInInspector]
    [Title("Current Level Data (Đang chỉnh sửa)")]
    public LevelDataSO currentLevelData;
    public bool isInEditMode = false;
    private List<LevelDataSO.Row> runtimeGrid = new List<LevelDataSO.Row>();

    [Button(" ENTER LIVE EDIT MODE")]
    private void EnterLiveEditMode()
    {
        if (levelData == null)
        {
            Debug.LogError("Chưa gán LevelData!");
            return;
        }
        CreateCurrentLevelDataClone();
        ClearChildren();
        GenerateLevelFromCurrentData();
        isInEditMode = true;
        Debug.Log(" ĐÃ VÀO LIVE EDIT MODE - Sử dụng currentLevelData");
    }
    [Button(" SAVE RUNTIME CHANGES → CurrentLevelData")]
    private void SaveRuntimeChangesToCurrent()
    {
        if (!isInEditMode || currentLevelData == null)
        {
            Debug.LogError("Phải vào Live Edit Mode trước!");
            return;
        }
        if (levelData == null) return;
        // Đồng bộ toàn bộ grid
        int rows = Mathf.Min(levelData.grid.Count, currentLevelData.grid.Count);
        for (int y = 0; y < rows; y++)
        {
            int cols = Mathf.Min(levelData.grid[y].tiles.Count, currentLevelData.grid[y].tiles.Count);
            for (int x = 0; x < cols; x++)
            {
                currentLevelData.grid[y].tiles[x] = levelData.grid[y].tiles[x];
            }
        }
        // Đồng bộ Player Pos
        Transform playerT = transform.Find("Player");
        if (playerT != null)
        {
            Vector3 p = playerT.localPosition;
            currentLevelData.PlayerPos = new Vector2(
            Mathf.RoundToInt(p.x / spacing),
            Mathf.RoundToInt(p.z / spacing)
            );
        }
        EditorUtility.SetDirty(currentLevelData);
        Debug.Log(" ĐÃ LƯU THÀNH CÔNG vào CurrentLevelData!");
    }
    [Button(" Revert to Original LevelData")]
    private void RevertToOriginal()
    {
        currentLevelData = null;
        ClearChildren();
        GenerateLevel();
        Debug.Log("Đã khôi phục về bản gốc");
    }
    private void CreateCurrentLevelDataClone()
    {
        currentLevelData = ScriptableObject.CreateInstance<LevelDataSO>();
        currentLevelData.name = levelData.name + " (Current Clone)";
        currentLevelData.PlayerPos = levelData.PlayerPos;
        currentLevelData.grid.Clear();
        foreach (var row in levelData.grid)
        {
            LevelDataSO.Row newRow = new LevelDataSO.Row();
            newRow.tiles.AddRange(row.tiles);
            currentLevelData.grid.Add(newRow);
        }
    }
    private void GenerateLevelFromCurrentData()
    {
        if (currentLevelData == null) return;
        for (int i = 0; i < currentLevelData.grid.Count; i++)
            for (int j = 0; j < currentLevelData.grid[i].tiles.Count; j++)
                SpawnTile(currentLevelData.grid[i].tiles[j], j, i);
        SpawnPlayer();
    }
    [Button("Generate Level From SO")]
    public void GenerateLevel()
    {
        ClearChildren();
        InitializeRuntimeGrid();

        if (isInEditMode && currentLevelData != null)
        {
            for (int y = 0; y < currentLevelData.grid.Count; y++)
                for (int x = 0; x < currentLevelData.grid[y].tiles.Count; x++)
                    SpawnTile(currentLevelData.grid[y].tiles[x], x, y);
        }
        else
        {
            for (int y = 0; y < runtimeGrid.Count; y++)
                for (int x = 0; x < runtimeGrid[y].tiles.Count; x++)
                    SpawnTile(runtimeGrid[y].tiles[x], x, y);
        }

        SpawnPlayer();
        Debug.Log("Level Generated from Runtime Data");
    }

    private void SpawnTile(LevelDataSO.TileType tile, int x, int y)
    {
        Vector3 pos = new Vector3(x * spacing, 0, y * spacing);

        GameObject obj;

        if (Application.isPlaying && ObjectPool.Instance != null)
        {
            string tag = tile.ToString();

            // ĐẶC BIỆT XỬ LÝ None → Empty
            if (tile == LevelDataSO.TileType.None)
                tag = "Empty";

            obj = ObjectPool.Instance.Spawn(tag, pos, Quaternion.identity);

            if (obj == null)
            {
                Debug.LogError($"Spawn thất bại: {tag}");
                return;
            }
        }
        else
        {
            // Editor mode
            GameObject prefab = GetPrefabByType(tile);
            obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            obj.transform.localPosition = pos;
        }

        obj.transform.SetParent(transform);
        obj.transform.localPosition = pos;
        obj.name = $"Tile_{y}_{x}";
    }
    private GameObject GetPrefabByType(LevelDataSO.TileType type)
    {
        switch (type)
        {
            case LevelDataSO.TileType.Wall: return WallPreb;
            case LevelDataSO.TileType.Finish: return FinishPreb;
            case LevelDataSO.TileType.Coin: return CoinPreb;
            case LevelDataSO.TileType.NeedCoin: return NeedCoinPreb;
            case LevelDataSO.TileType.RedirectUpRight: return RedirectUpRightPreb;
            case LevelDataSO.TileType.RedirectDownLeft: return RedirectDownLeftPreb;
            case LevelDataSO.TileType.RedirectUpLeft: return RedirectUpLeftPrep;
            case LevelDataSO.TileType.RedirectDownRight: return RedirectDownRightPreb;

            default:                                  // None và các trường hợp khác
                return EmptyPreb;
        }
    }
    private LevelDataSO.TileType GetTileTypeFromObject(GameObject obj)
    {
        if (obj == null) return LevelDataSO.TileType.None;
        GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(obj) as GameObject;
        if (source == WallPreb) return LevelDataSO.TileType.Wall;
        if (source == FinishPreb) return LevelDataSO.TileType.Finish;
        if (source == CoinPreb) return LevelDataSO.TileType.Coin;
        if (source == NeedCoinPreb) return LevelDataSO.TileType.NeedCoin;
        if (source == RedirectUpRightPreb) return LevelDataSO.TileType.RedirectUpRight;
        if (source == RedirectDownLeftPreb) return LevelDataSO.TileType.RedirectDownLeft;
        if (source == RedirectUpLeftPrep) return LevelDataSO.TileType.RedirectUpLeft;
        if (source == RedirectDownRightPreb) return LevelDataSO.TileType.RedirectDownRight;
        return LevelDataSO.TileType.None;
    }
    void SpawnPlayer()
    {
        if (Player == null) return;
        LevelDataSO dataToUse = currentLevelData != null ? currentLevelData : levelData;
        if (dataToUse == null) return;
        GameObject playerObj = (GameObject)PrefabUtility.InstantiatePrefab(Player);
        playerObj.transform.SetParent(transform, false);
        playerObj.transform.localPosition = new Vector3(
            dataToUse.PlayerPos.x * spacing,
            0,
            dataToUse.PlayerPos.y * spacing);
        playerObj.name = "Player";
        PlayerController = playerObj.GetComponent<PlayerController>();
        if (PlayerController != null)
        {
            PlayerController.levelData = dataToUse;
            PlayerController.gridPos = new Vector2Int((int)dataToUse.PlayerPos.x, (int)dataToUse.PlayerPos.y);
        }

    }
    public GameObject GetTileObject(Vector2Int pos)
    {
        foreach (Transform child in transform)
        {
            if (child.name == "Player") continue;
            Vector3 localPos = child.localPosition;
            int x = Mathf.RoundToInt(localPos.x / spacing);
            int y = Mathf.RoundToInt(localPos.z / spacing);
            if (x == pos.x && y == pos.y)
            {
                return child.gameObject;
            }
        }
        return null;
    }
    [Button("Clear Level")]
    public  void ClearChildren()
    {
        if (ObjectPool.Instance != null && Application.isPlaying)
        {
            List<GameObject> tilesToReturn = new List<GameObject>();

            foreach (Transform child in transform)
            {
                if (child.name == "Player") continue;
                tilesToReturn.Add(child.gameObject);
            }

            foreach (var tile in tilesToReturn)
            {
                string tag = GetTileTypeFromObject(tile).ToString();
                ObjectPool.Instance.ReturnToPool(tile, tag);
            }
        }
        else
        {
            while (transform.childCount > 0)
                DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
    public void ReplaceTile(Vector2Int pos, LevelDataSO.TileType newType)
    {
        // 1. Tìm và trả tile cũ về Pool
        GameObject oldTile = null;
        foreach (Transform child in transform)
        {
            if (child.name == "Player") continue;

            int x = Mathf.RoundToInt(child.localPosition.x / spacing);
            int y = Mathf.RoundToInt(child.localPosition.z / spacing);

            if (x == pos.x && y == pos.y)
            {
                oldTile = child.gameObject;
                break;
            }
        }

        if (oldTile != null)
        {
            string oldTag = GetTileTypeFromObject(oldTile).ToString();
            if (ObjectPool.Instance != null)
                ObjectPool.Instance.ReturnToPool(oldTile, oldTag);
            else
                DestroyImmediate(oldTile);
        }

        // 2. LUÔN spawn tile mới, kể cả khi là None (Empty)
        SpawnTile(newType, pos.x, pos.y);
    }
    private void InitializeRuntimeGrid()
    {
        if (levelData == null) return;

        runtimeGrid.Clear();
        foreach (var row in levelData.grid)
        {
            LevelDataSO.Row newRow = new LevelDataSO.Row();
            newRow.tiles.AddRange(row.tiles);
            runtimeGrid.Add(newRow);
        }
    }

    [Button("Reset To Original (Fix Coin/NeedCoin)")]
    public void ResetToOriginalData()
    {
        ClearChildren();
        InitializeRuntimeGrid();

        if (isInEditMode && currentLevelData != null)
        {
            // Edit Mode thì dùng currentLevelData
            for (int y = 0; y < currentLevelData.grid.Count; y++)
            {
                for (int x = 0; x < currentLevelData.grid[y].tiles.Count; x++)
                {
                    SpawnTile(currentLevelData.grid[y].tiles[x], x, y);
                }
            }
        }
        else
        {
            // Play Mode thì dùng runtimeGrid
            for (int y = 0; y < runtimeGrid.Count; y++)
            {
                for (int x = 0; x < runtimeGrid[y].tiles.Count; x++)
                {
                    SpawnTile(runtimeGrid[y].tiles[x], x, y);
                }
            }
        }

        SpawnPlayer();

        // Reset Player
        if (PlayerController != null)
            PlayerController.ResetState();
        else
        {
            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null) pc.ResetState();
        }

        Debug.Log(" RESET LEVEL THÀNH CÔNG - Coin & NeedCoin đã khôi phục!");
    }
}