using UnityEngine;

public class PlayerAttack : PlayerStateBase
{
    public Quaternion AttackDirection;

    public override void Enter()
    {
        AttackDirection = PlayerCtrl.AttackDirection;
        PlayerAnimation("Attack");
    }

    public override void Update()
    {
        // 旋转到攻击方向
        PlayerCtrl.PlayerTransform.localRotation = Quaternion.Slerp(
            PlayerCtrl.PlayerTransform.localRotation
          , AttackDirection
          , Time.deltaTime * PlayerCtrl.RotateSpeed * 2);
    }
}