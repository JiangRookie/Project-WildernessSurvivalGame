using JKFrame;
using UnityEngine;

[UIElement(false, "UI/UI_MainInventoryWindow", 1)]
public class UI_MainInventoryWindow : UI_InventoryWindowBase
{
    [SerializeField] UI_ItemSlot m_WeaponSlot;
    MainInventoryData m_MainInventoryData;

    void Update()
    {
        #region Test

        if (Input.GetKeyDown(KeyCode.Alpha1)) AddItemAndPlayAudio(0); // 木头
        if (Input.GetKeyDown(KeyCode.Alpha2)) AddItemAndPlayAudio(1); // 石头
        if (Input.GetKeyDown(KeyCode.Alpha3)) AddItemAndPlayAudio(7); // 树枝
        if (Input.GetKeyDown(KeyCode.Alpha4)) AddItemAndPlayAudio(2);
        if (Input.GetKeyDown(KeyCode.Alpha5)) AddItemAndPlayAudio(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) AddItemAndPlayAudio(5);

        #endregion
    }

    public override void OnInit()
    {
        // 由于目前数据是由这个窗口处理的，所以这个窗口不能销毁，即使关闭，也要持续监听事件
        EventManager.AddEventListener(EventName.PlayerWeaponAttackSucceed, OnPlayerWeaponAttackSucceed);
    }

    public void InitData()
    {
        // 初始化存档数据
        m_InventoryData = ArchiveManager.Instance.MainInventoryData;
        m_MainInventoryData = (MainInventoryData)m_InventoryData;

        // 初始化格子
        InitSlotData();
        PlayerController.Instance.ChangeWeapon(m_MainInventoryData.WeaponSlotItemData);
    }

    protected override void InitSlotData()
    {
        // 基于存档去初始化所有的格子
        for (int i = 0; i < m_InventoryData.ItemDatas.Length; i++)
        {
            m_Slots[i].Init(i, this, UseItem);
            m_Slots[i].InitData(m_InventoryData.ItemDatas[i]);
        }
        UI_ItemSlot.WeaponSlot = m_WeaponSlot;
        m_WeaponSlot.Init(m_InventoryData.ItemDatas.Length, this, UseItem);
        m_WeaponSlot.InitData(m_MainInventoryData.WeaponSlotItemData);
    }

    /// <summary>
    /// 当玩家使用武器攻击成功后
    /// </summary>
    void OnPlayerWeaponAttackSucceed()
    {
        if (m_MainInventoryData.WeaponSlotItemData == null) return;
        Item_WeaponData weaponData = m_MainInventoryData.WeaponSlotItemData.ItemTypeData as Item_WeaponData;
        Item_WeaponInfo weaponInfo = m_MainInventoryData.WeaponSlotItemData.Config.ItemTypeInfo as Item_WeaponInfo;
        weaponData.Durability -= weaponInfo.AttackDurabilityCost;
        if (weaponData.Durability <= 0)
        {
            // 武器损坏
            m_MainInventoryData.RemoveWeaponItem();

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

    protected override void RemoveItem(int index)
    {
        // Weapon
        if (index == m_InventoryData.ItemDatas.Length)
        {
            m_MainInventoryData.RemoveWeaponItem();
            m_WeaponSlot.InitData();
        }
        else
        {
            base.RemoveItem(index);
        }
    }

    public override void DiscardItem(int index)
    {
        // Weapon
        if (index == m_Slots.Count)
        {
            RemoveItem(index);
        }
        else
        {
            base.DiscardItem(index);
        }
    }

    public override void SetItem(int index, ItemData itemData)
    {
        if (index == m_InventoryData.ItemDatas.Length)
        {
            m_MainInventoryData.SetWeaponItem(itemData);
            m_WeaponSlot.InitData(itemData);

            // 将武器数据同步给玩家
            PlayerController.Instance.ChangeWeapon(itemData);
        }
        else
        {
            base.SetItem(index, itemData);
        }
    }

    AudioType UseItem(int index)
    {
        // 玩家的状态也许并不能使用物品
        if (PlayerController.Instance.CanUseItem == false) return AudioType.PlayerCannotUse;

        // 卸下武器
        if (index == m_Slots.Count)
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

    /// <summary>
    /// 基于建造配置，减少物品
    /// </summary>
    /// <param name="buildConfig"></param>
    public void UpdateItemsForBuild(BuildConfig buildConfig)
    {
        foreach (var buildConfigCondition in buildConfig.BuildConfigConditionList)
        {
            UpdateItemForBuild(buildConfigCondition);
        }
    }

    void UpdateItemForBuild(BuildConfigCondition buildConfigCondition)
    {
        int count = buildConfigCondition.Count;
        for (int i = 0; i < m_InventoryData.ItemDatas.Length; i++)
        {
            ItemData itemData = m_InventoryData.ItemDatas[i];
            if (itemData != null && itemData.ConfigID == buildConfigCondition.ItemID)
            {
                if (itemData.ItemTypeData is PileItemTypeDataBase)
                {
                    PileItemTypeDataBase pileItemTypeData = itemData.ItemTypeData as PileItemTypeDataBase;

                    // 差距：当前格子里面有的数量 - 需要的数量
                    int quantity = pileItemTypeData.Count - count;
                    if (quantity > 0) // 数量超过
                    {
                        pileItemTypeData.Count -= count;
                        m_Slots[i].UpdateCountTextView();
                        return;
                    }

                    // else // 数量不够（刚好）
                    // {
                    //     count -= pileItemTypeData.Count;
                    //     RemoveItem(i);
                    //     if (count == 0) return;
                    // }
                    count -= pileItemTypeData.Count;
                    RemoveItem(i);
                    if (count == 0) return;
                }
                else
                {
                    count -= 1;
                    RemoveItem(i);
                    if (count == 0) return;
                }
            }
        }

        // 如果执行到这里说明出现了Bug
        Debug.LogError("背包：建造配置内的条件，背包不满足");
    }
}