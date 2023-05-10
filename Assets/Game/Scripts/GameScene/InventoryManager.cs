using JKFrame;
using UnityEngine;

public class InventoryManager : SingletonMono<InventoryManager>
{
    UI_MainInventoryWindow m_MainInventoryWindow; // 快捷栏

    public void Init()
    {
        m_MainInventoryWindow = UIManager.Instance.Show<UI_MainInventoryWindow>();
        m_MainInventoryWindow.InitData();
        EventManager.AddEventListener(EventName.SaveGame, OnGameSave);
    }

    #region 快捷栏

    public int GetMainInventoryItemCount(int configID) => m_MainInventoryWindow.GetItemCount(configID);

    public bool AddItemAndPlayAudio2MainInventory(int itemConfigID) => m_MainInventoryWindow.AddItemAndPlayAudio(itemConfigID);

    public void UpdateMainInventoryItemsForBuild(BuildConfig buildConfig) => m_MainInventoryWindow.UpdateItemsForBuild(buildConfig);

    public bool AddItemToMainInventory(int itemConfigID) => m_MainInventoryWindow.AddItem(itemConfigID);

    #endregion

    static void OnGameSave() => ArchiveManager.Instance.SaveMainInventoryData();

    public void OpenStorageBoxWindow(StorageBoxController storageBox, InventoryData data, Vector2Int size)
    {
        ProjectTool.PlayAudio(AudioType.Bag);
        UIManager.Instance.Close<UI_StorageBoxWindow>();
        UIManager.Instance.Show<UI_StorageBoxWindow>().Init(storageBox, data, size);
    }
}