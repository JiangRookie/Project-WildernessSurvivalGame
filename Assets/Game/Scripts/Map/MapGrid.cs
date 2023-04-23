using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 网格，主要包含顶点和格子
/// </summary>
public class MapGrid
{
    readonly Dictionary<Vector2Int, MapVertex> m_VertexDic = new Dictionary<Vector2Int, MapVertex>();
    readonly Dictionary<Vector2Int, MapCell> m_CellDic = new Dictionary<Vector2Int, MapCell>();

    public int MapHeight { get; }
    public int MapWidth { get; }
    public float CellSize { get; }

    /// <summary>
    /// 生成地图网格数据
    /// </summary>
    /// <param name="mapWidth">宽（地图行总格子数）</param>
    /// <param name="mapHeight">高（地图列总格子数）</param>
    /// <param name="cellSize">格子尺寸</param>
    public MapGrid(int mapWidth, int mapHeight, float cellSize)
    {
        MapHeight = mapHeight;
        MapWidth = mapWidth;
        CellSize = cellSize;

        // 从 1 开始的原因是：地图的四边不算顶点
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

    /// <summary>
    /// 计算地图顶点类型
    /// </summary>
    /// <param name="noiseMap">噪声图</param>
    /// <param name="marshLimit">沼泽边界，大于该值则为沼泽</param>
    public void CalculateMapVertexType(float[,] noiseMap, float marshLimit)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        for (int x = 1; x < width; x++)
        {
            for (int y = 1; y < height; y++)
            {
                // 基于噪声中的值确定这个顶点的类型；减1是因为数组要从0开始。
                SetVertexType(x, y, noiseMap[x, y] >= marshLimit ? MapVertexType.Marsh : MapVertexType.Forest);
            }
        }
    }

    #region Vertex

    void AddVertex(int x, int z)
    {
        m_VertexDic.Add(new Vector2Int(x, z), new MapVertex { Position = new Vector3(x * CellSize, 0, z * CellSize) });
    }

    public MapVertex GetVertex(Vector2Int index)
    {
        m_VertexDic.TryGetValue(index, out MapVertex vertex);
        return vertex;
    }

    public MapVertex GetVertex(int x, int y) => GetVertex(new Vector2Int(x, y));

    public MapVertex GetVertex(Vector3 worldPosition)
    {
        int x = Mathf.Clamp(value: Mathf.RoundToInt(worldPosition.x / CellSize), min: 1, max: MapWidth);
        int z = Mathf.Clamp(value: Mathf.RoundToInt(worldPosition.z / CellSize), min: 1, max: MapHeight);
        return GetVertex(x, z);
    }

    void SetVertexType(Vector2Int vertexPos, MapVertexType vertexType)
    {
        var vertex = GetVertex(vertexPos);
        if (vertex.VertexType != vertexType) // FIXME:反转了if，可能会出错
        {
            vertex.VertexType = vertexType;

            // 只有沼泽需要计算
            if (vertex.VertexType == MapVertexType.Marsh) // FIXME:反转了if，可能会出错
            {
                // 计算附近的贴图权重
                MapCell cell = GetLBMapCell(vertexPos);
                if (cell != null) cell.TextureIndex += 1;
                cell = GetRBMapCell(vertexPos);
                if (cell != null) cell.TextureIndex += 2;
                cell = GetLTMapCell(vertexPos);
                if (cell != null) cell.TextureIndex += 4;
                cell = GetRTMapCell(vertexPos);
                if (cell != null) cell.TextureIndex += 8;
            }
        }
    }

    void SetVertexType(int x, int y, MapVertexType mapVertexType) => SetVertexType(new Vector2Int(x, y), mapVertexType);

    #endregion

    #region Cell

    void AddCell(int x, int y)
    {
        var offset = CellSize / 2;
        m_CellDic.Add(new Vector2Int(x, y)
                    , new MapCell { Position = new Vector3(x * CellSize - offset, 0, y * CellSize - offset) });
    }

    public MapCell GetCell(Vector2Int pos)
    {
        m_CellDic.TryGetValue(pos, out MapCell cell);
        return cell;
    }

    public MapCell GetCell(int x, int y) => GetCell(new Vector2Int(x, y));
    public MapCell GetLBMapCell(Vector2Int pos) => GetCell(pos);
    public MapCell GetRBMapCell(Vector2Int pos) => GetCell(pos.x + 1, pos.y);
    public MapCell GetLTMapCell(Vector2Int pos) => GetCell(pos.x, pos.y + 1);
    public MapCell GetRTMapCell(Vector2Int pos) => GetCell(pos.x + 1, pos.y + 1);

    #endregion
}

public enum MapVertexType { None, Forest, Marsh }

/// <summary>
/// 地图顶点
/// </summary>
public class MapVertex
{
    public MapVertexType VertexType;
    public Vector3 Position;
    public ulong MapObjectID; // 当前地图顶点上的地图对象ID
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