using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 网格，主要包含顶点和格子
/// </summary>
public class MapGrid
{
    /// <summary>
    /// 生成网格数据
    /// </summary>
    /// <param name="mapWidth">宽</param>
    /// <param name="mapHeight">高</param>
    /// <param name="cellSize">格子尺寸</param>
    public MapGrid(int mapWidth, int mapHeight, float cellSize)
    {
        MapHeight = mapHeight;
        MapWidth = mapWidth;
        CellSize = cellSize;

        // 从 1 开始的原因是：地图的四周（边界四个角）不算顶点
        for (int x = 1; x < mapWidth; x++)
        {
            for (int z = 1; z < mapHeight; z++)
            {
                AddVertex(x, z);
                AddCell(x, z);
            }
        }

        for (int x = 1; x <= mapWidth; x++) AddCell(x, mapHeight); // 给格子增加一行一列
        for (int z = 1; z < mapWidth; z++) AddCell(mapWidth, z);   // 不取等号的原因是行列有一个是重叠的
    }

    public int MapHeight { get; private set; }
    public int MapWidth { get; private set; }
    public float CellSize { get; private set; }

    /// <summary>
    /// 计算地图顶点类型
    /// </summary>
    /// <param name="noiseMap">噪声图</param>
    /// <param name="marshLimit">沼泽边界，大于该值则为沼泽</param>
    public void CalculateMapVertexType(float[,] noiseMap, float marshLimit)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        // 遍历的是格子所以要加上 “=” 号
        for (int x = 1; x <= width; x++)
        {
            for (int z = 1; z <= height; z++)
            {
                // 基于噪声中的值确定这个顶点的类型；减1是因为数组要从0开始。
                SetVertexType(x, z, noiseMap[x - 1, z - 1] >= marshLimit ? MapVertexType.Marsh : MapVertexType.Forest);
            }
        }
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

    void SetVertexType(Vector2Int vertexIndex, MapVertexType vertexType)
    {
        MapVertex vertex = GetVertex(vertexIndex);
        if (vertex.VertexType == vertexType) return; // FIXME:反转了if，可能会出错
        vertex.VertexType = vertexType;

        // 只有沼泽需要计算
        if (vertex.VertexType != MapVertexType.Marsh) return; // FIXME:反转了if，可能会出错

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

    void SetVertexType(int x, int y, MapVertexType mapVertexType)
        => SetVertexType(new Vector2Int(x, y), mapVertexType);

    #endregion

    #region Cell

    /// <summary>
    /// 格子数据<br/>
    /// key: 顶点坐标 value: 地图格子类
    /// </summary>
    public readonly Dictionary<Vector2Int, MapCell> CellDic = new();

    void AddCell(int x, int y)
    {
        float offset = CellSize / 2;
        CellDic.Add
        (
            new Vector2Int(x, y)
          , new MapCell { Position = new Vector3(x * CellSize - offset, 0, y * CellSize - offset) }
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

public enum MapVertexType { Forest, Marsh }

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
    public Vector3 Position; // 格子中心点位置

    /// <summary>
    /// 贴图索引（贴图权重），森林权重为0，沼泽权重（左下1、右下2、左上4、右上8）。
    /// </summary>
    public int TextureIndex;
}