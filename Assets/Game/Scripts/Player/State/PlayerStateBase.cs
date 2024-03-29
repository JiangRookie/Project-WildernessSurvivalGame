using JKFrame;

/// <summary>
/// 玩家状态基类，抽象出所有玩家状态所需要的共同字段、函数等
/// </summary>
public class PlayerStateBase : StateBase
{
    protected PlayerController m_PlayerCtrl;

    public override void Init(IStateMachineOwner owner, int stateType, StateMachine stateMachine)
    {
        base.Init(owner, stateType, stateMachine);
        m_PlayerCtrl = owner as PlayerController;
    }

    protected void ChangeState<T>(PlayerState playerState) where T : PlayerStateBase, new()
    {
        StateMachine.ChangeState<T>((int)playerState);
    }
}