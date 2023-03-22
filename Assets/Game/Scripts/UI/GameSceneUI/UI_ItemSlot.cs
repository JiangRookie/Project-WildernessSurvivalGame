using System.Collections.Generic;
using JKFrame;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 物品栏中的一个格子
/// </summary>
public class UI_ItemSlot : MonoBehaviour
{
    public static UI_ItemSlot CurrentMouseEnterSlot; // 当前鼠标进入的格子
    public static UI_ItemSlot WeaponSlot;
    public static List<RaycastResult> RaycastResultList = new(10);
    [SerializeField] Image m_BgImage;
    [SerializeField] Image m_IconImage;
    [SerializeField] Text m_CountText;
    Transform m_IconTransform;
    Transform m_SlotTransform;
    UI_InventoryWindow m_OwnerWindow; // 宿主窗口：物品栏或仓库
    public ItemData ItemData { get; private set; }
    public int Index { get; private set; }

    void Start()
    {
        m_IconTransform = m_IconImage.transform;
        m_SlotTransform = transform;
        m_BgImage.sprite = m_OwnerWindow.InventoryFrames[0];

        // 鼠标交互事件
        this.OnMouseEnter(MouseEnter);
        this.OnMouseExit(MouseExit);
        this.OnBeginDrag(BeginDrag);
        this.OnDrag(Drag);
        this.OnEndDrag(EndDrag);
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
        if (ItemData == null) return;
        if (m_IsSelect && Input.GetMouseButtonDown(1))
        {
            switch (ItemData.Config.ItemType)
            {
                case ItemType.Weapon:
                    Debug.Log("可以使用：" + ItemData.Config.ItemType);
                    break;
                case ItemType.Consumable:
                    Debug.Log("可以使用：" + ItemData.Config.ItemType);
                    break;
                default:
                    Debug.Log("无法使用");
                    break;
            }
        }
    }

    public void Init(int index, UI_InventoryWindow ownerWindow)
    {
        Index = index;
        m_OwnerWindow = ownerWindow;
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
        GameManager.Instance.SetCursorState(CursorState.Handle);
        m_BgImage.sprite = m_OwnerWindow.InventoryFrames[1];
        m_IsSelect = true;
        CurrentMouseEnterSlot = this;
    }

    void MouseExit(PointerEventData eventData, object[] arg2)
    {
        GameManager.Instance.SetCursorState(CursorState.Normal);
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
        GameManager.Instance.SetCursorState(CursorState.Handle);
        m_IconTransform.position = eventData.position;
    }

    void EndDrag(PointerEventData eventData, object[] arg2)
    {
        if (ItemData == null) return;
        if (CurrentMouseEnterSlot == null)
        {
            GameManager.Instance.SetCursorState(CursorState.Normal);
        }
        m_IconTransform.SetParent(m_SlotTransform);
        m_IconTransform.localPosition = Vector3.zero;

        if (CurrentMouseEnterSlot == this) return;
        if (CurrentMouseEnterSlot == WeaponSlot)
        {
            if (ItemData.Config.ItemType != ItemType.Weapon)
            {
                UIManager.Instance.AddTips("必须得是装备才行！");
                return;
            }
            Debug.Log("可以装备物品：" + ItemData.Config.Name);
        }

        if (CurrentMouseEnterSlot == null)
        {
            // 射线检测除了Mast外是否有其他UI物体
            EventSystem.current.RaycastAll(eventData, RaycastResultList);
            for (int i = 0; i < RaycastResultList.Count; i++)
            {
                RaycastResult raycastResult = RaycastResultList[i];

                // 是UI同时不是Mast作用的物体
                if (raycastResult.gameObject.GetComponent<RectTransform>()
                 && raycastResult.gameObject.name != "Mask")
                {
                    RaycastResultList.Clear();
                    return;
                }
            }
            RaycastResultList.Clear();

            m_OwnerWindow.RemoveItem(Index);

            // 从存档中移除这个数据
            Debug.Log("扔地上：" + ItemData.Config.Name);
            InitData();
        }
        else
        {
            // 快捷栏内部交换物品
            ItemData dragItem = ItemData;
            ItemData targetItem = CurrentMouseEnterSlot.ItemData;
            InitData(targetItem);
            CurrentMouseEnterSlot.InitData(dragItem);

            // 跨窗口交换物品
            m_OwnerWindow.SetItem(Index, targetItem);
            CurrentMouseEnterSlot.m_OwnerWindow.SetItem(CurrentMouseEnterSlot.Index, dragItem);
        }
        ArchiveManager.Instance.SaveInventoryData();
    }

    #endregion
}