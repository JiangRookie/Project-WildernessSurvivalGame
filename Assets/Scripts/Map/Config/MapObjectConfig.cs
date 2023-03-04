﻿using JKFrame;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "地图物品配置", menuName = "Config/地图物品配置")]
public class MapObjectConfig : ConfigBase
{
    [LabelText("空的 不生成物品")] public bool IsEmpty = false;
    [LabelText("所在的地图顶点类型")] public MapVertexType MapVertexType;
    [LabelText("生成的预制体")] public GameObject Prefab;
    [LabelText("生成概率 权重类型")] public int Probability;
}