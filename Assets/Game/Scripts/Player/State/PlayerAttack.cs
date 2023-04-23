using UnityEngine;

public class PlayerAttack : PlayerStateBase
{
    public Quaternion AttackDirection;

    public override void Enter()
    {
        AttackDirection = m_PlayerCtrl.AttackDir;
        m_PlayerCtrl.PlayerAnimation("Attack");
    }

    public override void Update()
    {
        // 旋转到攻击方向
        m_PlayerCtrl.PlayerTransform.localRotation = Quaternion.Slerp(
            m_PlayerCtrl.PlayerTransform.localRotation
          , AttackDirection
          , Time.deltaTime * m_PlayerCtrl.RotateSpeed * 2);
    }

    public override void Exit()
    {
        m_PlayerCtrl.OnStopHit();
    }
}