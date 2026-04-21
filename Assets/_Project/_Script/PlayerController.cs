using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
 
    public LevelDataSO levelData;
    public LeverManager leverManager;

    public float cellSize = 1.2f;
    public float timePerTile = 0.25f;

    public Vector2Int gridPos;
    public int Coin = 0;
    public Vector2Int moveDirection = Vector2Int.zero;
    

    private bool isMoving = false;
    private float timer = 0f;

    void Start()
    {
        UpdateWorldPosition();

        if (leverManager == null)
            leverManager = FindObjectOfType<LeverManager>();
    }

    void Update()
    {
       
        if (isMoving || levelData == null)
        {
        
            if (isMoving)
            {
                timer += Time.deltaTime;
                if (timer >= timePerTile)
                {
                    MoveOneStep();
                }
            }
            return;
        }

        Vector2Int inputDir = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) inputDir = Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) inputDir = Vector2Int.down;
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) inputDir = Vector2Int.left;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) inputDir = Vector2Int.right;

        if (inputDir != Vector2Int.zero)
        {
            StartMoving(inputDir);
        }
    }

    void StartMoving(Vector2Int dir)
    {
        moveDirection = dir;
        isMoving = true;
        timer = 0f;
        MoveOneStep();      
    }

    void MoveOneStep()
    {
        Vector2Int nextPos = gridPos + moveDirection;

    
        if (!IsWithinBounds(nextPos) ||
            levelData.grid[nextPos.y].tiles[nextPos.x] == LevelDataSO.TileType.Wall )
        {
            StopMoving();
            return;
        }

      
        gridPos = nextPos;
        UpdateWorldPosition();

        moveDirection = ProcessTile(gridPos, moveDirection);

        if (levelData.grid[gridPos.y].tiles[gridPos.x] == LevelDataSO.TileType.Finish)
        {
            Debug.Log("YOU WIN!");
            StopMoving();
            return;
        }

      
        timer = 0f;
    }

    Vector2Int ProcessTile(Vector2Int pos, Vector2Int currentDirection)
    {
        if (!IsWithinBounds(pos)) return currentDirection;

        LevelDataSO.TileType tileType = levelData.grid[pos.y].tiles[pos.x];

        switch (tileType)
        {
            case LevelDataSO.TileType.Coin:
                Coin++;
                Debug.Log($"Coin: {Coin}");
                if (leverManager == null || !leverManager.isInEditMode)
                {
                    ChangeTilesToNone(pos);
                }
                
                break;

            case LevelDataSO.TileType.NeedCoin:
                if (Coin > 0)
                {
                    Coin--;
                    Debug.Log($"Used 1 coin . Remaining {Coin}");
                    ChangeTilesToNone(pos);
                } 
                else
                {
                    Debug.Log($"không đủ coin dừng lại tại {pos}");
                    StopMoving();
                }
                    break;

            case LevelDataSO.TileType.RedirectUpRight:
                return currentDirection.x != 0 ? Vector2Int.up : Vector2Int.right;
            case LevelDataSO.TileType.RedirectDownLeft:
                return currentDirection.x != 0 ? Vector2Int.down : Vector2Int.left;
            case LevelDataSO.TileType.RedirectUpLeft:
                return currentDirection.x != 0 ? Vector2Int.up : Vector2Int.left;
            case LevelDataSO.TileType.RedirectDownRight:
                return currentDirection.x != 0 ? Vector2Int.down : Vector2Int.right;
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
        transform.position = new Vector3(gridPos.x * cellSize, 0f, gridPos.y * cellSize);
    }
    //void ChangeCoinToNone(Vector2Int pos)
    //{
    //    if (levelData == null || !IsWithinBounds(pos)) return;
    //    LevelDataSO targetData = (leverManager != null && leverManager.isInEditMode && leverManager.currentLevelData != null)
    //        ? leverManager.currentLevelData : levelData;
    //    targetData.grid[pos.y].tiles[pos.x] = LevelDataSO.TileType.None;

    //    if (leverManager != null)
    //        leverManager.ReplaceTile(pos, LevelDataSO.TileType.None);
    //}
    void ChangeTilesToNone(Vector2Int pos)

    {
        if (levelData == null || !IsWithinBounds(pos)) return;

        LevelDataSO targetData = (leverManager != null && leverManager.isInEditMode && leverManager.currentLevelData != null)
            ? leverManager.currentLevelData : levelData;

        targetData.grid[pos.y].tiles[pos.x] = LevelDataSO.TileType.None;

        if (leverManager != null)
            leverManager.ReplaceTile(pos, LevelDataSO.TileType.None);
    }

    void StopMoving()
    {
        isMoving = false;
        timer = 0f;
        Debug.Log($"Dừng di chuyển tại {gridPos}");
    }

    public void ResetState()
    {
        Coin = 0;

        isMoving = false;
        timer = 0f;
        moveDirection = Vector2Int.zero;
        Debug.Log("Player state reset.");
    }
}