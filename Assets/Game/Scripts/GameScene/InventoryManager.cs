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

    public int GetMainInventoryWindowItemCount(int configID)
    {
        return m_MainInventoryWindow.GetItemCount(configID);
    }

    public bool AddItemAndPlayAudioToMainInventoryWindow(int itemConfigID)
    {
        return m_MainInventoryWindow.AddItemAndPlayAudio(itemConfigID);
    }

    public void UpdateMainInventoryWindowItemsForBuild(BuildConfig buildConfig)
    {
        m_MainInventoryWindow.UpdateItemsForBuild(buildConfig);
    }

    public bool AddItemToMainInventoryWindow(int itemConfigID)
    {
        return m_MainInventoryWindow.AddItem(itemConfigID);
    }

    #endregion

    void OnGameSave()
    {
        // 保存住背包数据
        ArchiveManager.Instance.SaveMainInventoryData();
    }

    public void OpenStorageBoxWindow(StorageBoxController storageBox, InventoryData data, Vector2Int size)
    {
        ProjectTool.PlayAudio(AudioType.Bag);
        UIManager.Instance.Close<UI_StorageBoxInventoryWindow>();
        UIManager.Instance.Show<UI_StorageBoxInventoryWindow>().Init(storageBox, data, size);
    }
}