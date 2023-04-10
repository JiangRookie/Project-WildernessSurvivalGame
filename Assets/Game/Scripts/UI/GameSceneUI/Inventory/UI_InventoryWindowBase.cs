using System.Collections.Generic;
using JKFrame;
using UnityEngine;

[UIElement(true, "UI/UI_InventoryWindow", 1)]
public abstract class UI_InventoryWindowBase : UI_WindowBase
{
    [SerializeField] protected List<UI_ItemSlot> m_Slots;
    public Sprite[] InventoryFrames;
    protected InventoryData m_InventoryData;

    protected virtual void InitSlotData()
    {
        // 基于存档去初始化所有的格子
        for (int i = 0; i < m_InventoryData.ItemDatas.Length; i++)
        {
            m_Slots[i].Init(i, this);
            m_Slots[i].InitData(m_InventoryData.ItemDatas[i]);
        }
    }

    public bool AddItemAndPlayAudio(int itemConfigID)
    {
        bool result = AddItem(itemConfigID);
        ProjectTool.PlayAudio(result ? AudioType.Bag : AudioType.Fail);
        return result;
    }

    public bool AddItem(int itemConfigID)
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

    protected bool CheckAndAddItemForEmptySlot(int itemConfigID)
    {
        int index = GetEmptySlotIndex();
        if (index < 0) return false;
        SetItem(index, ItemData.CreateItemData(itemConfigID));
        return true;
    }

    /// <summary>
    /// 获取空格子索引，如果没有空格子则返回-1
    /// </summary>
    /// <returns></returns>
    protected int GetEmptySlotIndex()
    {
        for (int i = 0; i < m_Slots.Count; i++)
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
    protected bool CheckAndAddPileItemForSlot(int itemConfigID)
    {
        for (int i = 0; i < m_Slots.Count; i++)
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

    public virtual void DiscardItem(int index)
    {
        ItemData itemData = m_Slots[index].ItemData;
        switch (itemData.Config.ItemType)
        {
            case ItemType.Weapon:
                RemoveItem(index);
                break;
            default:
                PileItemTypeDataBase data = itemData.ItemTypeData as PileItemTypeDataBase;
                data.Count -= 1;
                if (data.Count == 0)
                {
                    RemoveItem(index);
                }
                else
                {
                    m_Slots[index].UpdateCountTextView();
                }
                break;
        }
    }

    protected virtual void RemoveItem(int index)
    {
        m_InventoryData.RemoveItem(index);
        m_Slots[index].InitData();
    }

    public virtual void SetItem(int index, ItemData itemData)
    {
        m_InventoryData.SetItem(index, itemData);
        m_Slots[index].InitData(itemData);
    }

    /// <summary>
    /// 获取某个物品的数量
    /// </summary>
    /// <param name="configID"></param>
    /// <returns></returns>
    public int GetItemCount(int configID)
    {
        int count = 0;
        foreach (var itemData in m_InventoryData.ItemDatas)
        {
            if (itemData != null && itemData.ConfigID == configID)
            {
                if (itemData.ItemTypeData is PileItemTypeDataBase)
                {
                    count += ((PileItemTypeDataBase)itemData.ItemTypeData).Count;
                }
                else
                {
                    count += 1;
                }
            }
        }

        return count;
    }
}