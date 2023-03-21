using System;

/// <summary>
/// 物品栏数据
/// </summary>
[Serializable]
public class InventoryData
{
    // 格子里面装的物品
    public ItemData[] ItemDatas { get; private set; }

    // 武器格子装的物品
    public ItemData WeaponSlotItemData { get; private set; }

    public InventoryData(int itemCount)
    {
        ItemDatas = new ItemData[itemCount];
    }

    public void RemoveItem(int index)
    {
        ItemDatas[index] = null;
    }

    public void SetItem(int index, ItemData itemData)
    {
        ItemDatas[index] = itemData;
    }

    public void RemoveWeaponItem()
    {
        WeaponSlotItemData = null;
    }

    public void SetWeaponItem(ItemData itemData)
    {
        WeaponSlotItemData = itemData;
    }
}