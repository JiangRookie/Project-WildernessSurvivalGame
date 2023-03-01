using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class MapGenerator : MonoBehaviour
{
    #region Field

    public MeshRenderer MeshRenderer;
    public MeshFilter MeshFilter;
    public Texture2D GroundTexture;
    public Texture2D[] MarshTextures;
    public MapObjectSpawnConfig SpawnConfig;

    public int MapHeight;
    public int MapWidth;
    public float CellSize;
    public float Lacunarity;
    [FormerlySerializedAs("Seed")] public int MapSeed;
    public int SpawnSeed;
    [Range(0f, 1f)] public float Limit;

    MapGrid m_MapGrid;

    #endregion

    public GameObject TestObj;

    [Button("生成地图")]
    public void GenerateMap()
    {
        MeshFilter.mesh = GenerateMapMesh(MapWidth, MapHeight, CellSize);
        m_MapGrid = new MapGrid(MapWidth, MapHeight, CellSize);
        float[,] noiseMap = GenerateNoiseMap(MapWidth, MapHeight, Lacunarity, MapSeed);
        int[,] cellTextureIndexMap = m_MapGrid.CalculateCellTextureIndex(noiseMap, Limit);
        Texture2D mapTexture = GenerateMapTexture(cellTextureIndexMap, GroundTexture, MarshTextures);
        MeshRenderer.sharedMaterial.mainTexture = mapTexture;
        SpawnMapObject(m_MapGrid, SpawnConfig, SpawnSeed);
    }

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

    /// <summary>
    /// 生成柏林噪声图
    /// </summary>
    /// <param name="width">宽</param>
    /// <param name="height">高</param>
    /// <param name="lacunarity">间隙，影响平滑度</param>
    /// <param name="seed">随机种子</param>
    /// <returns></returns>
    static float[,] GenerateNoiseMap(int width, int height, float lacunarity, int seed)
    {
        Random.InitState(seed);
        lacunarity += 0.1f;

        // 这里的噪声图是为了顶点服务的，而顶点是不包含地图四周的边界的，所以要在原有宽高的基础上 -1
        float[,] noiseMap = new float[width - 1, height - 1];
        float offsetX = Random.Range(-10000f, 10000f);
        float offsetZ = Random.Range(-10000f, 10000f);
        for (int x = 0; x < width - 1; x++)
        {
            for (int z = 0; z < height - 1; z++)
            {
                noiseMap[x, z] =
                    Mathf.PerlinNoise(x * lacunarity + offsetX, z * lacunarity + offsetZ);
            }
        }

        return noiseMap;
    }

    static Texture2D GenerateMapTexture(int[,] cellTextureIndexMap, Texture2D groundTexture, Texture2D[] marshTextures)
    {
        int mapWidth = cellTextureIndexMap.GetLength(0);
        int mapHeight = cellTextureIndexMap.GetLength(1);
        int textureCellSize = groundTexture.width;
        var mapTexture = new Texture2D
            (mapWidth * textureCellSize, mapHeight * textureCellSize, TextureFormat.RGB24, false);

        // 遍历每一个格子
        for (int outerZ = 0; outerZ < mapHeight; outerZ++)
        {
            int offsetZ = outerZ * textureCellSize;
            for (int outerX = 0; outerX < mapWidth; outerX++)
            {
                int offsetX = outerX * textureCellSize;

                // -1 是 groundTexture，0 是 marshTextures 中的某一张贴图
                int textureIndex = cellTextureIndexMap[outerX, outerZ] - 1;

                // 绘制每一个格子内的像素，访问每一个像素点
                for (int innerZ = 0; innerZ < textureCellSize; innerZ++)
                {
                    for (int innerX = 0; innerX < textureCellSize; innerX++)
                    {
                        // 设置某个像素点的颜色
                        // 确定是森林还是沼泽
                        // 这个地方是森林 || 这个地方是沼泽但是是透明的（这种情况需要绘制 groundTexture 同位置的像素颜色）
                        Color color;
                        if (textureIndex < 0)
                        {
                            color = groundTexture.GetPixel(innerX, innerZ);
                        }
                        else
                        {
                            color = marshTextures[textureIndex].GetPixel(innerX, innerZ);
                            if (color.a == 0)
                            {
                                color = groundTexture.GetPixel(innerX, innerZ);
                            }
                        }

                        mapTexture.SetPixel(innerX + offsetX, innerZ + offsetZ, color);
                    }
                }
            }
        }

        mapTexture.filterMode = FilterMode.Point;
        mapTexture.wrapMode = TextureWrapMode.Clamp;
        mapTexture.Apply();
        return mapTexture;
    }

    public List<GameObject> MapObjectList = new();

    /// <summary>
    /// 生成地图上的游戏物体
    /// </summary>
    /// <param name="mapGrid">用于获取地图的宽度和高度</param>
    /// <param name="spawnConfig">用于获取生成的物体及其概率配置信息</param>
    /// <param name="spawnSeed">用于随机生成物体</param>
    /// <remarks>遍历地图顶点，根据spawnConfig中的配置信息及其概率进行随机生成，并在对应位置实例化物体</remarks>
    void SpawnMapObject(MapGrid mapGrid, MapObjectSpawnConfig spawnConfig, int spawnSeed)
    {
        #region 临时测试逻辑

        foreach (var gameObj in MapObjectList)
        {
            DestroyImmediate(gameObj.gameObject);
        }

        MapObjectList.Clear();

        #endregion

        // 使用种子进行随机生成
        Random.InitState(spawnSeed);
        var mapWidth = mapGrid.MapWidth;
        var mapHeight = mapGrid.MapHeight;
        var cellSize = mapGrid.CellSize;

        // 遍历地图顶点
        for (int x = 1; x < mapWidth; x++)
        {
            for (int z = 1; z < mapHeight; z++)
            {
                var mapVertex = mapGrid.GetVertex(x, z);

                // 根据概率配置随机
                // 根据顶点的顶点类型获取对应的列表
                var spawnConfigModelList = spawnConfig.SpawnConfigDic[mapVertex.VertexType];

                // 确保整个配置概率值和为 100
                int randomValue = Random.Range(1, 101);
                float probabilitySum = 0; // 概率和
                int spawnConfigIndex = 0; // 最终要生成的物品的索引
                for (int i = 0; i < spawnConfigModelList.Count; i++)
                {
                    probabilitySum += spawnConfigModelList[i].Probability;
                    if (randomValue < probabilitySum) // 命中
                    {
                        spawnConfigIndex = i;
                        break;
                    }
                }

                var spawnModel = spawnConfigModelList[spawnConfigIndex];
                if (spawnModel.IsEmpty) continue;

                // 实例化物品
                var offset = new Vector3
                (
                    Random.Range(-cellSize / 2, cellSize / 2)
                  , 0
                  , Random.Range(-cellSize / 2, cellSize / 2)
                );
                var gameObj = Instantiate
                (
                    original: spawnModel.Prefab
                  , position: mapVertex.Position + offset
                  , rotation: Quaternion.identity
                  , parent: transform
                );
                MapObjectList.Add(gameObj);
            }
        }
    }
}