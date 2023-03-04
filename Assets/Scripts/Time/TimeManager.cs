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
    /// <summary>
    /// 某一个阶段的持续时间（如早上的持续时间、中午的持续时间等）
    /// </summary>
    public float PhaseDurationTime;

    public float SunIntensity;
    public Color SunColor;
    [OnValueChanged(nameof(SetRotation))] public Vector3 SunRotation;
    [HideInInspector] public Quaternion SunQuaternion;

    void SetRotation() => SunQuaternion = Quaternion.Euler(SunRotation);

    /// <summary>
    /// 检查并计算时间
    /// </summary>
    /// <param name="currPhaseRemainingTime">当前阶段剩余时间</param>
    /// <param name="nextPhase">下一个阶段</param>
    /// <param name="rotation">阳光的旋转</param>
    /// <param name="color">最终应用的颜色</param>
    /// <param name="sunIntensity">阳光强度</param>
    /// <returns>是否还在当前状态</returns>
    public bool CheckAndCalculateTime
    (
        float currPhaseRemainingTime, TimeStateData nextPhase, out Quaternion rotation, out Color color
      , out float sunIntensity
    )
    {
        var ratio = 1f - currPhaseRemainingTime / PhaseDurationTime;
        rotation = Quaternion.Slerp(SunQuaternion, nextPhase.SunQuaternion, ratio);
        color = Color.Lerp(SunColor, nextPhase.SunColor, ratio);
        sunIntensity = Mathf.Lerp(SunIntensity, nextPhase.SunIntensity, ratio);
        return currPhaseRemainingTime > 0;
    }
}

public class TimeManager : LogicManagerBase<TimeManager>
{
    [SerializeField] Light MainLight;                // 主灯光
    [SerializeField] TimeStateData[] TimeStateDatas; // 时间配置
    [SerializeField, Range(0f, 30f)] float TimeScale = 1f;
    int m_CurrStateIndex;
    float m_CurrPhaseRemainingTime = 0;
    int m_DayNum;

    protected override void RegisterEventListener() { }

    protected override void CancelEventListener() { }

    void Start()
    {
        StartCoroutine(UpdateTime());
    }

    IEnumerator UpdateTime()
    {
        m_CurrStateIndex = 0; // 第一个阶段默认是早上
        var nextStateIndex = m_CurrStateIndex + 1;
        m_CurrPhaseRemainingTime = TimeStateDatas[m_CurrStateIndex].PhaseDurationTime;
        m_DayNum = 0;
        while (true)
        {
            yield return null;

            m_CurrPhaseRemainingTime -= Time.deltaTime * TimeScale; // 时间流逝
            if (TimeStateDatas[m_CurrStateIndex].CheckAndCalculateTime(m_CurrPhaseRemainingTime
                                                                     , TimeStateDatas[nextStateIndex]
                                                                     , out var rotation
                                                                     , out var color
                                                                     , out var intensity) == false)
            {
                // 切换下一个状态
                m_CurrStateIndex = nextStateIndex;

                // 检查边界，超过就从0开始
                nextStateIndex = m_CurrStateIndex + 1 >= TimeStateDatas.Length ? 0 : m_CurrStateIndex + 1;
                if (m_CurrStateIndex == 0) m_DayNum++; // m_CurrStateIndex == 0 意味着新的一天开始了
                m_CurrPhaseRemainingTime = TimeStateDatas[m_CurrStateIndex].PhaseDurationTime;
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