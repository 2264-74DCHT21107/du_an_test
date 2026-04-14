using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteAlways]
public class LeverManager : MonoBehaviour
{
    public GameObject Player;
    public PlayerController PlayerController;
    public GameObject EmptyPreb;
    public GameObject WallPreb;
    public GameObject FinishPreb;
    public GameObject CoinPreb;
    public GameObject NeedCoinPreb;
    public LevelDataSO levelData;
    public GameObject UpPreb;

    public float spacing = 1.2f;

    void Start()
    {
        SpawnPlayer();
    }
    
    [Button("Generate Level From SO")]
    void GenerateLevel()
    {
        if (levelData == null) return;

        ClearChildren();

        for (int i = 0; i < levelData.grid.Count; i++)
        {
            for (int j = 0; j < levelData.grid[i].tiles.Count; j++)
            {
                var tile = levelData.grid[i].tiles[j];

                GameObject prefab = null;

                switch (tile)
                {
                    case LevelDataSO.TileType.None:
                        prefab = EmptyPreb;
                        break;
                    case LevelDataSO.TileType.Wall:
                        prefab = WallPreb;
                        break;
                    case LevelDataSO.TileType.Finish:
                        prefab = FinishPreb;
                        break;
                    case LevelDataSO.TileType.Coin:
                        prefab = CoinPreb;
                        break;
                    case LevelDataSO.TileType.NeedCoin:
                        prefab = NeedCoinPreb;
                        break;
                    case LevelDataSO.TileType.RidirectUp:
                        prefab = UpPreb;
                        break;

                }

                if (prefab == null) continue;

                GameObject obj = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
                obj.transform.SetParent(transform);
                obj.transform.localPosition = new Vector3(j * spacing, 0, i * spacing);
                obj.name = $"Tile_{i}_{j}";
            }
        }

        SpawnPlayer();

    }

    void SpawnPlayer()
    {
        
        if (Player != null)
        {
            GameObject playerObj = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(Player);
            playerObj.transform.SetParent(transform);
            playerObj.transform.localPosition = new Vector3(levelData.PlayerPos.x * spacing, 0, levelData.PlayerPos.y * spacing);
            PlayerController = playerObj.GetComponent<PlayerController>();
            PlayerController.levelData = levelData;
            PlayerController.gridPos = new Vector2Int((int)levelData.PlayerPos.x, (int)levelData.PlayerPos.y);
            playerObj.name = "Player";
        }
    }


    [Button("Clear Level")]
    void ClearChildren()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
}