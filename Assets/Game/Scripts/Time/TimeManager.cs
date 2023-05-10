using System.Collections;
using JKFrame;
using UnityEngine;

public class TimeManager : LogicManagerBase<TimeManager>
{
    // 这段代码实现了一个游戏中的时间系统，可以控制游戏的时间流逝和光照等特性。其中主要的变量包括：
    //
    // 1.  Light  m_MainLight：表示主灯光的引用；
    // 2.  float  TimeScale：时间流逝的速度；
    // 3.  TimeConfig  m_TimeConfig：表示游戏中时间配置的引用；
    // 4.  TimeData  m_TimeData：表示当前游戏所处时间的引用及信息；
    // 5.  int  m_NextStateIndex：表示下一个时间阶段的索引；
    // 6.  CurrDayNum：表示当前游戏进行到的时间天数。
    //
    // 其中，Init()函数主要用于初始化游戏时间系统，包括读取保存的游戏时间数据、载入时间配置、初始化游戏状态等。
    //
    // UpdateTime()函数用于计算当前游戏所处时间的变化，根据当前时间阶段和下一个时间阶段的信息计算光源的参数和状态，并进行相应的调整。如果当前时间阶段的时间已经用尽，则进入下一个时间阶段。
    //
    // EnterNextState()函数用于进入下一个时间阶段，包括更新当前时间阶段的索引、检查是否开始新的一天、更新相应的光源参数和状态等。同时还会发送一些游戏事件，例如更新时间状态和当前是第几天等。

    [SerializeField] Light m_MainLight; // 主灯光
    [SerializeField, Range(0f, 30f)] public float TimeScale = 1f;
    TimeConfig m_TimeConfig;
    TimeData m_TimeData;
    int m_NextStateIndex;
    public int CurrDayNum => m_TimeData.DayNum;

    void Update()
    {
        if (GameSceneManager.Instance.IsInitialized == false) return;
        UpdateTime();
    }

    public void Init()
    {
        m_TimeData = ArchiveManager.Instance.TimeData;
        m_TimeConfig = ConfigManager.Instance.GetConfig<TimeConfig>(ConfigName.Time);
        InitState();
        EventManager.AddEventListener(EventName.SaveGame, OnGameSave);
    }

    void InitState()
    {
        m_NextStateIndex = m_TimeData.StateIndex + 1 >= m_TimeConfig.TimeStateConfigs.Length ? 0 : m_TimeData.StateIndex + 1;
        RenderSettings.fog = m_TimeConfig.TimeStateConfigs[m_TimeData.StateIndex].Fog;
        if (m_TimeConfig.TimeStateConfigs[m_TimeData.StateIndex].BgAudioClip != null)
            StartCoroutine(ChangeBgAudio(m_TimeConfig.TimeStateConfigs[m_TimeData.StateIndex].BgAudioClip));

        // 发送是否为太阳的事件
        EventManager.EventTrigger(EventName.UpdateTimeState, m_TimeData.StateIndex <= 1);

        // 发送当前是第几天的事件
        EventManager.EventTrigger(EventName.UpdateDayNum, m_TimeData.DayNum);
    }

    void UpdateTime()
    {
        m_TimeData.CalculateTime -= Time.deltaTime * TimeScale; // 时间流逝 -> 当前阶段剩余时间减少

        if (m_TimeConfig.TimeStateConfigs[m_TimeData.StateIndex].CheckAndCalculateTime(
            m_TimeData.CalculateTime, m_TimeConfig.TimeStateConfigs[m_NextStateIndex]
          , out var rotation, out var color, out var intensity) == false)
        {
            EnterNextState();
        }

        SetLight(intensity, rotation, color);
    }

    void EnterNextState()
    {
        m_TimeData.StateIndex = m_NextStateIndex; // 切换下一个状态

        // 检查边界，超过就从0开始
        m_NextStateIndex = m_TimeData.StateIndex + 1 >= m_TimeConfig.TimeStateConfigs.Length ? 0 : m_TimeData.StateIndex + 1;

        if (m_TimeData.StateIndex == 0) // m_CurrStateIndex == 0 意味着新的一天开始了
        {
            m_TimeData.DayNum++;

            // 发送当前是第几天的事件
            EventManager.EventTrigger(EventName.UpdateDayNum, m_TimeData.DayNum);

            // 发送当前是早晨的事件
            EventManager.EventTrigger(EventName.OnMorning);
        }

        m_TimeData.CalculateTime = m_TimeConfig.TimeStateConfigs[m_TimeData.StateIndex].PhaseDurationTime;

        RenderSettings.fog = m_TimeConfig.TimeStateConfigs[m_TimeData.StateIndex].Fog;

        if (m_TimeConfig.TimeStateConfigs[m_TimeData.StateIndex].BgAudioClip != null)
            StartCoroutine(ChangeBgAudio(m_TimeConfig.TimeStateConfigs[m_TimeData.StateIndex].BgAudioClip));

        // 发送是否为太阳的事件
        EventManager.EventTrigger(EventName.UpdateTimeState, m_TimeData.StateIndex <= 1);
    }

    void SetLight(float intensity, Quaternion rotation, Color color)
    {
        // 设置环境光的亮度
        RenderSettings.ambientIntensity = intensity;
        m_MainLight.intensity = intensity;
        m_MainLight.transform.rotation = rotation;
        m_MainLight.color = color;
    }

    IEnumerator ChangeBgAudio(AudioClip audioClip)
    {
        var oldVolume = AudioManager.Instance.BGVolume;
        if (oldVolume <= 0) yield break;
        var currVolume = oldVolume;
        while (currVolume > 0)
        {
            yield return null;
            currVolume -= Time.deltaTime / 2;
            AudioManager.Instance.BGVolume = currVolume; // 降低背景音乐音量
        }
        AudioManager.Instance.PlayBGAudio(audioClip);

        while (currVolume < oldVolume)
        {
            yield return null;
            currVolume += Time.deltaTime / 2;
            AudioManager.Instance.BGVolume = currVolume; // 升高背景音乐音量
        }
        AudioManager.Instance.BGVolume = oldVolume;
    }

    void OnGameSave() => ArchiveManager.Instance.SaveTimeData();

    protected override void RegisterEventListener() { }

    protected override void CancelEventListener() { }
}