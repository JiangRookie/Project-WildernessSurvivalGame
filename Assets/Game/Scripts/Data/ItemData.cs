using System;
using JKFrame;

/// <summary>
/// 物品的动态数据（用于存档）
/// </summary>
[Serializable]
public class ItemData
{
    public int ConfigID;
    public IItemTypeData ItemTypeData;

    public ItemConfig Config => ConfigManager.Instance.GetConfig<ItemConfig>(ConfigName.ITEM, ConfigID);

    public static ItemData CreateItemData(int configID)
    {
        ItemData itemData = new ItemData();
        itemData.ConfigID = configID;

        // 根据物品的实际类型来创建符合类型的动态数据
        switch (itemData.Config.ItemType)
        {
            case ItemType.Weapon:
                itemData.ItemTypeData = new Item_WeaponData { Durability = 100 };
                break;
            case ItemType.Consumable:
                itemData.ItemTypeData = new Item_ConsumableData { Count = 1 };
                break;
            case ItemType.Material:
                itemData.ItemTypeData = new Item_MaterialData { Count = 1 };
                break;
        }
        return itemData;
    }
}

/// <summary>
/// 物品类型数据的接口
/// </summary>
public interface IItemTypeData { }

[Serializable]
public class Item_WeaponData : IItemTypeData
{
    public float Durability = 100;
}

[Serializable]
public abstract class PileItemTypeDataBase
{
    public int Count;
}

[Serializable] public class Item_ConsumableData : PileItemTypeDataBase, IItemTypeData { }

[Serializable] public class Item_MaterialData : PileItemTypeDataBase, IItemTypeData { }