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
    public LevelDataSO levelData;                    // Bản gốc cố định

    [ReadOnly]
    [ShowInInspector]
    [Title("Current Level Data (Đang chỉnh sửa)")]
    public LevelDataSO currentLevelData;             // Bản tạm thời

    private bool isInEditMode = false;

    // ==================== BUTTONS ====================
    [Button(" ENTER LIVE EDIT MODE", ButtonSizes.Large)]

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

    [Button(" SAVE SCENE → CurrentLevelData", ButtonSizes.Large)]

    private void SaveToCurrentLevelData()
    {
        if (currentLevelData == null || !isInEditMode)
        {
            Debug.LogError("Phải vào Live Edit Mode trước khi Save!");
            return;
        }

        // Code lưu dữ liệu từ Scene vào currentLevelData
        currentLevelData.grid.Clear();
        Dictionary<Vector2Int, LevelDataSO.TileType> tileMap = new Dictionary<Vector2Int, LevelDataSO.TileType>();
        int maxX = 0, maxY = 0;

        foreach (Transform child in transform)
        {
            if (child.name == "Player") continue;

            Vector3 pos = child.localPosition;
            int x = Mathf.RoundToInt(pos.x / spacing);
            int y = Mathf.RoundToInt(pos.z / spacing);

            LevelDataSO.TileType type = GetTileTypeFromObject(child.gameObject);
            tileMap[new Vector2Int(x, y)] = type;

            maxX = Mathf.Max(maxX, x);
            maxY = Mathf.Max(maxY, y);
        }

        for (int y = 0; y <= maxY; y++)
        {
            LevelDataSO.Row row = new LevelDataSO.Row();
            for (int x = 0; x <= maxX; x++)
            {
                Vector2Int p = new Vector2Int(x, y);
                row.tiles.Add(tileMap.ContainsKey(p) ? tileMap[p] : LevelDataSO.TileType.None);
            }
            currentLevelData.grid.Add(row);
        }

        // Lưu Player Position
        Transform playerT = transform.Find("Player");
        if (playerT != null)
        {
            Vector3 pPos = playerT.localPosition;
            currentLevelData.PlayerPos = new Vector2(
                Mathf.RoundToInt(pPos.x / spacing),
                Mathf.RoundToInt(pPos.z / spacing)
            );
        }

        EditorUtility.SetDirty(currentLevelData);
        Debug.Log($" SAVE THÀNH CÔNG! Level size: {maxX + 1} x {maxY + 1}");
    }

    [Button("↩️ Revert to Original LevelData")]
    private void RevertToOriginal()
    {
        currentLevelData = null;
        ClearChildren();
        GenerateLevel();
        Debug.Log("Đã khôi phục về bản gốc");
    }

    // ==================== TẠO CLONE ====================
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
        if (levelData == null) return;
        for (int i = 0; i < levelData.grid.Count; i++)
            for (int j = 0; j < levelData.grid[i].tiles.Count; j++)
                SpawnTile(levelData.grid[i].tiles[j], j, i);

        SpawnPlayer();
    }

    private void SpawnTile(LevelDataSO.TileType tile, int x, int y)
    {
        GameObject prefab = GetPrefabByType(tile);
        if (prefab == null) return;

        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        obj.transform.SetParent(transform);
        obj.transform.localPosition = new Vector3(x * spacing, 0, y * spacing);
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
            default: return EmptyPreb;
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

    [Button("Clear Level")]
    private void ClearChildren()
    {
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);
    }

}