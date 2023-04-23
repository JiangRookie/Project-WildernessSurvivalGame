using System;

/// <summary>
/// 通用背包数据
/// </summary>
[Serializable]
public class InventoryData
{
    public InventoryData(int itemCount) => ItemDatas = new ItemData[itemCount];

    // 格子里面装的物品
    public ItemData[] ItemDatas { get; protected set; }

    public void RemoveItem(int index) => ItemDatas[index] = null;

    public void SetItem(int index, ItemData itemData) => ItemDatas[index] = itemData;
}

/// <summary>
/// 物品栏数据
/// </summary>
[Serializable]
public class MainInventoryData : InventoryData
{
    public MainInventoryData(int itemCount) : base(itemCount) { }

    // 武器格子装的物品
    public ItemData WeaponSlotItemData { get; private set; }

    public void RemoveWeaponItem() => WeaponSlotItemData = null;

    public void SetWeaponItem(ItemData itemData) => WeaponSlotItemData = itemData;
}