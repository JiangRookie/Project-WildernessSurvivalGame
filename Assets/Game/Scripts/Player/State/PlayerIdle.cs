using Project_WildernessSurvivalGame;
using UnityEngine;

/// <summary>
/// 玩家待机状态
/// </summary>
public class PlayerIdle : PlayerStateBase
{
    public override void Enter()
    {
        PlayerAnimation("Idle");
    }

    public override void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        if (horizontal != 0 || vertical != 0)
        {
            PlayerCtrl.ChangeState(PlayerState.Move);
        }
    }
}