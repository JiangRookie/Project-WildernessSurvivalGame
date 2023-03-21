using JKFrame;
using Project_WildernessSurvivalGame;

/// <summary>
/// 玩家状态基类，抽象出所有玩家状态所需要的共同字段、函数等
/// </summary>
public class PlayerStateBase : StateBase
{
    protected PlayerController PlayerCtrl;

    public override void Init(IStateMachineOwner owner, int stateType, StateMachine stateMachine)
    {
        base.Init(owner, stateType, stateMachine);
        PlayerCtrl = owner as PlayerController;
    }

    protected void PlayerAnimation(string animationName, float fixedTransitionDuration = 0.25f)
    {
        PlayerCtrl.Animator.CrossFadeInFixedTime(animationName, fixedTransitionDuration);
    }

    protected void ChangeState<T>(PlayerState playerState) where T : PlayerStateBase, new()
    {
        StateMachine.ChangeState<T>((int)playerState);
    }

    protected void ChangeState(PlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerState.Idle:
                StateMachine.ChangeState<PlayerIdle>(0);
                break;
            case PlayerState.Move:
                StateMachine.ChangeState<PlayerMove>(1);
                break;
            case PlayerState.Attack: break;
            case PlayerState.BeAttack: break;
            case PlayerState.Dead: break;
        }
    }
}