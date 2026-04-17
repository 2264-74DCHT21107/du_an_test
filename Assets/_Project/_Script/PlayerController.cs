using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float cellSize = 1.2f;
    public Vector2Int gridPos;
    public int Coin = 0;
    public LevelDataSO levelData;
    public LeverManager leverManager;           
    public HashSet<Vector2Int> usedTiles = new HashSet<Vector2Int>();

    void Start()
    {
        UpdateWorldPosition();

       
        if (leverManager == null)
        {
            leverManager = FindObjectOfType<LeverManager>();
            if (leverManager == null)
                Debug.LogError(" Không tìm thấy LeverManager trong scene!");
            else
                Debug.Log(" Đã tìm thấy LeverManager");
        }
    }

    void Update()
    {
        if (levelData == null) return;

        Vector2Int move = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) move = new Vector2Int(0, 1);
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) move = new Vector2Int(0, -1);
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) move = new Vector2Int(-1, 0);
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) move = new Vector2Int(1, 0);

        if (move != Vector2Int.zero)
        {
            Vector2Int targetPos = GetFarthestPointAndCollect(gridPos, move);
            if (targetPos != gridPos)
            {
                gridPos = targetPos;
                UpdateWorldPosition();
                Debug.Log($"Moved to: {gridPos} | Coins: {Coin}");
            }
            else
            {
                Debug.Log("Blocked by wall or need coin");
            }
        }
    }

    Vector2Int GetFarthestPointAndCollect(Vector2Int startPos, Vector2Int direction)
    {
        Vector2Int current = startPos;
        while (true)
        {
            Vector2Int next = current + direction;
            if (!IsWithinBounds(next)) break;

            LevelDataSO.TileType tile = levelData.grid[next.y].tiles[next.x];

            if (tile == LevelDataSO.TileType.Wall) break;

            current = next;
            direction = ProcessTile(current, direction);

            if (tile == LevelDataSO.TileType.Finish) break;

            if (tile == LevelDataSO.TileType.NeedCoin && !usedTiles.Contains(next) && Coin <= 0)
                break;
        }
        return current;
    }

    Vector2Int ProcessTile(Vector2Int pos, Vector2Int currentDirection)
    {
        if (!IsWithinBounds(pos)) return currentDirection;

        LevelDataSO.TileType tileType = levelData.grid[pos.y].tiles[pos.x];

        switch (tileType)
        {
            case LevelDataSO.TileType.Coin:
                if (!usedTiles.Contains(pos))
                {
                    Coin++;
                    usedTiles.Add(pos);
                    Debug.Log($"Collected Coin at {pos}! Total: {Coin}");
                }
                break;

            case LevelDataSO.TileType.NeedCoin:
                if (!usedTiles.Contains(pos))
                {
                    if (Coin > 0)
                    {
                        Coin--;
                        usedTiles.Add(pos);
                        Debug.Log($"Used 1 Coin at {pos}. Remaining: {Coin}");
                        ChangeNeedCoinToNone(pos);        
                    }
                    else
                    {
                        Debug.Log("Need a coin to pass!");
                    }
                }
                break;

            case LevelDataSO.TileType.Finish:
                Debug.Log(" YOU WIN!");
                break;

            case LevelDataSO.TileType.RedirectUpRight:
                return currentDirection.x != 0 ? new Vector2Int(0, 1) : new Vector2Int(1, 0);
            case LevelDataSO.TileType.RedirectDownLeft:
                return currentDirection.x != 0 ? new Vector2Int(0, -1) : new Vector2Int(-1, 0);
            case LevelDataSO.TileType.RedirectUpLeft:
                return currentDirection.x != 0 ? new Vector2Int(0, 1) : new Vector2Int(-1, 0);
            case LevelDataSO.TileType.RedirectDownRight:
                return currentDirection.x != 0 ? new Vector2Int(0, -1) : new Vector2Int(1, 0);
        }
        return currentDirection;
    }

    bool IsWithinBounds(Vector2Int pos)
    {
        if (levelData == null || levelData.grid == null || levelData.grid.Count == 0)
            return false;

        return pos.x >= 0 && pos.x < levelData.grid[0].tiles.Count &&
               pos.y >= 0 && pos.y < levelData.grid.Count;
    }

    void UpdateWorldPosition()
    {
        transform.position = new Vector3(gridPos.x * cellSize, 0, gridPos.y * cellSize);
    }

    void ChangeNeedCoinToNone(Vector2Int pos)
    {
        if (levelData == null || !IsWithinBounds(pos)) return;

        levelData.grid[pos.y].tiles[pos.x] = LevelDataSO.TileType.None;

        if (leverManager != null)
        {
            leverManager.ReplaceTile(pos, LevelDataSO.TileType.None);
            if (leverManager.currentLevelData != null)
            {
                leverManager.currentLevelData.grid[pos.y].tiles[pos.x] = LevelDataSO.TileType.None;
            }
            Debug.LogWarning("leverManager is null - Cannot update visual");
        }
        Debug.Log($"NeedCoin tại {pos} → None (đã đồng bộ data)");
    }

    public void ResetState()
    {
        Coin = 0;
        usedTiles.Clear();
    }
}