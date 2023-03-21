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
        public List<SerializableVector2> MapChunkIndexList; // 当前玩家去过的所有地图块（已经生成过的地图块）
    }

    /// <summary>
    /// 地图块数据
    /// </summary>
    [Serializable]
    public class MapChunkData
    {
        /// <summary>
        /// 地图块中的各种地图对象组合成的列表
        /// </summary>
        public List<MapChunkMapObjectData> MapObjectList = new();
    }

    /// <summary>
    /// 地图块地图对象数据
    /// </summary>
    [Serializable]
    public class MapChunkMapObjectData
    {
        public int ConfigID;
        SerializableVector3 m_Position;

        public Vector3 Position
        {
            get => m_Position.Convert2Vector3();
            set => m_Position = value.Convert2SerializableVector3();
        }
    }
}