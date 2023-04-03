using JKFrame;
using Project_WildernessSurvivalGame;
using UnityEngine;

[UIElement(true, "UI/UI_InventoryWindow", 1)]
public class UI_InventoryWindow : UI_WindowBase
{
    public static UI_InventoryWindow Instance;

    [SerializeField] UI_ItemSlot[] m_Slots;
    [SerializeField] UI_ItemSlot m_WeaponSlot;
    InventoryData m_InventoryData;
    public Sprite[] InventoryFrames;

    void Update()
    {
        #region Test

        if (Input.GetKeyDown(KeyCode.Alpha0)) AddItemAndPlayAudio(0);
        if (Input.GetKeyDown(KeyCode.Alpha1)) AddItemAndPlayAudio(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) AddItemAndPlayAudio(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) AddItemAndPlayAudio(3);

        #endregion
    }

    public override void Init()
    {
        Instance = this;

        // 由于目前数据是由这个窗口处理的，所以这个窗口不能销毁，即使关闭，也要持续监听事件
        EventManager.AddEventListener(EventName.PlayerWeaponAttackSucceed, OnPlayerWeaponAttackSucceed);

        // 确定存档数据
        m_InventoryData = ArchiveManager.Instance.InventoryData;

        // 基于存档去初始化所有的格子
        for (int i = 0; i < m_Slots.Length; i++)
        {
            m_Slots[i].Init(i, this);
        }
        m_WeaponSlot.Init(m_Slots.Length, this);
        UI_ItemSlot.WeaponSlot = m_WeaponSlot;

        // 初始化玩家手中的武器
        PlayerController.Instance.ChangeWeapon(m_InventoryData.WeaponSlotItemData);
    }

    public override void OnShow()
    {
        base.OnShow();

        // 根据存档复原
        InitData(m_InventoryData);
    }

    /// <summary>
    /// 当玩家使用武器攻击成功后
    /// </summary>
    void OnPlayerWeaponAttackSucceed()
    {
        if (m_InventoryData.WeaponSlotItemData == null) return;
        Item_WeaponData weaponData = m_InventoryData.WeaponSlotItemData.ItemTypeData as Item_WeaponData;
        Item_WeaponInfo weaponInfo = m_InventoryData.WeaponSlotItemData.Config.ItemTypeInfo as Item_WeaponInfo;
        weaponData.Durability -= weaponInfo.AttackDurabilityCost;
        if (weaponData.Durability <= 0)
        {
            // 武器损坏
            m_InventoryData.RemoveWeaponItem();

            // 武器槽去掉这个武器
            m_WeaponSlot.InitData();

            // 通知玩家卸掉武器
            PlayerController.Instance.ChangeWeapon(null);
        }
        else
        {
            // 更新耐久度UI
            m_WeaponSlot.UpdateCountTextView();
        }
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

    public bool AddItemAndPlayAudio(int itemConfigID)
    {
        bool result = AddItem(itemConfigID);
        if (result)
        {
            ProjectTool.PlayAudio(AudioType.Bag);
        }
        else
        {
            ProjectTool.PlayAudio(AudioType.Fail);
        }
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

    bool CheckAndAddItemForEmptySlot(int itemConfigID)
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

    public void DiscardItem(int index)
    {
        if (index == m_Slots.Length)
        {
            RemoveItem(index);
            return;
        }
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

    void RemoveItem(int index)
    {
        // Weapon
        if (index == m_InventoryData.ItemDatas.Length)
        {
            m_InventoryData.RemoveWeaponItem();
            m_WeaponSlot.InitData();
        }
        else
        {
            m_InventoryData.RemoveItem(index);
            m_Slots[index].InitData();
        }
    }

    public void SetItem(int index, ItemData itemData)
    {
        if (index == m_InventoryData.ItemDatas.Length)
        {
            m_InventoryData.SetWeaponItem(itemData);
            m_WeaponSlot.InitData(itemData);

            // 将武器数据同步给玩家
            PlayerController.Instance.ChangeWeapon(itemData);
        }
        else
        {
            m_InventoryData.SetItem(index, itemData);
            m_Slots[index].InitData(itemData);
        }
    }

    public AudioType UseItem(int index)
    {
        // 玩家的状态也许并不能使用物品
        if (PlayerController.Instance.CanUseItem == false) return AudioType.PlayerCannotUse;

        // 卸下武器
        if (index == m_Slots.Length)
        {
            int emptySlotIndex = GetEmptySlotIndex();
            if (emptySlotIndex > 0)
            {
                // 武器和空格子进行交换
                UI_ItemSlot.SwapSlotItem(m_WeaponSlot, m_Slots[emptySlotIndex]);
                return AudioType.TakeDownWeapon;
            }

            // 没有空格子
            return AudioType.Fail;
        }

        ItemData itemData = m_Slots[index].ItemData;
        switch (itemData.Config.ItemType)
        {
            case ItemType.Weapon:
                // 装备武器
                UI_ItemSlot.SwapSlotItem(m_Slots[index], m_WeaponSlot);
                return AudioType.TakeUpWeapon;
            case ItemType.Consumable:
                Item_ConsumableInfo info = itemData.Config.ItemTypeInfo as Item_ConsumableInfo;
                if (info.RecoverHp != 0) PlayerController.Instance.RecoverHp(info.RecoverHp);
                if (info.RecoverHungry != 0) PlayerController.Instance.RecoverHungry(info.RecoverHungry);

                // 更新物品的数量
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
                return AudioType.UseConsumablesSuccess;
            default: return AudioType.Fail;
        }
    }
}