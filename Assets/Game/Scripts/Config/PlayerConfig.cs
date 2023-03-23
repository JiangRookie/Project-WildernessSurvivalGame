using JKFrame;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "玩家配置", menuName = "Config/玩家配置")]
public class PlayerConfig : ConfigBase
{
    #region 角色设置

    [FoldoutGroup("角色配置"), LabelText("移动速度")]
    public float MoveSpeed = 4;

    [FoldoutGroup("角色配置"), LabelText("旋转速度")]
    public float RotateSpeed = 10;

    [FoldoutGroup("角色配置"), LabelText("最大生命值")]
    public float MaxHp = 100;

    [FoldoutGroup("角色配置"), LabelText("最大饥饿值")]
    public float MaxHungry = 100;

    [FoldoutGroup("角色配置"), LabelText("饥饿值衰减速度")]
    public float HungryReducingSpeed = 0.2f;

    [FoldoutGroup("角色配置"), LabelText("当饥饿值为0时 生命值的衰减速度")]
    public float HpReducingSpeedOnHungryIsZero = 2;

    [FoldoutGroup("角色配置"), LabelText("脚步声")]
    public AudioClip[] FootStepAudioClips;

    [FoldoutGroup("角色配置"), LabelText("脚步声大小")]
    public float FootStepVolume = 0.5f;

    #endregion
}