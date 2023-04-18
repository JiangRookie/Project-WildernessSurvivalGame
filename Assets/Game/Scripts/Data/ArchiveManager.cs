using JKFrame;
using UnityEngine;

/// <summary>
/// 数据存档管理器
/// </summary>
public class ArchiveManager : Singleton<ArchiveManager>
{
    public ArchiveManager() => LoadArchiveData();
    public bool HasArchived { get; private set; }

    #region Archive Data

    public PlayerTransformData PlayerTransformData { get; private set; }
    public PlayerCoreData PlayerCoreData { get; private set; }
    public MapInitData MapInitData { get; private set; }
    public MapData MapData { get; private set; }
    public SerializableDictionary<ulong, IMapObjectTypeData> MapObjectTypeDataDict { get; private set; }
    public MainInventoryData MainInventoryData { get; private set; }
    public TimeData TimeData { get; private set; }
    public ScienceData ScienceData { get; private set; }

    #endregion

    #region Archive operation

    void LoadArchiveData()
    {
        // 单存档机制，所以是获取第0个存档数据
        SaveItem saveItem = SaveManager.GetSaveItem(0);
        HasArchived = saveItem != null;
    }

    public void ClearArchive()
    {
        SaveManager.Clear();
        LoadArchiveData();
    }

    public void CreateNewArchive(int mapSize, int mapSeed, int spawnSeed, float marshLimit)
    {
        SaveManager.Clear();          // 删除存档
        SaveManager.CreateSaveItem(); // 创建新存档
        HasArchived = true;

        // 初始化地图数据
        MapInitData = new MapInitData()
        {
            MapSize = mapSize
          , MapGenerationSeed = mapSeed
          , MapObjectRandomSpawnSeed = spawnSeed
          , MarshLimit = marshLimit
        };
        SaveManager.SaveObject(MapInitData);

        // 初始化玩家位置数据
        var mapConfig = ConfigManager.Instance.GetConfig<MapConfig>(ConfigName.Map);
        var mapSizeOnWorld = mapSize * mapConfig.MapChunkSize * mapConfig.CellSize;
        PlayerTransformData = new PlayerTransformData
        {
            Position = new Vector3(mapSizeOnWorld / 2, 0, mapSizeOnWorld / 2)
          , Rotation = Vector3.zero
        };
        SavePlayerTransformData();

        // 初始化玩家数据
        var playerConfig = ConfigManager.Instance.GetConfig<PlayerConfig>(ConfigName.Player);
        PlayerCoreData = new PlayerCoreData { Hp = playerConfig.MaxHp, Hungry = playerConfig.MaxHungry };
        SavePlayerCoreData();

        // 初始化地图数据
        MapData = new MapData();
        MapObjectTypeDataDict = new SerializableDictionary<ulong, IMapObjectTypeData>();
        SaveMapData();

        // 初始化玩家快捷栏数据
        MainInventoryData = new MainInventoryData(14);
        SaveMainInventoryData();

        #region Test

        // MainInventoryData.ItemDatas[0] = ItemData.CreateItemData(0);
        // ((Item_MaterialData)MainInventoryData.ItemDatas[0].ItemTypeData).Count = 5;
        //
        // MainInventoryData.ItemDatas[1] = ItemData.CreateItemData(1);
        //
        // MainInventoryData.ItemDatas[2] = ItemData.CreateItemData(2);
        // ((Item_WeaponData)MainInventoryData.ItemDatas[2].ItemTypeData).Durability = 60;
        //
        // MainInventoryData.ItemDatas[3] = ItemData.CreateItemData(3);
        // ((Item_ConsumableData)MainInventoryData.ItemDatas[3].ItemTypeData).Count = 10;
        //
        // MainInventoryData.ItemDatas[4] = ItemData.CreateItemData(4);
        // MainInventoryData.ItemDatas[5] = ItemData.CreateItemData(5);

        // SaveMainInventoryData();

        #endregion

        // 初始化时间数据
        var timeConfig = ConfigManager.Instance.GetConfig<TimeConfig>(ConfigName.Time);
        TimeData = new TimeData
        {
            StateIndex = 0 // 第一个阶段默认是早上
          , CalculateTime = timeConfig.TimeStateConfigs[0].PhaseDurationTime
          , DayNum = 0
        };
        SaveTimeData();

        // 初始化科技数据
        ScienceData = new ScienceData();
        SaveScienceData();
    }

    /// <summary>
    /// Continue the game (load the last saved data)
    /// </summary>
    public void LoadArchive()
    {
        MapInitData = SaveManager.LoadObject<MapInitData>();
        PlayerTransformData = SaveManager.LoadObject<PlayerTransformData>();
        MapData = SaveManager.LoadObject<MapData>();
        MapObjectTypeDataDict = SaveManager.LoadObject<SerializableDictionary<ulong, IMapObjectTypeData>>();
        MainInventoryData = SaveManager.LoadObject<MainInventoryData>();
        TimeData = SaveManager.LoadObject<TimeData>();
        PlayerCoreData = SaveManager.LoadObject<PlayerCoreData>();
        ScienceData = SaveManager.LoadObject<ScienceData>();
    }

    #endregion

    #region Save data

    public void SavePlayerTransformData() => SaveManager.SaveObject(PlayerTransformData);

    public void SavePlayerCoreData() => SaveManager.SaveObject(PlayerCoreData);

    public void SaveMapData()
    {
        SaveMapObjectTypeData();
        SaveManager.SaveObject(MapData);
    }

    public void SaveMainInventoryData() => SaveManager.SaveObject(MainInventoryData);
    public void SaveTimeData() => SaveManager.SaveObject(TimeData);

    public void SaveScienceData() => SaveManager.SaveObject(ScienceData);

    #endregion

    #region Map chunk data

    /// <summary>
    /// Adds and saves a map chunk data
    /// </summary>
    /// <param name="chunkIndex">Chunk index</param>
    /// <param name="mapChunkData">Map chunk data</param>
    public void AddAndSaveMapChunkData(Vector2Int chunkIndex, MapChunkData mapChunkData)
    {
        var index = chunkIndex.Convert2SerializableVector2();
        MapData.MapChunkIndexList.Add(index);
        SaveMapData();
        SaveManager.SaveObject(mapChunkData, "Map_" + index.ToString());
    }

    /// <summary>
    /// Save a map chunk data
    /// </summary>
    /// <param name="chunkIndex">Chunk index</param>
    /// <param name="mapChunkData">Map chunk data</param>
    public void SaveMapChunkData(Vector2Int chunkIndex, MapChunkData mapChunkData)
    {
        var index = chunkIndex.Convert2SerializableVector2();
        SaveManager.SaveObject(mapChunkData, "Map_" + index.ToString());
    }

    /// <summary>
    /// Gets a map chunk archive data
    /// </summary>
    /// <param name="chunkIndex">Chunk index</param>
    /// <returns>Return a map chunk data</returns>
    public MapChunkData GetMapChunkData(SerializableVector2 chunkIndex) => SaveManager.LoadObject<MapChunkData>("Map_" + chunkIndex.ToString());

    #endregion

    #region Map object type data

    void SaveMapObjectTypeData() => SaveManager.SaveObject(MapObjectTypeDataDict);

    public IMapObjectTypeData GetMapObjectTypeData(ulong mapObjectID) => MapObjectTypeDataDict.Dictionary[mapObjectID];

    public bool TryGetMapObjectTypeData
        (ulong mapObjectID, out IMapObjectTypeData mapObjectTypeData) =>
        MapObjectTypeDataDict.Dictionary.TryGetValue(mapObjectID, out mapObjectTypeData);

    public void AddMapObjectTypeData
        (ulong mapObjectID, IMapObjectTypeData mapObjectTypeData) => MapObjectTypeDataDict.Dictionary.Add(mapObjectID, mapObjectTypeData);

    public void RemoveMapObjectTypeData(ulong mapObjectID) => MapObjectTypeDataDict.Dictionary.Remove(mapObjectID);

    #endregion
}