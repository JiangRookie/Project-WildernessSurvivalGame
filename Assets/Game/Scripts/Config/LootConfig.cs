using System.Collections.Generic;
using JKFrame;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 战利品配置
/// </summary>
[CreateAssetMenu(fileName = "掉落配置", menuName = "Config/掉落配置")]
public class LootConfig : ConfigBase
{
    [LabelText("掉落配置列表")] public List<LootConfigModel> LootConfigList;
}

public class LootConfigModel
{
    [LabelText("掉落物品ID")] public int LootObjectConfigID;
    [LabelText("掉落概率 %")] public int Probability;
}