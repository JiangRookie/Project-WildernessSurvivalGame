using System;
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

public enum WeaponType
{
    [LabelText("斧头")] Axe
  , [LabelText("镐")] PickAxe
  , [LabelText("镰刀")] Sickle
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

    [LabelText("地图物品ID")] public int MapObjectConfigID;
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
[Serializable]
public class Item_WeaponInfo : IItemTypeInfo
{
    [LabelText("武器类型")] public WeaponType WeaponType;
    [LabelText("玩家手里的预制体")] public GameObject PrefabOnPlayer;
    [LabelText("武器坐标")] public Vector3 PositionOnPlayer;
    [LabelText("武器旋转")] public Vector3 RotationOnPlayer;
    [LabelText("世界地图上的预制体")] public GameObject PrefabOnWorld;
    [LabelText("动画控制器")] public AnimatorOverrideController AnimatorOverrideController;
    [LabelText("攻击力")] public float AttackValue;
    [LabelText("攻击损耗")] public float AttackDurabilityCost;
    [LabelText("攻击距离")] public float AttackDistance;
    [LabelText("攻击音效")] public AudioClip AttackAudio;
    [LabelText("命中音效")] public AudioClip HitAudio;
    [LabelText("命中效果")] public GameObject HitEffect;
}

/// <summary>
/// 可堆叠的物品类型数据基类
/// </summary>
[Serializable]
public abstract class PileItemTypeInfoBase
{
    [LabelText("堆积上限")] public int MaxCount;
}

/// <summary>
/// 消耗品类型信息
/// </summary>
[Serializable]
public class Item_ConsumableInfo : PileItemTypeInfoBase, IItemTypeInfo
{
    [LabelText("恢复生命值")] public float RecoverHp;
    [LabelText("恢复饥饿值")] public float RecoverHungry;
}

/// <summary>
/// 材料类型信息
/// </summary>
[Serializable]
public class Item_MaterialInfo : PileItemTypeInfoBase, IItemTypeInfo { }