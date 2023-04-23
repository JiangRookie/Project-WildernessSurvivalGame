using JKFrame;

public class ScienceManager : SingletonMono<ScienceManager>
{
    ScienceData m_ScienceData;

    public void Init()
    {
        m_ScienceData = ArchiveManager.Instance.ScienceData;
        EventManager.AddEventListener(EventName.SaveGame, OnGameSave);
    }

    public bool CheckUnlock(int id) => m_ScienceData.CheckUnlock(id);

    public void AddScience(int id) => m_ScienceData.AddScience(id);

    static void OnGameSave() => ArchiveManager.Instance.SaveScienceData();
}