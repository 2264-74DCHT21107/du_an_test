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


    }

    [Serializable]
    public class Row
    {
        public List<TileType> tiles = new List<TileType>();
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
#endif

    [Title("Level Data")]
    [ListDrawerSettings(Expanded = true)]
    public List<Row> grid = new List<Row>();
}