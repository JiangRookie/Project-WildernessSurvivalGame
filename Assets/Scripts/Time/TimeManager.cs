using System;
using System.Collections;
using JKFrame;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 时间状态数据
/// </summary>
[Serializable]
public class TimeStateData
{
    public float DurationTime;
    public float SunIntensity;
    public Color SunColor;
    [OnValueChanged(nameof(SetRotation))] public Vector3 SunRotation;
    [HideInInspector] public Quaternion SunQuaternion;

    void SetRotation() => SunQuaternion = Quaternion.Euler(SunRotation);

    /// <summary>
    /// 检查并计算时间
    /// </summary>
    /// <param name="currentTime">当前的时间(？剩余时间)</param>
    /// <param name="nextState">下一个阶段是什么</param>
    /// <param name="rotation">阳光的旋转</param>
    /// <param name="color">最终应用的颜色</param>
    /// <param name="sunIntensity">阳光强度</param>
    /// <returns>是否还在当前状态</returns>
    public bool CheckAndCalculateTime
        (float currentTime, TimeStateData nextState, out Quaternion rotation, out Color color, out float sunIntensity)
    {
        float ratio = 1f - currentTime / DurationTime;
        rotation = Quaternion.Slerp(this.SunQuaternion, nextState.SunQuaternion, ratio);
        color = Color.Lerp(this.SunColor, nextState.SunColor, ratio);
        sunIntensity = Mathf.Lerp(this.SunIntensity, nextState.SunIntensity, ratio);
        return currentTime > 0;
    }
}

public class TimeManager : LogicManagerBase<TimeManager>
{
    [SerializeField] Light MainLight;                // 主灯光
    [SerializeField] float SunIntensity;             // 当前太阳强度
    [SerializeField] TimeStateData[] TimeStateDatas; // 时间配置
    int m_CurrentStateIndex = 0;
    float m_CurrentTime = 0;
    int m_DayNum;
    [SerializeField, Range(0f, 30f)] float TimeScale = 1f;

    protected override void RegisterEventListener() { }

    protected override void CancelEventListener() { }

    void Start()
    {
        StartCoroutine(UpdateTime());
    }

    IEnumerator UpdateTime()
    {
        m_CurrentStateIndex = 0; // 默认是早上
        var nextIndex = m_CurrentStateIndex + 1;
        m_CurrentTime = TimeStateDatas[m_CurrentStateIndex].DurationTime;
        m_DayNum = 0;
        while (true)
        {
            yield return null;
            m_CurrentTime -= Time.deltaTime * TimeScale;
            if (TimeStateDatas[m_CurrentStateIndex].CheckAndCalculateTime(m_CurrentTime, TimeStateDatas[nextIndex]
                                                                        , out var rotation, out var color
                                                                        , out var intensity) == false)
            {
                // 切换下一个状态
                m_CurrentStateIndex = nextIndex;

                // 检查边界，超过就从0开始
                nextIndex = m_CurrentStateIndex + 1 >= TimeStateDatas.Length ? 0 : m_CurrentStateIndex + 1;

                // 如果现在是早上，也就是 m_CurrentStateIndex == 0，那么意味着天数 + 1
                if (m_CurrentStateIndex == 0) m_DayNum++;
                m_CurrentTime = TimeStateDatas[m_CurrentStateIndex].DurationTime;
            }

            MainLight.transform.rotation = rotation;
            MainLight.color = color;
            SetLight(intensity);
        }
    }

    void SetLight(float intensity)
    {
        MainLight.intensity = intensity;

        // 设置环境光的亮度
        RenderSettings.ambientIntensity = intensity;
    }
}