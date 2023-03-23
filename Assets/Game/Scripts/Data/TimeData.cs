using System;

/// <summary>
/// 时间数据
/// </summary>
[Serializable]
public class TimeData
{
    public int StateIndex;      // 状态索引
    public float CalculateTime; // 当前状态剩余时间
    public int DayNum;
}