using JKFrame;

public abstract class AIStateBase : StateBase
{
    protected AIBase m_AI;

    public override void Init(IStateMachineOwner owner, int stateType, StateMachine stateMachine)
    {
        base.Init(owner, stateType, stateMachine);
        m_AI = owner as AIBase;
    }
}