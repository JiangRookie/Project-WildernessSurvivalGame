using System;

[Serializable] public class MapData { }

/// <summary>
/// 地图初始化数据
/// </summary>
[Serializable]
public class MapInitData
{
    public int MapSize;
    public int MapGenerationSeed;
    public int MapObjectRandomSpawnSeed;
    public int MarshLimit;
}