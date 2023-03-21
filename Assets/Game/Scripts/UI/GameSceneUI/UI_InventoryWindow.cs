using JKFrame;

[UIElement(true, "UI/UI_InventoryWindow", 1)]
public class UI_InventoryWindow : UI_WindowBase
{
    InventoryData m_InventoryData;

    public override void OnShow()
    {
        base.OnShow();

        // 根据存档复原
        m_InventoryData = ArchiveManager.Instance.InventoryData;
    }

    void InitData()
    {
        // 基于存档去初始化所有的格子
    }

    public void AddItem() { }

    public void RemoveItem(int index) { }

    public void SetItem(int index, ItemData itemData) { }
}