using System;
using JKFrame;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 物品栏中的一个格子
/// </summary>
[Pool]
public class UI_ItemSlot : MonoBehaviour
{
    public static UI_ItemSlot CurrentMouseEnterSlot; // 当前鼠标进入的格子
    public static UI_ItemSlot WeaponSlot;
    [SerializeField] Image m_BgImage;
    [SerializeField] Image m_IconImage;
    [SerializeField] Text m_CountText;
    Transform m_IconTransform;
    UI_InventoryWindowBase m_OwnerWindow;
    Transform m_SlotTransform;
    public ItemData ItemData { get; private set; }
    public int Index { get; private set; }
    Func<int, AudioType> m_OnUseAction;

    void Start()
    {
        m_IconTransform = m_IconImage.transform;
        m_SlotTransform = transform;

        // 鼠标交互事件
        this.OnMouseEnter(MouseEnter);
        this.OnMouseExit(MouseExit);
        this.OnBeginDrag(BeginDrag);
        this.OnDrag(Drag);
        this.OnEndDrag(EndDrag);
        this.BindMouseEffect();
    }

    void OnEnable()
    {
        this.OnUpdate(CheckMouseRightClick);
    }

    void OnDisable()
    {
        this.RemoveUpdate(CheckMouseRightClick);
    }

    void CheckMouseRightClick()
    {
        if (ItemData == null || m_OnUseAction == null) return;
        if (m_IsSelect && Input.GetMouseButtonDown(1))
        {
            // 根据使用的情况来播放音效
            AudioType resultAudioType = m_OnUseAction.Invoke(Index);
            ProjectTool.PlayAudio(resultAudioType);
        }
    }

    public void Init(int index, UI_InventoryWindowBase ownerWindow, Func<int, AudioType> onUseAction = null)
    {
        Index = index;
        m_OwnerWindow = ownerWindow;
        m_OnUseAction = onUseAction;
        m_BgImage.sprite = m_OwnerWindow.InventoryFrames[0];
    }

    public void InitData(ItemData itemData = null)
    {
        ItemData = itemData;
        if (ItemData == null)
        {
            m_BgImage.color = Color.white;
            m_CountText.gameObject.SetActive(false);
            m_IconImage.sprite = null;
            m_IconImage.gameObject.SetActive(false);
            return;
        }
        m_CountText.gameObject.SetActive(true);
        m_IconImage.gameObject.SetActive(true);
        m_IconImage.sprite = ItemData.Config.Icon;

        UpdateCountTextView();
    }

    public void UpdateCountTextView()
    {
        switch (ItemData.Config.ItemType)
        {
            case ItemType.Weapon:
                m_BgImage.color = Color.white;
                m_CountText.text = ((Item_WeaponData)ItemData.ItemTypeData).Durability.ToString() + "%";
                break;
            case ItemType.Consumable:
                m_BgImage.color = new Color(0, 1, 0, 0.5f);
                m_CountText.text = ((Item_ConsumableData)ItemData.ItemTypeData).Count.ToString();
                break;
            case ItemType.Material:
                m_BgImage.color = Color.white;
                m_CountText.text = ((Item_MaterialData)ItemData.ItemTypeData).Count.ToString();
                break;
        }
    }

    #region 鼠标交互

    bool m_IsSelect;

    void MouseEnter(PointerEventData eventData, object[] arg2)
    {
        GameManager.Instance.SetCursorStyle(CursorStyle.Handle);
        m_BgImage.sprite = m_OwnerWindow.InventoryFrames[1];
        m_IsSelect = true;
        CurrentMouseEnterSlot = this;
    }

    void MouseExit(PointerEventData eventData, object[] arg2)
    {
        GameManager.Instance.SetCursorStyle(CursorStyle.Normal);
        m_BgImage.sprite = m_OwnerWindow.InventoryFrames[0];
        m_IsSelect = false;
        CurrentMouseEnterSlot = null;
    }

    void BeginDrag(PointerEventData eventData, object[] arg2)
    {
        if (ItemData == null) return;
        m_IconTransform.SetParent(UIManager.Instance.DragLayer);
    }

    void Drag(PointerEventData eventData, object[] arg2)
    {
        if (ItemData == null) return;
        GameManager.Instance.SetCursorStyle(CursorStyle.Handle);
        m_IconTransform.position = eventData.position;
    }

    void EndDrag(PointerEventData eventData, object[] arg2)
    {
        if (ItemData == null) return;

        // 格子和建筑物的交互
        if (m_OnUseAction != null && InputManager.Instance.CheckSlotEndDragOnBuilding(ItemData.ConfigID))
        {
            ResetIcon();
            ProjectTool.PlayAudio(AudioType.Bag);
            m_OwnerWindow.DiscardItem(Index);
            return;
        }

        if (CurrentMouseEnterSlot == null) GameManager.Instance.SetCursorStyle(CursorStyle.Normal);

        // 当前拖拽中的Icon归位
        ResetIcon();

        // 如果当前鼠标进入的格子是自己，不执行任何操作
        if (CurrentMouseEnterSlot == this) return;

        // 如果当前鼠标进入的格子为空（扔东西）
        if (CurrentMouseEnterSlot == null)
        {
            // 射线检测除了Mast外是否有其他UI物体
            if (InputManager.Instance.CheckMouseOnUI()) return;
            if (InputManager.Instance.CheckMouseOnBigMapObject()) return;

            // 射线去检测防止玩家将物品扔到大型物体身上
            if (InputManager.Instance.GetMouseWorldPosOnGround(eventData.position, out Vector3 mouseWorldPos))
            {
                // 在地面生成物品
                mouseWorldPos.y = 1;
                MapManager.Instance.SpawnMapObject(ItemData.Config.MapObjectConfigID, mouseWorldPos, false);

                // 丢弃一件物品
                m_OwnerWindow.DiscardItem(Index);
                ProjectTool.PlayAudio(AudioType.Bag);
            }

            return;
        }

        // 如果当前格子是武器格子，
        if (this == WeaponSlot)
        {
            // 但是目标格子是空的，则播放卸下武器的音效
            if (CurrentMouseEnterSlot.ItemData == null)
            {
                ProjectTool.PlayAudio(AudioType.TakeDownWeapon);
                SwapSlotItem(this, CurrentMouseEnterSlot);
            }

            // 目标格子是武器格子
            else if (CurrentMouseEnterSlot.ItemData.Config.ItemType == ItemType.Weapon)
            {
                ProjectTool.PlayAudio(AudioType.TakeUpWeapon);
                SwapSlotItem(this, CurrentMouseEnterSlot);
            }

            // 目标格子不是武器格子
            else
            {
                ProjectTool.PlayAudio(AudioType.Fail);
                UIManager.Instance.AddTips("必须得是装备才行！");
            }
        }
        else // 当前是普通格子
        {
            // 目标格子是武器格子（装备武器）
            if (CurrentMouseEnterSlot == WeaponSlot)
            {
                if (ItemData.Config.ItemType != ItemType.Weapon)
                {
                    ProjectTool.PlayAudio(AudioType.Fail);
                    UIManager.Instance.AddTips("必须得是装备才行！");
                }
                else
                {
                    ProjectTool.PlayAudio(AudioType.TakeUpWeapon);
                    Debug.Log("可以装备物品：" + ItemData.Config.Name);
                    SwapSlotItem(this, CurrentMouseEnterSlot);
                }
            }
            else
            {
                SwapSlotItem(this, CurrentMouseEnterSlot);
                ProjectTool.PlayAudio(AudioType.Bag);
            }
        }
    }

    void ResetIcon()
    {
        // 格子归位
        m_IconTransform.SetParent(m_SlotTransform);
        m_IconTransform.localPosition = Vector3.zero;
        m_IconTransform.localScale = Vector3.one;
    }

    public static void SwapSlotItem(UI_ItemSlot currSlot, UI_ItemSlot targetSlot)
    {
        ItemData currData = currSlot.ItemData;
        ItemData targetData = targetSlot.ItemData;
        currSlot.m_OwnerWindow.SetItem(currSlot.Index, targetData);
        targetSlot.m_OwnerWindow.SetItem(targetSlot.Index, currData);
    }

    #endregion
}