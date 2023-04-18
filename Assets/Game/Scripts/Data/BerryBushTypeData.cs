using System;

[Serializable]
public class BerryBushTypeData : IMapObjectTypeData
{
    public int LastPickUpDayNum = -1; // 浆果最后一次被采摘的天数
}