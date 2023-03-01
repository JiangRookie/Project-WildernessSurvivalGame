using System.Collections.Generic;
using JKFrame;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 地图物品的生成配置
/// </summary>
[CreateAssetMenu(fileName = "场景物品生成配置", menuName = "Config/场景物品")]
public class MapObjectSpawnConfig : ConfigBase
{
    public Dictionary<MapVertexType, List<MapObjectSpawnConfigModel>> SpawnConfigDic = new();
}

public class MapObjectSpawnConfigModel
{
    [LabelText("空的 不生成物品")] public bool IsEmpty = false;
    [LabelText("生成的预制体")] public GameObject Prefab;
    [LabelText("生成概率 百分比类型")] public int Probability;
}