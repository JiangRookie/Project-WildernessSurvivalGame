using Sirenix.OdinInspector;
using UnityEngine;

public class StorageBoxController : BuildingBase
{
    StorageBoxData m_StorageBoxData;
    [LabelText("物品操数量"), SerializeField] Vector2Int m_UIWindowGridSize;

    public override void Init(MapChunkController chunk, ulong objectId, bool isFromBuild)
    {
        base.Init(chunk, objectId, isFromBuild);

        // 查找是否有存档
        // 先找到自身的地图对象数据，然后从中拿到箱子代表的背包数据
        if (isFromBuild)
        {
            m_StorageBoxData = new StorageBoxData(m_UIWindowGridSize.x * m_UIWindowGridSize.y);
            ArchiveManager.Instance.AddMapObjectTypeData(objectId, m_StorageBoxData);
        }
        else
        {
            m_StorageBoxData = ArchiveManager.Instance.GetMapObjectTypeData(objectId) as StorageBoxData;
        }
    }

    public override void OnSelect()
    {
        Debug.Log(m_StorageBoxData.InventoryData);

        // 打开储物箱的UI窗口
        InventoryManager.Instance.OpenStorageBoxWindow(this, m_StorageBoxData.InventoryData, m_UIWindowGridSize);
    }
}