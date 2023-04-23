using System;
using System.Collections.Generic;

[Serializable]
public class ScienceData
{
    public List<int> UnlockedScienceList = new List<int>(10); // 已解锁科技列表

    public bool CheckUnlock(int id) => UnlockedScienceList.Contains(id);

    public void AddScience(int id)
    {
        if (UnlockedScienceList.Contains(id) == false)
        {
            UnlockedScienceList.Add(id);
        }
    }
}