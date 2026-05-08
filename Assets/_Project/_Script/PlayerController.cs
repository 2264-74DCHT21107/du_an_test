using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public LevelDataSO levelData;
    public LeverManager leverManager;

    [Header("Movement Settings")]
    public float cellSize = 1.2f;
    public float timePerTile = 0.25f;

    public Vector2Int gridPos;
    public int Coin = 0;

    private Vector2Int moveDirection = Vector2Int.zero;
    private bool isMoving = false;
    private float timer = 0f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private Vector2Int nextGridPos;

    void Start()
    {
        UpdateWorldPositionInstant();

        if (leverManager == null)
            leverManager = FindObjectOfType<LeverManager>();
        if (leverManager != null)
            cellSize = leverManager.spacing;
    }

    void Update()
    {
        if (levelData == null) return;


        if (!isMoving)
        {
            Vector2Int moveDir = Vector2Int.zero;

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) moveDir = Vector2Int.up;
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) moveDir = Vector2Int.down;
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) moveDir = Vector2Int.left;
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) moveDir = Vector2Int.right;

            if (moveDir != Vector2Int.zero)
                StartMoving(moveDir);
        }


        if (isMoving)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / timePerTile);

            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);

            if (timer >= timePerTile)
                MoveOneStep();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.RestartCurrentLevel();
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        }
    }

    void StartMoving(Vector2Int dir)
    {
        moveDirection = dir;
        nextGridPos = gridPos + dir;

        if (!IsWithinBounds(nextGridPos) ||
            levelData.grid[nextGridPos.y].tiles[nextGridPos.x] == LevelDataSO.TileType.Wall)
        {
            StopMoving();
            return;
        }

        isMoving = true;
        timer = 0f;

        startPos = transform.localPosition;
        targetPos = GridToLocal(nextGridPos);
    }

    void MoveOneStep()
    {
        gridPos = nextGridPos;


        transform.localPosition = GridToLocal(gridPos);

        moveDirection = ProcessTile(gridPos, moveDirection);


        if (levelData.grid[gridPos.y].tiles[gridPos.x] == LevelDataSO.TileType.Finish)
        {
            Debug.Log("YOU WIN!");
            TeleToNextLevel();
            return;
        }

        if (moveDirection != Vector2Int.zero)
        {
            nextGridPos = gridPos + moveDirection;

            if (!IsWithinBounds(nextGridPos) ||
                levelData.grid[nextGridPos.y].tiles[nextGridPos.x] == LevelDataSO.TileType.Wall)
            {
                StopMoving();
                return;
            }

            startPos = transform.localPosition;
            targetPos = GridToLocal(nextGridPos);
            timer = 0f;
        }
        else
        {
            StopMoving();
        }
    }

    void StopMoving()
    {
        if (!isMoving) return;

        isMoving = false;
        timer = 0f;
        moveDirection = Vector2Int.zero;

        Debug.Log($"Dừng tại {gridPos}");
    }

    Vector2Int ProcessTile(Vector2Int pos, Vector2Int currentDirection)
    {
        if (!IsWithinBounds(pos)) return currentDirection;

        var tileType = levelData.grid[pos.y].tiles[pos.x];

        switch (tileType)
        {
            case LevelDataSO.TileType.Coin:
                Coin++;
                Debug.Log($"Coin: {Coin}");
                ChangeTilesToNone(pos);
                break;

            case LevelDataSO.TileType.NeedCoin:
                if (Coin > 0)
                {
                    Coin--;
                    Debug.Log($"Used coin, còn {Coin}");
                    ChangeTilesToNone(pos);
                }
                else
                {
                    Debug.Log("Không đủ coin");
                    StopMoving();
                    return Vector2Int.zero;
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

    public void UpdateWorldPositionInstant()
    {
        Vector3 correctPosition = new Vector3(gridPos.x * cellSize, 0f, gridPos.y * cellSize);
        transform.localPosition = correctPosition;
    }

    Vector3 GridToLocal(Vector2Int pos)
    {
        return new Vector3(pos.x * cellSize, 0f, pos.y * cellSize);
    }


    void ChangeTilesToNone(Vector2Int pos)
    {
        if (!IsWithinBounds(pos)) return;

        levelData.grid[pos.y].tiles[pos.x] = LevelDataSO.TileType.None;

        if (leverManager != null)
        {
            GameObject tileObj = leverManager.GetTileObject(pos);

            if (tileObj != null)
            {
                CoinEffect effect = tileObj.GetComponent<CoinEffect>();

                if (effect != null)
                {
                    effect.PlayEffect();


                    leverManager.ReplaceTile(pos, LevelDataSO.TileType.None);
                }
                else
                {
                    Destroy(tileObj);
                    leverManager.ReplaceTile(pos, LevelDataSO.TileType.None);
                }
            }
        }
    }

    private void TeleToNextLevel()
    {
        StopMoving();
        Debug.Log("YOU WIN! Chuyển level...");

        if (GameManager.Instance != null)
            GameManager.Instance.NextLevel();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // fallback
    }

    public void ResetState()
    {
        Coin = 0;
        StopMoving();
    }
}