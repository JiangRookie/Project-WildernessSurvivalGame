using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project_WildernessSurvivalGame
{
    /// <summary>
    /// 地图初始化数据
    /// </summary>
    [Serializable]
    public class MapInitData
    {
        public int MapSize;
        public int MapGenerationSeed;
        public int MapObjectRandomSpawnSeed;
        public float MarshLimit;
    }

    /// <summary>
    /// 地图数据
    /// </summary>
    [Serializable]
    public class MapData
    {
        [Tooltip("当前地图对象ID取值")] public ulong CurrentID = 1;
        [Tooltip("当前玩家去过的所有地图块（已经生成过的地图块）")] public List<SerializableVector2> MapChunkIndexList = new List<SerializableVector2>();
    }

    /// <summary>
    /// 地图块数据
    /// </summary>
    [Serializable]
    public class MapChunkData
    {
        [Tooltip("地图块中的各种地图对象组合成的列表")]
        public SerializableDictionary<ulong, MapObjectData> MapObjectDict = new SerializableDictionary<ulong, MapObjectData>();
    }

    /// <summary>
    /// 地图块地图对象数据
    /// </summary>
    [Serializable]
    public class MapObjectData
    {
        public ulong ID; // 唯一身份标识
        public int ConfigID;
        public int DestroyDays; // 剩余的腐烂天数，-1代表无效
        SerializableVector3 m_Position;

        public Vector3 Position
        {
            get => m_Position.Convert2Vector3();
            set => m_Position = value.Convert2SerializableVector3();
        }
    }
}