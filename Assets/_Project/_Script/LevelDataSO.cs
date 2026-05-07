using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelDataSO : ScriptableObject
{
    public Vector2 PlayerPos;

    public enum TileType
    {
        None,
        Wall,
        Finish,
        Coin,
        NeedCoin,
        RedirectUpRight,
        RedirectDownLeft,
        RedirectUpLeft,
        RedirectDownRight,
    }

    [Serializable]
    public class Row
    {
        public List<TileType> tiles = new List<TileType>();
    }

    [ListDrawerSettings(Expanded = true)]
    public List<LevelDataSO> allLevels = new List<LevelDataSO>();

    public LevelDataSO GetNextLevel()
    {
        if (allLevels == null || allLevels.Count == 0) return null;

        int currentIndex = allLevels.IndexOf(this);
        if (currentIndex >= 0 && currentIndex + 1 < allLevels.Count)
            return allLevels[currentIndex + 1];

        return null;
    }

#if UNITY_EDITOR
    [BoxGroup("Editor Only Settings")]
    [LabelText("Width")]
    public int width = 5;

    [BoxGroup("Editor Only Settings")]
    [LabelText("Height")]
    public int height = 5;

    [BoxGroup("Editor Only Settings")]
    [Button("Fill Grid Data")]
    private void FillGrid()
    {
        grid.Clear();
        for (int i = 0; i < height; i++)
        {
            Row row = new Row();
            for (int j = 0; j < width; j++)
            {
                row.tiles.Add(TileType.None);
            }
            grid.Add(row);
        }
        UnityEditor.EditorUtility.SetDirty(this);
    }

    [Button("Random Coin and NeedCoin")]
    private void RandomCoinandNeedCoin()
    {
        RandomTile(TileType.Coin, 1);
        RandomTile(TileType.NeedCoin, 1);
        Debug.Log("random coin and needcoin");
    }

    private void RandomTile(TileType tileType, int amount)
    {
        if (grid == null || grid.Count == 0)
        {
            Debug.Log("No data ");
            return;
        }

        List<Vector2Int> nonePos = new List<Vector2Int>();
        for (int y = 0; y < grid.Count; y++)
        {
            for (int x = 0; x < grid[y].tiles.Count; x++)
            {
                if (grid[y].tiles[x] == TileType.None)
                {
                    nonePos.Add(new Vector2Int(x, y));
                }
            }
        }

        ClearTileType(tileType);

        int placed = 0;
        for (int i = 0; i < amount && nonePos.Count > 0; i++)
        {
            int ranDom = UnityEngine.Random.Range(0, nonePos.Count);
            Vector2Int pos = nonePos[ranDom];
            grid[pos.y].tiles[pos.x] = tileType;
            nonePos.RemoveAt(ranDom);
            placed++;
        }
    }

    private void ClearTileType(TileType tileType)
    {
        for (int y = 0; y < grid.Count; y++)
        {
            for (int x = 0; x < grid[y].tiles.Count; x++)
            {
                if (grid[y].tiles[x] == tileType)
                {
                    grid[y].tiles[x] = TileType.None;
                }
            }
        }
    }
#endif

    [Title("Level Data")]
    [ListDrawerSettings(Expanded = true)]
    public List<Row> grid = new List<Row>();
}