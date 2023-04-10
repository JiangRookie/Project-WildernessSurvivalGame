using System;

[Serializable]
public class StorageBoxData : IMapObjectTypeData
{
    InventoryData m_InventoryData;

    public StorageBoxData(int itemCount) => m_InventoryData = new InventoryData(itemCount);

    public InventoryData InventoryData => m_InventoryData;
}