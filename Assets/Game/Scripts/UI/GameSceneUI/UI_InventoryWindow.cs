using JKFrame;
using UnityEngine;

[UIElement(false, "UI/UI_InventoryWindow", 1)]
public class UI_InventoryWindow : UI_WindowBase
{
    [SerializeField] UI_ItemSlot[] m_Slots;
    [SerializeField] UI_ItemSlot m_WeaponSlot;
    InventoryData m_InventoryData;
    public Sprite[] InventoryFrames;

    public override void Init()
    {
        // 确定存档数据
        m_InventoryData = ArchiveManager.Instance.InventoryData;

        // 基于存档去初始化所有的格子
        for (int i = 0; i < m_Slots.Length; i++)
        {
            m_Slots[i].Init(i, this);
        }
        m_WeaponSlot.Init(m_Slots.Length, this);
        UI_ItemSlot.WeaponSlot = m_WeaponSlot;
    }

    public override void OnShow()
    {
        base.OnShow();

        // 根据存档复原
        InitData(m_InventoryData);
    }

    void InitData(InventoryData inventoryData)
    {
        // 基于存档去初始化所有的格子
        for (int i = 0; i < inventoryData.ItemDatas.Length; i++)
        {
            m_Slots[i].Init(i, this);
            m_Slots[i].InitData(inventoryData.ItemDatas[i]);
        }
        m_WeaponSlot.InitData(inventoryData.WeaponSlotItemData);
    }

    public void AddItem() { }

    public void RemoveItem(int index)
    {
        // Weapon
        if (index == m_InventoryData.ItemDatas.Length)
        {
            m_InventoryData.RemoveWeaponItem();
        }
        else
        {
            m_InventoryData.RemoveItem(index);
        }
    }

    public void SetItem(int index, ItemData itemData)
    {
        if (index == m_InventoryData.ItemDatas.Length)
        {
            m_InventoryData.SetWeaponItem(itemData);
        }
        else
        {
            m_InventoryData.SetItem(index, itemData);
        }
    }
}