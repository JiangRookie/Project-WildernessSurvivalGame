using Sirenix.OdinInspector;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public MeshRenderer MeshRenderer;
    public MeshFilter MeshFilter;

    public int MapHeight;
    public int MapWidth;
    public float CellSize;

    MapGrid m_MapGrid;

    [Button("生成地图")]
    public void GenerateMap()
    {
        MeshFilter.mesh = GenerateMapMesh(MapWidth, MapHeight, CellSize);
        m_MapGrid = new MapGrid(MapWidth, MapHeight, CellSize);
    }

    public GameObject TestObj;

    [Button("测试顶点")]
    public void TestVertex()
    {
        print(m_MapGrid.GetVertex(TestObj.transform.position).Position.ToString());
    }

    [Button("测试格子")]
    public void TestCell(Vector2Int index)
    {
        print(m_MapGrid.GetLBMapCell(index).Position.ToString());
        print(m_MapGrid.GetRBMapCell(index).Position.ToString());
        print(m_MapGrid.GetLTMapCell(index).Position.ToString());
        print(m_MapGrid.GetRTMapCell(index).Position.ToString());
    }

    /// <summary>
    /// 生成地图 Mesh
    /// </summary>
    /// <param name="width">地图 Mesh 的宽</param>
    /// <param name="height">地图 Mesh 的高</param>
    /// <param name="cellSize">格子尺寸</param>
    /// <returns></returns>
    static Mesh GenerateMapMesh(int width, int height, float cellSize)
    {
        Mesh mesh = new Mesh();

        // 确定顶点在哪里
        mesh.vertices = new[]
        {
            new Vector3(0, 0, 0)
          , new Vector3(0, 0, height * cellSize)
          , new Vector3(width * cellSize, 0, height * cellSize)
          , new Vector3(width * cellSize, 0, 0)
        };

        // 确定哪些点形成三角形
        mesh.triangles = new[]
        {
            0, 1, 2
          , 0, 2, 3
        };

        // 设置 UV
        mesh.uv = new Vector2[]
        {
            new Vector3(0, 0)
          , new Vector3(0, 1)
          , new Vector3(1, 1)
          , new Vector3(1, 0)
        };

        // 重新计算法线
        mesh.RecalculateNormals(); // (0, 1, 0)
        return mesh;
    }
}