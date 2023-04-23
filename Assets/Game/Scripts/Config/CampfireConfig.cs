using System.Collections.Generic;
using JKFrame;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "篝火配置", menuName = "Config/篝火配置")]
public class CampfireConfig : ConfigBase
{
    [LabelText("默认燃料数值")] public float DefaultFuelValue;
    [LabelText("上限燃料数值")] public float MaxFuelValue;
    [LabelText("燃烧速度(每秒消耗)")] public float BurningSpeed;
    [LabelText("最大灯光亮度")] public float MaxLightIntensity;
    [LabelText("最大灯光范围")] public float MaxLightRange;

    [LabelText("燃料和物品对照表 (Key: ItemID Value: FuelValue)")]
    public Dictionary<int, float> ItemFuelDict;

    [LabelText("烧烤和物品对照表 (Key: ItemID Value: ItemID)")]
    public Dictionary<int, int> BakedDict;

    public bool TryGetFuelValueByItemID(int itemID, out float fuelValue) => ItemFuelDict.TryGetValue(itemID, out fuelValue);
    public bool TryGetBakedValueByItemID(int itemID, out int bakedItemID) => BakedDict.TryGetValue(itemID, out bakedItemID);
}