using System.Collections.Generic;
using JKFrame;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 储物箱UI窗口
/// </summary>
[UIElement(true, "UI/UI_StorageBoxInventoryWindow", 1)]
public class UI_StorageBoxWindow : UI_InventoryWindowBase
{
    [SerializeField] Transform m_ItemParent;
    [SerializeField] Button m_CloseButton;
    StorageBoxController m_StorageBox;

    void Update()
    {
        if (PlayerController.Instance != null)
        {
            if (Vector3.Distance(PlayerController.Instance.transform.position, m_StorageBox.transform.position) > m_StorageBox.InteractiveDistance)
            {
                CloseButtonClick();
            }
        }
    }

    public override void OnInit()
    {
        m_Slots = new List<UI_ItemSlot>(10);
        m_CloseButton.onClick.AddListener(CloseButtonClick);
    }

    public void Init(StorageBoxController storageBox, InventoryData inventoryData, Vector2Int size)
    {
        m_StorageBox = storageBox;
        m_InventoryData = inventoryData;
        SetWindowSize(size);

        // 生成格子

        for (var i = 0; i < inventoryData.ItemDatas.Length; i++)
        {
            UI_ItemSlot slot = ResManager.Load<UI_ItemSlot>("UI/UI_ItemSlot", m_ItemParent);
            slot.Init(i, this);
            slot.InitData(inventoryData.ItemDatas[i]);
            m_Slots.Add(slot);
        }
    }

    void SetWindowSize(Vector2Int size)
    {
        // 宽度 = 两边 15 + 中间格子区域
        // 高度 = 上方50 + 中间格子区域 + 底部 15
        RectTransform rectTransform = transform as RectTransform;
        rectTransform.sizeDelta = new Vector2(30 + size.x * 100, 65 + size.y * 100);
    }

    void CloseButtonClick()
    {
        ProjectTool.PlayAudio(AudioType.Bag);
        Close();
    }

    public override void OnClose()
    {
        base.OnClose();
        ProjectTool.PlayAudio(AudioType.Bag);
        for (var i = 0; i < m_Slots.Count; i++)
        {
            m_Slots[i].PushGameObj2Pool();
        }
        m_Slots.Clear();
        m_InventoryData = null;
    }
}