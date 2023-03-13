using System;
using System.Collections;
using System.Collections.Generic;
using JKFrame;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Project_WildernessSurvivalGame
{
    /// <summary>
    /// 地图生成工具
    /// </summary>
    public class MapGenerator
    {
        #region Field

        Texture2D m_ForestTexture;
        Texture2D[] m_MarshTextures;
        Material m_MapMaterial;
        Material m_MarshMaterial;
        MapGrid m_MapGrid;
        Mesh m_ChunkMesh;

        [Tooltip("一行/列地图块数量")] int m_MapSize;       //  m_MapSize -> m_MapChunkNum
        [Tooltip("一个地图块的格子数量")] int m_MapChunkSize; //  m_MapChunkSize -> m_CellNum
        float m_CellSize;
        float m_NoiseLacunarity;
        int m_MapGenerationSeed;
        int m_MapObjectRandomSpawnSeed;
        float m_MarshLimit;

        int m_ForestSpawnWeightTotal; // 森林生成物品的权重总和
        int m_MarshSpawnWeightTotal;  // 沼泽生成物品的权重总和

        /// <summary>
        /// Key：顶点类型 Value：可以生成的地图对象配置的ID列表
        /// </summary>
        Dictionary<MapVertexType, List<int>> m_SpawnConfigDict;

        static readonly int s_MainTex = Shader.PropertyToID("_MainTex");

        #endregion

        public MapGenerator
        (
            Texture2D forestTexture, Texture2D[] marshTextures, Material mapMaterial
          , Dictionary<MapVertexType, List<int>> spawnConfigDict, int mapSize, int mapChunkSize, float cellSize
          , float noiseLacunarity, int mapGenerationSeed, int mapObjectRandomSpawnSeed, float marshLimit
        )
        {
            m_ForestTexture = forestTexture;
            m_MarshTextures = marshTextures;
            m_MapMaterial = mapMaterial;
            m_SpawnConfigDict = spawnConfigDict;
            m_MapSize = mapSize;
            m_MapChunkSize = mapChunkSize;
            m_CellSize = cellSize;
            m_NoiseLacunarity = noiseLacunarity;
            m_MapGenerationSeed = mapGenerationSeed;
            m_MapObjectRandomSpawnSeed = mapObjectRandomSpawnSeed;
            m_MarshLimit = marshLimit;
        }

        /// <summary>
        /// 生成地图数据，主要是所有地图块都通用的数据
        /// </summary>
        public void GenerateMapData()
        {
            // 应用地图随机生成种子
            Random.InitState(m_MapGenerationSeed);

            var rowTotalCellNum = m_MapSize * m_MapChunkSize; // 一行/列总格子数
            var mapChunkLength = m_MapChunkSize * m_CellSize; // 单个地图块的长度

            var noiseMap = GenerateNoiseMap(rowTotalCellNum, rowTotalCellNum, m_NoiseLacunarity);

            m_MapGrid = new MapGrid(rowTotalCellNum, rowTotalCellNum, m_CellSize);
            m_MapGrid.CalculateMapVertexType(noiseMap, m_MarshLimit);

            m_MapMaterial.mainTexture = m_ForestTexture; // Set the main texture
            m_MapMaterial.SetTextureScale(s_MainTex, new Vector2(mapChunkLength, mapChunkLength)); // 设置纹理缩放
            m_MarshMaterial = new Material(m_MapMaterial); // 通过复制其他材质的所有属性来创建一个沼泽材质
            m_MarshMaterial.SetTextureScale(s_MainTex, Vector2.one);

            m_ChunkMesh = GenerateMapChunkMesh(m_MapChunkSize, m_MapChunkSize, m_CellSize);

            // 应用地图随机对象（花草树木）生成种子
            Random.InitState(m_MapObjectRandomSpawnSeed);

            List<int> idList = m_SpawnConfigDict[MapVertexType.Forest];
            foreach (int id in idList)
                m_ForestSpawnWeightTotal
                    += ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, id).Probability;

            idList = m_SpawnConfigDict[MapVertexType.Marsh];
            foreach (int id in idList)
                m_MarshSpawnWeightTotal
                    += ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, id).Probability;
        }

        /// <summary>
        /// 生成地图块
        /// </summary>
        /// <param name="chunkIndex">地图块索引</param>
        /// <param name="parent">父物体</param>
        /// <param name="callBackForMapTexture"></param>
        /// <returns></returns>
        public MapChunkController GenerateMapChunk
            (Vector2Int chunkIndex, Transform parent, Action callBackForMapTexture)
        {
            // 生成地图块物体
            var mapChunkGameObj = new GameObject("Chunk_" + chunkIndex);
            var mapChunk = mapChunkGameObj.AddComponent<MapChunkController>();

            // 为地图块生成 Mesh 并添加碰撞体
            mapChunkGameObj.AddComponent<MeshFilter>().mesh = m_ChunkMesh;
            mapChunkGameObj.AddComponent<MeshCollider>();

            // 生成地图块的贴图
            // Texture2D mapTexture;
            // bool allForest;

            this.StartCoroutine(GenerateMapTexture(chunkIndex, CallBack));

            void CallBack(Texture2D texture, bool isAllForest)
            {
                // allForest = isAllForest;
                if (isAllForest)
                {
                    mapChunkGameObj.AddComponent<MeshRenderer>().sharedMaterial = m_MapMaterial;
                }
                else
                {
                    // mapTexture = texture;
                    Material material = new Material(m_MarshMaterial);
                    material.mainTexture = texture;
                    mapChunkGameObj.AddComponent<MeshRenderer>().material = material;
                }
                callBackForMapTexture?.Invoke();

                var chunkSize = m_MapChunkSize * m_CellSize; // 地图块大小
                var position = new Vector3(chunkIndex.x * chunkSize, 0, chunkIndex.y * chunkSize);
                mapChunk.transform.position = position;
                mapChunkGameObj.transform.SetParent(parent);

                // 生成场景物体
                var mapObjectModelList = SpawnMapObject(chunkIndex);
                mapChunk.Init(chunkIndex, isAllForest, mapObjectModelList);

                // mapChunk.Init(chunkIndex, position + new Vector3(chunkSize / 2, 0, chunkSize / 2), isAllForest, mapObjectModelList);
            }

            return mapChunk;
        }

        /// <summary>
        /// 生成地图块 Mesh
        /// </summary>
        /// <param name="width">地图 Mesh 的宽</param>
        /// <param name="height">地图 Mesh 的高</param>
        /// <param name="cellSize">格子尺寸</param>
        /// <returns></returns>
        static Mesh GenerateMapChunkMesh(int width, int height, float cellSize)
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
        static float[,] GenerateNoiseMap(int width, int height, float lacunarity)
        {
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

        /// <summary>
        /// 分帧生成地图块的贴图
        /// </summary>
        /// <param name="chunkIndex">地图块索引</param>
        /// <param name="callBack">Texture2D：贴图，bool：是否全是森林</param>
        /// <returns></returns>
        IEnumerator GenerateMapTexture(Vector2Int chunkIndex, Action<Texture2D, bool> callBack)
        {
            // 当前地图块的偏移量 找到这个地图块具体的每一个格子
            var cellOffsetX = chunkIndex.x * m_MapChunkSize + 1;
            var cellOffsetZ = chunkIndex.y * m_MapChunkSize + 1;
            bool isAllForest = true; // 是不是一张完整的森林地图块

            // 遍历地图快检查是否只有森林类型的格子
            for (int z = 0; z < m_MapChunkSize; z++)
            {
                if (isAllForest == false) break;

                for (int x = 0; x < m_MapChunkSize; x++)
                {
                    var cell = m_MapGrid.GetCell(x + cellOffsetX, z + cellOffsetZ);
                    if (cell != null && cell.TextureIndex != 0)
                    {
                        isAllForest = false;
                        break;
                    }
                }
            }

            Texture2D mapTexture = null;

            if (isAllForest == false) // 如果是沼泽
            {
                var textureCellSize = m_ForestTexture.width;           // 贴图都是正方形
                var mapChunkLength = m_MapChunkSize * textureCellSize; // 整个地图块的边长
                mapTexture = new Texture2D(mapChunkLength, mapChunkLength, TextureFormat.RGB24, false);

                // 遍历每一个格子
                for (int chunkZ = 0; chunkZ < m_MapChunkSize; chunkZ++)
                {
                    yield return null; // 一帧只绘制一列像素

                    int pixelOffsetZ = chunkZ * textureCellSize;

                    for (int chunkX = 0; chunkX < m_MapChunkSize; chunkX++)
                    {
                        int pixelOffsetX = chunkX * textureCellSize;

                        // <0 是森林，>=0 是 沼泽
                        int textureIndex = m_MapGrid.GetCell(chunkX + cellOffsetX, chunkZ + cellOffsetZ).TextureIndex
                          - 1;

                        // 绘制每一个格子内的像素
                        // 访问每一个像素点
                        for (int cellZ = 0; cellZ < textureCellSize; cellZ++)
                        {
                            for (int cellX = 0; cellX < textureCellSize; cellX++)
                            {
                                // 设置某个像素点的颜色
                                // 确定是森林还是沼泽
                                // 这个地方是森林 || 这个地方是沼泽但是是透明的（这种情况需要绘制 groundTexture 同位置的像素颜色）
                                Color color;

                                if (textureIndex < 0) // <0 是森林
                                {
                                    color = m_ForestTexture.GetPixel(cellX, cellZ);
                                }
                                else
                                {
                                    color = m_MarshTextures[textureIndex].GetPixel(cellX, cellZ);

                                    if (color.a < 1f)
                                    {
                                        color = m_ForestTexture.GetPixel(cellX, cellZ);
                                    }
                                }

                                mapTexture.SetPixel(cellX + pixelOffsetX, cellZ + pixelOffsetZ, color);
                            }
                        }
                    }
                }

                // 遍历完一个地图块的所有格子后Apply更改
                mapTexture.filterMode = FilterMode.Point;
                mapTexture.wrapMode = TextureWrapMode.Clamp;
                mapTexture.Apply();
            }

            callBack?.Invoke(mapTexture, isAllForest);
        }

        /// <summary>
        /// 生成地图上的游戏物体
        /// </summary>
        /// <remarks>遍历地图顶点，根据spawnConfig中的配置信息及其概率进行随机生成，并在对应位置实例化物体</remarks>
        List<MapObjectModelInMapChunk> SpawnMapObject(Vector2Int chunkIndex)
        {
            var mapChunkMapObjectList = new List<MapObjectModelInMapChunk>();
            var cellSize = m_MapGrid.CellSize;
            var offsetX = chunkIndex.x * m_MapChunkSize;
            var offsetZ = chunkIndex.y * m_MapChunkSize;

            // 遍历地图顶点
            for (int x = 1; x < m_MapChunkSize; x++)
            {
                for (int z = 1; z < m_MapChunkSize; z++)
                {
                    var mapVertex = m_MapGrid.GetVertex(x + offsetX, z + offsetZ);

                    // 根据概率配置随机
                    // 根据顶点的顶点类型获取对应的列表
                    var spawnConfigIdList = m_SpawnConfigDict[mapVertex.VertexType];

                    // 确定权重的总和
                    int weightTotal = mapVertex.VertexType == MapVertexType.Forest
                        ? m_ForestSpawnWeightTotal
                        : m_MarshSpawnWeightTotal;

                    int randomValue = Random.Range(1, weightTotal + 1);
                    float probabilitySum = 0; // 概率和
                    int spawnConfigIndex = 0; // 最终要生成的物品的索引

                    for (int i = 0; i < spawnConfigIdList.Count; i++)
                    {
                        probabilitySum += ConfigManager.Instance
                                                       .GetConfig<MapObjectConfig>(
                                                            ConfigName.MapObject, spawnConfigIdList[i]).Probability;

                        if (randomValue < probabilitySum) // 命中
                        {
                            spawnConfigIndex = i;
                            break;
                        }
                    }
                    var configID = spawnConfigIdList[spawnConfigIndex];
                    var spawnModel = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, configID);

                    if (spawnModel.IsEmpty) continue;

                    var position = mapVertex.Position + new Vector3
                    (
                        Random.Range(-cellSize / 2, cellSize / 2)
                      , 0
                      , Random.Range(-cellSize / 2, cellSize / 2)
                    );
                    mapChunkMapObjectList.Add
                    (
                        new MapObjectModelInMapChunk { ConfigID = configID, Position = position }
                    );
                }
            }
            return mapChunkMapObjectList;
        }
    }
}