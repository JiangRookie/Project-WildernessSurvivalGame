using JKFrame;
using Project_WildernessSurvivalGame;

/// <summary>
/// 玩家状态基类，抽象出所有玩家状态所需要的共同字段、函数等
/// </summary>
public class PlayerStateBase : StateBase
{
    protected PlayerController PlayerController;

    public override void Init(IStateMachineOwner owner, int stateType, StateMachine stateMachine)
    {
        base.Init(owner, stateType, stateMachine);
        PlayerController = owner as PlayerController;
    }

    protected void PlayerAnimation(string animationName, float fixedTransitionDuration = 0.25f)
    {
        PlayerController.Animator.CrossFadeInFixedTime(animationName, fixedTransitionDuration);
    }
}