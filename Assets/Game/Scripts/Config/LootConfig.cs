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

    public void GenerateMapObject(MapChunkController mapChunk, Vector3 position)
    {
        foreach (var lootConfigModel in LootConfigList)
        {
            int randomValue = Random.Range(1, 101);
            if (randomValue < lootConfigModel.Probability)
            {
                // 生成掉落物品
                MapManager.Instance.SpawnMapObject(mapChunk, lootConfigModel.LootObjectConfigID, position, false);
            }
        }
    }
}

public class LootConfigModel
{
    [LabelText("掉落物品ID")] public int LootObjectConfigID;
    [LabelText("掉落概率 %")] public int Probability;
}