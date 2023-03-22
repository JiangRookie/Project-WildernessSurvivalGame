using JKFrame;
using Project_WildernessSurvivalGame;
using UnityEngine;

/// <summary>
/// 数据存档管理器
/// </summary>
public class ArchiveManager : Singleton<ArchiveManager>
{
    public ArchiveManager() => LoadArchiveData();

    public PlayerTransformData PlayerTransformData { get; private set; }
    public MapInitData MapInitData { get; private set; }
    public MapData MapData { get; private set; }
    public InventoryData InventoryData { get; private set; }

    public bool HasArchived { get; private set; }

    public void LoadArchiveData()
    {
        // 单存档机制，所以是获取第0个存档数据
        SaveItem saveItem = SaveManager.GetSaveItem(0);
        HasArchived = saveItem != null;
    }

    /// <summary>
    /// Create a new archive in overlay mode
    /// </summary>
    public void CreateNewArchive(int mapSize, int mapSeed, int spawnSeed, float marshLimit)
    {
        SaveManager.Clear();
        SaveManager.CreateSaveItem();
        HasArchived = true;

        MapInitData = new MapInitData()
        {
            MapSize = mapSize
          , MapGenerationSeed = mapSeed
          , MapObjectRandomSpawnSeed = spawnSeed
          , MarshLimit = marshLimit
        };
        SaveManager.SaveObject(MapInitData);

        MapConfig mapConfig = ConfigManager.Instance.GetConfig<MapConfig>(ConfigName.MAP);
        float mapSizeOnWorld = mapSize * mapConfig.MapChunkSize * mapConfig.CellSize;
        PlayerTransformData = new PlayerTransformData
        {
            Position = new Vector3(mapSizeOnWorld / 2, 0, mapSizeOnWorld / 2)
          , Rotation = Vector3.zero
        };
        SavePlayerTransformData();

        MapData = new MapData();
        SaveMapData();

        InventoryData = new InventoryData(14);

        #region Test

        InventoryData.ItemDatas[0] = ItemData.CreateItemData(0);
        ((Item_MaterialData)InventoryData.ItemDatas[0].ItemTypeData).Count = 5;

        InventoryData.ItemDatas[1] = ItemData.CreateItemData(1);

        InventoryData.ItemDatas[2] = ItemData.CreateItemData(2);
        ((Item_WeaponData)InventoryData.ItemDatas[2].ItemTypeData).Durability = 60;

        InventoryData.ItemDatas[3] = ItemData.CreateItemData(3);
        ((Item_ConsumableData)InventoryData.ItemDatas[3].ItemTypeData).Count = 10;

        #endregion

        SaveInventoryData();
    }

    /// <summary>
    /// Continue the game (load the last saved data)
    /// </summary>
    public void LoadCurrentArchive()
    {
        MapInitData = SaveManager.LoadObject<MapInitData>();
        PlayerTransformData = SaveManager.LoadObject<PlayerTransformData>();
        MapData = SaveManager.LoadObject<MapData>();
        InventoryData = SaveManager.LoadObject<InventoryData>();
    }

    public void SavePlayerTransformData() => SaveManager.SaveObject(PlayerTransformData);

    public void SaveMapData() => SaveManager.SaveObject(MapData);

    /// <summary>
    /// Adds and saves a map chunk data
    /// </summary>
    /// <param name="chunkIndex">chunkIndex</param>
    /// <param name="mapChunkData">mapChunkData</param>
    public void AddAndSaveMapChunkData(Vector2Int chunkIndex, MapChunkData mapChunkData)
    {
        SerializableVector2 index = chunkIndex.Convert2SerializableVector2();
        MapData.MapChunkIndexList.Add(index);
        SaveMapData();
        SaveManager.SaveObject(mapChunkData, "Map_" + index.ToString());
    }

    /// <summary>
    /// Save a map chunk data
    /// </summary>
    /// <param name="chunkIndex">chunkIndex</param>
    /// <param name="mapChunkData">mapChunkData</param>
    public void SaveMapChunkData(Vector2Int chunkIndex, MapChunkData mapChunkData)
    {
        SerializableVector2 index = chunkIndex.Convert2SerializableVector2();
        SaveManager.SaveObject(mapChunkData, "Map_" + index.ToString());
    }

    /// <summary>
    /// Gets a map chunk archive data
    /// </summary>
    /// <param name="chunkIndex"></param>
    /// <returns></returns>
    public MapChunkData GetMapChunkData(SerializableVector2 chunkIndex)
    {
        return SaveManager.LoadObject<MapChunkData>("Map_" + chunkIndex.ToString());
    }

    public void SaveInventoryData()
    {
        SaveManager.SaveObject(InventoryData);
    }
}