using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public LevelDataSO levelData;
    public LeverManager leverManager;
    public float cellSize = 1.2f;
    public Vector2Int gridPos;
    public int Coin = 0;

    public float timePerTile = 0.12f;

    private bool isMoving = false;

    public HashSet<Vector2Int> usedTiles = new HashSet<Vector2Int>();

    void Start()
    {
        UpdateWorldPosition();

        if (leverManager == null)
        {
            leverManager = FindObjectOfType<LeverManager>();
        }
    }


    void Update()
    {
        if (isMoving || levelData == null) return;

        Vector2Int moveDir = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) moveDir = new Vector2Int(0, 1);
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) moveDir = new Vector2Int(0, -1);
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) moveDir = new Vector2Int(-1, 0);
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) moveDir = new Vector2Int(1, 0);

        if (moveDir != Vector2Int.zero)
        {
            StartCoroutine(MoveWithTime(moveDir));
        }
    }


    private IEnumerator MoveWithTime(Vector2Int direction)
    {
        isMoving = true;

        Vector2Int current = gridPos;
        Vector2Int currentDir = direction;

        while (true)
        {
            Vector2Int next = current + currentDir;

            if (!IsWithinBounds(next)) break;
            if (levelData.grid[next.y].tiles[next.x] == LevelDataSO.TileType.Wall) break;
            if (levelData.grid[next.y].tiles[next.x] == LevelDataSO.TileType.NeedCoin &&
                !usedTiles.Contains(next) && Coin <= 0)
            {
                break;
            }

            current = next;
            gridPos = current;
            UpdateWorldPosition();

            currentDir = ProcessTile(current, currentDir);


            yield return new WaitForSeconds(timePerTile);

            if (levelData.grid[current.y].tiles[current.x] == LevelDataSO.TileType.Finish)
            {
                Debug.Log(" YOU WIN!");
                break;
            }
        }

        isMoving = false;
        Debug.Log($"Di chuyển hoàn tất! Vị trí cuối: {gridPos} | Coins: {Coin}");
    }


    public float GetTimePerTile()
    {
        return timePerTile;
    }


    public float GetTimeForTiles(int numberOfTiles)
    {
        if (numberOfTiles <= 0) return 0f;
        return numberOfTiles * timePerTile;
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
                if (!usedTiles.Contains(pos) && Coin > 0)
                {
                    Coin--;
                    usedTiles.Add(pos);
                    Debug.Log($"Used 1 Coin at {pos}. Remaining: {Coin}");
                    ChangeNeedCoinToNone(pos);
                }
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

        LevelDataSO targetData = (leverManager != null && leverManager.isInEditMode && leverManager.currentLevelData != null)
            ? leverManager.currentLevelData : levelData;

        targetData.grid[pos.y].tiles[pos.x] = LevelDataSO.TileType.None;

        if (leverManager != null)
            leverManager.ReplaceTile(pos, LevelDataSO.TileType.None);
    }

    public void ResetState()
    {
        Coin = 0;
        usedTiles.Clear();
        isMoving = false;          // ← THÊM: Reset trạng thái isMoving
    }
}