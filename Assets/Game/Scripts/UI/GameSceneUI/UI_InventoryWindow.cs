using JKFrame;
using UnityEngine;

[UIElement(false, "UI/UI_InventoryWindow", 1)]
public class UI_InventoryWindow : UI_WindowBase
{
    [SerializeField] UI_ItemSlot[] m_Slots;
    [SerializeField] UI_ItemSlot m_WeaponSlot;
    InventoryData m_InventoryData;
    public Sprite[] InventoryFrames;

    void Update()
    {
        #region Test

        if (Input.GetKeyDown(KeyCode.Alpha0)) AddItem(0);
        if (Input.GetKeyDown(KeyCode.Alpha1)) AddItem(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) AddItem(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) AddItem(3);

        #endregion
    }

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

    public void AddItem(int itemConfigID)
    {
        bool result = AddItemForLogic(itemConfigID);
        if (result)
        {
            ProjectTool.PlayAudio(AudioType.Bag);
        }
        else
        {
            ProjectTool.PlayAudio(AudioType.Fail);
        }
    }

    public bool AddItemForLogic(int itemConfigID)
    {
        ItemConfig itemConfig = ConfigManager.Instance.GetConfig<ItemConfig>(ConfigName.ITEM, itemConfigID);
        switch (itemConfig.ItemType)
        {
            case ItemType.Weapon: return CheckAndAddItemForEmptySlot(itemConfigID);
            case ItemType.Consumable:
                // 优先堆叠
                return CheckAndAddPileItemForSlot(itemConfigID) || CheckAndAddItemForEmptySlot(itemConfigID);
            case ItemType.Material:
                // 优先堆叠
                return CheckAndAddPileItemForSlot(itemConfigID) || CheckAndAddItemForEmptySlot(itemConfigID);
        }
        return false;
    }

    bool CheckAndAddItemForEmptySlot(int itemConfigID)
    {
        int index = GetEmptySlotIndex();
        if (index > 0)
        {
            SetItem(index, ItemData.CreateItemData(itemConfigID));
            return true;
        }
        return false;
    }

    /// <summary>
    /// 获取空格子索引，如果没有空格子则返回-1
    /// </summary>
    /// <returns></returns>
    int GetEmptySlotIndex()
    {
        for (int i = 0; i < m_Slots.Length; i++)
        {
            if (m_Slots[i].ItemData == null)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// 检测并且堆放物品到格子上
    /// </summary>
    /// <returns></returns>
    bool CheckAndAddPileItemForSlot(int itemConfigID)
    {
        for (int i = 0; i < m_Slots.Length; i++)
        {
            // 不为空 && 是同一个物品 && 没堆满
            if (m_Slots[i].ItemData != null
             && m_Slots[i].ItemData.ConfigID == itemConfigID)
            {
                // 比较的是配置中的最大堆叠数量和当前存档数据的对比
                PileItemTypeDataBase data = m_Slots[i].ItemData.ItemTypeData as PileItemTypeDataBase;
                PileItemTypeInfoBase info = m_Slots[i].ItemData.Config.ItemTypeInfo as PileItemTypeInfoBase;
                if (data.Count < info.MaxCount)
                {
                    data.Count += 1; // 增加一个
                    m_Slots[i].UpdateCountTextView();
                    return true;
                }
            }
        }
        return false;
    }

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
            m_WeaponSlot.InitData(itemData);
        }
        else
        {
            m_InventoryData.SetItem(index, itemData);
            m_Slots[index].InitData(itemData);
        }
    }
}