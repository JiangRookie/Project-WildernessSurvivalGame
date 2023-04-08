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
    public PlayerCoreData PlayerCoreData { get; private set; }
    public MapInitData MapInitData { get; private set; }
    public MapData MapData { get; private set; }
    public SerializableDictionary<ulong, IMapObjectTypeData> MapObjectTypeDataDict { get; private set; }
    public InventoryData InventoryData { get; private set; }
    public TimeData TimeData { get; private set; }
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

        PlayerConfig playerConfig = ConfigManager.Instance.GetConfig<PlayerConfig>(ConfigName.PLAYER);
        PlayerCoreData = new PlayerCoreData
        {
            Hp = playerConfig.MaxHp
          , Hungry = playerConfig.MaxHungry
        };
        SavePlayerCoreData();

        MapData = new MapData();
        MapObjectTypeDataDict = new SerializableDictionary<ulong, IMapObjectTypeData>();
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

        InventoryData.ItemDatas[4] = ItemData.CreateItemData(4);
        InventoryData.ItemDatas[5] = ItemData.CreateItemData(5);

        #endregion

        SaveInventoryData();

        TimeConfig timeConfig = ConfigManager.Instance.GetConfig<TimeConfig>(ConfigName.TIME);
        TimeData = new TimeData
        {
            StateIndex = 0 // 第一个阶段默认是早上
          , CalculateTime = timeConfig.TimeStateConfigs[0].PhaseDurationTime
          , DayNum = 0
        };
        SaveTimeData();
    }

    /// <summary>
    /// Continue the game (load the last saved data)
    /// </summary>
    public void LoadCurrentArchive()
    {
        MapInitData = SaveManager.LoadObject<MapInitData>();
        PlayerTransformData = SaveManager.LoadObject<PlayerTransformData>();
        MapData = SaveManager.LoadObject<MapData>();
        MapObjectTypeDataDict = SaveManager.LoadObject<SerializableDictionary<ulong, IMapObjectTypeData>>();
        InventoryData = SaveManager.LoadObject<InventoryData>();
        TimeData = SaveManager.LoadObject<TimeData>();
        PlayerCoreData = SaveManager.LoadObject<PlayerCoreData>();
    }

    public void SavePlayerTransformData() => SaveManager.SaveObject(PlayerTransformData);

    public void SavePlayerCoreData() => SaveManager.SaveObject(PlayerCoreData);

    public void SaveMapData()
    {
        SaveMapObjectTypeData();
        SaveManager.SaveObject(MapData);
    }

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

    public void SaveTimeData()
    {
        SaveManager.SaveObject(TimeData);
    }

    public IMapObjectTypeData GetMapObjectTypeData(ulong id)
    {
        return MapObjectTypeDataDict.Dictionary[id];
    }

    public bool TryGetMapObjectTypeData(ulong id, out IMapObjectTypeData mapObjectTypeData)
    {
        return MapObjectTypeDataDict.Dictionary.TryGetValue(id, out mapObjectTypeData);
    }

    public void AddMapObjectTypeData(ulong mapObjectID, IMapObjectTypeData mapObjectTypeData)
    {
        MapObjectTypeDataDict.Dictionary.Add(mapObjectID, mapObjectTypeData);
    }

    public void RemoveMapObjectTypeData(ulong mapObjectID)
    {
        MapObjectTypeDataDict.Dictionary.Remove(mapObjectID);
    }

    public void SaveMapObjectTypeData()
    {
        SaveManager.SaveObject(MapObjectTypeDataDict);
    }
}