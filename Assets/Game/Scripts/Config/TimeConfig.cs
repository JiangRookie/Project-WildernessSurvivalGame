using System;
using JKFrame;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "时间配置", menuName = "Config/时间配置")]
public class TimeConfig : ConfigBase
{
    [LabelText("时间阶段/状态数据")] public TimeStateConfig[] TimeStateConfigs; // 时间配置
}

/// <summary>
/// 时间阶段/状态配置
/// </summary>
[Serializable]
public class TimeStateConfig
{
    /// <summary>
    /// 某一个阶段的持续时间（如早上的持续时间、中午的持续时间等）
    /// </summary>
    public float PhaseDurationTime;

    public float SunIntensity;
    public Color SunColor;
    [OnValueChanged(nameof(SetRotation))] public Vector3 SunRotation;
    [HideInInspector] public Quaternion SunQuaternion;
    public bool Fog;
    public AudioClip BgAudioClip;

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
        float currPhaseRemainingTime, TimeStateConfig nextPhase, out Quaternion rotation, out Color color
      , out float sunIntensity
    )
    {
        // ratio 从 0 到 1
        var ratio = 1f - currPhaseRemainingTime / PhaseDurationTime;
        rotation = Quaternion.Slerp(SunQuaternion, nextPhase.SunQuaternion, ratio);
        color = Color.Lerp(SunColor, nextPhase.SunColor, ratio);
        sunIntensity = Mathf.Lerp(SunIntensity, nextPhase.SunIntensity, ratio);

        if (Fog) RenderSettings.fogDensity = 0.05f * (1 - ratio);

        return currPhaseRemainingTime > 0;
    }
}