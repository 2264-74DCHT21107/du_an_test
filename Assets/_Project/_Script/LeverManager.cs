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
    public float spacing = 1.2f;
    [Title("Level Data")]
    public LevelDataSO levelData;
    [ReadOnly]
    [ShowInInspector]
    [Title("Current Level Data (Đang chỉnh sửa)")]
    public LevelDataSO currentLevelData;
    public bool isInEditMode = false;
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
    private void GenerateLevel()
    {
        ClearChildren();
        LevelDataSO dataToUse = (isInEditMode && currentLevelData != null) ? currentLevelData : levelData;
        if (dataToUse == null)
        {
            Debug.LogError("Không có dữ liệu level!");
            return;
        }
        Debug.Log($"Đang generate từ: {(isInEditMode && currentLevelData != null ? "CURRENT LEVEL DATA" : "LevelData gốc")}");
        for (int y = 0; y < dataToUse.grid.Count; y++)
        {
            for (int x = 0; x < dataToUse.grid[y].tiles.Count; x++)
            {
                SpawnTile(dataToUse.grid[y].tiles[x], x, y);
            }
        }
        SpawnPlayer();
    }
    private void SpawnTile(LevelDataSO.TileType tile, int x, int y)
    {
        Vector3 pos = new Vector3(x * spacing, 0, y * spacing);

        GameObject obj;

        if (Application.isPlaying && ObjectPool.Instance != null)
        {
            obj = ObjectPool.Instance.Spawn(tile.ToString(), pos, Quaternion.identity);
            obj.transform.SetParent(transform);
        }
        else
        {
            GameObject prefab = GetPrefabByType(tile);
            obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            obj.transform.SetParent(transform);
            obj.transform.localPosition = pos;
        }

        obj.name = $"Tile_{y}_{x}";
    }
    private GameObject GetPrefabByType(LevelDataSO.TileType type)
    {
        switch (type)
        {
            case LevelDataSO.TileType.Wall:
                return WallPreb;
            case LevelDataSO.TileType.Finish:
                return FinishPreb;
            case LevelDataSO.TileType.Coin:
                return CoinPreb;
            case LevelDataSO.TileType.NeedCoin:
                return NeedCoinPreb;
            case LevelDataSO.TileType.RedirectUpRight:
                return RedirectUpRightPreb;
            case LevelDataSO.TileType.RedirectDownLeft:
                return RedirectDownLeftPreb;
            case LevelDataSO.TileType.RedirectUpLeft:
                return RedirectUpLeftPrep;
            case LevelDataSO.TileType.RedirectDownRight:
                return RedirectDownRightPreb;
            default:
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
        playerObj.transform.SetParent(transform);
        playerObj.transform.localPosition = new Vector3(dataToUse.PlayerPos.x * spacing, 0, dataToUse.PlayerPos.y * spacing);
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
    private void ClearChildren()
    {
        while (transform.childCount > 0)
        {
            GameObject obj = transform.GetChild(0).gameObject;

            if (ObjectPool.Instance != null)
                ObjectPool.Instance.ReturnToPool(obj, GetTileTypeFromObject(obj).ToString());
            else
                DestroyImmediate(obj);
        }
    }
    public void ReplaceTile(Vector2Int pos, LevelDataSO.TileType newType)
    {
        foreach (Transform child in transform)
        {
            if (child.name == "Player") continue;

            int x = Mathf.RoundToInt(child.localPosition.x / spacing);
            int y = Mathf.RoundToInt(child.localPosition.z / spacing);

            if (x == pos.x && y == pos.y)
            {
                if (ObjectPool.Instance != null)
                    ObjectPool.Instance.ReturnToPool(child.gameObject, GetTileTypeFromObject(child.gameObject).ToString());
                else
                    DestroyImmediate(child.gameObject);

                SpawnTile(newType, x, y);
                return;
            }
        }
    }
}