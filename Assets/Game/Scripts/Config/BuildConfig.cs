using System.Collections.Generic;
using JKFrame;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "建造合成配置", menuName = "Config/建造合成配置")]
public class BuildConfig : ConfigBase
{
    [LabelText("合成类型")] public BuildType BuildType;
    [LabelText("前置科技（MapObjectID）")] public List<int> PreconditionScienceIDList;
    [LabelText("合成条件列表")] public List<BuildConfigCondition> BuildConfigConditionList = new();
    [LabelText("合成产物（MapObjectID）")] public int TargetID;

    public bool CheckBuildConfigCondition()
    {
        foreach (var condition in BuildConfigConditionList)
        {
            int currCount = InventoryManager.Instance.GetMainInventoryItemCount(condition.ItemID);

            // 检查当前数量是否满足这个条件
            if (currCount < condition.Count) return false;
        }
        return true;
    }
}

public class BuildConfigCondition
{
    [LabelText("物品ID（ItemID）"), HorizontalGroup] public int ItemID;
    [LabelText("物品数量"), HorizontalGroup] public int Count;
}