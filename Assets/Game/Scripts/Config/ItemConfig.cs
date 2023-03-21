using JKFrame;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 物品类型
/// </summary>
public enum ItemType
{
    [LabelText("武器")] Weapon
  , [LabelText("消耗品")] Consumable
  , [LabelText("材料")] Material
}

/// <summary>
/// 物品配置（物品静态数据）
/// </summary>
[CreateAssetMenu(fileName = "物品配置", menuName = "Config/物品配置")]
public class ItemConfig : ConfigBase
{
    [LabelText("名称")] public string Name;

    [LabelText("类型"), OnValueChanged(nameof(OnItemTypeChanged))]
    public ItemType ItemType;

    [LabelText("描述"), MultiLineProperty] public string Description;
    [LabelText("图标")] public Sprite Icon;
    [LabelText("类型专属信息")] public IItemTypeInfo ItemTypeInfo;

    /// <summary>
    /// 当物品类型被修改时自动生成同等类型应有的专属类型
    /// </summary>
    void OnItemTypeChanged()
    {
        switch (ItemType)
        {
            case ItemType.Weapon:
                ItemTypeInfo = new Item_WeaponInfo();
                break;
            case ItemType.Consumable:
                ItemTypeInfo = new Item_ConsumableInfo();
                break;
            case ItemType.Material:
                ItemTypeInfo = new Item_MaterialInfo();
                break;
        }
    }
}

/// <summary>
/// 物品类型信息接口
/// </summary>
public interface IItemTypeInfo { }

/// <summary>
/// 武器类型信息
/// </summary>
public class Item_WeaponInfo : IItemTypeInfo
{
    [LabelText("攻击力")] public float AttackValue;
}

/// <summary>
/// 消耗品类型信息
/// </summary>
public class Item_ConsumableInfo : IItemTypeInfo
{
    [LabelText("堆积上限")] public float MaxCount;
}

/// <summary>
/// 材料类型信息
/// </summary>
public class Item_MaterialInfo : IItemTypeInfo
{
    [LabelText("堆积上限")] public float MaxCount;
}