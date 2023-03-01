using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 网格，主要包含顶点和格子
/// </summary>
public class MapGrid
{
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
        for (int x = 1; x <= mapWidth; x++) AddCell(x, mapHeight);

        // 不取等号的原因是行列有一个是重叠的
        for (int z = 1; z < mapWidth; z++) AddCell(mapWidth, z);

        #region Test

        // foreach (var vertex in VertexDic.Values)
        // {
        //     var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //     sphere.transform.position = vertex.Position;
        //     sphere.transform.localScale = Vector3.one * 0.25f;
        // }
        //
        // foreach (var cell in CellDic.Values)
        // {
        //     var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //     cube.transform.position = cell.Position - new Vector3(0f, 0.49f, 0f);
        //     cube.transform.localScale = new Vector3(cellSize, 1, cellSize);
        // }

        #endregion
    }

    public int MapHeight { get; private set; }
    public int MapWidth { get; private set; }
    public float CellSize { get; private set; }

    /// <summary>
    /// 计算格子贴图的索引数字
    /// </summary>
    /// <param name="noiseMap"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    public int[,] CalculateCellTextureIndex(float[,] noiseMap, float limit)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        // 遍历的是格子所以要加上 “=” 号
        for (int x = 1; x < width; x++)
        {
            for (int z = 1; z < height; z++)
            {
                // 基于噪声中的值确定这个顶点的类型
                // 数组要从0开始
                SetVertexType(x, z, noiseMap[x - 1, z - 1] >= limit ? MapVertexType.Marsh : MapVertexType.Forest);
            }
        }

        int[,] cellTextureIndex = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                cellTextureIndex[x, z] = GetCell(x + 1, z + 1).TextureIndex;
            }
        }

        return cellTextureIndex;
    }

    #region Vertex

    /// <summary>
    /// 顶点数据<br/>
    /// key: 顶点坐标 value: 地图顶点类
    /// </summary>
    public readonly Dictionary<Vector2Int, MapVertex> VertexDic = new();

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
        VertexDic.TryGetValue(index, out MapVertex vertex);
        return vertex;
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

    void SetVertexType(Vector2Int vertexIndex, MapVertexType mapVertexType)
    {
        var vertex = GetVertex(vertexIndex);
        if (vertex.VertexType != mapVertexType)
        {
            vertex.VertexType = mapVertexType;

            // 只有沼泽需要计算
            if (vertex.VertexType == MapVertexType.Marsh)
            {
                // 计算附近的贴图权重
                MapCell cell = GetLBMapCell(vertexIndex);
                if (cell != null) cell.TextureIndex += 1;
                cell = GetRBMapCell(vertexIndex);
                if (cell != null) cell.TextureIndex += 2;
                cell = GetLTMapCell(vertexIndex);
                if (cell != null) cell.TextureIndex += 4;
                cell = GetRTMapCell(vertexIndex);
                if (cell != null) cell.TextureIndex += 8;
            }
        }
    }

    void SetVertexType(int x, int y, MapVertexType mapVertexType)
    {
        SetVertexType(new Vector2Int(x, y), mapVertexType);
    }

    #endregion

    #region Cell

    /// <summary>
    /// 格子数据<br/>
    /// key: 顶点坐标 value: 地图格子类
    /// </summary>
    public readonly Dictionary<Vector2Int, MapCell> CellDic = new();

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
        CellDic.TryGetValue(index, out MapCell cell);
        return cell;
    }

    public MapCell GetCell(int x, int y)
    {
        return GetCell(new Vector2Int(x, y));
    }

    public MapCell GetLBMapCell(Vector2Int vertexIndex)
    {
        return GetCell(vertexIndex);
    }

    public MapCell GetRBMapCell(Vector2Int vertexIndex)
    {
        return GetCell(vertexIndex.x + 1, vertexIndex.y);
    }

    public MapCell GetLTMapCell(Vector2Int vertexIndex)
    {
        return GetCell(vertexIndex.x, vertexIndex.y + 1);
    }

    public MapCell GetRTMapCell(Vector2Int vertexIndex)
    {
        return GetCell(vertexIndex.x + 1, vertexIndex.y + 1);
    }

    #endregion
}

public enum MapVertexType
{
    Forest, Marsh
}

/// <summary>
/// 地图顶点
/// </summary>
public class MapVertex
{
    public MapVertexType VertexType;
    public Vector3 Position;
}

/// <summary>
/// 地图格子
/// </summary>
public class MapCell
{
    public Vector3 Position;
    public int TextureIndex; // 贴图权重 左下1 右下2 左上4 右上8
}