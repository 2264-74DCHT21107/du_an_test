using UnityEngine;
using static UnityEditor.PlayerSettings;

public class PlayerController : MonoBehaviour
{
    public float cellSize = 1f;
    public Vector2Int gridPos;
    public int Coin = 0;
    public LevelDataSO levelData;
   
    void Start()
    {
        UpdateWorldPosition();
    }

    void Update()
    {
        Vector2Int move = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W)) move = new Vector2Int(0, 1);
        if (Input.GetKeyDown(KeyCode.S)) move = new Vector2Int(0, -1);
        if (Input.GetKeyDown(KeyCode.A)) move = new Vector2Int(-1, 0);
        if (Input.GetKeyDown(KeyCode.D)) move = new Vector2Int(1, 0);

        if (move != Vector2Int.zero)
        {

            Vector2Int targetPos = GetFarthestPoint(gridPos, move);

            if (targetPos != gridPos)
            {
                gridPos = targetPos; 
                UpdateWorldPosition();
                //checkCollectCoin(gridPos);
                Debug.Log("Move to: " + gridPos);
                if(IsAtFinish())
                {
                    Debug.Log("Reached the FINISH!");
                }
            }
            else
            {
                Debug.Log(" Blocked by WALL");
            }
        }
    }
    Vector2Int GetFarthestPoint(Vector2Int startPos, Vector2Int direction)
    {
        Vector2Int currentPos = startPos;

        while (true)
        {
            Vector2Int next  = currentPos + direction;

            if (!IsWithinBounds(next ))
                break;  
            
            var tile = levelData.grid[next.y].tiles[next .x];
            if (tile == LevelDataSO.TileType.Coin)
            {
                Coin++;
                levelData.grid[next.y].tiles[next.x] = LevelDataSO.TileType.None;
                Debug.Log("Collected a coin! Score: " + Coin);

            }
            if (tile == LevelDataSO.TileType.NeedCoin)
            {
                if (Coin > 0)
                {
                    Coin--;
                    Debug.Log("Used a coin to pass! Remaining coins: " + Coin);
                    levelData.grid[next.y].tiles[next.x] = LevelDataSO.TileType.None;
                }
                else
                {
                    Debug.Log("Need a coin to pass!");
                    break;
                }
            }
            if ( tile == LevelDataSO.TileType.Wall)
                break;
            currentPos = next;
            if (tile == LevelDataSO.TileType.Finish)          
                break;
            
        }
        return currentPos;
    }

    bool CanMove(Vector2Int pos)
    {
        if (!IsWithinBounds(pos))
            return false;

        if (levelData.grid[pos.y].tiles[pos.x] == LevelDataSO.TileType.Wall)
            return false;
        if (levelData.grid[pos.y].tiles[pos.x] == LevelDataSO.TileType.NeedCoin && Coin <= 0)
            return false;
        if (levelData.grid[pos.y].tiles[pos.x] == LevelDataSO.TileType.NeedCoin && Coin > 0)
        {
            Coin--;
            levelData.grid[pos.y].tiles[pos.x] = LevelDataSO.TileType.None;
            Debug.Log("Used a coin to pass! Remaining coins: " + Coin);
        }
        if (levelData.grid[pos.y].tiles[pos.x] == LevelDataSO.TileType.Coin)
        {
            Coin++;
            levelData.grid[pos.y].tiles[pos.x] = LevelDataSO.TileType.None;
            Debug.Log("Collected a coin! Score: " + Coin);
        }
        return true;
    }
    bool IsAtFinish()
    {
        if (levelData.grid[gridPos.y].tiles[gridPos.x] == LevelDataSO.TileType.Finish)
            return true;

        return false;
    }
    bool IsWithinBounds(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= levelData.grid[0].tiles.Count ||
            pos.y < 0 || pos.y >= levelData.grid.Count)
            return false;

        return true;
    }
    void UpdateWorldPosition()
    {
        transform.position = new Vector3(
            gridPos.x * cellSize,
            0,
            gridPos.y * cellSize 
        );
    }

    //void checkCollectCoin(Vector2Int pos)
    //{
    //    if (levelData.grid[pos.y].tiles[pos.x] == LevelDataSO.TileType.Coin)
    //    {
    //        Coin++;
    //        levelData.grid[pos.y].tiles[pos.x] = LevelDataSO.TileType.None;
    //        Debug.Log("Collected a coin! Score: " + Coin);
    //    }
    //}
}