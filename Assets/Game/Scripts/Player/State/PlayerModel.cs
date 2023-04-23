using System;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    Action<int> m_FootstepAction;
    Action m_StartHitAction;
    Action m_StopHitAction;
    Action m_AttackOverAction;
    Action m_HurtOverAction;
    Action m_DeadOverAction;
    [SerializeField] Transform m_WeaponRoot;
    public Transform WeaponRoot => m_WeaponRoot;

    public void Init
    (
        Action<int> footstepAction, Action startHitAction, Action stopHitAction, Action attackOverAction, Action hurtOverAction, Action deadOverAction
    )
    {
        m_FootstepAction = footstepAction;
        m_StartHitAction = startHitAction;
        m_StopHitAction = stopHitAction;
        m_AttackOverAction = attackOverAction;
        m_HurtOverAction = hurtOverAction;
        m_DeadOverAction = deadOverAction;
    }

    #region 动画事件

    void Footstep(int index) => m_FootstepAction?.Invoke(index);

    // 开始有伤害
    void StartHit() => m_StartHitAction?.Invoke();

    // 这里之后没有伤害
    void StopHit() => m_StopHitAction?.Invoke();

    // 整个攻击的结束
    void AttackOver() => m_AttackOverAction?.Invoke();

    void HurtOver() => m_HurtOverAction?.Invoke();

    void DeadOver() => m_DeadOverAction?.Invoke();

    #endregion
}