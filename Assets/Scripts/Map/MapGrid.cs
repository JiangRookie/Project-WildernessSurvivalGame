using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 网格，主要包含顶点和格子
/// </summary>
public class MapGrid
{
    /// <summary>
    /// 顶点数据<br/>
    /// key: 顶点坐标 value: 地图顶点类
    /// </summary>
    public readonly Dictionary<Vector2Int, MapVertex> VertexDic = new Dictionary<Vector2Int, MapVertex>();

    /// <summary>
    /// 格子数据<br/>
    /// key: 顶点坐标 value: 地图格子类
    /// </summary>
    public readonly Dictionary<Vector2Int, MapCell> CellDic = new Dictionary<Vector2Int, MapCell>();

    /// <param name="mapWidth">宽</param>
    /// <param name="mapHeight">高</param>
    /// <param name="cellSize">格子尺寸</param>
    public MapGrid(int mapWidth, int mapHeight, float cellSize)
    {
        MapHeight = mapHeight;
        MapWidth = mapWidth;
        CellSize = cellSize;

        // 生成顶点数据 VertexDic
        // 从 1 开始的原因是：地图的四周（边界四个角）不算顶点
        for (int x = 1; x < mapWidth; x++)
        {
            for (int z = 1; z < mapHeight; z++)
            {
                AddVertex(x, z);
                AddCell(x, z);
            }
        }

        // 给格子增加一行一列
        for (int x = 1; x <= mapWidth; x++) AddCell(x, mapWidth);
        for (int z = 1; z < mapHeight; z++) AddCell(mapHeight, z);

        #region Test

        foreach (var vertex in VertexDic.Values)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = vertex.Position;
            sphere.transform.localScale = Vector3.one * 0.25f;
        }

        foreach (var cell in CellDic.Values)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = cell.Position - new Vector3(0f, 0.49f, 0f);
            cube.transform.localScale = new Vector3(cellSize, 1, cellSize);
        }

        #endregion
    }

    public int MapHeight { get; private set; }
    public int MapWidth { get; private set; }
    public float CellSize { get; private set; }

    #region Vertex

    void AddVertex(int x, int z)
    {
        VertexDic.Add
        (
            new Vector2Int(x, z)
          , new MapVertex { Position = new Vector3(x * CellSize, 0, z * CellSize) }
        );
    }

    public MapVertex GetVertex(Vector2Int index)
    {
        return VertexDic[index];
    }

    public MapVertex GetVertex(int x, int y)
    {
        return GetVertex(new Vector2Int(x, y));
    }

    public MapVertex GetVertex(Vector3 worldPosition)
    {
        int x = Mathf.Clamp(value: Mathf.RoundToInt(worldPosition.x / CellSize), min: 1, max: MapWidth);
        int z = Mathf.Clamp(value: Mathf.RoundToInt(worldPosition.z / CellSize), min: 1, max: MapHeight);
        return GetVertex(x, z);
    }

    #endregion

    #region Cell

    void AddCell(int x, int z)
    {
        float offset = CellSize / 2;
        CellDic.Add
        (
            new Vector2Int(x, z)
          , new MapCell { Position = new Vector3(x * CellSize - offset, 0, z * CellSize - offset) }
        );
    }

    public MapCell GetCell(Vector2Int index)
    {
        return CellDic[index];
    }

    public MapCell GetCell(int x, int y)
    {
        return GetCell(new Vector2Int(x, y));
    }

    public MapCell GetLBMapCell(Vector2Int vertexIndex)
    {
        return CellDic[vertexIndex];
    }

    public MapCell GetRBMapCell(Vector2Int vertexIndex)
    {
        return CellDic[new Vector2Int(vertexIndex.x + 1, vertexIndex.y)];
    }

    public MapCell GetLTMapCell(Vector2Int vertexIndex)
    {
        return CellDic[new Vector2Int(vertexIndex.x, vertexIndex.y + 1)];
    }

    public MapCell GetRTMapCell(Vector2Int vertexIndex)
    {
        return CellDic[new Vector2Int(vertexIndex.x + 1, vertexIndex.y + 1)];
    }

    #endregion
}

/// <summary>
/// 地图顶点
/// </summary>
public class MapVertex
{
    public Vector3 Position;
}

/// <summary>
/// 地图格子
/// </summary>
public class MapCell
{
    public Vector3 Position;
}