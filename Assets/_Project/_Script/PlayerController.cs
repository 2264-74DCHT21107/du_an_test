using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float cellSize = 1.2f;
    public Vector2Int gridPos;
    public int Coin = 0;
    public LevelDataSO levelData;

    void Start()
    {
        UpdateWorldPosition();
    }

    void Update()
    {
        if (levelData == null) return;

        Vector2Int move = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.W)) move = new Vector2Int(0, 1);
        if (Input.GetKeyDown(KeyCode.S)) move = new Vector2Int(0, -1);
        if (Input.GetKeyDown(KeyCode.A)) move = new Vector2Int(-1, 0);
        if (Input.GetKeyDown(KeyCode.D)) move = new Vector2Int(1, 0);

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

            if (!IsWithinBounds(next))
                break;

            LevelDataSO.TileType tile = levelData.grid[next.y].tiles[next.x];

            if (tile == LevelDataSO.TileType.Wall)
                break;

        
            current = next;

         
            ProcessTile(current);

       
            if (tile == LevelDataSO.TileType.Finish)
                break;

      
            if (tile == LevelDataSO.TileType.NeedCoin && Coin <= 0)
                break;
        }

        return current;
    }

 
    void ProcessTile(Vector2Int pos)
    {
        if (!IsWithinBounds(pos)) return;

        var tileList = levelData.grid[pos.y].tiles;
        LevelDataSO.TileType tileType = tileList[pos.x];

        switch (tileType)
        {
            case LevelDataSO.TileType.Coin:
                Coin++;
                tileList[pos.x] = LevelDataSO.TileType.None;   
                Debug.Log($"Collected Coin at {pos}! Total: {Coin}");
                break;

            case LevelDataSO.TileType.NeedCoin:
                if (Coin > 0)
                {
                    Coin--;
                    tileList[pos.x] = LevelDataSO.TileType.None;
                    Debug.Log($"Used 1 Coin at {pos}. Remaining: {Coin}");
                }
                else
                {
                    Debug.Log("Need a coin to pass this tile!");
                }
                break;

            case LevelDataSO.TileType.Finish:
                Debug.Log("=== YOU WIN! ===");
                break;
        }
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
        transform.position = new Vector3(
            gridPos.x * cellSize,
            0,
            gridPos.y * cellSize
        );
    }
}